using Microsoft.EntityFrameworkCore;
using Returns.DTOs.AdditionalInfo;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class AdditionalInformationRequestService : IAdditionalInformationRequestService
    {
        private readonly IEmailService _emailSender;
        private readonly ILogger<AdditionalInformationRequestService> _logger;
        private readonly ReturnsDbContext _context;
        private readonly IComplianceService _complianceService;

        public AdditionalInformationRequestService(ReturnsDbContext context, IEmailService emailSender, ILogger<AdditionalInformationRequestService> logger, IComplianceService complianceService)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
            _complianceService = complianceService;
        }
        public async Task<AdditionalInformationRequestDto> RequestAdditionalInformationAsync(CreateAdditionalInformationRequestDto createAdditionalInformationRequestDto, string RequestedBy)
        {
            var submissionDetails = await _context.ReturnSubmissions
                .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                .FirstOrDefaultAsync(rs => rs.Id == createAdditionalInformationRequestDto.ReturnSubmissionId);
            if (submissionDetails == null)
                throw new KeyNotFoundException("Return submission not found");

            var entity = new AdditionalInformationRequest
            {
                ReturnSubmissionId = createAdditionalInformationRequestDto.ReturnSubmissionId,
                Description = createAdditionalInformationRequestDto.Description,
                RequestedBy = RequestedBy,
                SaccoId = submissionDetails.SaccoId,
            };

            _context.AdditionalInformationRequests.Add(entity);
            await _context.SaveChangesAsync();

            // Get proper SACCO name from compliance service
            string saccoName = "Unknown";
            try
            {
                if (long.TryParse(submissionDetails.SaccoId, out long saccoId))
                {
                    var sacco = await _complianceService.GetSaccoByIdAsync(saccoId);
                    saccoName = sacco?.SaccoName ?? "Unknown";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get SACCO name for ID {SaccoId}, using fallback", submissionDetails.SaccoId);
                saccoName = submissionDetails.ExpectedReturn?.ReturnForm?.SaccoTypeId == "0" ? "DT" : "NWDT";
            }

            return new AdditionalInformationRequestDto
            {
                Id = entity.Id,
                Description = entity.Description,
                RequestedBy = entity.RequestedBy,
                Status = entity.RequestStatus,
                IsResponded = entity.IsResponded,
                CreatedAt = entity.CreatedAt,
                SaccoId = entity.SaccoId,
                SaccoName = saccoName,
                ReturnSubmissionId = entity.ReturnSubmissionId,
                ReturnType = submissionDetails.ExpectedReturn?.ReturnForm?.Category.ToString(),
                Responses = new List<AdditionalInfoResponseDto>()
            };
        }

        public async Task<AdditionalInfoResponseDto> AddResponseAsync(CreateAdditionalInfoResponseDto dto, string RespondedBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var request = await _context.AdditionalInformationRequests.FirstOrDefaultAsync(r => r.Id == dto.Id);

                if (request == null)
                {
                    throw new KeyNotFoundException("Request not found");
                }

                var response = new AdditionalInfoResponse
                {
                    RequestId = dto.Id,
                    RespondedBy = RespondedBy,
                    ReponseMessage = dto.ResponseMessage
                };

                _context.AdditionalInfoResponses.Add(response);

                request.IsResponded = true;
                request.RequestStatus = AdditionalInfoRequestStatus.Responded.ToString();
                request.RespondedAt = DateTime.Now;

                // Explicitly tell Entity Framework that the entity has been modified
                _context.Entry(request).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                // Handle attachments if any
                if (dto.Attachments != null && dto.Attachments.Count > 0)
                {
                    foreach (var attachment in dto.Attachments)
                    {
                        var FileUrl = await FormsHelper.SaveFileAsync(attachment.File, "Additional Returns", attachment.FileName);
                        var attachmentEntity = new ResponseAttachement
                        {
                            ResponseId = response.Id,
                            FileUrl = FileUrl,
                            FileName = attachment.FileName,
                        };
                        _context.ResponseAttachements.Add(attachmentEntity);
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Retrieve saved attachments for the response
                var SavedAttachments = await _context.ResponseAttachements
                    .Where(a => a.ResponseId == response.Id)
                    .Select(a => new AdditionalInfoAttachmentDto
                    {
                        FileUrl = a.FileUrl,
                        Name = a.FileName,
                    }).ToListAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await NotifySasraUser(request.RequestedBy);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notification to user {UserId}", request.RequestedBy);
                    }
                });

                return new AdditionalInfoResponseDto
                {
                    Id = response.Id,
                    ResponseMessage = response.ReponseMessage,
                    RespondedAt = request.RespondedAt.Value,
                    RespondedBy = RespondedBy,
                    Attachments = SavedAttachments
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task NotifySasraUser(string UserId)
        {
            var SasrsUser = await _complianceService.GetUserById(UserId);
            if (SasrsUser == null)
            {
                throw new Exception("Sacco not found.");
            }
            var email = SasrsUser.Email;
            var subject = "Additional Info Response Received";
            var message = "Your additional information request has been responded to.";
            await _emailSender.SendEmailAsync(email, subject, message);
        }


    }
}