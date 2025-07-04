using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.Returns;
using Returns.Helpers;
using Returns.Helpers.Interfaces;
using static Returns.Helpers.TokenHelper;

namespace Returns.Controllers
{
    [Route("api/v2/returns")]
    [ApiController]
    public class ReturnsV2Controller : ControllerBase
    {
        private readonly IReturnsSubmissionService _submissionService;
        private readonly ILogger<ReturnsV2Controller> _logger;

        public ReturnsV2Controller(
            IReturnsSubmissionService submissionService,
            ILogger<ReturnsV2Controller> logger)
        {
            _submissionService = submissionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all expected returns for a SACCO with calculated status
        /// </summary>
        [HttpGet("expected")]
        public async Task<ActionResult<List<ExpectedReturnDto>>> GetExpectedReturns(
            [FromQuery] string? periodId = null)
        {
            try
            {
                var loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null || string.IsNullOrEmpty(loggedInEntity.SaccoId))
                {
                    return Unauthorized();
                }

                var expectedReturns = await _submissionService.GetExpectedReturnsAsync(
                    loggedInEntity.SaccoId, 
                    periodId);

                return Ok(expectedReturns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expected returns");
                return StatusCode(500, "An error occurred while fetching expected returns");
            }
        }

        /// <summary>
        /// Get returns that are due or late for filing
        /// </summary>
        [HttpGet("due")]
        public async Task<ActionResult<List<ExpectedReturnDto>>> GetDueReturns()
        {
            try
            {
                var loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null || string.IsNullOrEmpty(loggedInEntity.SaccoId))
                {
                    return Unauthorized();
                }

                var dueReturns = await _submissionService.GetDueReturnsAsync(loggedInEntity.SaccoId);
                return Ok(dueReturns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due returns");
                return StatusCode(500, "An error occurred while fetching due returns");
            }
        }

        /// <summary>
        /// Check if a SACCO can file returns for a specific period
        /// </summary>
        [HttpPost("check-eligibility")]
        public async Task<ActionResult<ReturnFilingEligibility>> CheckFilingEligibility(
            [FromBody] FilingEligibilityRequest request)
        {
            try
            {
                var loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null || string.IsNullOrEmpty(loggedInEntity.SaccoId))
                {
                    return Unauthorized();
                }

                var eligibility = await _submissionService.CheckFilingEligibilityAsync(
                    loggedInEntity.SaccoId,
                    request.PeriodId,
                    request.FormIds);

                return Ok(eligibility);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking filing eligibility");
                return StatusCode(500, "An error occurred while checking eligibility");
            }
        }

        /// <summary>
        /// Submit returns for a period
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<ReturnSubmissionResult>> SubmitReturns(
            [FromForm] NewReturnSubmissionDto submission)
        {
            try
            {
                var loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null || string.IsNullOrEmpty(loggedInEntity.SaccoId))
                {
                    return Unauthorized();
                }

                // Ensure the submission is for the logged-in SACCO
                submission.SaccoId = loggedInEntity.SaccoId;
                submission.SaccoType = loggedInEntity.SaccoType;

                var result = await _submissionService.SubmitReturnsAsync(submission);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting returns");
                return StatusCode(500, "An error occurred while submitting returns");
            }
        }

        /// <summary>
        /// Get submitted returns history
        /// </summary>
        [HttpGet("submitted")]
        public async Task<ActionResult<List<SubmittedReturnSummaryDto>>> GetSubmittedReturns(
            [FromQuery] int? year = null)
        {
            try
            {
                var loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null || string.IsNullOrEmpty(loggedInEntity.SaccoId))
                {
                    return Unauthorized();
                }

                var submittedReturns = await _submissionService.GetSubmittedReturnsAsync(
                    loggedInEntity.SaccoId, 
                    year);

                return Ok(submittedReturns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submitted returns");
                return StatusCode(500, "An error occurred while fetching submitted returns");
            }
        }

        /// <summary>
        /// Waive a return requirement (Admin only)
        /// </summary>
        [HttpPost("waive/{expectedReturnId}")]
        public async Task<ActionResult> WaiveReturn(
            string expectedReturnId,
            [FromBody] WaiveReturnRequest request)
        {
            try
            {
                var loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInEntity == null || string.IsNullOrEmpty(loggedInEntity.UserId))
                {
                    return Unauthorized();
                }

                // TODO: Add authorization check to ensure user is admin

                var success = await _submissionService.WaiveReturnAsync(
                    expectedReturnId,
                    request.Reason,
                    loggedInEntity.UserId);

                if (success)
                {
                    return Ok(new { message = "Return waived successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to waive return" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiving return");
                return StatusCode(500, "An error occurred while waiving the return");
            }
        }
    }

    // Request DTOs
    public class FilingEligibilityRequest
    {
        public string PeriodId { get; set; } = "";
        public List<string> FormIds { get; set; } = new();
    }

    public class WaiveReturnRequest
    {
        public string Reason { get; set; } = "";
    }
}