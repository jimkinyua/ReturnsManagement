using DocumentFormat.OpenXml.Spreadsheet;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Compliance;
using Returns.DTOs.Enforcement;
using Returns.DTOs.Returns.Admin;
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
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly ReturnsDbContext _db;
        private readonly IComplianceService _complianceService;
        private readonly IEmailService _emailService;
        private readonly IEnforcementService _enforcementService;
        private readonly ICamelsAnalysisService _camelsAnalysisService;
        private readonly ISaccoAssignmentService _saccoAssignmentService;
        private readonly ILogger<WorkflowEngineService> _logger;


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

        public WorkflowEngineService(ReturnsDbContext db, ISaccoAssignmentService saccoAssignmentService, ILogger<WorkflowEngineService> logger, ICamelsAnalysisService camelsAnalysisService, IComplianceService complianceService, IEmailService emailService, IEnforcementService enforcementService)
        {
            _db = db;
            _logger = logger;
            _complianceService = complianceService;
            _emailService = emailService;
            _enforcementService = enforcementService;
            _camelsAnalysisService = camelsAnalysisService;
            _saccoAssignmentService = saccoAssignmentService;
        }

        public async Task<WorkflowStateDto> RecommendForEnforcementAsync(RecommendStepRequest dto, string userId, string loggedInUserToken)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var instance = await _db.WorkflowInstances
                    .Include(w => w.CurrentStep)
                    .FirstOrDefaultAsync(w => w.Id == dto.WorkFlowInstanceId)
                    ?? throw new InvalidOperationException("Workflow instance not found.");

                if (instance.UserId != userId)
                    throw new UnauthorizedAccessException("User is not assigned to this step.");

                var comment = dto.Reason?.Trim() ?? "";

                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    PeriodId = instance.PeriodId,
                    SaccoId = instance.SaccoId,
                    ReturnSubmissionId = instance.ReturnSubmissionId,
                    UserId = userId,
                    Comment = comment,
                    Status = ApprovalStatus.RecommendedForEnForcement.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                // Check if current step is the last in template
                var lastSeq = await _db.WorkFlowSteps
                    .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                    .MaxAsync(s => s.Sequence);

                var isLastStep = instance.CurrentStep?.Sequence >= lastSeq;

                if (isLastStep)
                {
                    // Finalize → Enforcement case
                    instance.CurrentStepId = null;
                    instance.Status = ApprovalStatus.RecommendedForEnForcement.ToString();
                    instance.CanBeSeen = false;

                    long longSaccoId = long.Parse(instance.SaccoId);
                    var sacco = await _complianceService.GetSaccoByIdAsync(longSaccoId);

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

                    BackgroundJob.Enqueue(() => NotifySaccoAsync(instance.SaccoId, instance.PeriodId, instance.Type, comment));
                }
                else
                {
                    var nextStep = await GetNextStepIdAsync(instance, bypassRating: true)
                                   ?? throw new InvalidOperationException("Template expects another step but none was found.");

                    var (assigneeUserId, _, assigneeEmail) =
                        await ResolveAssigneeAsync(nextStep, instance.SaccoId, instance.TeamId);

                    instance.CurrentStepId = nextStep.Id;
                    instance.UserId = assigneeUserId;
                    instance.Status = ApprovalStatus.Pending.ToString();
                    instance.CanBeSeen = true;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Notify next assignee
                    try
                    {
                        long longSaccoId = long.Parse(instance.SaccoId);
                        var sacco = await _complianceService.GetSaccoByIdAsync(longSaccoId);
                        await _emailService.SendEmailAsync(
                            assigneeEmail,
                            "Return Recommended for Enforcement",
                            $"A {(instance.Type == "QGroup" ? "quarterly return group" : "return")} for SACCO {sacco?.SaccoName ?? instance.SaccoId} " +
                            $"(Period: {instance.PeriodId}) has been recommended for enforcement. Reason: {comment}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send enforcement email for workflow {WorkflowInstanceId}", instance.Id);
                    }
                }

                return ConvertToDto(instance);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }




        public async Task<List<PendingReturnDto>> GetPendingReturnsAsync(string userId)
        {
            var workflowInstances = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .Where(w => w.CanBeSeen && w.UserId == userId)
                .OrderBy(w => w.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var pendingReturns = new List<PendingReturnDto>();

            foreach (var instance in workflowInstances)
            {
                // Period
                var period = await _db.ReturnPeriods.FindAsync(instance.PeriodId)
                             ?? throw new InvalidOperationException("Period not found");

                // SACCO (convert id safely)
                var saccoIdText = instance.SaccoId;
                long saccoIdLong = 0;
                _ = long.TryParse(saccoIdText, out saccoIdLong);
                var sacco = saccoIdLong > 0 ? await _complianceService.GetSaccoByIdAsync(saccoIdLong) : null;

                // Defaults
                bool isConsistent = true;
                var consistencyErrors = new List<ValidationError>();
                int? rating = instance.Rating;
                DateTime submittedDate = DateTime.MinValue;
                var formSubmissionIds = new List<string>();

                // Consistency check (latest)
                var consistencyCheck = await _db.ConsistencyCheckResults
                    .Where(cc => cc.SaccoId == instance.SaccoId && cc.PeriodId == instance.PeriodId)
                    .OrderByDescending(cc => cc.CheckedAt)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (consistencyCheck != null)
                {
                    isConsistent = consistencyCheck.IsValid;
                    consistencyErrors = JsonSerializer.Deserialize<List<ValidationError>>(consistencyCheck.ErrorsJson)
                                       ?? new List<ValidationError>();
                }

                // CAELS rating (latest)
                var caelsRating = await _db.CAELSRatings
                    .Where(r => r.SaccoId == instance.SaccoId && r.PeriodId == instance.PeriodId)
                    .OrderByDescending(r => r.CreatedAt)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (caelsRating != null)
                {
                    rating = (int?)caelsRating.OverallRating; 
                }

                // Submissions (QGroup vs single)
                if (string.Equals(instance.Type, "QGroup", StringComparison.OrdinalIgnoreCase))
                {
                    var submissions = await _db.ReturnSubmissions
                        .Where(rs => rs.SaccoId == instance.SaccoId && rs.ExpectedReturn.PeriodId == instance.PeriodId)
                        .AsNoTracking()
                        .ToListAsync();

                    formSubmissionIds = submissions.Select(rs => rs.Id).ToList();
                    submittedDate = submissions.Count > 0 ? submissions.Max(rs => rs.SubmittedAt) : DateTime.MinValue;
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

                // Build "CurrentStep" display safely
                var stepSeq = instance.CurrentStep?.Sequence;
                var stepRole = instance.CurrentStep?.RoleName;
                var currentStepText = (stepSeq.HasValue || !string.IsNullOrWhiteSpace(stepRole))
                    ? $"Step {(stepSeq.GetValueOrDefault() + 1)}{(string.IsNullOrWhiteSpace(stepRole) ? "" : $" ({stepRole})")}"
                    : "No Step";

                pendingReturns.Add(new PendingReturnDto
                {
                    WorkflowInstanceId = instance.Id,
                    PeriodId = instance.PeriodId,
                    ReturnSubmissionId = instance.ReturnSubmissionId,
                    SaccoId = instance.SaccoId,
                    SaccoName = sacco?.SaccoName ?? "Unknown SACCO",
                    SaccoType = sacco?.SaccoType?.ToString() ?? "Unknown",
                    Period = period.Name,
                    SubmittedDate = submittedDate,
                    IsConsistent = isConsistent,
                    ConsistencyErrors = consistencyErrors,
                    Rating = rating,
                    CurrentRole = instance.CurrentStep?.RoleName ?? string.Empty,
                    CurrentStep = currentStepText,
                    Status = instance.Status,
                    Type = instance.Type,
                    CanTakeAction = instance.UserId == userId,
                    FormSubmissionIds = formSubmissionIds
                });
            }

            return pendingReturns;
        }

        public async Task<WorkflowStateDto?> GetCurrentStateAsync(string? periodId, string? saccoId, string? returnSubmissionId)
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
                    .FirstOrDefaultAsync(w => w.ReturnSubmissionId == returnSubmissionId && w.Type == "Standalone");
            }
            else
            {
                // Q group
                instance = await _db.WorkflowInstances
                    .Include(w => w.CurrentStep)
                    .FirstOrDefaultAsync(w => w.PeriodId == periodId && w.SaccoId == saccoId && w.Type == "QGroup");

                var period = await _db.ReturnPeriods.FindAsync(periodId);
                isQuarterly = period?.FrequencyId == 5; // QTR
            }

            if (instance == null)
            {
                return null;
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

            /* // 5. Fetch CAELS rating and consistency check (for Q groups)
             decimal? rating = instance.Rating;
             bool isConsistent = true;
             List<ValidationError> consistencyErrors = new();
 */
            /* if (instance.Type == "QGroup")
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
             }*/

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
                //Rating = (int)rating.Value,
                //Type = instance.Type,
                //CanBeSeen = instance.CanBeSeen,
                IsFirst = instance.CurrentStep?.Sequence == firstSeq,
                IsLast = instance.CurrentStep == null || instance.CurrentStep.Sequence == lastSeq,
                NextSteps = nextStepDtos,
                //IsConsistent = isConsistent,
                //ConsistencyErrors = consistencyErrors
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
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1) Load instance
                var instance = await _db.WorkflowInstances
                    .Include(w => w.CurrentStep)
                    .FirstOrDefaultAsync(w => w.Id == req.WorkFlowInstanceId)
                    ?? throw new InvalidOperationException("Workflow instance not found.");

                // 2) Must be current assignee
                if (instance.UserId != userId)
                    throw new UnauthorizedAccessException("User is not assigned to this step.");

                var comment = req.Comment?.Trim() ?? string.Empty;

                var currentSeq = instance.CurrentStep != null ? instance.CurrentStep.Sequence : int.MaxValue;
                var hasReturnSubmission = !string.IsNullOrEmpty(instance.ReturnSubmissionId);
                var returnSubmissionId = instance.ReturnSubmissionId;
                var pendingStatus = ApprovalStatus.Pending.ToString();

                // 3) Get previous approver action (strictly before current step in sequence)
                // 
                var prevActionQ = from a in _db.ApprovalActions
                    join s in _db.WorkFlowSteps on a.WorkFlowStepId equals s.Id
                    where a.PeriodId == instance.PeriodId
                       && a.SaccoId == instance.SaccoId
                       && (hasReturnSubmission ? a.ReturnSubmissionId == returnSubmissionId
                                               : a.ReturnSubmissionId == null)
                       && a.Status != pendingStatus
                       && s.WorkFlowTemplateId == instance.WorkflowTemplateId
                       && s.Sequence < currentSeq
                    orderby a.CreatedAt descending
                    select new { Action = a, Step = s };

                var lastPrev = await prevActionQ.FirstOrDefaultAsync();

                // 4) Log the return action for current step
                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    PeriodId = instance.PeriodId,
                    SaccoId = instance.SaccoId,
                    ReturnSubmissionId = instance.ReturnSubmissionId,
                    UserId = userId,
                    Comment = comment,
                    Status = ApprovalStatus.ReturnedWithReservations.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                WorkFlowStep targetStep;
                string targetUserId;
                string? targetEmail = null;

                if (lastPrev != null)
                {
                    // 5a) Go back to the actual previous approver (user-specific)
                    targetStep = lastPrev.Step;

                    targetUserId = lastPrev.Action.UserId; // real person who acted previously
                    var user = await _complianceService.GetUserDetailsAsync(targetUserId);
                    targetEmail = user?.Email;
                }
                else
                {
                    // 5b) Fallback: no previous approver — send to first step's assignee
                    var firstStep = await _db.WorkFlowSteps
                        .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                        .OrderBy(s => s.Sequence)
                        .FirstOrDefaultAsync()
                        ?? throw new InvalidOperationException("No steps found for this workflow template.");

                    var (assigneeUserId, _, assigneeEmail) =
                        await ResolveAssigneeAsync(firstStep, instance.SaccoId, instance.TeamId);

                    targetStep = firstStep;
                    targetUserId = assigneeUserId;
                    targetEmail = assigneeEmail;
                }

                // 6) Update instance
                instance.CurrentStepId = targetStep.Id;
                instance.UserId = targetUserId;
                instance.Status = ApprovalStatus.ReturnedWithReservations.ToString();
                instance.CanBeSeen = true;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // 7) Notify target
                try
                {
                    if (!string.IsNullOrWhiteSpace(targetEmail))
                    {
                        long longSaccoId = long.Parse(instance.SaccoId);
                        var sacco = await _complianceService.GetSaccoByIdAsync(longSaccoId);
                        await _emailService.SendEmailAsync(
                            targetEmail,
                            "Return Sent Back with Reservations",
                            $"A {(instance.Type == "QGroup" ? "quarterly return group" : "return")} for " +
                            $"SACCO {sacco?.SaccoName ?? instance.SaccoId} (Period: {instance.PeriodId}) " +
                            $"has been sent back to you for further review. Reason: {comment}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send return-with-reservations email for workflow {WorkflowInstanceId}", instance.Id);
                }

                return ConvertToDto(instance);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }




        public async Task<WorkflowReassignmentResultDTO> ReassignWorkflowAsync(WorkflowReassignmentRequestDTO request, LoggedInEntity loggedInEntity)
        {
            try
            {
                var workflowInstance = await _db.WorkflowInstances
                    .FirstOrDefaultAsync(wi => wi.Id == request.WorkflowInstanceId);

                if (workflowInstance == null)
                {
                    return new WorkflowReassignmentResultDTO
                    {
                        Success = false,
                        Message = "Workflow instance not found"
                    };
                }
                var NewGuyDetails = await _complianceService.GetUserDetailsAsync(request.NewAssigneeId);
                if (NewGuyDetails is null)
                {
                    return new WorkflowReassignmentResultDTO
                    {
                        Success = false,
                        Message = "Assignee Does Not Exist"
                    };
                }
                var previousAssigneeId = workflowInstance.UserId;

                // Update the assignee
                workflowInstance.UserId = request.NewAssigneeId;
                workflowInstance.UpdatedAt = DateTime.UtcNow;
                workflowInstance.UpdatedBy = loggedInEntity.UserId;
                workflowInstance.RoleName = NewGuyDetails.RoleName;

                _db.WorkflowInstances.Entry(workflowInstance).State = EntityState.Modified;
                _db.WorkflowInstances.Update(workflowInstance);
                await _db.SaveChangesAsync();

                return new WorkflowReassignmentResultDTO
                {
                    Success = true,
                    Message = "Workflow reassigned successfully",
                    PreviousAssigneeId = previousAssigneeId,
                    NewAssigneeId = request.NewAssigneeId,
                    ReassignedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning workflow {WorkflowInstanceId}", request.WorkflowInstanceId);
                return new WorkflowReassignmentResultDTO
                {
                    Success = false,
                    Message = $"Error reassigning workflow: {ex.Message}"
                };
            }
        }

        public async Task<WorkflowStateDto> StartWorkflowAsync(string periodId, string saccoId, string? returnSubmissionId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(periodId) || string.IsNullOrWhiteSpace(saccoId))
                    throw new ArgumentException("PeriodId and SaccoId are required.");

                // 1) Load published template (with steps)
                var template = await _db.WorkFlowTemplates
                    .Include(t => t.WorkFlowSteps)
                    .FirstOrDefaultAsync(t => t.IsPublished)
                    ?? throw new InvalidOperationException("No published workflow template found.");

                if (template.WorkFlowSteps == null || !template.WorkFlowSteps.Any())
                    throw new InvalidOperationException("Workflow template has no steps.");

                // 2) Period info
                var period = await _db.ReturnPeriods.FindAsync(periodId)
                             ?? throw new InvalidOperationException("Period not found.");
                bool isQuarterly = period.FrequencyId == 5;
                string wfType = isQuarterly ? "QGroup" : "Standalone"; // <- keep consistent across codebase

                // 3) CAELS & completeness (quarterly)
                decimal ratingScore = 0;
                string? riskBand = null;
                bool hasCAELS = false;
                bool isPeriodComplete = false;
                string? missingFormsJson = null;

                if (isQuarterly)
                {
                    var ratingRow = await _db.CAELSRatings
                        .Where(r => r.SaccoId == saccoId && r.PeriodId == periodId)
                        .OrderByDescending(r => r.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (ratingRow != null)
                    {
                        ratingScore = ratingRow.OverallRating;
                        riskBand = ratingRow.RiskLevel;
                        hasCAELS = true;
                    }
                    else
                    {
                        _logger.LogWarning("No CAELS rating found for quarterly return: SaccoId {SaccoId}, PeriodId {PeriodId}", saccoId, periodId);
                    }

                    var completeness = await _db.ReturnCompleteness
                        .FirstOrDefaultAsync(rc => rc.PeriodId == periodId && rc.SaccoId == saccoId);

                    if (completeness != null)
                    {
                        isPeriodComplete = completeness.IsComplete;
                        missingFormsJson = completeness.MissingForms;
                        if (!isPeriodComplete)
                        {
                            _logger.LogWarning(
                                "Incomplete quarterly return group: SaccoId {SaccoId}, PeriodId {PeriodId}. Expected {ExpectedCount}, Submitted {SubmittedCount}, Missing: {MissingForms}",
                                saccoId, periodId, completeness.ExpectedReturnCount, completeness.ActualReturnCount, missingFormsJson);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No ReturnCompleteness record found for SaccoId {SaccoId}, PeriodId {PeriodId}. Falling back to manual check.", saccoId, periodId);

                        var expected = await _db.ExpectedReturns
                            .Where(er => er.PeriodId == periodId)
                            .Select(er => er.Id)
                            .ToListAsync();

                        var submitted = await _db.ReturnSubmissions
                            .Where(rs => rs.SaccoId == saccoId && expected.Contains(rs.ExpectedReturnId))
                            .Select(rs => rs.ExpectedReturnId)
                            .ToListAsync();

                        isPeriodComplete = expected.Count == submitted.Count;
                        if (!isPeriodComplete)
                        {
                            _logger.LogWarning(
                                "Incomplete quarterly return group (fallback): SaccoId {SaccoId}, PeriodId {PeriodId}. Expected {ExpectedCount}, Submitted {SubmittedCount}",
                                saccoId, periodId, expected.Count, submitted.Count);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(returnSubmissionId))
                {
                    throw new ArgumentException("ReturnSubmissionId is required for non-quarterly returns.");
                }

                // 4) Completeness flags summary
                bool isComplete = !isQuarterly || (hasCAELS && isPeriodComplete);
                string incompletenessNotes = string.Empty;
                if (!isComplete)
                {
                    var notes = new List<string>();
                    if (!hasCAELS) notes.Add("Missing CAELS rating");
                    if (!isPeriodComplete)
                    {
                        string missingDetails = string.IsNullOrEmpty(missingFormsJson)
                            ? "Incomplete return submissions"
                            : $"Incomplete return submissions (Missing: {missingFormsJson.Replace("[", "").Replace("]", "").Replace("\"", "")})";
                        notes.Add(missingDetails);
                    }
                    incompletenessNotes = string.Join("; ", notes);
                }

                // 5) Upsert workflow instance (by QGroup or Standalone key)
                WorkflowInstance? instance =
                    isQuarterly
                        ? await _db.WorkflowInstances
                            .FirstOrDefaultAsync(w => w.PeriodId == periodId && w.SaccoId == saccoId && w.Type == "QGroup")
                        : await _db.WorkflowInstances
                            .FirstOrDefaultAsync(w => w.ReturnSubmissionId == returnSubmissionId && w.Type == "Standalone");

                // 6) Determine first step and resolve concrete assignee (user-specific)
                var firstStep = template.WorkFlowSteps.OrderBy(s => s.Sequence).First();

                // TeamId is derived from SACCO assignment
                var teamId = await _complianceService.GetTeamIdForSaccoAsync(saccoId)
                             ?? throw new InvalidOperationException("No team assigned for this SACCO.");

                var (assigneeUserId, assigneeName, assigneeEmail) = await ResolveAssigneeAsync(firstStep, saccoId, teamId); // <- user-specific resolver per new model

                // get assignee role (for display)
                var assigneeDetails = await _complianceService.GetUserDetailsAsync(assigneeUserId)
                                      ?? throw new InvalidOperationException("Assignee user not found.");

                if (instance == null)
                {
                    instance = new WorkflowInstance
                    {
                        PeriodId = periodId,
                        ReturnSubmissionId = returnSubmissionId,
                        SaccoId = saccoId,
                        WorkflowTemplateId = template.Id,
                        TeamId = teamId,
                        UserId = assigneeUserId,                 // user-specific assignment
                        RoleName = assigneeDetails.RoleName,     // optional label
                        Rating = (int)ratingScore,
                        CurrentStepId = firstStep.Id,
                        Status = ApprovalStatus.Pending.ToString(),
                        Type = wfType,
                        CanBeSeen = true,
                        IsComplete = isComplete,
                        IncompletenessNotes = incompletenessNotes
                    };
                    await _db.WorkflowInstances.AddAsync(instance);
                }
                else
                {
                    instance.UserId = assigneeUserId;               // reassign to resolved user
                    instance.RoleName = assigneeDetails.RoleName;
                    instance.Status = ApprovalStatus.Pending.ToString();
                    instance.Rating = (int)ratingScore;
                    instance.CurrentStepId = firstStep.Id;
                    instance.CanBeSeen = true;
                    instance.TeamId = teamId;                       // ensure set
                    instance.IsComplete = isComplete;
                    instance.IncompletenessNotes = incompletenessNotes;
                    _db.WorkflowInstances.Update(instance);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // 7) Notify assignee (fire-and-forget), include incompleteness
                _ = Task.Run(async () =>
                {
                    try
                    {
                        long longSaccoId = long.Parse(saccoId);
                        var sacco = await _complianceService.GetSaccoByIdAsync(longSaccoId);
                        var emailBody =
                            $"A new {(isQuarterly ? "quarterly " : "")}return for SACCO {sacco?.SaccoName ?? saccoId} (Period: {period.Name}) has been submitted.";
                        if (!isComplete)
                        {
                            emailBody += $" Note: This submission is incomplete: {incompletenessNotes}.";
                        }

                        await _emailService.SendEmailAsync(
                            assigneeEmail,
                            "New Return Submitted for Review",
                            emailBody);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send start-workflow email for workflow {WorkflowInstanceId}", instance.Id);
                    }
                });

                return ConvertToDto(instance);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }



        public async Task<WorkflowStateDto> ApproveStepAsync(ApproveStepRequestDTO request, string userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var instance = await _db.WorkflowInstances
                    .Include(i => i.CurrentStep)
                    .FirstOrDefaultAsync(i => i.Id == request.WorkFlowInstanceId)
                    ?? throw new InvalidOperationException($"Workflow instance '{request.WorkFlowInstanceId}' not found.");

                if (instance.UserId != userId)
                    throw new UnauthorizedAccessException("User is not assigned to approve this step.");

                // Prevent duplicate approvals by same user on same step
                var alreadyApproved = await _db.ApprovalActions.AnyAsync(a =>
                    a.WorkFlowStepId == instance.CurrentStepId &&
                    a.PeriodId == instance.PeriodId &&
                    a.UserId == userId &&
                    a.Status == ApprovalStatus.RecommendForApproval.ToString());

                if (alreadyApproved)
                    return ConvertToDto(instance);

                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    PeriodId = instance.PeriodId,
                    SaccoId = instance.SaccoId,
                    ReturnSubmissionId = instance.ReturnSubmissionId,
                    UserId = userId,
                    Status = ApprovalStatus.RecommendForApproval.ToString(),
                    Comment = request.Comment?.Trim() ?? "",
                    CreatedAt = DateTime.UtcNow
                });

                var nextStep = await GetNextStepIdAsync(instance);
                if (nextStep == null)
                {
                    // Complete
                    instance.Status = ApprovalStatus.RecommendForApproval.ToString();
                    instance.CanBeSeen = false;
                    instance.CurrentStepId = null;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    BackgroundJob.Enqueue(() => NotifySaccoAsync(instance.SaccoId, instance.PeriodId, instance.Type));
                }
                else
                {

                    var (assigneeUserId, _, assigneeEmail) = await ResolveAssigneeAsync(nextStep, instance.SaccoId, instance.TeamId);

                    instance.CurrentStepId = nextStep.Id;
                    instance.UserId = assigneeUserId;
                    instance.Status = ApprovalStatus.Pending.ToString();
                    instance.CanBeSeen = true;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Notify next assignee
                    try
                    {
                        long longSaccoId = long.Parse(instance.SaccoId);
                        var sacco = await _complianceService.GetSaccoByIdAsync(longSaccoId);
                        await _emailService.SendEmailAsync(
                            assigneeEmail,
                            "New Approval Request",
                            $"You have a new approval request for SACCO {sacco?.SaccoName ?? instance.SaccoId} " +
                            $"({(instance.Type == "QGroup" ? "Quarterly Group" : "Return")}) for period {instance.PeriodId}.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send approval email for workflow {WorkflowInstanceId}", instance.Id);
                    }
                }

                return ConvertToDto(instance);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        private async Task NotifySaccoAsync(string saccoId, string periodId, string type)
        {
            try
            {
                long LongsaccoId = long.Parse(saccoId);
                var sacco = await _complianceService.GetSaccoByIdAsync(LongsaccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialEmail))
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

                await _emailService.SendEmailAsync(sacco.OfficialEmail, subject, message);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task NotifySaccoAsync(string saccoId, string periodId, string type, string rejectionComment)
        {
            try
            {
                long LongsaccoId = long.Parse(saccoId);
                var sacco = await _complianceService.GetSaccoByIdAsync(LongsaccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialEmail))
                {
                    return;
                }

                var period = await _db.ReturnPeriods.FindAsync(periodId);
                var subject = "Return Rejection Notification";
                var message = type == "QGroup"
                    ? $"Your quarterly return group for {period?.Name ?? periodId} has been rejected. Reason: {rejectionComment}"
                    : $"Your return for {period?.Name ?? periodId} has been rejected. Reason: {rejectionComment}";

                await _emailService.SendEmailAsync(sacco.OfficialEmail, subject, message);
            }
            catch (Exception ex)
            {
            }
        }


        private async Task<CommonFieldForUser?> GetApproverForStep(WorkFlowStep step, string teamId, string SaccoId = "")
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
            if (RoleAssignedThisStep == "Assistant Manager")
            {
                var teamLead = await _complianceService.GetTeamLeaderAsync(teamId);
                if (teamLead == null)
                {
                    throw new Exception("No Team Lead found for this team.");
                }
                commonFieldForUser.FullName = teamLead.FullName;
                commonFieldForUser.Email = teamLead.Email;
                commonFieldForUser.RoleId = teamLead.RoleName;
                commonFieldForUser.UserId = teamLead.UserId;
            }
            // If we are back to step 0, get assigned compliance officer
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
                var approvers = await _complianceService.GetUsersByRole(stepdetails.RoleId);
                if (approvers == null || !approvers.Any())
                {
                    return null;
                }
                else
                {
                    // Select the first user from the list (or apply other logic)
                    var approver = approvers.First();
                    commonFieldForUser.FullName = approver.FullName;
                    commonFieldForUser.Email = approver.Email;
                    commonFieldForUser.RoleId = approver.Id;
                    commonFieldForUser.UserId = approver.Id;
                }
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

        private async Task<WorkFlowStep?> GetNextStepIdAsync(WorkflowInstance instance, bool bypassRating = false)          // 👈 new flag (default = old behaviour)
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
            var current = await _db.WorkFlowSteps.FindAsync(instance.CurrentStepId) ?? throw new Exception("Current step not found");


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

        public async Task<List<TLWorkflowOverviewDTO>> GetTLWorkflowOverviewAsync(string teamLeadId)
        {
            var saccos = await _saccoAssignmentService.GetAssignableSaccosAsync(teamLeadId);
            if (saccos == null || !saccos.Any())
            {
                return new List<TLWorkflowOverviewDTO>();
            }
            var saccoIds = saccos.Select(s => s.SaccoId).ToList();
            var workflowInstances = await _db.WorkflowInstances
                .Where(wi => saccoIds.Contains(wi.SaccoId))
                .OrderByDescending(wi => wi.CreatedAt)
                .ToListAsync();
            var overview = new List<TLWorkflowOverviewDTO>();
            foreach (var instance in workflowInstances)
            {
                var Sacco = await _complianceService.GetSaccoByIdAsync(long.Parse(instance.SaccoId));
                if (Sacco == null)
                {
                    continue; // Skip if Sacco not found
                }
                var Assignee = await _complianceService.GetUserDetailsAsync(instance.UserId);
                if (Assignee == null)
                {
                    continue; // Skip if Assignee not found
                }
                var Period = await _db.ReturnPeriods
                    .FirstOrDefaultAsync(p => p.Id == instance.PeriodId);

                var dto = new TLWorkflowOverviewDTO
                {
                    WorkflowInstanceId = instance.Id,
                    ReturnSubmissionId = instance.ReturnSubmissionId ?? string.Empty,
                    SaccoId = instance.SaccoId,
                    SaccoName = Sacco.SaccoName,
                    PeriodId = instance.PeriodId,
                    PeriodName = Period?.Name ?? string.Empty,
                    SubmittedAt = instance?.CreatedAt ?? DateTime.MinValue,
                    CurrentAssigneeId = Assignee.UserId,
                    CurrentAssigneeName = Assignee.FullName,
                    WorkflowStatus = instance.Status.ToString(),
                    AssignedAt = instance.CreatedAt,
                    CanReassign = true
                };

                // Get previous comments/actions
                var comments = await _db.ApprovalActions
                    .Where(c => c.WorkFlowStepId == instance.CurrentStepId &&
                                c.PeriodId == instance.PeriodId &&
                                c.SaccoId == instance.SaccoId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new WorkflowCommentDTO
                    {
                        Comment = c.Comment,
                        CommentedBy = c.UserId,
                        CommentedAt = c.CreatedAt,
                        Action = c.Status
                    })
                    .ToListAsync();
                dto.PreviousComments = comments;
                overview.Add(dto);
            }
            return overview;
        }


        private async Task<(string userId, string displayName, string email)>  ResolveAssigneeAsync( WorkFlowStep step, string saccoId, string teamId)
        {
            switch (step.AssigneeType)
            {
                case StepAssignee.Officer:
                    {
                     var officer =   await _db.SaccoAssignments
                                    .Where(a => a.SaccoId == saccoId)
                                    .FirstOrDefaultAsync();
                        if (officer != null)
                        {
                            var userDetails = await _complianceService.GetUserDetailsAsync(officer.AssignedUserId);
                            if (userDetails == null)
                                throw new InvalidOperationException("Assigned officer user not found.");
                            return (userDetails.UserId, userDetails.FullName, userDetails.Email);
                        }
                        else
                        {
                            throw new InvalidOperationException("No officer assigned to this SACCO.");
                        }

                    }

                case StepAssignee.TeamLead:
                    {
                        var tl = await _complianceService.GetTeamLeaderAsync(teamId)
                                 ?? throw new InvalidOperationException("No team lead found for this team.");
                        return (tl.UserId, tl.FullName, tl.Email);
                    }

                case StepAssignee.SpecificUser:
                    {
                        if (string.IsNullOrWhiteSpace(step.SpecificUserId))
                            throw new InvalidOperationException("SpecificUserId is required for SpecificUser assignee type.");

                        var user = await _complianceService.GetUserDetailsAsync(step.SpecificUserId)
                                   ?? throw new InvalidOperationException("Specific user not found.");
                        return (user.UserId, user.FullName, user.Email);
                    }

                default:
                    throw new InvalidOperationException("Unknown assignee type.");
            }
        }



    }
}