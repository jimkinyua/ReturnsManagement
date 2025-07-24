using iText.Forms.Form.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Text.Json;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public class AmendmentService : IAmendmentService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AmendmentService> _logger;
        private readonly IReturnSubmissionService _returnSubmissionService;
        private readonly IReturnAmendmentPolicy _returnAmendmentPolicy;
        private readonly IExcelParser _excelParser;

        public AmendmentService( ReturnsDbContext context,IEmailService emailService,ILogger<AmendmentService> logger,IReturnSubmissionService returnSubmissionService, IReturnAmendmentPolicy returnAmendmentPolicy, IExcelParser excelParser)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _returnSubmissionService = returnSubmissionService;
            _returnAmendmentPolicy = returnAmendmentPolicy;
            _excelParser = excelParser;
        }


        public async Task<AmendmentRequest> CreateAmendRequestForSacco(AmendmentRequestDTO dto, LoggedInEntity loggedInEntity)
        {
            var today = DateTime.Now.Date;
            if (_returnAmendmentPolicy.CanAutoAmend(today))
            {
                throw new InvalidOperationException("Amendment requests are not allowed on or before the 15th. Use direct submission instead.");
            }

            // Retrieve the submission
            var submission = await _context.ReturnSubmissions
                .Include(s => s.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);

            if (submission == null)
            {
                throw new InvalidOperationException("Submission not found.");
            }

            // Check for existing amendment requests
            var existingRequest = await _context.AmendmentRequests
                .Where(r => r.ReturnSubmissionId == dto.SubmissionId
                            && (r.Status == AmendmentStatus.Pending || r.Status == AmendmentStatus.Approved))
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync();

            if (existingRequest != null)
            {
                throw new InvalidOperationException("An amendment request is already pending or approved for this submission.");
            }

            if (dto.FormFile != null)
            {
                var category = (FormCategory)submission.ExpectedReturn.ReturnForm.Category;
                var parseResult = await _excelParser.ParseAsync(dto.FormFile, category, submission.ExpectedReturn.ReturnForm.SaccoTypeId);
                var contentsJson = parseResult.Success
                    ? JsonSerializer.Serialize(parseResult.Rows.Select(row => row.ToEntity()))
                    : null;
                var draft = new AmendmentRequest
                {
                    ExpectedReturnId = submission.ExpectedReturnId,
                    SaccoId = submission.SaccoId,
                    FileUrl = await FormsHelper.SaveFileAsync(dto.FormFile, "Drafts"),
                    Reason = dto.AmendmentReason.Trim(),
                    Status = AmendmentStatus.Pending,
                    ParseSuccess = parseResult.Success,
                    ParseErrorsJson = JsonSerializer.Serialize(parseResult.Errors),
                    ContentsJson = contentsJson
                };

                if (!parseResult.Success)
                {
                    throw new InvalidOperationException($"Failed to parse Excel file: {string.Join(", ", parseResult.Errors)}");
                }

                await _context.AmendmentRequests.AddAsync(draft);
                await _context.SaveChangesAsync();

                return draft;

            }

            throw new InvalidOperationException("No file provided for amendment request.");
        }

        public async Task<IList<SubmissionResultDto>> DoAmendmentIfNecessasy(NewReturnDTO dto, TokenHelper.LoggedInEntity sacco)
        {
            var today = DateTime.UtcNow.Date;
            var results = new List<SubmissionResultDto>();


            foreach (var item in dto.FormUploads)
            {
                try
                {
                    // Before or on  15th  auto‑amend
                    if (_returnAmendmentPolicy.CanAutoAmend(today))
                    {
                        var result = await _returnSubmissionService.UploadDraftAsync(dto, sacco);
                        results.AddRange(result);
                        continue;
                    }

                    // After 15th: check for existing submission
                    var latest = await _context.ReturnSubmissions
                        .Where(s => s.ExpectedReturnId == item.ExpectedReturnId &&
                                    s.SaccoId == sacco.SaccoId &&
                                    s.IsLatest)
                        .SingleOrDefaultAsync();

                    if (latest == null)
                    {
                        throw new InvalidOperationException("New submissions after the 15th are not allowed. Please submit an amendment request.");
                    }

                    var req = await _context.AmendmentRequests
                    .Where(r => r.ExpectedReturnId == item.ExpectedReturnId
                                && r.SaccoId == sacco.SaccoId
                               && (r.Status == AmendmentStatus.Pending || r.Status == AmendmentStatus.Approved))
                    .OrderByDescending(r => r.RequestedAt)
                    .FirstOrDefaultAsync();


                    if (req != null)
                    {
                        throw new InvalidOperationException("An amendment request is already pending approval. Please wait until SASRA processes it.");
                    }

                    var newReq = new AmendmentRequest
                    {
                        ExpectedReturnId = item.ExpectedReturnId!,
                        ReturnSubmissionId = latest.Id,
                        SaccoId = sacco.SaccoId,
                        RequestedById = sacco.UserId,
                        RequestedAt = DateTime.Now,
                        Reason = "Amendent after After cutoff Date",
                        Status = AmendmentStatus.Pending
                    };
                    await _context.AmendmentRequests.AddAsync(newReq);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return results;
        }

        public async Task<IList<PendingAmendmentRequestDTO>> GetPendingAmendmentRequestsAsync()
        {
            var query = _context.AmendmentRequests
                           .Include(r => r.ReturnSubmission)
                           .ThenInclude(er => er.ExpectedReturn)
                           .Where(r => r.Status == AmendmentStatus.Pending);

            var requests = await query
               .OrderByDescending(r => r.RequestedAt)
               .Select(r => new PendingAmendmentRequestDTO
               {
                   Id = r.Id,
                   ExpectedReturnId = r.ExpectedReturnId,
                   ReturnSubmissionId = r.ReturnSubmissionId,
                   SaccoId = r.SaccoId,
                   RequestedById = r.RequestedById,
                   RequestedAt = r.RequestedAt,
                   Reason = r.Reason,
                   Status = r.Status,
                   ReturnType = r.ReturnSubmission.ExpectedReturn.ReturnForm != null && r.ReturnSubmission.ExpectedReturn.ReturnForm != null
                       ? (FormCategory?)r.ReturnSubmission.ExpectedReturn.ReturnForm.Category
                       : null
               })
               .ToListAsync();
            return requests;
        }


        public async Task<AmendmentRequestDetailsDTO> GetAmendmentRequestDetailsAsync(string requestId, LoggedInEntity admin)
        {
            var query = _context.AmendmentRequests
                          .Include(r => r.ReturnSubmission)
                          .ThenInclude(er => er.ExpectedReturn)
                          .Where(r => r.Status == AmendmentStatus.Pending);

            // Retrieve amendment request
            var request = await _context.AmendmentRequests
                .Include(r => r.ReturnSubmission)
                 .ThenInclude(er => er.ExpectedReturn)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogWarning("Amendment request {RequestId} not found.", requestId);
                throw new InvalidOperationException("Amendment request not found.");
            }


            // Deserialize JSON contents
            List<string> parseErrors = new List<string>();
            List<object> rows = new List<object>();
            try
            {
                if (!string.IsNullOrEmpty(request.ParseErrorsJson))
                {
                    parseErrors = JsonSerializer.Deserialize<List<string>>(request.ParseErrorsJson) ?? new List<string>();
                }
                if (!string.IsNullOrEmpty(request.ContentsJson))
                {
                    rows = JsonSerializer.Deserialize<List<object>>(request.ContentsJson) ?? new List<object>();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON for amendment request {RequestId}", requestId);
                parseErrors.Add("Failed to deserialize amendment request contents.");
            }

            // Map to DTO
            var details = new AmendmentRequestDetailsDTO
            {
                Id = request.Id,
                ExpectedReturnId = request.ExpectedReturnId,
                ReturnSubmissionId = request.ReturnSubmissionId,
                SaccoId = request.SaccoId,
                RequestedById = request.RequestedById,
                RequestedAt = request.RequestedAt,
                ReviewedById = request.ReviewedById,
                ReviewedAt = request.ReviewedAt,
                Reason = request.Reason,
                Status = request.Status,
                FileUrl = request.FileUrl,
                /*ParseSuccess = request.ParseSuccess,
                ParseErrors = parseErrors,*/
                Rows = rows,
                ReturnType = request.ReturnSubmission.ExpectedReturn.ReturnForm != null && request.ReturnSubmission.ExpectedReturn.ReturnForm != null
                       ? (FormCategory?)request.ReturnSubmission.ExpectedReturn.ReturnForm.Category
                       : null
            };

            return details;
        }

        public async Task ReviewAmendmentRequest(string requestId, bool approve, string reviewerId)
        {
             // Retrieve amendment request
            var request = await _context.AmendmentRequests
                .Include(r => r.ExpectedReturn)
                .Include(r => r.ReturnSubmission)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogWarning("Amendment request {RequestId} not found.", requestId);
                throw new InvalidOperationException("Amendment request not found.");
            }

            if (request.Status != AmendmentStatus.Pending)
            {
                _logger.LogWarning("Amendment request {RequestId} is not in Pending status. Current status: {Status}", requestId, request.Status);
                throw new InvalidOperationException($"Amendment request is not pending. Current status: {request.Status}.");
            }

            request.ReviewedById = reviewerId;
            request.ReviewedAt = DateTime.Now;
            request.Status = approve ? AmendmentStatus.Approved : AmendmentStatus.Rejected;
            // If approved, process the new submission
            if (approve)
            {
                if (string.IsNullOrEmpty(request.FileUrl))
                {
                    _logger.LogWarning("Amendment request {RequestId} has no associated file.", requestId);
                    throw new InvalidOperationException("No file associated with the amendment request.");
                }

                if (!request.ParseSuccess)
                {
                    _logger.LogWarning("Amendment request {RequestId} has invalid contents: {Errors}", requestId, request.ParseErrorsJson);
                    throw new InvalidOperationException("Cannot approve request with invalid file contents.");
                }

                var dto = new NewReturnDTO
                {
                    FormUploads = new List<ReturnFormUploadDTO>
                    {   
                        new ReturnFormUploadDTO
                        {
                            ExpectedReturnId = request.ExpectedReturnId,
                            formFile = await FormsHelper.GetFileFromUrlAsync(request.FileUrl)
                        }
                    }
                };

                var sacco = new LoggedInEntity
                {
                    SaccoId = request.SaccoId,
                    UserId = request.RequestedById,
                    SaccoType = "0" ?? string.Empty
                };

                var result = await _returnSubmissionService.UploadDraftAsync(dto, sacco);
                if (result.Any(r => r.Status == SubmissionStatus.Failed))
                {
                    _logger.LogWarning("Failed to process approved amendment request {RequestId}: {Errors}", requestId, string.Join(", ", result.SelectMany(r => r.Messages)));
                    request.Status = AmendmentStatus.Pending; // Revert to Pendng on failure
                    await _context.SaveChangesAsync();
                    throw new InvalidOperationException($"Failed to process approved submission: {string.Join(", ", result.SelectMany(r => r.Messages))}");
                }

                request.Status = AmendmentStatus.Approved;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Amendment request {RequestId} {Status} by reviewer {ReviewerId}", requestId, request.Status, reviewerId);


            }
        }
    }
}
