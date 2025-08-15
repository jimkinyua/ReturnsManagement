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
        private readonly IConfiguration _configuration;
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
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
               .AddEnvironmentVariables()
               .Build();
        }

        [HttpGet("AdditionalInformationRequests")]
        public async Task<ActionResult<IEnumerable<AdditionalInformationRequestDto>>> GetAdditionalInformationRequests()
        {
            var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURLForDocuments").Value;

            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401, "Unauthorized");
                }
                // Fetch the main list of requests without includes (using AsNoTracking for performance)
                var requests = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Where(x => x.SaccoId == loggedInSacco.SaccoId)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();
                if (!requests.Any())
                {
                    return Ok(new List<AdditionalInformationRequestDto>());
                }
                var requestIds = requests.Select(r => r.Id).ToList();
                // Fetch all responses and their attachments in one query for all requests
                var allResponses = await _context.AdditionalInfoResponses
                    .AsNoTracking()
                    .Include(resp => resp.ResponseAttachements)
                    .Where(resp => requestIds.Contains(resp.RequestId))
                    .ToListAsync();
                // Group responses by request ID
                var responsesByRequest = allResponses
                    .GroupBy(resp => resp.RequestId)
                    .ToDictionary(g => g.Key, g => g.ToList());
                // Get all unique submission IDs from requests
                var submissionIds = requests
                    .Select(r => r.ReturnSubmissionId)
                    .Where(id => id != null) // Assuming nullable; adjust if not
                    .Distinct()
                    .ToList();
                // Fetch all related submissions with their nested entities in one query
                var allSubmissions = await _context.ReturnSubmissions
                    .AsNoTracking()
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                    .Where(rs => submissionIds.Contains(rs.Id))
                    .ToListAsync();
                // Dictionary for quick lookup of submissions by ID
                var submissionsById = allSubmissions.ToDictionary(rs => rs.Id, rs => rs);
                // Map to DTOs
                var resultDtos = requests.Select(request =>
                {
                    var submission = submissionsById.TryGetValue(request.ReturnSubmissionId ?? "", out var sub) ? sub : null;
                    var responsesForRequest = responsesByRequest.TryGetValue(request.Id, out var resps) ? resps : new List<Returns.Models.AdditionalInfoResponse>();
                    return new AdditionalInformationRequestDto
                    {
                        Id = request.Id,
                        Description = request.Description,
                        RequestedBy = request.RequestedBy,
                        Status = request.RequestStatus,
                        IsResponded = request.IsResponded,
                        CreatedAt = request.CreatedAt,
                        RespondedAt = request.RespondedAt,
                        SaccoId = request.SaccoId,
                        SaccoName = loggedInSacco.SaccoName ?? "Unknown",
                        ReturnSubmissionId = request.ReturnSubmissionId,
                        ReturnType = submission?.ExpectedReturn?.ReturnForm?.Category.ToString(),
                        Responses = responsesForRequest.Select(response => new AdditionalInfoResponseDto
                        {
                            Id = response.Id,
                            RespondedBy = response.RespondedBy,
                            ResponseMessage = response.ReponseMessage,
                            RespondedAt = response.CreatedAt,
                            Attachments = response.ResponseAttachements?.Select(a => new AdditionalInfoAttachmentDto
                            {
                                FileUrl = $"{baseUrl}{a.FileUrl}",
                                Name = a.FileName
                            }).ToList() ?? new List<AdditionalInfoAttachmentDto>()
                        }).ToList()
                    };
                }).ToList();
                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, CustomErrorHandler.HandleException(ex));
            }
        }


        private async Task<string> GetSaccoNameAsync(string saccoId)
        {
            try
            {
                if (long.TryParse(saccoId, out long saccoIdLong))
                {
                    var sacco = await _complianceService.GetSaccoByIdAsync(saccoIdLong);
                    return sacco?.SaccoName ?? "Unknown";
                }
                return "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get SACCO name for ID {SaccoId}", saccoId);
                return "Unknown";
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
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURLForDocuments").Value;

                // Fetch the main list of requests without includes
                var requests = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Where(x => x.ReturnSubmissionId == ReturnSubmissionId)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();

                if (!requests.Any())
                {
                    return Ok(new List<AdditionalInformationRequestDto>());
                }

                var requestIds = requests.Select(r => r.Id).ToList();

                // Fetch all responses and their attachments in one query for all requests
                var allResponses = await _context.AdditionalInfoResponses
                    .AsNoTracking()
                    .Include(resp => resp.ResponseAttachements)
                    .Where(resp => requestIds.Contains(resp.RequestId))
                    .ToListAsync();

                // Group responses by request ID
                var responsesByRequest = allResponses
                    .GroupBy(resp => resp.RequestId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Get all unique submission IDs from requests
                var submissionIds = requests
                    .Select(r => r.ReturnSubmissionId)
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                // Fetch all related submissions with their nested entities in one query
                var allSubmissions = await _context.ReturnSubmissions
                    .AsNoTracking()
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                    .Where(rs => submissionIds.Contains(rs.Id))
                    .ToListAsync();

                // Dictionary for quick lookup of submissions by ID
                var submissionsById = allSubmissions.ToDictionary(rs => rs.Id, rs => rs);

                // Map to DTOs
                var resultDtos = requests.Select(request =>
                {
                    var submission = submissionsById.TryGetValue(request.ReturnSubmissionId ?? "", out var sub) ? sub : null;
                    var responsesForRequest = responsesByRequest.TryGetValue(request.Id, out var resps) ? resps : new List<Returns.Models.AdditionalInfoResponse>();

                    return new AdditionalInformationRequestDto
                    {
                        Id = request.Id,
                        Description = request.Description,
                        RequestedBy = request.RequestedBy,
                        Status = request.RequestStatus,
                        IsResponded = request.IsResponded,
                        CreatedAt = request.CreatedAt,
                        RespondedAt = request.RespondedAt,
                        SaccoId = request.SaccoId,
                        SaccoName = GetSaccoNameAsync(request.SaccoId).Result,
                        ReturnSubmissionId = request.ReturnSubmissionId,
                        ReturnType = submission?.ExpectedReturn?.ReturnForm?.Category.ToString(),
                        Responses = responsesForRequest.Select(response => new AdditionalInfoResponseDto
                        {
                            Id = response.Id,
                            RespondedBy = response.RespondedBy,
                            ResponseMessage = response.ReponseMessage,
                            RespondedAt = response.CreatedAt,
                            Attachments = response.ResponseAttachements?.Select(a => new AdditionalInfoAttachmentDto
                            {
                                FileUrl = $"{baseUrl}{a.FileUrl}",
                                Name = a.FileName
                            }).ToList() ?? new List<AdditionalInfoAttachmentDto>()
                        }).ToList()
                    };
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
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURLForDocuments").Value;

                var request = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == Id);

                if (request == null)
                {
                    return NotFound("Additional information request not found.");
                }

                // Fetch responses and attachments
                var responses = await _context.AdditionalInfoResponses
                    .AsNoTracking()
                    .Include(resp => resp.ResponseAttachements)
                    .Where(resp => resp.RequestId == Id)
                    .ToListAsync();

                // Fetch submission details
                var submission = await _context.ReturnSubmissions
                    .AsNoTracking()
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                    .FirstOrDefaultAsync(rs => rs.Id == request.ReturnSubmissionId);

                var requestDto = new AdditionalInformationRequestDto
                {
                    Id = request.Id,
                    Description = request.Description,
                    RequestedBy = request.RequestedBy,
                    Status = request.RequestStatus,
                    IsResponded = request.IsResponded,
                    CreatedAt = request.CreatedAt,
                    RespondedAt = request.RespondedAt,
                    SaccoId = request.SaccoId,
                    SaccoName = await GetSaccoNameAsync(request.SaccoId),
                    ReturnSubmissionId = request.ReturnSubmissionId,
                    ReturnType = submission?.ExpectedReturn?.ReturnForm?.Category.ToString(),
                    Responses = responses.Select(response => new AdditionalInfoResponseDto
                    {
                        Id = response.Id,
                        RespondedBy = response.RespondedBy,
                        ResponseMessage = response.ReponseMessage,
                        RespondedAt = response.CreatedAt,
                        Attachments = response.ResponseAttachements?.Select(a => new AdditionalInfoAttachmentDto
                        {
                            FileUrl = $"{baseUrl}{a.FileUrl}",
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
                long LongsaccoId = long.Parse(submissionDetails.SaccoId);
                var sacco = await _complianceService.GetSaccoByIdAsync(LongsaccoId);
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
                    sacco.OfficialEmail,
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