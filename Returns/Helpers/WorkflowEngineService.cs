using DocumentFormat.OpenXml.Spreadsheet;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Compliance;
using Returns.DTOs.Enforcement;
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers
{
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly ReturnsDbContext _db;
        private readonly IComplianceService _complianceService;
        private readonly IEmailService _emailService;
        private readonly IEnforcementService _enforcementService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICamelsAnalysisService _camelsAnalysisService;

        public enum ApprovalStatus
        {
            [Description("Pending")]
            Pending = 0,

            [Description("Recommendend for Approval")]
            RecommendForApproval = 1,

            [Description("Recommendend for Enforcement")]
            RecommendedForEnForcement = 2,

            [Description("Returned with Reservations")]
            ReturnedWithReservations = 3,

            [Description("Pending Enforcement")]
            PendingEnforcement = 4,
        }
        private string GetDescription(Enum value)
        {
            var fi = value.GetType()
                          .GetField(value.ToString())!;
            var attr = fi.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }

        public WorkflowEngineService(ReturnsDbContext db, ICamelsAnalysisService camelsAnalysisService, IBackgroundJobClient backgroundJobClient, IComplianceService complianceService, IEmailService emailService, IEnforcementService enforcementService)
        {
            _db = db;
            _complianceService = complianceService;
            _emailService = emailService;
            _enforcementService = enforcementService;
            _backgroundJobClient = backgroundJobClient;
            _camelsAnalysisService = camelsAnalysisService;
        }

        public async Task<WorkflowStateDto> RecommendForEnforcementAsync(RecommendStepRequest dto, string userId, string loggedInUserToken)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Fetch workflow instance
                var instance = await _db.WorkflowInstances
                    .Include(w => w.CurrentStep)
                    .FirstOrDefaultAsync(w => w.Id == dto.WorkFlowInstanceId)
                    ?? throw new InvalidOperationException("Workflow instance not found.");

                // 2. Validate user is the current assignee
                if (instance.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User is not assigned to this step.");
                }

                // 3. Fetch consistency check and CAELS rating for context
                string comment = dto.Reason?.Trim() ?? "";
           

                // 4. Log enforcement recommendation
                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    PeriodId = instance.PeriodId,
                    SaccoId = instance.SaccoId,
                    ReturnSubmissionId = instance.ReturnSubmissionId, // Null for Q groups
                    UserId = userId,
                    Comment = comment,
                    Status = ApprovalStatus.RecommendedForEnForcement.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

           

                // 6. Check if last step
                bool isLastStep = false;
                var lastSeq = await _db.WorkFlowSteps
                    .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                    .MaxAsync(s => s.Sequence);

                if (instance.CurrentStep.Sequence >= lastSeq)
                {
                    isLastStep = true;
                }

                if (isLastStep)
                {
                    // 7a. Final step: Hand off to enforcement
                    instance.CurrentStepId = null;
                    instance.Status = ApprovalStatus.RecommendedForEnForcement.ToString();
                    instance.CanBeSeen = false; // Hide after enforcement

                    var sacco = await _complianceService.GetSaccoByIdAsync(instance.SaccoId);
                    var caseDto = new EnforcementCaseRequestDTO
                    {
                        Title = $"Enforcement Case for {(instance.Type == "QGroup" ? "Quarterly Return" : "Return")}",
                        Description = comment,
                        SaccoId = instance.SaccoId,
                        SaccoName = sacco?.SaccoName ?? "Unknown SACCO",
                        Source = "Returns Module",
                        SourceReferenceNo = instance.Id,
                        Classification = dto.Classification ?? "Minor",
                        DateRequested = DateTime.UtcNow,
                        Remarks = comment
                    };

                    await _db.SaveChangesAsync();
                    await _enforcementService.SubmitCaseAsync(caseDto, loggedInUserToken);
                    await transaction.CommitAsync();

                    // Notify SACCO
                    _backgroundJobClient.Enqueue(() => NotifySaccoAsync(instance.SaccoId, instance.PeriodId, instance.Type, comment));
                }
                else
                {
                    // 7b. Hand over to next approver
                    var nextStep = await GetNextStepIdAsync(instance, bypassRating: true)
                        ?? throw new InvalidOperationException("Template expects another step but none was found.");

                    var nextApprover = await GetApproverForStep(nextStep, instance.TeamId, instance.SaccoId)
                        ?? throw new InvalidOperationException($"No approver found for next step '{nextStep.Id}'.");

                    instance.CurrentStepId = nextStep.Id;
                    instance.UserId = nextApprover.UserId;
                    instance.Status = ApprovalStatus.Pending.ToString(); // Reset to Pending for next step
                    instance.CanBeSeen = true;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Notify next approver
                    try
                    {
                        var sacco = await _complianceService.GetSaccoByIdAsync(instance.SaccoId);
                        await _emailService.SendEmailAsync(
                            nextApprover.Email,
                            "Return Recommended for Enforcement",
                            $"A {(instance.Type == "QGroup" ? "quarterly return group" : "return")} for SACCO {sacco?.SaccoName ?? instance.SaccoId} (Period: {instance.PeriodId}) has been recommended for enforcement. Reason: {comment}");
                    }
                    catch (Exception ex)
                    {
                        // Log email failure but don't fail the operation
                        Console.WriteLine($"Failed to send notification email: {ex.Message}");
                    }
                }

                return ConvertToDto(instance);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<List<PendingReturnDto>> GetPendingReturnsAsync(string userId)
        {
            var mySaccoIds = (await _complianceService.GetSaccosAssignedToOfficerAsync(userId))
                .Select(s => s.Id)
                .ToHashSet();

           /* // Visible statuses
            var visibleStatuses = new[]
            {
                ApprovalStatus.Pending.ToString(),
                ApprovalStatus.RecommendForApproval.ToString(),
                ApprovalStatus.RecommendedForEnForcement.ToString(),
                ApprovalStatus.ReturnedWithReservations.ToString(),
                ApprovalStatus.PendingEnforcement.ToString()
            };*/

            var workflowInstances = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .Where(w => w.CanBeSeen &&
                            //visibleStatuses.Contains(w.Status) &&
                            (w.UserId == userId || mySaccoIds.Contains(w.SaccoId)))
                .OrderBy(w => w.CreatedAt)
                .ToListAsync();

            var pendingReturns = new List<PendingReturnDto>();

            foreach (var instance in workflowInstances)
            {
                // Fetch period and SACCO details
                var period = await _db.ReturnPeriods.FindAsync(instance.PeriodId)
                    ?? throw new InvalidOperationException("Period not found");
                var sacco = await _complianceService.GetSaccoByIdAsync(instance.SaccoId);

                // Default values
                bool isConsistent = true;
                List<ValidationError> consistencyErrors = new();
                int? rating = instance.Rating;
                DateTime submittedDate = DateTime.MinValue;
                List<string> formSubmissionIds = new();

                // Fetch consistency check
                var consistencyCheck = await _db.ConsistencyCheckResults
                    .Where(cc => cc.SaccoId == instance.SaccoId && cc.PeriodId == instance.PeriodId)
                    .OrderByDescending(cc => cc.CheckedAt)
                    .FirstOrDefaultAsync();

                if (consistencyCheck != null)
                {
                    isConsistent = consistencyCheck.IsValid;
                    consistencyErrors = JsonSerializer.Deserialize<List<ValidationError>>(consistencyCheck.ErrorsJson) ?? new List<ValidationError>();
                }

                // Fetch CAELS rating
                var caelsRating = await _db.CAELSRatings
                    .Where(r => r.SaccoId == instance.SaccoId && r.PeriodId == instance.PeriodId)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (caelsRating != null)
                {
                    rating = (int?)caelsRating.OverallRating;  // Adjust based on your rating field
                }

                // Handle Q groups vs standalone
                if (instance.Type == "QGroup")
                {
                    var submissions = await _db.ReturnSubmissions
                        .Where(rs => rs.SaccoId == instance.SaccoId && rs.ExpectedReturn.PeriodId == instance.PeriodId)
                        .ToListAsync();

                    formSubmissionIds = submissions.Select(rs => rs.Id).ToList();
                    submittedDate = submissions.Any() ? submissions.Max(rs => rs.SubmittedAt) : DateTime.MinValue;
                }
                else
                {
                    var submission = await _db.ReturnSubmissions.FindAsync(instance.ReturnSubmissionId);
                    if (submission != null)
                    {
                        formSubmissionIds = new List<string> { submission.Id };
                        submittedDate = submission.SubmittedAt;
                    }
                }

                pendingReturns.Add(new PendingReturnDto
                {
                    WorkflowInstanceId = instance.Id,
                    PeriodId = instance.PeriodId,
                    ReturnSubmissionId = instance.ReturnSubmissionId,
                    SaccoId = instance.SaccoId,
                    SaccoName = sacco?.SaccoName ?? "Unknown SACCO",
                    SaccoType = sacco?.SaccoType ?? "Unknown",
                    Period = period.Name,
                    SubmittedDate = submittedDate,
                    IsConsistent = isConsistent,
                    ConsistencyErrors = consistencyErrors,
                    Rating = rating,
                    CurrentRole = instance.CurrentStep?.RoleName ?? "",
                    CurrentStep = instance.CurrentStep?.Sequence ?? 0,
                    Status = instance.Status,
                    Type = instance.Type,
                    //CanBeSeen = instance.CanBeSeen,
                    CanTakeAction = instance.UserId == userId,
                    FormSubmissionIds = formSubmissionIds
                });
            }

            return pendingReturns;
        }

        public async Task<WorkflowStateDto> GetCurrentStateAsync(string? periodId, string? saccoId, string? returnSubmissionId)
        {
            // 1. Validate inputs
            if ((string.IsNullOrEmpty(periodId) || string.IsNullOrEmpty(saccoId)) && string.IsNullOrEmpty(returnSubmissionId))
            {
                throw new ArgumentException("Either PeriodId and SaccoId (for Q groups) or ReturnSubmissionId (for non-Q) must be provided.");
            }

            // 2. Fetch workflow instance
            WorkflowInstance? instance = null;
            bool isQuarterly = false;

            if (!string.IsNullOrEmpty(returnSubmissionId))
            {
                // Non-Q return
                instance = await _db.WorkflowInstances
                    .Include(w => w.CurrentStep)
                    .FirstOrDefaultAsync(w => w.ReturnSubmissionId == returnSubmissionId && w.Type == "Standalone")
                    ?? throw new InvalidOperationException($"Workflow not found for return submission {returnSubmissionId}.");
            }
            else
            {
                // Q group
                instance = await _db.WorkflowInstances
                    .Include(w => w.CurrentStep)
                    .FirstOrDefaultAsync(w => w.PeriodId == periodId && w.SaccoId == saccoId && w.Type == "QGroup")
                    ?? throw new InvalidOperationException($"Workflow not found for Q group (Period: {periodId}, SACCO: {saccoId}).");

                var period = await _db.ReturnPeriods.FindAsync(periodId);
                isQuarterly = period?.FrequencyId == 5; // QTR
            }

            // 3. Fetch all steps for the workflow template
            var allSteps = await _db.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                .OrderBy(s => s.Sequence)
                .ToListAsync();

            var firstSeq = allSteps.First().Sequence;
            var lastSeq = allSteps.Last().Sequence;

            // 4. Fetch next steps
            var nextSteps = allSteps
                .Where(s => s.Sequence > (instance.CurrentStep?.Sequence ?? -1))
                .ToList();

            var nextStepDtos = new List<WorkflowStepDto>();
            foreach (var step in nextSteps)
            {
                var approver = await GetApproverForStep(step, instance.TeamId, instance.SaccoId)
                    ?? throw new InvalidOperationException($"No approver found for step {step.Id}.");

                nextStepDtos.Add(new WorkflowStepDto
                {
                    StepId = step.Id,
                    ApproverRole = step.RoleName,
                    ApproverUserId = approver.UserId,
                    IsFirst = step.Sequence == firstSeq,
                    IsLast = step.Sequence == lastSeq
                });
            }

            // 5. Fetch CAELS rating and consistency check (for Q groups)
            decimal? rating = instance.Rating;
            bool isConsistent = true;
            List<ValidationError> consistencyErrors = new();

            if (instance.Type == "QGroup")
            {
                var caelsRating = await _db.CAELSRatings
                    .Where(r => r.SaccoId == instance.SaccoId && r.PeriodId == instance.PeriodId)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (caelsRating != null)
                {
                    rating = caelsRating.OverallRating;
                }

                var consistencyCheck = await _db.ConsistencyCheckResults
                    .Where(cc => cc.SaccoId == instance.SaccoId && cc.PeriodId == instance.PeriodId)
                    .OrderByDescending(cc => cc.CheckedAt)
                    .FirstOrDefaultAsync();

                if (consistencyCheck != null)
                {
                    isConsistent = consistencyCheck.IsValid;
                    consistencyErrors = JsonSerializer.Deserialize<List<ValidationError>>(consistencyCheck.ErrorsJson) ?? new();
                }
            }

            // 6. Build DTO
            return new WorkflowStateDto
            {
                WorkflowInstanceId = instance.Id,
                PeriodId = instance.PeriodId,
                ReturnSubmissionId = instance.ReturnSubmissionId,
                SaccoId = instance.SaccoId,
                CurrentStepId = instance.CurrentStep?.RoleName ?? "",
                CurrentApproverId = instance.UserId,
                Status = instance.Status,
                Rating = (int)rating.Value,
                //Type = instance.Type,
                CanBeSeen = instance.CanBeSeen,
                IsFirst = instance.CurrentStep?.Sequence == firstSeq,
                IsLast = instance.CurrentStep == null || instance.CurrentStep.Sequence == lastSeq,
                NextSteps = nextStepDtos,
                IsConsistent = isConsistent,
                ConsistencyErrors = consistencyErrors
            };
        }

        public async Task<List<CommentDetails>> GetComments(string periodId, string saccoId, string? returnSubmissionId)
        {
            // 1. Validate inputs
            if ((string.IsNullOrEmpty(periodId) || string.IsNullOrEmpty(saccoId)) && string.IsNullOrEmpty(returnSubmissionId))
            {
                throw new ArgumentException("Either PeriodId and SaccoId (for Q groups) or ReturnSubmissionId (for non-Q) must be provided.");
            }

            // 2. Query ApprovalActions
            var commentsQuery = _db.ApprovalActions.AsQueryable();
            if (!string.IsNullOrEmpty(returnSubmissionId))
            {
                // Non-Q return: Filter by ResubmissionRequestId
                commentsQuery = commentsQuery.Where(c => c.ReturnSubmissionId == returnSubmissionId);
            }
            else
            {
                // Q group: Filter by PeriodId and SaccoId
                commentsQuery = commentsQuery.Where(c => c.PeriodId == periodId && c.SaccoId == saccoId);
            }

            var comments = await commentsQuery
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            // 3. Build CommentDetails list
            var commentDetails = new List<CommentDetails>();
            if (!comments.Any())
            {
                return commentDetails; // Return empty list if no comments
            }

            foreach (var comment in comments)
            {
                // 4. Fetch user details
                var userDetails = await _complianceService.GetUserDetailsAsync(comment.UserId);
                var approverName = userDetails?.FullName ?? "Unknown User";

                commentDetails.Add(new CommentDetails
                {
                    Comment = comment.Comment ?? "",
                    UserId = comment.UserId,
                    ApproverName = approverName,
                    Status = comment.Status,
                    CreatedAt = comment.CreatedAt
                });
            }

            return commentDetails;
        }



        public async Task<WorkflowStateDto> ReturnWithReservationsAsync(ReturnWithReservationsRequest req, string userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Fetch workflow instance
                var instance = await _db.WorkflowInstances
                    .Include(w => w.CurrentStep)
                    .FirstOrDefaultAsync(w => w.Id == req.WorkFlowInstanceId)
                    ?? throw new InvalidOperationException("Workflow instance not found.");

                // 2. Validate user is the current assignee
                if (instance.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User is not assigned to this step.");
                }

                // 3. Fetch consistency check for context (optional, to include in comment)
                string comment = req.Comment?.Trim() ?? "";
              

                // 4. Get approval history to find previous step
                var approvalHistory = await _db.ApprovalActions
                    .Where(a => a.PeriodId == instance.PeriodId && a.SaccoId == instance.SaccoId &&
                                a.Status != ApprovalStatus.Pending.ToString())
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                WorkFlowStep? stepToRevertTo = null;
                CommonFieldForUser? prevApprover = null;

                if (approvalHistory.Any())
                {
                    var lastActionTaken = approvalHistory.First();
                    stepToRevertTo = await _db.WorkFlowSteps
                        .FirstOrDefaultAsync(s => s.Id == lastActionTaken.WorkFlowStepId)
                        ?? throw new InvalidOperationException("Previous step not found.");

                    prevApprover = await GetApproverForStep(stepToRevertTo, instance.TeamId, instance.SaccoId)
                        ?? throw new InvalidOperationException("Previous approver not found.");
                }
                else
                {
                    throw new InvalidOperationException("No previous approver found.");

                    /*                    // No prior action: Revert to SACCO for resubmission
                    // Use step 0 (first step) or handle as resubmission
                    stepToRevertTo = await _db.WorkFlowSteps
                        .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId && s.Sequence == 0)
                        .FirstOrDefaultAsync()
                        ?? throw new InvalidOperationException("No first step found for resubmission.");

                    prevApprover = await GetApproverForStep(stepToRevertTo, instance.TeamId, instance.SaccoId)
                        ?? throw new InvalidOperationException("No initial approver found.");*/
                }

                // 5. Log the current action
                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    PeriodId = instance.PeriodId,
                    SaccoId = instance.SaccoId,
                    ReturnSubmissionId = instance.ReturnSubmissionId, // Null for Q groups
                    UserId = userId,
                    Comment = comment,
                    Status = ApprovalStatus.ReturnedWithReservations.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

            
                // 7. Update workflow instance
                instance.CurrentStepId = stepToRevertTo.Id;
                instance.UserId = prevApprover.UserId;
                instance.Status = ApprovalStatus.ReturnedWithReservations.ToString();
                instance.CanBeSeen = true; // Ensure visible to new assignee

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // 8. Notify previous approver or SACCO
                if (approvalHistory.Any())
                {
                    // Notify previous approver
                    try
                    {
                        var sacco = await _complianceService.GetSaccoByIdAsync(instance.SaccoId);
                        await _emailService.SendEmailAsync(
                            prevApprover.Email,
                            "Return Sent Back with Reservations",
                            $"A {(instance.Type == "QGroup" ? "quarterly return" : "return")} for SACCO {sacco?.SaccoName ?? instance.SaccoId} (Period: {instance.PeriodId}) has been sent back to you for further review. Reason: {comment}");
                    }
                    catch (Exception ex)
                    {
                        // Log email failure but don't fail the operation
                        Console.WriteLine($"Failed to send notification email: {ex.Message}");
                    }
                }
                else
                {
                    // Notify SACCO for resubmission
                    _backgroundJobClient.Enqueue(() => NotifySaccoAsync(instance.SaccoId, instance.PeriodId, instance.Type, comment));
                }

                return ConvertToDto(instance);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WorkflowStateDto> StartWorkflowAsync(string periodId, string saccoId, string? returnSubmissionId, int rating)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(periodId) || string.IsNullOrEmpty(saccoId))
                {
                    throw new ArgumentException("PeriodId and SaccoId are required.");
                }

                // 2. Get published workflow template
                var template = await _db.WorkFlowTemplates
                    .Include(t => t.WorkFlowSteps)
                    .FirstOrDefaultAsync(t => t.IsPublished)
                    ?? throw new InvalidOperationException("No published workflow template found.");

                // 3. Determine if this is a Q group or standalone return
                var period = await _db.ReturnPeriods.FindAsync(periodId)
                    ?? throw new InvalidOperationException("Period not found.");
                bool isQuarterly = period.FrequencyId == 5; // QTR
                string workflowType = isQuarterly ? "QGroup" : "Standalone";

                // 4. For Q groups, check completeness
                if (isQuarterly)
                {
                    var expectedForms = await _db.ExpectedReturns
                        .Where(er => er.PeriodId == periodId)
                        .Select(er => er.Id)
                        .ToListAsync();

                    var submittedForms = await _db.ReturnSubmissions
                        .Where(rs => rs.SaccoId == saccoId && expectedForms.Contains(rs.ExpectedReturnId))
                        .Select(rs => rs.ExpectedReturnId)
                        .ToListAsync();

                    if (expectedForms.Count != submittedForms.Count)
                    {
                        throw new InvalidOperationException("Incomplete Q return group. All expected forms must be submitted.");
                    }
                }
                else if (string.IsNullOrEmpty(returnSubmissionId))
                {
                    throw new ArgumentException("ReturnSubmissionId is required for non-quarterly returns.");
                }

                // 5. Get initial assignee from SaccoAssignment or fallback to TL
                string? assigneeUserId = null;
                var assignment = await _db.SaccoAssignments
                    .FirstOrDefaultAsync(a => a.SaccoId == saccoId);

                if (assignment != null)
                {
                    assigneeUserId = assignment.AssignedUserId;
                }
                else
                {
                    // Fallback to Teal Lead
                    var teamId = await _complianceService.GetTeamIdForSaccoAsync(saccoId)
                        ?? throw new InvalidOperationException("No team assigned to SACCO.");
                    var teamLead = await _complianceService.GetTeamLeaderAsync(teamId)
                        ?? throw new InvalidOperationException("No Team Lead found for SACCO's team.");
                    assigneeUserId = teamLead.UserId;
                }

                var assigneeDetails = await _complianceService.GetUserDetailsAsync(assigneeUserId)
                    ?? throw new InvalidOperationException("Assignee user not found.");

                // 6. Check for existing workflow instance
                WorkflowInstance? instance = null;
                if (isQuarterly)
                {
                    instance = await _db.WorkflowInstances
                        .FirstOrDefaultAsync(w => w.PeriodId == periodId && w.SaccoId == saccoId && w.Type == "QGroup");
                }
                else
                {
                    instance = await _db.WorkflowInstances
                        .FirstOrDefaultAsync(w => w.ReturnSubmissionId == returnSubmissionId && w.Type == "Standalone");
                }

                if (instance != null)
                {
                    instance.UserId = assigneeUserId;
                    instance.RoleName = assigneeDetails.Role;
                    instance.Status = ApprovalStatus.Pending.ToString();
                    instance.Rating = rating;
                    instance.CurrentStepId = template.WorkFlowSteps.OrderBy(s => s.Sequence).First().Id;
                    instance.CanBeSeen = true;
                    _db.WorkflowInstances.Update(instance);
                }
                else
                {
                    // Create new instance
                    instance = new WorkflowInstance
                    {
                        PeriodId = periodId,
                        ReturnSubmissionId = returnSubmissionId, // Null for Q groups
                        SaccoId = saccoId,
                        WorkflowTemplateId = template.Id,
                        TeamId = assigneeDetails.TeamId ?? throw new InvalidOperationException("Assignee has no team."),
                        UserId = assigneeUserId,
                        RoleName = assigneeDetails.Role,
                        Rating = rating,
                        CurrentStepId = template.WorkFlowSteps.OrderBy(s => s.Sequence).First().Id,
                        Status = ApprovalStatus.Pending.ToString(),
                        Type = workflowType,
                        CanBeSeen = true
                    };
                    await _db.WorkflowInstances.AddAsync(instance);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // 7. Notify assignee
                try
                {
                    var sacco = await _complianceService.GetSaccoByIdAsync(saccoId);
                    await _emailService.SendEmailAsync(
                        assigneeDetails.Email,
                        "New Return Assigned for Review",
                        $"A new {(isQuarterly ? "quarterly return group" : "return")} for SACCO {sacco?.SaccoName ?? saccoId} (Period: {period.Name}) has been assigned to you for review.");
                }
                catch (Exception ex)
                {
                    // Log email failure but don't fail the operation
                    Console.WriteLine($"Failed to send notification email: {ex.Message}");
                }

                return ConvertToDto(instance);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WorkflowStateDto> ApproveStepAsync(ApproveStepRequestDTO request, string userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Fetch workflow instance
                var instance = await _db.WorkflowInstances
                    .Include(i => i.CurrentStep)
                    .FirstOrDefaultAsync(i => i.Id == request.WorkFlowInstanceId)
                    ?? throw new InvalidOperationException($"Workflow instance '{request.WorkFlowInstanceId}' not found.");

                // 2. Validate user is the current assignee
                if (instance.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User is not assigned to approve this step.");
                }

                // 3. Check if already approved
                var alreadyApproved = await _db.ApprovalActions.AnyAsync(a =>
                    a.WorkFlowStepId == instance.CurrentStepId &&
                   /* a.PeriodId == instance.PeriodId &&
                    a.SaccoId == instance.SaccoId &&*/
                    a.UserId == userId &&
                    a.Status == ApprovalStatus.RecommendForApproval.ToString());

                if (alreadyApproved)
                {
                    return ConvertToDto(instance);
                }

            

                // 5. Log approval action
                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    PeriodId = instance.PeriodId,
                    SaccoId = instance.SaccoId,
                    ReturnSubmissionId = instance.ReturnSubmissionId, // Null for Q groups
                    UserId = userId,
                    Status = ApprovalStatus.RecommendForApproval.ToString(),
                    Comment = request.Comment?.Trim() ?? "",
                    CreatedAt = DateTime.UtcNow
                });


                // 7. Determine next step
                var nextStep = await GetNextStepIdAsync(instance);
                if (nextStep == null)
                {
                    // Workflow complete
                    instance.Status = ApprovalStatus.RecommendForApproval.ToString();
                    instance.CanBeSeen = false; // Hide after completion
                    instance.CurrentStepId = null;

                  
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Notify SACCO
                    _backgroundJobClient.Enqueue(() => NotifySaccoAsync(instance.SaccoId, instance.PeriodId, instance.Type));
                }
                else
                {
                    // Advance to next step
                    var nextApprover = await GetApproverForStep(nextStep, instance.TeamId, instance.SaccoId)
                        ?? throw new InvalidOperationException($"No approver found for next step '{nextStep.Id}'.");

                    instance.CurrentStepId = nextStep.Id;
                    instance.UserId = nextApprover.UserId;
                    instance.Status = ApprovalStatus.Pending.ToString();
                    instance.CanBeSeen = true;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    try
                    {
                        var sacco = await _complianceService.GetSaccoByIdAsync(instance.SaccoId);
                        await _emailService.SendEmailAsync(
                            nextApprover.Email,
                            "New Approval Request",
                            $"You have a new approval request for SACCO {sacco?.SaccoName ?? instance.SaccoId} ({(instance.Type == "QGroup" ? "Quarterly Group" : "Return")}) for period {instance.PeriodId}.");
                    }
                    catch (Exception ex)
                    {
                        // Log email failure but don't fail the operation
                        Console.WriteLine($"Failed to send notification email: {ex.Message}");
                    }

                }

                return ConvertToDto(instance);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task NotifySaccoAsync(string saccoId, string periodId, string type)
        {
            try
            {
                var sacco = await _complianceService.GetSaccoByIdAsync(saccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialSaccoEmail))
                {
                    return;
                }

                var period = await _db.ReturnPeriods.FindAsync(periodId);
                var rating = await _db.CAELSRatings
                    .Where(r => r.SaccoId == saccoId && r.PeriodId == periodId)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                var subject = "Compliance Report";
                var message = type == "QGroup"
                    ? $"Your quarterly returns for {period?.Name ?? periodId} has been approved. {(rating != null ? $"CAELS Rating: {rating.OverallRating} ({rating.RiskLevel})" : "Rating pending.")}"
                    : $"Your return for {period?.Name ?? periodId} has been approved.";

                await _emailService.SendEmailAsync(sacco.OfficialSaccoEmail, subject, message);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task NotifySaccoAsync(string saccoId, string periodId, string type, string rejectionComment)
        {
            try
            {
                var sacco = await _complianceService.GetSaccoByIdAsync(saccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialSaccoEmail))
                {
                    return;
                }

                var period = await _db.ReturnPeriods.FindAsync(periodId);
                var subject = "Return Rejection Notification";
                var message = type == "QGroup"
                    ? $"Your quarterly return group for {period?.Name ?? periodId} has been rejected. Reason: {rejectionComment}"
                    : $"Your return for {period?.Name ?? periodId} has been rejected. Reason: {rejectionComment}";

                await _emailService.SendEmailAsync(sacco.OfficialSaccoEmail, subject, message);
            }
            catch (Exception ex)
            {
            }
        }


        private async Task<CommonFieldForUser?> GetApproverForStep(WorkFlowStep step, string teamId, string SaccoId="")
        {
            var stepdetails = await _db.WorkFlowSteps
                .Include(s => s.ApprovalActions)
                .FirstOrDefaultAsync(s => s.Id == step.Id);
            if (stepdetails == null)
            {
                throw new Exception("Step not found.");
            }

            var RoleDetails = await _complianceService.GetRoleDetails(stepdetails.RoleId);
            if (RoleDetails == null)
            {
                throw new Exception("Role not assigned to this step.");
            }

            var RoleAssignedThisStep = RoleDetails.RoleName;
            CommonFieldForUser commonFieldForUser = new CommonFieldForUser();

            if (RoleAssignedThisStep == "Team Lead")
            {
                var teamLead = await _complianceService.GetTeamLead(teamId);
                if (teamLead == null)
                {
                    throw new Exception("No Team Lead found for this team.");
                }
                commonFieldForUser.FullName = teamLead.FullName;
                commonFieldForUser.Email = teamLead.Email;
                commonFieldForUser.RoleId = teamLead.RoleId;
                commonFieldForUser.UserId = teamLead.Id;
            }
            // if we are back to step 0, get assigned compliance officer
            else if (stepdetails.Sequence == 0)
            {
                var complianceOfficer = await _complianceService.GetAssignedComplianceOfficer(SaccoId);
                if (complianceOfficer == null)
                {
                    throw new Exception("No Compliance Officer assigned to this Sacco.");
                }
                commonFieldForUser.FullName = complianceOfficer.FullName;
                commonFieldForUser.Email = complianceOfficer.Email;
                commonFieldForUser.RoleId = complianceOfficer.Role;
                commonFieldForUser.UserId = complianceOfficer.Id;
            }
            else
            {
                var approver = await _complianceService.GetUserByRole(stepdetails.RoleId);
                if (approver == null)
                {
                    throw new Exception("No approver found for this step.");
                }
                commonFieldForUser.FullName = approver.FullName;
                commonFieldForUser.Email = approver.Email;
                commonFieldForUser.RoleId = approver.RoleId;
                commonFieldForUser.UserId = approver.Id;
            }

            return commonFieldForUser;
        }


        public class CommonFieldForUser
        {
            public string Email { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public string UserId { get; set; } = null!;
            public string RoleId { get; set; } = null!;
        }

        private async Task<WorkFlowStep?> GetNextStepIdAsync(WorkflowInstance instance,bool bypassRating = false)          // 👈 new flag (default = old behaviour)
        {
            var allSteps = await _db.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                .OrderBy(s => s.Sequence)
                .ToListAsync();

            if (allSteps.Count == 0)
            {
                throw new Exception("No steps defined in this workflow template");
            }

            // 2. Identify first, director, and last steps
            var firstStep = allSteps[0];
            var lastStep = allSteps[^1];
            var directorStep = allSteps[2];

            // 3. Load the step we just finished
            var current = await _db.WorkFlowSteps.FindAsync(instance.CurrentStepId)?? throw new Exception("Current step not found");


            if (!bypassRating)
            {
                //  Rating ≤ 2   stop after first step
                if (instance.Rating is <= 2)
                {
                    return current.Id == firstStep.Id ? null : null; 
                }

                // Rating == 3  stop after director step
                if (instance.Rating == 3 && current.Id == directorStep.Id)
                {
                    return null;
                }
            }

            // 5. Locate current in the ordered list
            int pos = allSteps.FindIndex(s => s.Id == current.Id);
            if (pos < 0)
                throw new Exception("Current step is not part of the active workflow steps");

            // 6. If already at last step → end (publish)
            if (pos == allSteps.Count - 1)
            {
                return null;
            }

            //next step in sequence
            return allSteps[pos + 1];
        }




        private WorkflowStateDto ConvertToDto(WorkflowInstance workflow)
        {
            return new WorkflowStateDto
            {
                WorkflowInstanceId = workflow.Id,
                PeriodId = workflow.PeriodId,
                ReturnSubmissionId = workflow.ReturnSubmissionId,
                SaccoId = workflow.SaccoId,
                CurrentStepId = workflow.CurrentStep?.RoleName,
                Status = workflow.Status,
                Rating = workflow.Rating,
                //Type = workflow.Type,
                //CanBeSeen = workflow.CanBeSeen,
                CurrentApproverId = workflow.UserId,
                IsFirst = workflow.CurrentStep?.Sequence == 0,
                IsLast = workflow.CurrentStep == null
            };
        }

    /*    public async Task<string> GetReturnStatus(string returnId)
        {
            var instance = await _db.WorkflowInstances
                .AsNoTracking()
                .Include(w => w.CurrentStep)
                .FirstOrDefaultAsync(w => w.ReturnId == returnId);

            if (instance == null)
                return "WorkflowNotFound";          

            if (!int.TryParse(instance.Status, out var code))
                return instance.Status;             

            var statusEnum = (ApprovalStatus)code;
            return GetDescription(statusEnum);    
        }*/

    }
}
