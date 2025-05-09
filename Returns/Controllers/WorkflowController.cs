using Azure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers;
using static Returns.Helpers.TokenHelper;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowEngineService _workflowService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(IWorkflowEngineService workflowService,ILogger<WorkflowController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        [HttpGet("ApprovalRequests")]
        public async Task<ActionResult<List<PendingReturnDto>>> ApprovalRequests()
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401);
                }

                var results = await _workflowService.GetPendingReturnsAsync(loggedInSacco.UserId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch pending returns");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("ApprovalRequestComments/{ReturnId}")]
        public async Task<ActionResult<List<CommentDetails>>> ApprovalRequestComments(string ReturnId)
        {
            try
            {
                var results = await _workflowService.GetComments(ReturnId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch pending returns");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ApproveRequest")]
        public async Task<IActionResult> ApproveRequest([FromBody] ApproveStepRequestDTO approveStepRequestDTO)
        {
            /*LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
            {
                return StatusCode(401);
            }*/

            try
            {
                //var result = await _workflowService.ApproveStepAsync(approveStepRequestDTO.WorkFlowInstanceId, loggedInSacco.UserId);
                var result = await _workflowService.ApproveStepAsync(approveStepRequestDTO, "83c53482-724a-428c-b31a-110bde88fe18");
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Approval failed for workflow {approveStepRequestDTO.WorkFlowInstanceId}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RejectRequest")]
        public async Task<IActionResult> RejectRequest([FromBody] RejectStepRequest rejectStepRequestDTO)
        {
            LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
            {
                return StatusCode(401);
            }
            try
            {
                var result = await _workflowService.RejectStepAsync(rejectStepRequestDTO.WorkFlowInstanceId, loggedInSacco.UserId, rejectStepRequestDTO);
                //var result = await _workflowService.RejectStepAsync(rejectStepRequestDTO.WorkFlowInstanceId, "7ded1b0a-bca9-491e-8880-3743d4b3cae5", rejectStepRequestDTO);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rejection failed for workflow {rejectStepRequestDTO.WorkFlowInstanceId}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("CurrentState/{returnId}")]
        public async Task<IActionResult> GetCurrentState(string returnId)
        {
            /*LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
            {
                return StatusCode(401);
            }*/
            try
            {
                var result = await _workflowService.GetCurrentStateAsync(returnId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch current state for return {returnId}");
                return BadRequest(ex.Message);
            }
        }



    }
}
