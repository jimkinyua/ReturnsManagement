using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers;
using Returns.Helpers.Interfaces.WorkFlow;

namespace Returns.Controllers
{
    [Route("api/returns/[controller]")]
    [ApiController]
    public class WorkflowTemplateController : ControllerBase
    {
        private readonly IWorkflowTemplateAdminService _adminService;
        private readonly ILogger<WorkflowTemplateController> _logger;

        public WorkflowTemplateController(IWorkflowTemplateAdminService workflowTemplateAdminService, ILogger<WorkflowTemplateController> logger)
        {
            this._adminService = workflowTemplateAdminService;
            _logger = logger;
        }

        [HttpPost("CreateTemplate")]
        public async Task<ActionResult<WorkflowTemplateDTO>> CreateTemplate([FromBody] CreateWorkflowTemplateDTO dto)
        {
            try
            {
                var result = await _adminService.CreateWorkflowTemplate(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow template");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetTemplateDetails/{templateId}")]
        public async Task<ActionResult<WorkflowTemplateDTO>> GetTemplateDetails(string templateId)
        {
            try
            {
                var result = await _adminService.GetWorkflowTemplate(templateId);
                if (result == null)
                {
                    return NotFound("Workflow template not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching workflow template details");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("PreviewStepAssignee/{stepId}")]
        public async Task<ActionResult<StepAssigneeDTO>> PreviewStepAssignee(string stepId, [FromQuery] string? saccoId = null)
        {
            try
            {
                var preview = await _adminService.StepAssigneeDetailsAsync(stepId);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing step assignee");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("GetWorkFlowTemplates")]
        public async Task<ActionResult<List<WorkflowTemplateDTO>>> GetWorkFlowTemplates()
        {
            bool includeUnpublished = true;
            try
            {
                var result = await _adminService.GetAllWorkflowTemplates(includeUnpublished);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching workflow templates");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateTemplate")]
        public async Task<ActionResult<WorkflowTemplateDTO>> UpdateTemplate([FromBody] UpdateWorkflowTemplateDTO dto)
        {
            try
            {
                var result = await _adminService.UpdateWorkflowTemplate(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow template");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("PublishTemplate/{templateId}")]
        public async Task<ActionResult<bool>> PublishTemplate(string templateId)
        {
            try
            {
                var result = await _adminService.PublishWorkflowTemplate(templateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing workflow template");
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("UnpublishTemplate/{templateId}")]
        public async Task<ActionResult<bool>> UnpublishTemplate(string templateId)
        {
            try
            {
                var result = await _adminService.UnpublishWorkflowTemplate(templateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing workflow template");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("DeleteTemplate/{templateId}")]
        public async Task<ActionResult<bool>> DeleteTemplate(string templateId)
        {
            try
            {
                var result = await _adminService.DeleteWorkflowTemplate(templateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow template");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("AddStepToTemplate")]
        public async Task<ActionResult<List<WorkflowStepDTO>>> AddStepToTemplate([FromBody] List<CreateWorkflowStepDTO> dto)
        {
            try
            {
                var result = await _adminService.AddStepToWorkflowTemplate(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding step to workflow template");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateStepInTemplate")]
        public async Task<ActionResult<WorkflowStepDTO>> UpdateStepInTemplate([FromBody] List<UpdateWorkflowStepDTO> dto)
        {
            try
            {
                var result = await _adminService.UpdateStepInWorkflowTemplate(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating step in workflow template");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("RemoveStepFromTemplate/{templateId}/{stepId}")]

        public async Task<ActionResult<bool>> RemoveStepFromTemplate(string templateId, string stepId)
        {
            try
            {
                var result = await _adminService.RemoveStepFromWorkflowTemplate(templateId, stepId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing step from workflow template");
                return StatusCode(500, ex.Message);
            }
        }   



    }
}
