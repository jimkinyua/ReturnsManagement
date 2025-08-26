using Microsoft.EntityFrameworkCore;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers.Interfaces;
using Returns.Helpers.Interfaces.WorkFlow;
using Returns.Models;
using Returns.Models.Data;
using static Returns.DTOs.WorkFlowTemplate.WorkflowStepDTO;

namespace Returns.Helpers
{
    public class WorkflowTemplateService : IWorkflowTemplateAdminService
    {
        private readonly ReturnsDbContext _context;
        private readonly IComplianceService _complianceService;

        public WorkflowTemplateService(ReturnsDbContext context, IComplianceService complianceService)
        {
            _complianceService = complianceService;
            _context = context;
        }

        public async Task<List<WorkflowStepDTO>> AddStepToWorkflowTemplate(List<CreateWorkflowStepDTO> dtos)
        {
            if (dtos is null || dtos.Count == 0)
                throw new ArgumentException("No steps supplied.", nameof(dtos));

            // All steps belong to the same template, so use the first item
            var templateId = dtos[0].WorkTemplateId;
            var template = await _context.WorkFlowTemplates.FindAsync(templateId);
            if (template is null)
                throw new InvalidOperationException("Workflow template not found.");
            if (template.IsPublished)
                throw new InvalidOperationException("Cannot modify a published template.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Delete existing steps for the template
                var existingSteps = await _context.WorkFlowSteps
                    .Where(s => s.WorkFlowTemplateId == templateId)
                    .ToListAsync();
                _context.WorkFlowSteps.RemoveRange(existingSteps);

                // Prepare new steps
                var newSteps = new List<WorkFlowStep>();
                foreach (var dto in dtos)
                {
                    if (dto.WorkTemplateId != templateId)
                        throw new InvalidOperationException("All steps must belong to the same template.");

                    var role = await _complianceService.GetRoleDetails(dto.RoleId);
                    if (role is null)
                    {
                        throw new InvalidOperationException($"Role not assigned to step (RoleId: {dto.RoleId}).");
                    }

                    var step = new WorkFlowStep
                    {
                        Sequence = dto.Sequence,
                        RoleId = dto.RoleId,
                        RoleName = role.NormalizedName,
                        WorkFlowTemplateId = dto.WorkTemplateId,
                        CreatedAt = DateTime.UtcNow,
                        SpecificUserId = dto.SpecificUserId,
                        AssigneeType = dto.AssigneeType 
                    };
                    newSteps.Add(step);
                }

                // Add new steps
                await _context.WorkFlowSteps.AddRangeAsync(newSteps);

                // Save changes
                await _context.SaveChangesAsync();

                // Create results after save to ensure IDs are set
                var results = newSteps.Select(step => new WorkflowStepDTO
                {
                    StepId = step.Id,
                    Sequence = step.Sequence,
                    RoleId = step.RoleId,
                    RoleName = step.RoleName,
                    TemplateId = step.WorkFlowTemplateId,
                    CreatedAt = step.CreatedAt
                }).ToList();

                await transaction.CommitAsync();
                return results;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<WorkflowTemplateDTO> CreateWorkflowTemplate(CreateWorkflowTemplateDTO dto)
        {
            var template = new WorkFlowTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
            };
            await _context.WorkFlowTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            return new WorkflowTemplateDTO
            {
                TemplateId = template.Id,
                Name = template.Name,
                Description = template.Description,
                IsPublished = template.IsPublished,
                CreatedAt = template.CreatedAt,
                Steps = new List<WorkflowStepDTO>()
            };
        }

        public async Task<bool> DeleteWorkflowTemplate(string templateId)
        {
            var template = await _context.WorkFlowTemplates .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                throw new Exception("Workflow template not found");

            if (template.IsPublished)
            {
                throw new Exception("Cannot delete a published template");
            }

            _context.WorkFlowTemplates.Remove(template);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<WorkflowTemplateDTO>> GetAllWorkflowTemplates(bool includeUnpublished = false)
        {
            var query = _context.WorkFlowTemplates.Include(t => t.WorkFlowSteps).AsQueryable();

            if (!includeUnpublished)
            {
                query = query.Where(t => t.IsPublished);
            }

            var templates = await query.ToListAsync();
            if (templates == null || !templates.Any())
            {
                return new List<WorkflowTemplateDTO>();
            }

            var result = new List<WorkflowTemplateDTO>();
            foreach (var template in templates)
            {
                result.Add(await ConvertToTemplateDto(template));
            }

            return result;
        }


        public async Task<WorkflowTemplateDTO> GetWorkflowTemplate(string templateId)
        {
            return await GetWorkflowTemplateInternal(templateId);
        }

        public async Task<bool> PublishWorkflowTemplate(string templateId)
        {
            var template = await _context.WorkFlowTemplates
                .Include(t => t.WorkFlowSteps)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template is null)
                throw new InvalidOperationException("Workflow template not found.");

            if (!template.WorkFlowSteps.Any())
                throw new InvalidOperationException("Cannot publish a template without steps.");

            // is some other template already published?
            var anotherPublished = await _context.WorkFlowTemplates
                .AnyAsync(t => t.IsPublished && t.Id != templateId);

            if (anotherPublished)
                throw new InvalidOperationException("Another workflow template is already published. Unpublish it first.");

            // 3. Publish this one
            template.IsPublished = true;
            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<bool> RemoveStepFromWorkflowTemplate(string templateId, string stepId)
        {
            var template = await _context.WorkFlowTemplates.FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                throw new Exception("Workflow template not found");

            if (template.IsPublished)
                throw new Exception("Cannot modify a published template");

            var step = await _context.WorkFlowSteps
                .FirstOrDefaultAsync(s => s.Id == stepId && s.WorkFlowTemplateId == templateId);

            if (step == null)
                throw new Exception("Step not found in specified template");

            _context.WorkFlowSteps.Remove(step);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UnpublishWorkflowTemplate(string templateId)
        {
            var template = await _context.WorkFlowTemplates.FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
            {
                throw new Exception("Workflow template not found");
            }
            template.IsPublished = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<WorkflowStepDTO>> UpdateStepInWorkflowTemplate(List<UpdateWorkflowStepDTO> dtos)
        {
            if (dtos is null || dtos.Count == 0)
                throw new ArgumentException("No update payload supplied.", nameof(dtos));

            // All updates refer to the same template
            var templateId = dtos[0].WorkTemplateId;
            var template = await _context.WorkFlowTemplates.FindAsync(templateId);
            if (template is null)
                throw new InvalidOperationException("Workflow template not found.");
            if (template.IsPublished)
                throw new InvalidOperationException("Cannot modify a published template.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Delete existing steps for the template
                var existingSteps = await _context.WorkFlowSteps
                    .Where(s => s.WorkFlowTemplateId == templateId)
                    .ToListAsync();
                _context.WorkFlowSteps.RemoveRange(existingSteps);

                // Prepare new steps
                var newSteps = new List<WorkFlowStep>();
                foreach (var dto in dtos)
                {
                    if (dto.WorkTemplateId != templateId)
                        throw new InvalidOperationException("All steps must belong to the same template.");

                    var role = await _complianceService.GetRoleDetails(dto.RoleId);
                    if (role is null)
                    {
                        throw new InvalidOperationException($"Role not assigned to step (RoleId: {dto.RoleId}).");
                    }

                    var step = new WorkFlowStep
                    {
                        Sequence = dto.Sequence,
                        RoleId = dto.RoleId,
                        RoleName = role.NormalizedName,
                        WorkFlowTemplateId = dto.WorkTemplateId,
                        CreatedAt = DateTime.UtcNow,
                        SpecificUserId = dto.SpecificUserId,
                        AssigneeType = dto.AssigneeType,
                    };
                    newSteps.Add(step);
                }

                // Add new steps
                await _context.WorkFlowSteps.AddRangeAsync(newSteps);

                // Save changes
                await _context.SaveChangesAsync();

                // Create results after save to ensure IDs are set
                var results = newSteps.Select(step => new WorkflowStepDTO
                {
                    StepId = step.Id,
                    Sequence = step.Sequence,
                    RoleId = step.RoleId,
                    RoleName = step.RoleName,
                    TemplateId = step.WorkFlowTemplateId,
                    CreatedAt = step.CreatedAt
                }).ToList();

                await transaction.CommitAsync();
                return results;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WorkflowTemplateDTO> UpdateWorkflowTemplate(UpdateWorkflowTemplateDTO dto)
        {
            var template = await _context.WorkFlowTemplates.FirstOrDefaultAsync(t => t.Id == dto.TemplateId);

            if (template == null)
            {
                throw new Exception("Workflow template not found");
            }

            template.Name = dto.Name;
            template.Description = dto.Description;
            await _context.SaveChangesAsync();

            return await GetWorkflowTemplateInternal(template.Id);
        }


        private async Task<WorkflowTemplateDTO> GetWorkflowTemplateInternal(string templateId)
        {
            var template = await _context.WorkFlowTemplates
                .Include(t => t.WorkFlowSteps)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
            {
                throw new Exception("Workflow template not found");
            }

            return await ConvertToTemplateDto(template);
        }

        private async Task<WorkflowTemplateDTO> ConvertToTemplateDto(WorkFlowTemplate template)
        {
            var steps = new List<WorkflowStepDTO>();

            foreach (var s in template.WorkFlowSteps.OrderBy(x => x.Sequence))
            {
                var RoleDetails = await _complianceService.GetRoleDetails(s.RoleId);
                if (RoleDetails != null)
                {
                    s.RoleName = RoleDetails.NormalizedName;
                }
                else
                {
                    s.RoleName = "Role not found";
                }
                var dto = new WorkflowStepDTO
                {
                    StepId = s.Id,
                    Sequence = s.Sequence,
                    RoleId = s.RoleId,
                    RoleName = s.RoleName,
                    TemplateId = s.WorkFlowTemplateId,

                    AssigneeType = s.AssigneeType.ToString(),
                    SpecificUserId = s.SpecificUserId
                };

                switch (s.AssigneeType)
                {
                    case StepAssignee.SpecificUser:
                        if (string.IsNullOrWhiteSpace(s.SpecificUserId))
                        {
                            dto.AssigneeResolved = false;
                            dto.AssigneeMessage = "Specific user not set for this step.";
                        }
                        else
                        {
                            var user = await _complianceService.GetUserDetailsAsync(s.SpecificUserId);
                            if (user == null)
                            {
                                dto.AssigneeResolved = false;
                                dto.AssigneeMessage = "Selected user not found.";
                            }
                            else
                            {
                                dto.AssigneeResolved = true;
                                dto.Assignee = new AssigneeDTO
                                {
                                    UserId = user.UserId,
                                    FullName = user.FullName,
                                    Email = user.Email,
                                    RoleName = user.RoleName
                                };
                            }
                        }
                        break;

                    case StepAssignee.Officer:
                        dto.AssigneeResolved = false;
                        dto.AssigneeMessage = "System-determined: Compliance Officer assigned to the SACCO.";
                        break;

                    case StepAssignee.TeamLead:
                        dto.AssigneeResolved = false;
                        dto.AssigneeMessage = "System-determined: Team Lead for the SACCO’s team.";
                        break;

                    default:
                        dto.AssigneeResolved = false;
                        dto.AssigneeMessage = "Unknown assignee type.";
                        break;
                }

                steps.Add(dto);
            }

            return new WorkflowTemplateDTO
            {
                TemplateId = template.Id,
                Name = template.Name,
                Description = template.Description,
                IsPublished = template.IsPublished,
                CreatedAt = template.CreatedAt,
                Steps = steps
            };
        }


        public async Task<StepAssigneeDTO> StepAssigneeDetailsAsync(string stepId)
        {
            var step = await _context.WorkFlowSteps
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stepId)
                ?? throw new InvalidOperationException("Workflow step not found.");

            var result = new StepAssigneeDTO
            {
                StepId = step.Id,
                AssigneeType = step.AssigneeType
            };

            if (!string.IsNullOrWhiteSpace(step.SpecificUserId))
            {
                var user = await _complianceService.GetUserDetailsAsync(step.SpecificUserId);
                if (user != null)
                {
                    result.IsResolved = true;
                    result.Assignees.Add(new AssigneeDTO
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        RoleName = user.RoleName ?? string.Empty
                    });
                    return result;
                }

                result.IsResolved = false;
                result.Message = "Specified user not found.";
                return result;
            }

            result.IsResolved = false;
            result.Message = step.AssigneeType switch
            {
                StepAssignee.Officer => "System-determined: Compliance Officer will be resolved at runtime.",
                StepAssignee.TeamLead => "System-determined: Team Lead will be resolved at runtime.",
                StepAssignee.SpecificUser => "Specific user not set for this step.",
                _ => "Unknown assignee type."
            };
            return result;
        }



    }
}
