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

        public AdditionalInformationController(ReturnsDbContext context, IAdditionalInformationRequestService informationRequestService, IEmailService emailSender)
        {
            this._context = context;
            this._informationRequestService = informationRequestService;
            this._emailSender = emailSender;
        }

        [HttpGet("AdditionalInformationRequests")]
        public async Task<ActionResult<IEnumerable<AdditionalInformationRequestDto>>> GetAdditionalInformationRequests()
        {
            try
            {
                var requests = await _context.AdditionalInformationRequests
                    .AsNoTracking()
                    .Include(r => r.ReturnReponses)
                        .ThenInclude(resp => resp.ResponseAttachements)
                    //.Where(r => r.ReturnId == returnId)
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

                    if (request.ReturnReponses != null)
                    {
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
                    Responses = new List<AdditionalInfoResponseDto>()
                };
                var responses = await _context.AdditionalInfoResponses
                    .AsNoTracking()
                    .Include(r => r.ResponseAttachements)
                    .Where(r => r.RequestId == Id)
                    .ToListAsync();

                if (responses.Count > 0)
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
                var result = await _informationRequestService.RequestAdditionalInformationAsync(createAdditionalInformationRequestDto, loggedPerson.UserId);
                if (result == null)
                {
                    return NotFound();
                }
                var ReturnDetails = await _context.Returns.FirstOrDefaultAsync(r => r.Id == createAdditionalInformationRequestDto.ReturnId);
                // Send email notification to the user or sacco
                await _emailSender.SendEmailAsync(loggedPerson.EmailAddress, "Additional Information Request", "You have a new request for additional information.");
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
                // Send email notification to the user or sacco
                await _emailSender.SendEmailAsync(loggedPerson.EmailAddress, "Response to Additional Information Request", "You have a new response to your request for additional information.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
