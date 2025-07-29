using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.SaccoAssignment;
using Returns.Helpers;
using Returns.Helpers.Interfaces;

namespace Returns.Controllers
{
    [Route("api/returns/[controller]")]
    [ApiController]
    public class SaccoAssignmentController : ControllerBase
    {
        private readonly ISaccoAssignmentService _saccoAssignmentService;
        private readonly ILogger<SaccoAssignmentController> _logger;

        public SaccoAssignmentController(
            ISaccoAssignmentService saccoAssignmentService,
            ILogger<SaccoAssignmentController> logger)
        {
            _saccoAssignmentService = saccoAssignmentService;
            _logger = logger;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignSacco([FromBody] AssignSaccoRequestDTO request)
        {
            try
            {
                var loggedInUser = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInUser == null || string.IsNullOrEmpty(loggedInUser.UserId))
                {
                    return StatusCode(401, "Unauthorized");
                }

                await _saccoAssignmentService.AssignSaccoToMemberAsync(
                    request.SaccoId,
                    request.MemberId,
                    loggedInUser.UserId);

                return Ok(new { Message = $"SACCO {request.SaccoId} assigned to member {request.MemberId}." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("assignable-saccos")]
        public async Task<ActionResult<List<SaccoDTO>>> GetAssignableSaccos()
        {
            try
            {
                var loggedInUser = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInUser == null || string.IsNullOrEmpty(loggedInUser.UserId))
                {
                    return StatusCode(401, "Unauthorized");
                }

                var saccos = await _saccoAssignmentService.GetAssignableSaccosAsync(loggedInUser.UserId);
                return Ok(saccos);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assignable SACCOs.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("team-members")]
        public async Task<ActionResult<List<TeamMemberDTO>>> GetTeamMembers()
        {
            try
            {
                var loggedInUser = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInUser == null || string.IsNullOrEmpty(loggedInUser.UserId))
                {
                    return StatusCode(401, "Unauthorized");
                }

                var members = await _saccoAssignmentService.GetTeamMembersAsync(loggedInUser.UserId);
                return Ok(members);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching team members.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpPost("unassign")]
        public async Task<IActionResult> UnassignSacco([FromBody] UnassignSaccoRequestDTO request)
        {
            try
            {
                var loggedInUser = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInUser == null || string.IsNullOrEmpty(loggedInUser.UserId))
                {
                    return StatusCode(401, "Unauthorized");
                }

                await _saccoAssignmentService.UnassignSaccoAsync(request.SaccoId, loggedInUser.UserId);
                return Ok(new { Message = $"SACCO {request.SaccoId} unassigned." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning SACCO {SaccoId}.", request.SaccoId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("assignments")]
        public async Task<ActionResult<List<SaccoAssignmentDTO>>> GetTeamAssignments()
        {
            try
            {
                var loggedInUser = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInUser == null || string.IsNullOrEmpty(loggedInUser.UserId))
                {
                    return StatusCode(401, "Unauthorized");
                }

                var assignments = await _saccoAssignmentService.GetTeamAssignmentsAsync(loggedInUser.UserId);
                return Ok(assignments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching team assignments.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

    }
}
