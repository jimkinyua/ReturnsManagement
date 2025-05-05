using Returns.DTOs.WorkFlowTemplate;
using Returns.Models;

namespace Returns.Helpers.Interfaces.WorkFlow
{
    public interface IWorkflowTemplateAdminService
    {
        Task<WorkflowTemplateDTO> CreateWorkflowTemplate(CreateWorkflowTemplateDTO dto);
        Task<WorkflowTemplateDTO> UpdateWorkflowTemplate(UpdateWorkflowTemplateDTO dto);
        Task<bool> DeleteWorkflowTemplate(string templateId);
        Task<bool> PublishWorkflowTemplate(string templateId);
        Task<bool> UnpublishWorkflowTemplate(string templateId);
        Task<List<WorkflowStepDTO>> AddStepToWorkflowTemplate(List<CreateWorkflowStepDTO> dto);
        Task<bool> RemoveStepFromWorkflowTemplate(string templateId, string stepId);
        Task<List<WorkflowStepDTO>> UpdateStepInWorkflowTemplate(List<UpdateWorkflowStepDTO> dto);
        Task<WorkflowTemplateDTO> GetWorkflowTemplate(string templateId);
        Task<List<WorkflowTemplateDTO>> GetAllWorkflowTemplates(bool includeUnpublished = false);

    }
}