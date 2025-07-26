using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.AdditionalInfo;
using Returns.Helpers;
using Returns.Helpers.Interfaces;
using Returns.Models.Data;
using static Returns.Helpers.TokenHelper;
using Microsoft.Extensions.Logging;  // Added for ILogger

namespace Returns.Controllers
{
    [Route("api/returns/[controller]")]
    [ApiController]
    public class AdditionalInformationController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        private readonly IAdditionalInformationRequestService _informationRequestService;
        private readonly IEmailService _emailSender;
        private readonly IComplianceService _complianceService;
        private readonly ILogger<AdditionalInformationController> _logger;  // Added

        public AdditionalInformationController(
            ReturnsDbContext context,
            IAdditionalInformationRequestService informationRequestService,
            IEmailService emailSender,
            IComplianceService complianceService,
            ILogger<AdditionalInformationController> logger)
        {
            _context = context;
            _informationRequestService = informationRequestService;
            _emailSender = emailSender;
            _complianceService = complianceService;
            _logger = logger;
        }

        [HttpGet("AdditionalInformationRequests")]
        public async Task<ActionResult<IEnumerable<AdditionalInformationRequestDto>>> GetAdditionalInformationRequests()
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401, "Unauthorized");
                }

                var requests = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Include(r => r.ReturnReponses).ThenInclude(resp => resp.ResponseAttachements)
                    .Where(x => x.SaccoId == loggedInSacco.SaccoId)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();

                var resultDtos = requests.Select(request => new AdditionalInformationRequestDto
                {
                    Id = request.Id,
                    Description = request.Description,
                    RequestedBy = request.RequestedBy,
                    Status = request.RequestStatus,
                    IsResponded = request.IsResponded,
                    CreatedAt = request.CreatedAt,
                    RespondedAt = request.RespondedAt,
                    Responses = request.ReturnReponses.Select(response => new AdditionalInfoResponseDto
                    {
                        Id = response.Id,
                        RespondedBy = response.RespondedBy,
                        ResponseMessage = response.ReponseMessage,
                        RespondedAt = response.CreatedAt,
                        Attachments = response.ResponseAttachements?.Select(a => new AdditionalInfoAttachmentDto
                        {
                            FileUrl = a.FileUrl,
                            Name = a.FileName
                        }).ToList() ?? new List<AdditionalInfoAttachmentDto>()
                    }).ToList()
                }).ToList();

                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("AdditionalInformationRequestsForReturnSubmission/{ReturnSubmissionId}")]
        public async Task<ActionResult<IEnumerable<AdditionalInformationRequestDto>>> AdditionalInformationRequestsForReturnSubmission(string ReturnSubmissionId)
        {
            if (string.IsNullOrEmpty(ReturnSubmissionId))
            {
                return BadRequest("ReturnSubmissionId cannot be null or empty.");
            }
            try
            {
                var requests = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Include(r => r.ReturnReponses).ThenInclude(resp => resp.ResponseAttachements)
                    .Where(x => x.ReturnSubmissionId == ReturnSubmissionId)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();

                var resultDtos = requests.Select(request => new AdditionalInformationRequestDto
                {
                    Id = request.Id,
                    Description = request.Description,
                    RequestedBy = request.RequestedBy,
                    Status = request.RequestStatus,
                    IsResponded = request.IsResponded,
                    CreatedAt = request.CreatedAt,
                    RespondedAt = request.RespondedAt,
                    Responses = request.ReturnReponses.Select(response => new AdditionalInfoResponseDto
                    {
                        Id = response.Id,
                        RespondedBy = response.RespondedBy,
                        ResponseMessage = response.ReponseMessage,
                        RespondedAt = response.CreatedAt,
                        Attachments = response.ResponseAttachements?.Select(a => new AdditionalInfoAttachmentDto
                        {
                            FileUrl = a.FileUrl,
                            Name = a.FileName
                        }).ToList() ?? new List<AdditionalInfoAttachmentDto>()
                    }).ToList()
                }).ToList();

                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving additional information requests for ReturnSubmissionId {ReturnSubmissionId}", ReturnSubmissionId);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("AdditionalInformationRequestDetails/{id}")]
        public async Task<ActionResult<AdditionalInformationRequestDto>> GetAdditionalInformationRequestDetails(string Id)
        {
            try
            {
                var request = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Include(r => r.ReturnReponses).ThenInclude(resp => resp.ResponseAttachements)
                    .FirstOrDefaultAsync(r => r.Id == Id);

                if (request == null)
                {
                    return NotFound("Additional information request not found.");
                }

                var requestDto = new AdditionalInformationRequestDto
                {
                    Id = request.Id,
                    Description = request.Description,
                    RequestedBy = request.RequestedBy,
                    Status = request.RequestStatus,
                    IsResponded = request.IsResponded,
                    CreatedAt = request.CreatedAt,
                    RespondedAt = request.RespondedAt,
                    Responses = request.ReturnReponses.Select(response => new AdditionalInfoResponseDto
                    {
                        Id = response.Id,
                        RespondedBy = response.RespondedBy,
                        ResponseMessage = response.ReponseMessage,
                        RespondedAt = response.CreatedAt,
                        Attachments = response.ResponseAttachements?.Select(a => new AdditionalInfoAttachmentDto
                        {
                            FileUrl = a.FileUrl,
                            Name = a.FileName
                        }).ToList() ?? new List<AdditionalInfoAttachmentDto>()
                    }).ToList()
                };

                return Ok(requestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for additional information request {Id}", Id);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpPost("RequestAdditionalInformation")]
        public async Task<ActionResult<AdditionalInformationRequestDto>> RequestAdditionalInformation([FromBody] CreateAdditionalInformationRequestDto createAdditionalInformationRequestDto)
        {
            LoggedInEntity loggedPerson = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedPerson == null || string.IsNullOrEmpty(loggedPerson.UserId))
            {
                return StatusCode(401, "Unauthorized");
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(createAdditionalInformationRequestDto.ReturnSubmissionId))
            {
                return BadRequest("Invalid request data or missing ReturnSubmissionId.");
            }

            try
            {
                var submissionDetails = await _context.ReturnSubmissions
                    .FirstOrDefaultAsync(rs => rs.Id == createAdditionalInformationRequestDto.ReturnSubmissionId);
                if (submissionDetails == null)
                {
                    return NotFound("Return submission not found.");
                }

                var result = await _informationRequestService.RequestAdditionalInformationAsync(createAdditionalInformationRequestDto, loggedPerson.UserId);
                if (result == null)
                {
                    return NotFound("Failed to create additional information request.");
                }

                var coUser = await _complianceService.GetAssignedComplianceOfficer(submissionDetails.SaccoId);
                var sacco = await _complianceService.GetSaccoByIdAsync(submissionDetails.SaccoId);
                if (sacco == null || coUser == null)
                {
                    return NotFound("Sacco or compliance officer not found.");
                }

                var teamMembers = await _complianceService.GetTeamMembers(coUser.TeamId);
                var ccAddresses = teamMembers
                    .Select(m => m.Email)
                    .Where(e => !string.IsNullOrWhiteSpace(e) && !e.Equals(coUser.Email, StringComparison.OrdinalIgnoreCase))
                    .Distinct();

                await _emailSender.SendEmailAsyncWithCC(
                    sacco.OfficialSaccoEmail,
                    "Additional Information Request",
                    "You have a new request for additional information.",
                    ccAddresses);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting additional information for ReturnSubmissionId {ReturnSubmissionId}", createAdditionalInformationRequestDto.ReturnSubmissionId);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpPost("RespondToAdditionalInformationRequest")]
        public async Task<ActionResult<AdditionalInfoResponseDto>> RespondToAdditionalInformationRequest([FromForm] CreateAdditionalInfoResponseDto createAdditionalInfoResponseDto)
        {
            LoggedInEntity loggedPerson = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedPerson == null || string.IsNullOrEmpty(loggedPerson.UserId))
            {
                return StatusCode(401, "Unauthorized");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _informationRequestService.AddResponseAsync(createAdditionalInfoResponseDto, loggedPerson.UserId);
                if (result == null)
                {
                    return NotFound("Additional information request not found.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to additional information request {Id}", createAdditionalInfoResponseDto.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomErrorHandler.HandleException(ex));
            }
        }
    }
}