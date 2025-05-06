using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly ReturnsDbContext _db;
        private readonly IComplianceService _complianceService;
        private readonly IEmailService _emailService;

        public WorkflowEngineService(ReturnsDbContext db, IComplianceService complianceService, IEmailService emailService)
        {
            _db = db;
            _complianceService = complianceService;
            _emailService = emailService;
        }

     

        public async Task<List<PendingReturnDto>> GetPendingReturnsAsync(string userId)
        {
            // Get all workflow instances where:
            // 1. The workflow is still active (not completed/rejected)
            // 2. The current step is assigned to this user
            var pendingReturns = await _db.WorkflowInstances
                .Include(w => w.CurrentStep)
                .Include(w => w.Return)
                .Where(w => w.Status == ApprovalStatus.Pending.ToString() && w.UserId == userId) // UserId stores current approver
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
                    CurrentRole = w.CurrentStep.RoleName,
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
                Status = ApprovalStatus.Rejected.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            instance.Status = ApprovalStatus.Rejected.ToString();
            instance.CurrentStepId = null;
            await _db.SaveChangesAsync();

            return ConvertToDto(instance);
        }

        public async Task<WorkflowStateDto> StartWorkflowAsync(Return SubmittedReturn, int Rating)
        {
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
                await _emailService.SendEmailAsync(coUserId.Email, "Return Submitted", $"A new return has been submitted for your review. Return ID: {instance.Return.SaccoName}");
                return ConvertToDto(instance);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<WorkflowStateDto> ApproveStepAsync(ApproveStepRequestDTO request, string userId)
        {
            var instance = await _db.WorkflowInstances
                .Include(i => i.CurrentStep)
                .FirstOrDefaultAsync(i => i.Id == request.WorkFlowInstanceId);

            if (instance == null)
            {
                throw new Exception($"Workflow instance '{request.WorkFlowInstanceId}' not found.");
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
                a.Status == ApprovalStatus.Approved.ToString() 
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
                return ConvertToDto(instance);
            }

            // 4. Record the approval action
            _db.ApprovalActions.Add(new ApprovalAction
            {
                WorkFlowStepId = instance.CurrentStepId,
                ReturnId = instance.ReturnId,
                UserId = userId,
                Status = ApprovalStatus.Approved.ToString(),
                Comment = request.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                // Determine the next step
                var nextStep = await GetNextStepIdAsync(instance);

                if (nextStep == null)
                {
                    // workflow is complete
                    //instance.CurrentStepId = "Approved";
                    instance.Status = ApprovalStatus.Approved.ToString();

                    // update the return status CanReportBeViewed to be true
                    var Return = await _db.Returns.FindAsync(instance.ReturnId);
                    if (Return == null)
                    {
                        throw new Exception("Return not found.");
                    }
                    Return.CanReportBeViewed = true;
                    
                    _db.Returns.Update(Return);
                    _db.WorkflowInstances.Update(instance);
                    await _db.SaveChangesAsync();
                    //await NotifySacco(instance.ReturnId);
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
                    // notify the next approver
                    await _emailService.SendEmailAsync(nextApprover.Email, "New Approval Request", $"You have a new approval request for return ID: {instance.Return.SaccoName}");
                }

                return ConvertToDto(instance);
            }
            catch (Exception ex)
            {
                throw ;
            }
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

        private async Task<WorkFlowStep?> GetNextStepIdAsync(WorkflowInstance instance)
        {
            // 1. Load and order every step in the template
            var allSteps = await _db.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                .OrderBy(s => s.Sequence)
                .ToListAsync();

            if (allSteps.Count == 0)
            {
                throw new Exception("No steps defined in this workflow template");
            }

            // 2. Identify first and last steps
            var firstStep = allSteps[0];
            var lastStep = allSteps[allSteps.Count - 1];

            // 3. Load the step we just finished
            var current = await _db.WorkFlowSteps.FindAsync(instance.CurrentStepId);

            if (current == null){
                throw new Exception("Current step not found");
            }

            // 4. HIGH-RATED RETURNS: if rating ≤ 2, we only ever run the first step
            if (instance.Rating.HasValue && instance.Rating.Value <= 2)
            {
                // 4a. If we just finished the first step → end (publish)
                if (current.Id == firstStep.Id)
                {
                    return null;
                }

                // 4b. If somehow we’re on any other step, treat as “done”
                return null;
            }

            // 5. LOW RATED Returns flow (rating > 2):

            //    Find where “current” sits in the allSteps list
            int position = allSteps.FindIndex(s => s.Id == current.Id);

            if (position < 0)
            {
                throw new Exception("Current step is not part of the active workflow steps");
            }

            // 6. If we’re already at the last step → end (publish)
            if (position == allSteps.Count - 1)
            {
                return null;
            }

            // 7. Otherwise → return the very next step in sequence
            return allSteps[position + 1];
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


    }
}
