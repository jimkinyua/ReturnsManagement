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

        public WorkflowEngineService(ReturnsDbContext db, IComplianceService complianceService)
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

        public async Task<WorkflowStateDto> ApproveStepAsync(ApproveStepRequestDTO approveStepRequestDTO, string userId)
        {
            try
            {
                var instance = await _db.WorkflowInstances
              .Include(i => i.CurrentStep)
              .FirstOrDefaultAsync(i => i.Id == approveStepRequestDTO.WorkFlowInstanceId);
                if (instance == null)
                {
                    throw new Exception("Workflow instance not found");
                }
                // 1. Validate user can approve this step
                var approver = instance.UserId; //await GetApproverForStep(instance.CurrentStep, instance.TeamId);
                if (approver != userId)
                {
                    throw new UnauthorizedAccessException();
                }
                // 2. Log approval
                _db.ApprovalActions.Add(new ApprovalAction
                {
                    WorkFlowStepId = instance.CurrentStepId,
                    UserId = userId,
                    ReturnId = instance.ReturnId,
                    Status = "Approved",
                    Comment = approveStepRequestDTO.Comment
                });

                var nextStep = await GetNextStepIdAsync(instance);
                if (nextStep == null)
                {
                    throw new Exception("Next step not found");
                }
                var nextApprover = await GetApproverForStep(nextStep, instance.TeamId);
                if (nextStep != null)
                {
                    // 3. Move to next step
                    instance.CurrentStepId = nextStep.Id;
                    instance.UserId = nextApprover.UserId;
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
                return ConvertToDto(instance);
            }
            catch (Exception ex)
            {

                throw new Exception("",ex);
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
            var currentStep = await _db.WorkFlowSteps.FindAsync(instance.CurrentStepId);
            if (currentStep == null)
            {
                throw new Exception("Current step not found");
            }

            // Fix for CS1061: Ensure GetRoleDetails is awaited and its result is used correctly.  
            var roleDetails = await _complianceService.GetRoleDetails(currentStep.RoleId);
            if (roleDetails == null)
            {
                throw new Exception("Role details not found for the current step.");
            }
            var RoleOfCurrentStep = roleDetails.RoleName.Trim().ToUpper();

            // Get all steps in the workflow template that are greater to this  
            var nextSteps = await _db.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == instance.WorkflowTemplateId && s.Sequence > currentStep.Sequence)
                .OrderBy(s => s.Sequence)
                .ToListAsync();


            if (currentStep.Sequence == 1)
            {
                // CO always goes to TeamLead next so pick the next step  
                return nextSteps.FirstOrDefault(s => s.Sequence == nextSteps.Min(ns => ns.Sequence));
            }
            else if (currentStep.Sequence == 2)
            {
                if (instance.Rating <= 2)
                {
                    return null; // Auto-publish for ratings 1-2  
                }
                else if (instance.Rating == 3)
                {
                    // Rating 3 goes to Director  
                    return nextSteps.FirstOrDefault(s => s.Sequence == nextSteps.Min(ns => ns.Sequence));
                }
                else // Rating 4-5  
                {
                    // Rating 4-5 goes to Director  
                    return nextSteps.FirstOrDefault(s => s.Sequence == nextSteps.Min(ns => ns.Sequence));
                }
            }
            else if (currentStep.Sequence ==3) 
            {
                if (instance.Rating == 3)
                {
                    // For rating 3, loop back to TeamLead for publishing  
                    return nextSteps.FirstOrDefault(s => s.Sequence == nextSteps.Min(ns => ns.Sequence));
                }
                else if (instance.Rating >= 4)
                {
                    // For ratings 4-5, go to CEO  
                    return nextSteps.FirstOrDefault(s => s.Sequence == nextSteps.Min(ns => ns.Sequence));
                }
            }




            // Default: next sequential step  
            return nextSteps.FirstOrDefault(s => s.Sequence > currentStep.Sequence);


            // Rating-based routing (simple if conditions)  
            /*     if (currentStep.ApproverRole == "OFFICER")
                 {
                     // CO always goes to TeamLead next  
                     var TeamLeadRoleId = await _complianceService.GetRoleIdByName("TeamLead");

                     return nextSteps.FirstOrDefault(s => s.RoleId == TeamLeadRoleId);
                 }
                 else if (currentStep.ApproverRole == "Team Lead")
                 {
                     if (instance.Rating <= 2)
                     {
                         return null; // Auto-publish for ratings 1-2  
                     }
                     else if (instance.Rating == 3)
                     {
                         // Rating 3 goes to Director  
                         return nextSteps.FirstOrDefault(s => s.RoleId == roleDetails.RoleId);
                     }
                     else // Rating 4-5  
                     {
                         // Rating 4-5 goes to Director  
                         return nextSteps.FirstOrDefault(s => s.RoleId == roleDetails.RoleId);
                     }
                 }
                 else if (currentStep.ApproverRole == "Director")
                 {
                     if (instance.Rating == 3)
                     {
                         // For rating 3, loop back to TeamLead for publishing  
                         return nextSteps.FirstOrDefault(s => s.RoleId == roleDetails.RoleId);
                     }
                     else if (instance.Rating >= 4)
                     {
                         // For ratings 4-5, go to CEO  
                         return nextSteps.FirstOrDefault(s => s.RoleId == roleDetails.RoleId);
                     }
                 }*/


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
