using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.ComponentModel;
using System.Reflection;

namespace Returns.Helpers
{
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly ReturnsDbContext _db;
        private readonly IComplianceService _complianceService;
        private readonly IEmailService _emailService;

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
        private string GetDescription(this Enum value)
        {
            var fi = value.GetType()
                          .GetField(value.ToString())!;
            var attr = fi.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }


        public WorkflowEngineService(ReturnsDbContext db, IComplianceService complianceService, IEmailService emailService)
        {
            _db = db;
            _complianceService = complianceService;
            _emailService = emailService;
        }

        public async Task<WorkflowStateDto> RecommendForEndorsementAsync(RecommendStepRequest dto, string userId)
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var inst = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .FirstOrDefaultAsync(w => w.Id == dto.WorkFlowInstanceId)
                ?? throw new Exception("Workflow not found.");

            if (inst.UserId != userId)
            {
                throw new UnauthorizedAccessException("Step not assigned to you.");
            }

            _db.ApprovalActions.Add(new ApprovalAction
            {
                WorkFlowStepId = inst.CurrentStepId,
                ReturnId = inst.ReturnId,
                UserId = userId,
                Comment = dto.Reason,
                Status = ApprovalStatus.RecommendedForEnForcement.ToString(),
                CreatedAt = DateTime.Now
            });

            var next = await GetNextStepIdAsync(inst, bypassRating: true);
            if (next is null)
            {
                inst.CurrentStepId = null;
                inst.Status = ApprovalStatus.RecommendedForEnForcement.ToString();
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return ConvertToDto(inst);
            }

            // 3️⃣ assign to next approver
            var nextApprover = await GetApproverForStep(next, inst.TeamId)
                ?? throw new Exception($"No approver found for next step '{next.Id}'.");

            inst.CurrentStepId = next.Id;
            inst.UserId = nextApprover.UserId;
            inst.Status = ApprovalStatus.RecommendedForEnForcement.ToString();

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            await _emailService.SendEmailAsync(
                nextApprover.Email,
                "Return recommended for endorsement",
                $"A return has been recommended for endorsement and awaits your action.");
            return ConvertToDto(inst);
        }

        public async Task<WorkflowStateDto> ReturnWithReservationsAsync(ReturnWithReservationsRequest req, string userId)
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var inst = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .FirstOrDefaultAsync(w => w.Id == req.WorkFlowInstanceId)
                ?? throw new Exception("Workflow not found.");

            if (inst.UserId != userId)
                throw new UnauthorizedAccessException("User is not assigned to this step.");

            var allSteps = await _db.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == inst.WorkflowTemplateId)
                .OrderBy(s => s.Sequence)
                .ToListAsync();

            var current = allSteps.First(s => s.Id == inst.CurrentStepId);
            if (current.Sequence == 0)
            {
                throw new UnauthorizedAccessException("Compliance Officer cannot take back further.");
            }

            var previous = allSteps[current.Sequence - 1];
            var prevApprover = await GetApproverForStep(previous, inst.TeamId)
                ?? throw new Exception("Previous approver not found.");

            // Log action
            _db.ApprovalActions.Add(new ApprovalAction
            {
                WorkFlowStepId = inst.CurrentStepId,
                ReturnId = inst.ReturnId,
                UserId = userId,
                Comment = req.Comment,
                Status = ApprovalStatus.ReturnedWithReservations.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            // Move workflow pointer back
            inst.CurrentStepId = previous.Id;
            inst.UserId = prevApprover.UserId;
            inst.Status = ApprovalStatus.ReturnedWithReservations.ToString();

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            await _emailService.SendEmailAsync(
                prevApprover.Email,
                "Return sent back with reservations",
                $"A return has been sent back to you for further review.");

            return ConvertToDto(inst);
        }


        public async Task<List<PendingReturnDto>> GetPendingReturnsAsync(string userId)
        {
            var mySaccoIds = (await _complianceService
                    .GetSaccosAssignedToOfficerAsync(userId))
                    .Select(s => s.Id)
                    .ToHashSet();

            // 2. Fetch pending workflow items the user should see:
            //    – they’re the current approver OR
            //    – the Sacco is one they supervise.
            var pendingReturns = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .Include(w => w.Return)
                .Where(w =>
                    w.Status == ApprovalStatus.Pending.ToString() &&
                    (w.UserId == userId || mySaccoIds.Contains(w.Return.SaccoId)))
                .OrderBy(w => w.CreatedAt)
                .Select(w => new PendingReturnDto
                {
                    ReturnId = w.ReturnId,
                    IsConsistent = w.Return.IsNotConsistent,
                    Period = w.Return.Period,
                    SaccoType = w.Return.SaccoType,
                    SaccoName = w.Return.SaccoName,
                    SubmittedDate = w.Return.SubmittedAt,
                    SaccoId = w.Return.SaccoId,
                    WorkFlowInstanceId = w.Id,
                    CurrentRole = w.CurrentStep.RoleName ?? "",
                    CurrentStep = w.CurrentStep.Sequence,
                    Rating = w.Rating
                })
                .ToListAsync();

            return pendingReturns;
        }



        public async Task<WorkflowStateDto> GetCurrentStateAsync(string returnId)
        {
            var instance = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .FirstOrDefaultAsync(w => w.ReturnId == returnId);

            if (instance == null)
                throw new Exception("Workflow not found for this return");

            var nextSteps = await _db.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId && s.Sequence > instance.CurrentStep.Sequence)
                .OrderBy(s => s.Sequence)
                .ToListAsync();

            var nextStepDtos = new List<WorkflowStepDto>();
            foreach (var step in nextSteps)
            {
                var approver = await GetApproverForStep(step, instance.TeamId);
                if (approver == null)
                {
                    throw new Exception($"No approver found for step {step.Id}.");
                }
               nextStepDtos.Add(new WorkflowStepDto
                {
                    StepId = step.Id,
                    ApproverRole = step.RoleName,
                    ApproverUserId = approver.UserId
                });
            }

            return new WorkflowStateDto
            {
                WorkflowId = instance.Id,
                ReturnId = instance.ReturnId,
                CurrentStep = instance.CurrentStep?.RoleName,
                Status = instance.Status,
                Rating = instance.Rating,
                NextSteps = nextStepDtos
            };
        }

        public async Task<List<CommentDetails>> GetComments(string ReturnId)
        {
            List <CommentDetails> commentDetails = new List<CommentDetails>();
            var comments = await _db.ApprovalActions.Where(c => c.ReturnId == ReturnId).ToListAsync();
           if (comments == null)
            {
               return commentDetails;
            }
            foreach (var comment in comments)
            {
                var UserDetails = await  _complianceService.GetUserById(comment.UserId);
                var Name = string.Empty;
                if (UserDetails == null)
                {
                }
                else
                {
                    Name = UserDetails.FullName;
                }
                commentDetails.Add(new CommentDetails
                {
                    Comment = comment.Comment,
                    UserId = comment.UserId,
                    ApproverName = Name,
                    Status = comment.Status,
                    CreatedAt = comment.CreatedAt
                });
            }
            return commentDetails;
        }


        public async Task<WorkflowStateDto> RejectStepAsync(string WorkFlowInstanceId, string userId, RejectStepRequest request)
        {
            var instance = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .FirstOrDefaultAsync(w => w.Id == WorkFlowInstanceId);

            if (instance == null)
                throw new Exception("Workflow not found");

            // Validate user can reject
            var approver = await GetApproverForStep(instance.CurrentStep, instance.TeamId);
            if (approver == null)
            {
                throw new Exception("No approver found for this step.");
            }
            if (userId != approver.UserId)
            {
                throw new UnauthorizedAccessException("User cannot reject this step");
            }

            _db.ApprovalActions.Add(new ApprovalAction
            {
                WorkFlowStepId = instance.CurrentStepId,
                UserId = userId,
                ReturnId = instance.ReturnId,
                Comment = request.Reason,
                Status = ApprovalStatus.PendingEnforcement.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            instance.Status = ApprovalStatus.PendingEnforcement.ToString();
            instance.CurrentStepId = null;
            await _db.SaveChangesAsync();

            return ConvertToDto(instance);
        }

        public async Task<WorkflowStateDto> StartWorkflowAsync(Return SubmittedReturn, int Rating)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var coUserId = await _complianceService.GetAssignedComplianceOfficer(SubmittedReturn.SaccoId);
                var teamId = coUserId.TeamId;
                var template = await _db.WorkFlowTemplates.Include(t => t.WorkFlowSteps).FirstOrDefaultAsync(t => t.IsPublished);
                if (template == null)
                {
                    throw new Exception("No published workflow template found.");
                }

                var instance = new WorkflowInstance
                {
                    ReturnId = SubmittedReturn.Id,
                    WorkflowTemplateId = template.Id,
                    TeamId = teamId,
                    UserId = coUserId.Id,
                    RoleName = coUserId.Role,
                    Rating = Rating,
                    CurrentStepId = template.WorkFlowSteps.OrderBy(s => s.Sequence).First().Id
                };

                await _db.WorkflowInstances.AddAsync(instance);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                try
                {
                    await _emailService.SendEmailAsync(coUserId.Email, "Return Submitted", $"A new return has been submitted for your review. Return ID: {instance.Return.SaccoName}");
                }
                catch
                {
                    // Email failure shouldn't fail the whole operation
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

                var instance = await _db.WorkflowInstances
                    .Include(i => i.CurrentStep)
                    .FirstOrDefaultAsync(i => i.Id == request.WorkFlowInstanceId);

                if (instance == null)
                {
                    throw new Exception($"Workflow instance '{request.WorkFlowInstanceId}' not found.");
                }
                var ReturnToApprove = await _db.Returns.FindAsync(instance.ReturnId);
                if (ReturnToApprove == null)
                {
                    throw new Exception("Return not found.");
                }
                // 2. Ensure the current user is the approver
                if (instance.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User is not assigned to approve this step.");
                }

                // if already approved, just return existing state
                bool alreadyApproved = await _db.ApprovalActions.AnyAsync(a =>
                    a.WorkFlowStepId == instance.CurrentStepId &&
                    a.ReturnId == instance.ReturnId &&
                    a.UserId == userId &&
                    a.Status == ApprovalStatus.RecommendForApproval.ToString()
                    );

                if (alreadyApproved)
                {
                    // update return status CanReportBeViewed to be true
                    var Return = await _db.Returns.FindAsync(instance.ReturnId);
                    if (Return == null)
                    {
                        throw new Exception("Return not found.");
                    }
                    Return.CanReportBeViewed = true;
                    _db.Returns.Update(Return);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return ConvertToDto(instance);
                }

                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    ReturnId = instance.ReturnId,
                    UserId = userId,
                    Status = ApprovalStatus.RecommendForApproval.ToString(),
                    Comment = request.Comment.Trim(),
                    CreatedAt = DateTime.UtcNow
                });

                // Determine the next step
                var nextStep = await GetNextStepIdAsync(instance);

                if (nextStep == null)
                {
                    // workflow is complete
                    instance.Status = ApprovalStatus.RecommendForApproval.ToString();

                    // update the return status CanReportBeViewed to be true
                    
                    ReturnToApprove.CanReportBeViewed = true;

                    _db.Returns.Update(ReturnToApprove);
                    _db.WorkflowInstances.Update(instance);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    try
                    {
                         NotifySacco(instance.ReturnId);
                    }
                    catch
                    {
                        // Notification failure shouldn't fail the operation
                    }
                }
                else
                {
                    // 6b. Advance to the next step
                    var nextApprover = await GetApproverForStep(nextStep, instance.TeamId);
                    if (nextApprover == null)
                    {
                        throw new Exception($"No approver found for next step '{nextStep.Id}'.");
                    }

                    instance.CurrentStepId = nextStep.Id;
                    instance.UserId = nextApprover.UserId;
                    instance.Status = ApprovalStatus.Pending.ToString();

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    try
                    {
                        await _emailService.SendEmailAsync(nextApprover.Email, "New Approval Request", $"You have a new approval request for Sacco: {ReturnToApprove.SaccoName}");
                    }
                    catch
                    {
                        // Email failure shouldn't fail the operation
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

        private void NotifySacco(string SaccoId)
        {
            var SaccoDetails = _complianceService.GetSaccoByIdAsync(SaccoId).Result;
            if (SaccoDetails == null)
            {
                throw new Exception("Sacco not found.");
            }
            var email = SaccoDetails.OfficialSaccoEmail;
            var subject = "Compliance Report";
            var message = $"Your return has been approved";
            _emailService.SendEmailAsync(email, subject, message);
        }


        private async Task<CommonFieldForUser?> GetApproverForStep(WorkFlowStep step, string teamId)
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
                WorkflowId = workflow.Id,
                ReturnId = workflow.ReturnId,
                CurrentStep = workflow.CurrentStep?.RoleName,
                Status = workflow.Status,
                Rating = workflow.Rating
            };
        }

        public async Task<string> GetReturnStatus(string returnId)
        {
            var instance = await  _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .FirstOrDefaultAsync(w => w.ReturnId == returnId);
            if (instance == null)
            {
                throw new Exception("Workflow not found for this return");
            }
            if (!int.TryParse(instance.Status, out var code))
            {
                return instance.Status;
            }

            var statusEnum = (ApprovalStatus)code;
            return GetDescription(statusEnum);
        }
    }
}
