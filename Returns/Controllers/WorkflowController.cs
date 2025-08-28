using Azure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Helpers;
using static Returns.Helpers.TokenHelper;

namespace Returns.Controllers
{
    [Route("api/returns/[controller]")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowEngineService _workflowService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(IWorkflowEngineService workflowService, ILogger<WorkflowController> logger)
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
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var results = await _workflowService.GetPendingReturnsAsync(loggedInSacco.UserId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch pending returns.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("ApprovalRequestComments")]
        public async Task<ActionResult<List<CommentDetails>>> ApprovalRequestComments(
            [FromQuery] string? periodId,
            [FromQuery] string? saccoId,
            [FromQuery] string? returnSubmissionId)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var results = await _workflowService.GetComments(periodId, saccoId, returnSubmissionId);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid parameters for fetching comments.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch comments for period {PeriodId}, SACCO {SaccoId}, or return {ReturnSubmissionId}.", periodId, saccoId, returnSubmissionId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpPost("ApproveRequest")]
        public async Task<IActionResult> ApproveRequest([FromBody] ApproveStepRequestDTO approveStepRequestDTO)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var result = await _workflowService.ApproveStepAsync(approveStepRequestDTO, loggedInSacco.UserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized approval attempt for workflow {WorkflowId}.", approveStepRequestDTO.WorkFlowInstanceId);
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for approving workflow {WorkflowId}.", approveStepRequestDTO.WorkFlowInstanceId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to approve workflow {WorkflowId}.", approveStepRequestDTO.WorkFlowInstanceId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpPost("RecommendForEnforcement")]
        public async Task<IActionResult> RecommendForEnforcement([FromBody] RecommendStepRequest recommendStepRequestDTO)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var bearer = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(bearer) || !bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing bearer token.");
                }

                var accessToken = bearer["Bearer ".Length..].Trim();
                var result = await _workflowService.RecommendForEnforcementAsync(recommendStepRequestDTO, loggedInSacco.UserId, accessToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized enforcement recommendation for workflow {WorkflowId}.", recommendStepRequestDTO.WorkFlowInstanceId);
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for recommending enforcement for workflow {WorkflowId}.", recommendStepRequestDTO.WorkFlowInstanceId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recommend enforcement for workflow {WorkflowId}.", recommendStepRequestDTO.WorkFlowInstanceId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpPost("RejectWithReservations")]
        public async Task<IActionResult> RejectWithReservations([FromBody] ReturnWithReservationsRequest rejectStepRequestDTO)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var result = await _workflowService.ReturnWithReservationsAsync(rejectStepRequestDTO, loggedInSacco.UserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized rejection attempt for workflow {WorkflowId}.", rejectStepRequestDTO.WorkFlowInstanceId);
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for rejecting workflow {WorkflowId}.", rejectStepRequestDTO.WorkFlowInstanceId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reject workflow {WorkflowId}.", rejectStepRequestDTO.WorkFlowInstanceId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }




        [HttpGet("CurrentState")]
        public async Task<IActionResult> GetCurrentState(
            [FromQuery] string? periodId,
            [FromQuery] string? saccoId,
            [FromQuery] string? returnSubmissionId)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var result = await _workflowService.GetCurrentStateAsync(periodId, saccoId, returnSubmissionId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid parameters for fetching current state.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Workflow not found for period {PeriodId}, SACCO {SaccoId}, or return {ReturnSubmissionId}.", periodId, saccoId, returnSubmissionId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch current state for period {PeriodId}, SACCO {SaccoId}, or return {ReturnSubmissionId}.", periodId, saccoId, returnSubmissionId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("RatedSaccosForInspection")]
        public async Task<ActionResult<List<RatedSaccoForInspectionDto>>> GetRatedSaccosForInspection()
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var results = await _workflowService.GetRatedSaccosForInspectionAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch rated saccos for inspection.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpPost("RecommendForInspection")]
        public async Task<ActionResult<InspectionRecommendationResult>> RecommendForInspection([FromBody] RecommendForInspectionRequest request)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.UserId))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing user authentication.");
                }

                var bearer = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(bearer) || !bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(401, "Unauthorized: Invalid or missing bearer token.");
                }

                var accessToken = bearer["Bearer ".Length..].Trim();
                var result = await _workflowService.RecommendForInspectionAsync(request, loggedInSacco.UserId, accessToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized inspection recommendation for workflow {WorkflowId}.", request.WorkFlowInstanceId);
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for recommending inspection for workflow {WorkflowId}.", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recommend inspection for workflow {WorkflowId}.", request.WorkFlowInstanceId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

      
    }
}