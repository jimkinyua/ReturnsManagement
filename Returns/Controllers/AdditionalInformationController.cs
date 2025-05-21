using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.AdditionalInfo;
using Returns.Helpers;
using Returns.Helpers.Interfaces;
using Returns.Models.Data;
using static Returns.Helpers.TokenHelper;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class AdditionalInformationController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        private readonly IAdditionalInformationRequestService _informationRequestService;
        private readonly IEmailService _emailSender;
        private readonly IComplianceService _complianceService;

        public AdditionalInformationController(ReturnsDbContext context, IAdditionalInformationRequestService informationRequestService, IEmailService emailSender, IComplianceService complianceService)
        {
            this._context = context;
            this._informationRequestService = informationRequestService;
            this._emailSender = emailSender;
            this._complianceService = complianceService;
        }

        [HttpGet("AdditionalInformationRequests")]
        public async Task<ActionResult<IEnumerable<AdditionalInformationRequestDto>>> GetAdditionalInformationRequests()
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401);
                }

                var requests = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Include(r => r.ReturnReponses)
                    .Where(x=>x.SaccoId == loggedInSacco.SaccoId)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();

                if (requests == null || requests.Count == 0)
                {
                    return Ok(new List<AdditionalInformationRequestDto>());
                }

                var resultDtos = new List<AdditionalInformationRequestDto>();

                foreach (var request in requests)
                {
                    AdditionalInformationRequestDto requestDto = new AdditionalInformationRequestDto
                    {
                        Id = request.Id,
                        Description = request.Description,
                        RequestedBy = request.RequestedBy,
                        Status = request.RequestStatus,
                        IsResponded = request.IsResponded,
                        CreatedAt = request.CreatedAt,
                        RespondedAt = request.RespondedAt,
                        Responses = new List<AdditionalInfoResponseDto>()
                    };
                    var responses = await _context.AdditionalInfoResponses
                        .Include(r => r.ResponseAttachements)
                        .Where(x=>x.RequestId == request.Id).ToListAsync();
                    if (responses.Any())
                    {
                        foreach (var response in responses)
                        {
                            var responseDto = new AdditionalInfoResponseDto
                            {
                                Id = response.Id,
                                RespondedBy = response.RespondedBy,
                                ResponseMessage = response.ReponseMessage,
                                RespondedAt = response.CreatedAt,
                            };

                            // Process each attachment if they exist
                            if (response.ResponseAttachements != null)
                            {
                                foreach (var attachment in response.ResponseAttachements)
                                {
                                    responseDto.Attachments.Add(new AdditionalInfoAttachmentDto
                                    {
                                        FileUrl = attachment.FileUrl
                                    });
                                }
                            }

                            requestDto.Responses.Add(responseDto);
                        }
                    }

                    resultDtos.Add(requestDto);
                }

                // 5. Return successful response
                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                // 6. Handle errors gracefully
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }


        [HttpGet("AdditionalInformationRequestsForReturn/{ReturnId}")]
        public async Task<ActionResult<IEnumerable<AdditionalInformationRequestDto>>> AdditionalInformationRequestsForReturn(string ReturnId)
        {
            if (string.IsNullOrEmpty(ReturnId))
            {
                return BadRequest("ReturnId cannot be null or empty.");
            }
            try
            {
                var requests = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Include(r => r.ReturnReponses)
                    .OrderBy(r => r.CreatedAt)
                    .Where(x=>x.ReturnId == ReturnId)
                    .ToListAsync();

                if (requests == null || requests.Count == 0)
                {
                    return Ok(new List<AdditionalInformationRequestDto>());
                }

                var resultDtos = new List<AdditionalInformationRequestDto>();

                foreach (var request in requests)
                {
                    AdditionalInformationRequestDto requestDto = new AdditionalInformationRequestDto
                    {
                        Id = request.Id,
                        Description = request.Description,
                        RequestedBy = request.RequestedBy,
                        Status = request.RequestStatus,
                        IsResponded = request.IsResponded,
                        CreatedAt = request.CreatedAt,
                        RespondedAt = request.RespondedAt,
                        Responses = new List<AdditionalInfoResponseDto>()
                    };
                    var responses = await _context.AdditionalInfoResponses
                        .Include(r => r.ResponseAttachements)
                        .Where(x => x.RequestId == request.Id).ToListAsync();
                    if (responses.Any())
                    {
                        foreach (var response in responses)
                        {
                            var responseDto = new AdditionalInfoResponseDto
                            {
                                Id = response.Id,
                                RespondedBy = response.RespondedBy,
                                ResponseMessage = response.ReponseMessage,
                                RespondedAt = response.CreatedAt,
                            };

                            // Process each attachment if they exist
                            if (response.ResponseAttachements != null)
                            {
                                foreach (var attachment in response.ResponseAttachements)
                                {
                                    responseDto.Attachments.Add(new AdditionalInfoAttachmentDto
                                    {
                                        FileUrl = attachment.FileUrl
                                    });
                                }
                            }

                            requestDto.Responses.Add(responseDto);
                        }
                    }

                    resultDtos.Add(requestDto);
                }

                // 5. Return successful response
                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                // 6. Handle errors gracefully
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpGet("AdditionalInformationRequestDetails/{id}")]
        public async Task<ActionResult<AdditionalInformationRequestDto>> GetAdditionalInformationRequestDetails(string Id)
        {
            try
            {
                var request = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == Id);
                if (request == null)
                {
                    return NotFound();
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
                };
                var responses = await _context.AdditionalInfoResponses
                    .AsNoTracking()
                    .Include(r => r.ResponseAttachements)
                    .Where(r => r.RequestId == Id)
                    .ToListAsync();

                if (responses.Any())
                {

                    requestDto.RespondedAt = responses.Max(r => r.CreatedAt);
                    requestDto.IsResponded = true;
                    foreach (var response in request.ReturnReponses)
                    {
                        var responseDto = new AdditionalInfoResponseDto
                        {
                            Id = response.Id,
                            RespondedBy = response.RespondedBy,
                            ResponseMessage = response.ReponseMessage,
                            RespondedAt = response.CreatedAt,
                            Attachments = new List<AdditionalInfoAttachmentDto>()
                        };
                        if (response.ResponseAttachements != null)
                        {
                            foreach (var attachment in response.ResponseAttachements)
                            {
                                responseDto.Attachments.Add(new AdditionalInfoAttachmentDto
                                {
                                    FileUrl = attachment.FileUrl
                                });
                            }
                        }
                        requestDto.Responses.Add(responseDto);
                    }
                }
                return Ok(requestDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // request sacco to submit additional information
        [HttpPost("RequestAdditionalInformation")]
        public async Task<ActionResult<AdditionalInformationRequestDto>> RequestAdditionalInformation([FromBody] CreateAdditionalInformationRequestDto createAdditionalInformationRequestDto)
        {
            LoggedInEntity loggedPerson = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedPerson == null || string.IsNullOrEmpty(loggedPerson.UserId))
            {
                return StatusCode(401);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var ReturnDetails = await _context.Returns.FirstOrDefaultAsync(r => r.Id == createAdditionalInformationRequestDto.ReturnId);
                if (ReturnDetails == null)
                {
                    return NotFound();
                }

                var result = await _informationRequestService.RequestAdditionalInformationAsync(createAdditionalInformationRequestDto, loggedPerson.UserId);
                if (result == null)
                {
                    return NotFound();
                }
               
                var coUserId = await _complianceService.GetAssignedComplianceOfficer(ReturnDetails.SaccoId);

                var TeamMembers = await _complianceService.GetTeamMembers(coUserId.TeamId);

                var ccAddresses = TeamMembers
                  .Select(m => m.Email)
                  .Where(e => !string.IsNullOrWhiteSpace(e) && !e.Equals(coUserId.Email, StringComparison.OrdinalIgnoreCase))
                  .Distinct();

                await _emailSender.SendEmailAsyncWithCC(loggedPerson.EmailAddress, "Additional Information Request", "You have a new request for additional information.", ccAddresses);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // respond to additional information request
        [HttpPost("RespondToAdditionalInformationRequest")]
        public async Task<ActionResult<AdditionalInfoResponseDto>> RespondToAdditionalInformationRequest([FromForm] CreateAdditionalInfoResponseDto createAdditionalInfoResponseDto)
        {
            LoggedInEntity loggedPerson = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedPerson == null || string.IsNullOrEmpty(loggedPerson.UserId))
            {
                return StatusCode(401);
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
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
