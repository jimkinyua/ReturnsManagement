using Microsoft.EntityFrameworkCore;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers.Interfaces;
using Returns.Helpers.Interfaces.WorkFlow;
using Returns.Models;
using Returns.Models.Data;

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

        public async Task<List<WorkflowStepDTO>> AddStepToWorkflowTemplate( List<CreateWorkflowStepDTO> dtos)
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

            var results = new List<WorkflowStepDTO>();

            foreach (var dto in dtos)
            {
                var role = await _complianceService.GetRoleDetails(dto.RoleId);
                if (role is null)
                {
                    throw new InvalidOperationException($"Role not assigned to step (RoleId: {dto.RoleId}).");
                }

                var step = new WorkFlowStep
                {
                    Sequence = dto.Sequence,
                    RoleId = dto.RoleId,
                    RoleName = role.RoleName,
                    WorkFlowTemplateId = dto.WorkTemplateId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.WorkFlowSteps.AddAsync(step);

                results.Add(new WorkflowStepDTO
                {
                    StepId = step.Id,
                    Sequence = step.Sequence,
                    RoleId = step.RoleId,
                    RoleName = step.RoleName,
                    TemplateId = step.WorkFlowTemplateId,
                    CreatedAt = step.CreatedAt
                });
            }

            await _context.SaveChangesAsync();
            return results;
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
            var query = _context.WorkFlowTemplates .Include(t => t.WorkFlowSteps) .AsQueryable();

            if (!includeUnpublished)
            {
                query = query.Where(t => t.IsPublished);
            }

            var templates = await query.ToListAsync();
            if (templates == null || !templates.Any())
            {
                return new List<WorkflowTemplateDTO>();
            }
            return templates.Select(t => ConvertToTemplateDto(t)).ToList();
        }


        public async Task<WorkflowTemplateDTO> GetWorkflowTemplate(string templateId)
        {
            return await GetWorkflowTemplateInternal(templateId);
        }

        public async Task<bool> PublishWorkflowTemplate(string templateId)
        {
            var template = await _context.WorkFlowTemplates.Include(t => t.WorkFlowSteps).FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
            {
                throw new Exception("Workflow template not found");
            }

            if (!template.WorkFlowSteps.Any())
            {
                throw new Exception("Cannot publish template without steps");
            }

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

        public async Task<List<WorkflowStepDTO>> UpdateStepInWorkflowTemplate( List<UpdateWorkflowStepDTO> dtos)
        {
            if (dtos is null || dtos.Count == 0)
            {
                throw new ArgumentException("No update payload supplied.", nameof(dtos));
            }

            // All updates refer to the same template
            var templateId = dtos[0].WorkTemplateId;

            var template = await _context.WorkFlowTemplates.FindAsync(templateId);
            if (template is null)
                throw new InvalidOperationException("Workflow template not found.");
            if (template.IsPublished)
                throw new InvalidOperationException("Cannot modify a published template.");

            var stepDict = await _context.WorkFlowSteps
                .Where(s => s.WorkFlowTemplateId == templateId)
                .ToDictionaryAsync(s => s.Id);

            var results = new List<WorkflowStepDTO>();

            foreach (var dto in dtos)
            {
                if (!stepDict.TryGetValue(dto.StepId, out var step))
                {
                    throw new InvalidOperationException($"Step {dto.StepId} not found in template {templateId}.");
                }

                var role = await _complianceService.GetRoleDetails(dto.RoleId);
                if (role is null)
                {
                    throw new InvalidOperationException($"Role not assigned to step (RoleId: {dto.RoleId}).");
                }

                step.Sequence = dto.Sequence;
                step.RoleId = dto.RoleId;
                step.RoleName = role.RoleName;

                results.Add(new WorkflowStepDTO
                {
                    StepId = step.Id,
                    Sequence = step.Sequence,
                    RoleId = step.RoleId,
                    RoleName = step.RoleName,
                    TemplateId = step.WorkFlowTemplateId,
                    CreatedAt = step.CreatedAt
                });
            }

            await _context.SaveChangesAsync();
            return results;
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

            return ConvertToTemplateDto(template);
        }

        private WorkflowTemplateDTO ConvertToTemplateDto(WorkFlowTemplate template)
        {
            return new WorkflowTemplateDTO
            {
                TemplateId = template.Id,
                Name = template.Name,
                Description = template.Description,
                IsPublished = template.IsPublished,
                CreatedAt = template.CreatedAt,
                Steps = template.WorkFlowSteps.Select(s => new WorkflowStepDTO
                {
                    StepId = s.Id,
                    Sequence = s.Sequence,
                    RoleId = s.RoleId,
                    RoleName = s.RoleName,
                    TemplateId = s.WorkFlowTemplateId,
                }).OrderBy(s => s.Sequence).ToList()
            };
        }

      
    }
}
