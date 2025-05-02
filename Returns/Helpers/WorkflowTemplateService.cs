using Microsoft.EntityFrameworkCore;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers.Interfaces.WorkFlow;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class WorkflowTemplateService: IWorkflowTemplateAdminService
    {
        private readonly ReturnsDbContext _context;

        public WorkflowTemplateService(ReturnsDbContext context)
        {
            _context = context;
        }

        public async Task<WorkflowStepDTO> AddStepToWorkflowTemplate(CreateWorkflowStepDTO dto)
        {
            var template = await _context.WorkFlowTemplates.FirstOrDefaultAsync(t => t.Id == dto.WorkTemplateId);

            if (template == null)
                throw new Exception("Workflow template not found");

            if (template.IsPublished)
            {
                throw new Exception("Cannot modify a published template");
            }

            var step = new WorkFlowStep
            {
                Sequence = dto.Sequence,
                RoleId = dto.RoleId,
                WorkFlowTemplateId = dto.WorkTemplateId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.WorkFlowSteps.AddAsync(step);
            await _context.SaveChangesAsync();

            return new WorkflowStepDTO
            {
                StepId = step.Id,
                Sequence = step.Sequence,
                RoleId = step.RoleId,
                RoleName = step.RoleName,
                TemplateId = step.WorkFlowTemplateId,
                CreatedAt = step.CreatedAt
            };
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

        public async Task<WorkflowStepDTO> UpdateStepInWorkflowTemplate(UpdateWorkflowStepDTO dto)
        {
            var template = await _context.WorkFlowTemplates
                .FirstOrDefaultAsync(t => t.Id == dto.WorkTemplateId);

            if (template == null)
                throw new Exception("Workflow template not found");

            if (template.IsPublished)
                throw new Exception("Cannot modify a published template");

            var step = await _context.WorkFlowSteps
                .FirstOrDefaultAsync(s => s.Id == dto.StepId && s.WorkFlowTemplateId == dto.WorkTemplateId);

            if (step == null)
                throw new Exception("Step not found in specified template");

            step.Sequence = dto.Sequence;
            step.RoleId = dto.RoleId;

            await _context.SaveChangesAsync();

            return new WorkflowStepDTO
            {
                StepId = step.Id,
                Sequence = step.Sequence,
                RoleId = step.RoleId,
                RoleName = step.RoleName,
                TemplateId = step.WorkFlowTemplateId,
                CreatedAt = step.CreatedAt
            };
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
