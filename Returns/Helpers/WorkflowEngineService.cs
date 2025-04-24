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

        public WorkflowEngineService(ReturnsDbContext db, IComplianceService complianceService
        {
            _db = db;
            _complianceService = complianceService;
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
                    SaccoId = w.Return.SaccoId,
                    WorkFlowInstanceId = w.Id,
                    CurrentRole = w.CurrentStep.RoleName,
                    CurrentStep = w.CurrentStep.Sequence,
                    SubmittedDate = w.Return.CreatedAt,
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
                .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                .OrderBy(s => s.Sequence)
                .ToListAsync();

            var nextStepDtos = new List<WorkflowStepDto>();
            foreach (var step in nextSteps)
            {
                nextStepDtos.Add(new WorkflowStepDto
                {
                    StepId = step.Id,
                    RoleName = step.RoleName,
                    AssignedUserId = await GetApproverForStep(step, instance.TeamId),
                    DueDate = DateTime.Now.AddDays(2)
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
            if (userId != approver)
            {
                throw new UnauthorizedAccessException("User cannot reject this step");
            }

            _db.ApprovalActions.Add(new ApprovalAction
            {
                WorkFlowStepId = instance.CurrentStepId,
                UserId = userId,
                ReturnId = instance.ReturnId,
                Comment = request.Reason,
                Status = "Rejected",
                CreatedAt = DateTime.UtcNow
            });

            instance.Status = "Rejected";
            instance.CurrentStepId = null;
            await _db.SaveChangesAsync();

            return ConvertToDto(instance);
        }

        public async Task<WorkflowStateDto> StartWorkflowAsync(Return SubmittedReturn, int Rating)
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
                CurrentStepId = template.WorkFlowSteps.OrderBy(s => s.Sequence).First().Id
            };

            await _db.WorkflowInstances.AddAsync(instance);
            await _db.SaveChangesAsync();
            return ConvertToDto(instance);
        }

        public async Task ApproveStepAsync(string workflowInstanceId, string userId)
        {
            var instance = await _db.WorkflowInstances
                .Include(i => i.CurrentStep)
                .FirstOrDefaultAsync(i => i.Id == workflowInstanceId);

            // 1. Validate user can approve this step
            var approver = await GetApproverForStep(instance.CurrentStep, instance.TeamId);
            if (approver != userId) throw new UnauthorizedAccessException();

            // 2. Log approval
            _db.ApprovalActions.Add(new ApprovalAction
            {
                WorkFlowStepId = instance.CurrentStepId,
                UserId = userId,
                ReturnId = instance.ReturnId,
                Status = "Approved"
            });

            var nextStep = await GetNextStepIdAsync(instance);
            if (nextStep != null) {
                // 3. Move to next step
                instance.CurrentStepId = nextStep.Id;
                instance.UserId = await GetApproverForStep(nextStep, instance.TeamId);
            }
            else
            {
                // 4. Mark workflow as completed
                instance.CurrentStepId = null;
                instance.Status = "Completed";
                // 5. Trigger enforcement if needed
                if (instance.Rating >= 4)
                {
                    //await _enforcementService.TriggerForReturn(instance.ReturnId);
                }

                // nOTIFY Sacco TODO
            }

            _db.WorkflowInstances.Update(instance);
            await _db.SaveChangesAsync();
        }

        private async Task<string> GetApproverForStep(WorkFlowStep step, string teamId)
        {
            var stepdetails = await _db.WorkFlowSteps
                .Include(s => s.ApprovalActions)
                .FirstOrDefaultAsync(s => s.Id == step.Id);
            string approverId = null;
            // if next step is not found, throw exception
            if (stepdetails == null)
            {
                throw new Exception("Step not found.");
            }

            var RoleAssignedThisStep = stepdetails.RoleName;

            if (RoleAssignedThisStep == null)
            {
                throw new Exception("Role not assigned to this step.");
            }

            if (RoleAssignedThisStep == "TeamLead")
            {
                // get team lead for the team 
                var teamLead = await _complianceService.GetTeamLead(teamId);

                if (teamLead == null)
                {
                    throw new Exception("No Team Lead found for this team.");
                }

                approverId = teamLead.Id;
            }

            // next just get user with that role
            var approver = await _complianceService.GetUserByRole(RoleAssignedThisStep);
            if (approver == null)
            {
                throw new Exception("No approver found for this step.");
            }

            return approver.Id;

        }

        private async Task<WorkFlowStep?> GetNextStepIdAsync(WorkflowInstance instance)
        {
            var currentStep = await _db.WorkFlowSteps.FindAsync(instance.CurrentStepId);
            if (currentStep == null) throw new Exception("Current step not found");

            var templateSteps = await _db.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId)
                .OrderBy(s => s.Sequence)
                .ToListAsync();

            // Rating-based routing (simple if conditions)
            if (currentStep.RoleName == "ComplianceOfficer")
            {
                // CO always goes to TeamLead next
                return templateSteps.FirstOrDefault(s => s.RoleName == "TeamLead");
            }
            else if (currentStep.RoleName == "TeamLead")
            {
                if (instance.Rating <= 2)
                {
                    return null; // Auto-publish for ratings 1-2
                }
                else if (instance.Rating == 3)
                {
                    // Rating 3 goes to Director
                    return templateSteps.FirstOrDefault(s => s.RoleName == "Director");
                }
                else // Rating 4-5
                {
                    // Rating 4-5 goes to Director
                    return templateSteps.FirstOrDefault(s => s.RoleName == "Director");
                }
            }
            else if (currentStep.RoleName == "Director")
            {
                if (instance.Rating == 3)
                {
                    // For rating 3, loop back to TeamLead for publishing
                    return templateSteps.FirstOrDefault(s => s.RoleName == "TeamLead");
                }
                else if (instance.Rating >= 4)
                {
                    // For ratings 4-5, go to CEO
                    return templateSteps.FirstOrDefault(s => s.RoleName == "CEO");
                }
            }

            // Default: next sequential step
            return templateSteps.FirstOrDefault(s => s.Sequence > currentStep.Sequence);
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
