using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.Returns.Adhoc;
using Returns.Helpers;
using Returns.Helpers.Interfaces;
using static Returns.Helpers.TokenHelper;

namespace Returns.Controllers
{
    [Route("api/returns/[controller]")]
    [ApiController]
    public class AdHocReturnsController : ControllerBase
    {
        private readonly IAdhocReturnsService _adhocReturnsService;
        private readonly ILogger<AdHocReturnsController> _logger;

        public AdHocReturnsController(IAdhocReturnsService adhocReturnsService,ILogger<AdHocReturnsController> logger)
        {
            _adhocReturnsService = adhocReturnsService;
            _logger = logger;
        }

    
        [HttpPost("admin/RequestAdHocReturn")]
        public async Task<IActionResult> CreateAdHocReturnRequestAsync([FromForm] AdHocReturnRequestDTO dto)
        {
            try
            {
                LoggedInEntity admin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (admin == null)
                {
                    return Unauthorized();
                }

                var adHocRequest = await _adhocReturnsService.CreateAdHocReturnRequestAsync(dto, admin);
                return Ok(adHocRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for ad hoc return request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized( ex.Message );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex) );
            }
        }


        [HttpPost("sacco/SaccoRespondToAdHocReturn")]
        public async Task<IActionResult> RespondToAdHocReturnAsync([FromForm] AdHocReturnResponseDTO dto)
        {
            try
            {
                LoggedInEntity loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null || string.IsNullOrEmpty(loggedInEntity.SaccoId))
                {
                    return Unauthorized("Invalid SACCO credentials.");
                }

                var adHocRequest = await _adhocReturnsService.RespondToAdHocReturnRequestAsync(dto, loggedInEntity);
                return Ok(new { message = "Ad hoc return response submitted successfully.", requestId = adHocRequest.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("AdHocReturnRequestDetails/{requestId}")]
        public async Task<IActionResult> GetAdHocReturnRequestDetailsAsync(string requestId)
        {
            try
            {
                LoggedInEntity loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null)
                {
                    _logger.LogWarning("Unauthorized attempt to get ad hoc return request details {RequestId}.", requestId);
                    return Unauthorized();
                }

                var details = await _adhocReturnsService.GetAdHocReturnRequestDetailsAsync(requestId, loggedInEntity);
                return Ok(details);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to get ad hoc return request details {RequestId}: {Message}", requestId, ex.Message);
                return Unauthorized(ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for ad hoc return request details {RequestId}: {Message}", requestId, ex.Message);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving ad hoc return request details {RequestId}.", requestId);
                return StatusCode(500,  CustomErrorHandler.HandleException(ex) );
            }
        }

        [HttpGet("sacco/PendingAdHocReturnRequests")]
        public async Task<IActionResult> GetPendingAdHocReturnRequestsAsync([FromQuery] string? saccoId = null)
        {
            try
            {
                LoggedInEntity loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null)
                {
                    return Unauthorized();
                }

                var requests = await _adhocReturnsService.GetPendingAdHocReturnRequestsAsync(loggedInEntity.SaccoId);
                return Ok(requests);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }
        [HttpGet("admin/RespondedAdHocReturnRequests")]
        public async Task<IActionResult> GetRespondedAdHocReturnRequestsAsync([FromQuery] string? saccoId = null)
        {
            try
            {
                LoggedInEntity loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null)
                {
                    return Unauthorized();
                }

                var requests = await _adhocReturnsService.GetRespondedAdHocReturnRequestsAsync(loggedInEntity.SaccoId);
                return Ok(requests);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }
        [HttpGet("admin/CompletedAdHocReturnRequests")]
        public async Task<IActionResult> GetCompletedAdHocReturnRequestsAsync([FromQuery] string? saccoId = null)
        {
            try
            {
                LoggedInEntity loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null)
                {
                    return Unauthorized();
                }

                var requests = await _adhocReturnsService.GetCompletedAdHocReturnRequestsAsync(loggedInEntity.SaccoId);
                return Ok(requests);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

    }
}
