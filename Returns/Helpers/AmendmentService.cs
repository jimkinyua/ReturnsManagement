using iText.Commons.Utils;
using iText.Forms.Form.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Compliance;
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

        public AmendmentService(
            ReturnsDbContext context,
            IEmailService emailService,
            ILogger<AmendmentService> logger,
            IReturnSubmissionService returnSubmissionService,
            IReturnAmendmentPolicy returnAmendmentPolicy,
            IExcelParser excelParser)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _returnSubmissionService = returnSubmissionService;
            _returnAmendmentPolicy = returnAmendmentPolicy;
            _excelParser = excelParser;
        }

        public async Task<ReturnSubmission?> GetSubmissionUsingExpectedIdAsync(string submissionId, string saccoId, Boolean Filing = false)
        {
            var submission = await _context.ReturnSubmissions
                .Include(s => s.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .FirstOrDefaultAsync(s => s.ExpectedReturnId == submissionId);

            if (submission == null)
            {
                _logger.LogWarning("Return submission {SubmissionId} not found.", submissionId);
                if (Filing == true)
                {
                    return null;
                }
                throw new InvalidOperationException("Return submission not found.");
            }

            if (saccoId != submission.SaccoId)
            {
                _logger.LogWarning("Unauthorized attempt to access submission {SubmissionId} by SACCO {SaccoId}", submissionId, saccoId);
                throw new UnauthorizedAccessException("You do not have permission to access this submission.");
            }

            return submission;
        }      
        
        public async Task<ReturnSubmission> GetSubmissionUsingReturnIdAsync(string submissionId, string saccoId)
        {
            var submission = await _context.ReturnSubmissions
                .Include(s => s.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                _logger.LogWarning("Return submission {SubmissionId} not found.", submissionId);
                throw new InvalidOperationException("Return submission not found.");
            }

            if (saccoId != submission.SaccoId)
            {
                _logger.LogWarning("Unauthorized attempt to access submission {SubmissionId} by SACCO {SaccoId}", submissionId, saccoId);
                throw new UnauthorizedAccessException("You do not have permission to access this submission.");
            }

            return submission;
        }

        private async Task<(string FileUrl, bool ParseSuccess, string ContentsJson, string ParseErrorsJson)> ParseAndSaveFileAsync(IFormFile formFile, FormCategory category, string saccoTypeId, string submissionId)
        {
            if (!FormsHelper.IsValidExcelFile(formFile))
            {
                _logger.LogWarning("Invalid Excel file provided for submission {SubmissionId}.", submissionId);
                throw new InvalidOperationException("Invalid Excel file format. Only .xlsx files are supported.");
            }

            var parseResult = await _excelParser.ParseAsync(formFile, category, saccoTypeId);
            var fileUrl = await FormsHelper.SaveFileAsync(formFile, "Drafts");
            var parseSuccess = parseResult.Success;
            var contentsJson = parseResult.Success ? JsonSerializer.Serialize(parseResult.Rows.Select(row => row.ToEntity())) : null;
            var parseErrorsJson = JsonSerializer.Serialize(parseResult.Errors);

            if (!parseResult.Success)
            {
                _logger.LogWarning("Failed to parse Excel file for submission {SubmissionId}: {Errors}", submissionId, string.Join(", ", parseResult.Errors));
                throw new InvalidOperationException($"Failed to parse Excel file: {string.Join(", ", parseResult.Errors)}");
            }

            return (fileUrl, parseSuccess, contentsJson, parseErrorsJson);
        }

        private async Task<AmendmentRequest?> CheckExistingAmendmentRequestAsync(string submissionId)
        {
            return await _context.AmendmentRequests
                .Where(r => r.ReturnSubmissionId == submissionId
                            && (r.Status == AmendmentStatus.Pending || r.Status == AmendmentStatus.Approved))
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<AmendmentRequest> CreateAmendRequestForSacco(AmendmentRequestDTO dto, LoggedInEntity loggedInEntity)
        {
  

            if (dto.FormFile == null)
            {
                throw new ArgumentException("An Excel file is required for amendment requests.", nameof(dto.FormFile));
            }

            // Check for existing amendment requests
            var existingRequest = await CheckExistingAmendmentRequestAsync(dto.SubmissionId);
            if (existingRequest != null)
            {
                if (existingRequest.IsAdminInitiated)
                {
                    throw new InvalidOperationException("An admin-initiated amendment request exists. Please Respond to It.");
                }
                else
                {
                    throw new InvalidOperationException("An amendment request is already pending or approved for this submission.");
                }
            }

            var today = DateTime.Now.Date;
            if (_returnAmendmentPolicy.CanAutoAmend(today))
            {
                throw new InvalidOperationException("Amendment requests are not allowed on or before the 15th. Use direct submission instead.");
            }

            // Retrieve and validate submission
            var submission = await GetSubmissionUsingReturnIdAsync(dto.SubmissionId, loggedInEntity.SaccoId);

            // Parse and save file
            var (fileUrl, parseSuccess, contentsJson, parseErrorsJson) = await ParseAndSaveFileAsync(
                dto.FormFile,
                (FormCategory)submission.ExpectedReturn.ReturnForm.Category,
                submission.ExpectedReturn.ReturnForm.SaccoTypeId,
                dto.SubmissionId);

            var newRequest = new AmendmentRequest
            {
                ExpectedReturnId = submission.ExpectedReturnId,
                ReturnSubmissionId = dto.SubmissionId,
                SaccoId = submission.SaccoId,
                RequestedById = loggedInEntity.UserId,
                RequestedAt = DateTime.Now,
                Reason = dto.AmendmentReason.Trim(),
                Status = AmendmentStatus.Pending,
                FileUrl = fileUrl,
                ParseSuccess = parseSuccess,
                ContentsJson = contentsJson,
                ParseErrorsJson = parseErrorsJson
            };

            await _context.AmendmentRequests.AddAsync(newRequest);
            await _context.SaveChangesAsync();

            return newRequest;
        }

        public async Task<AmendmentRequest> RespondToAdminAmendmentRequestAsync(SaccoAmendmentResponseDTO dto, LoggedInEntity loggedInEntity)
        {
         

            // Retrieve and validate submission
            var submission = await GetSubmissionUsingExpectedIdAsync(dto.ReturnSubmissionId, loggedInEntity.SaccoId);

            // Check for admin-initiated amendment request
            var existingRequest = await _context.AmendmentRequests
                .Where(r => r.ReturnSubmissionId == dto.ReturnSubmissionId
                            && r.Status == AmendmentStatus.Pending
                            && r.IsAdminInitiated)
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync();

            if (existingRequest == null)
            {
                throw new InvalidOperationException("No admin-initiated amendment request found for this submission.");
            }

            // Parse and save file
            var (fileUrl, parseSuccess, contentsJson, parseErrorsJson) = await ParseAndSaveFileAsync(
                dto.FormFile,
                (FormCategory)submission.ExpectedReturn.ReturnForm.Category,
                submission.ExpectedReturn.ReturnForm.SaccoTypeId,
                dto.ReturnSubmissionId);

            // Update existing amendment request
            existingRequest.FileUrl = fileUrl;
            existingRequest.ParseSuccess = parseSuccess;
            existingRequest.ContentsJson = contentsJson;
            existingRequest.ParseErrorsJson = parseErrorsJson;
            existingRequest.Status = AmendmentStatus.Cancelled; // Mark as Cancelled after processing

            // Process submission directly
            var newReturnDto = new NewReturnDTO
            {
                FormUploads = new List<ReturnFormUploadDTO>
                {
                    new ReturnFormUploadDTO
                    {
                        ExpectedReturnId = submission.ExpectedReturnId,
                        formFile = dto.FormFile
                    }
                }
            };

            var result = await _returnSubmissionService.UploadDraftAsync(newReturnDto, loggedInEntity);
            if (result.Any(r => r.Status == SubmissionStatus.Failed))
            {
                throw new InvalidOperationException($"Failed to process amendment: {string.Join(", ", result.SelectMany(r => r.Messages))}");
            }

            await _context.SaveChangesAsync();


            return existingRequest;
        }

        public async Task<AmendmentRequest> CreateAdminAmendmentRequestAsync(AdminAmendmentRequestDTO dto, LoggedInEntity admin)
        {
    
            // Retrieve and validate submission
            var submission = await GetSubmissionUsingExpectedIdAsync(dto.ReturnSubmissionId, admin.SaccoId);

            // Check for existing amendment requests
            var existingRequest = await CheckExistingAmendmentRequestAsync(dto.ReturnSubmissionId);
            if (existingRequest != null)
            {
                throw new InvalidOperationException("An amendment request is already pending or approved for this submission.");
            }

            // Create new amendment request
            var newRequest = new AmendmentRequest
            {
                Id = Guid.NewGuid().ToString(),
                ExpectedReturnId = submission.ExpectedReturnId,
                ReturnSubmissionId = dto.ReturnSubmissionId,
                SaccoId = submission.SaccoId,
                RequestedById = admin.UserId,
                RequestedAt = DateTime.UtcNow,
                Reason = dto.Reason.Trim(),
                Status = AmendmentStatus.Pending,
                FileUrl = string.Empty,
                ParseSuccess = true,
                ContentsJson = null,
                ParseErrorsJson = null,
                IsAdminInitiated = true
            };

            await _context.AmendmentRequests.AddAsync(newRequest);
            await _context.SaveChangesAsync();

            // Notify SACCO
         
            _logger.LogInformation("Admin amendment request {RequestId} created for submission {ReturnSubmissionId} by admin {UserId}", newRequest.Id, dto.ReturnSubmissionId, admin.UserId);

            return newRequest;
        }

        public async Task<IList<PendingAmendmentRequestDTO>> GetPendingAmendmentRequestsAsync()
        {
            var requests = await _context.AmendmentRequests
                .Include(r => r.ReturnSubmission)
                .ThenInclude(er => er.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .Where(r => r.Status == AmendmentStatus.Pending)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new PendingAmendmentRequestDTO
                {
                    Id = r.Id,
                    ExpectedReturnId = r.ExpectedReturnId,
                    ReturnSubmissionId = r.ReturnSubmissionId,
                    SaccoId = r.SaccoId,
                    RequestedById = r.RequestedById,
                    RequestedAt = r.RequestedAt.ToLongDateString(),
                    Reason = r.Reason,
                    Status = r.Status,
                    ReturnType = r.ReturnSubmission.ExpectedReturn.ReturnForm != null
                        ? (FormCategory?)r.ReturnSubmission.ExpectedReturn.ReturnForm.Category
                        : null
                })
                .ToListAsync();

            return requests;
        }

        public async Task<AmendmentRequestDetailsDTO> GetAmendmentRequestDetailsAsync(string requestId, LoggedInEntity admin)
        {
            var request = await _context.AmendmentRequests
                .Include(r => r.ReturnSubmission)
                .ThenInclude(er => er.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogWarning("Amendment request {RequestId} not found.", requestId);
                throw new InvalidOperationException("Amendment request not found.");
            }

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
                Rows = rows,
                ReturnType = request.ReturnSubmission.ExpectedReturn.ReturnForm != null
                    ? (FormCategory?)request.ReturnSubmission.ExpectedReturn.ReturnForm.Category
                    : null
            };

            return details;
        }

        public async Task ReviewAmendmentRequest(string requestId, bool approve, string reviewerId)
        {
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
            request.ReviewedAt = DateTime.UtcNow;
            request.Status = approve ? AmendmentStatus.Approved : AmendmentStatus.Rejected;

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
                    SaccoType = request.ReturnSubmission.ExpectedReturn.ReturnForm?.SaccoTypeId ?? string.Empty
                };

                var result = await _returnSubmissionService.UploadDraftAsync(dto, sacco);
                if (result.Any(r => r.Status == SubmissionStatus.Failed))
                {
                    _logger.LogWarning("Failed to process approved amendment request {RequestId}: {Errors}", requestId, string.Join(", ", result.SelectMany(r => r.Messages)));
                    request.Status = AmendmentStatus.Pending; // Revert to Pending on failure
                    await _context.SaveChangesAsync();
                    throw new InvalidOperationException($"Failed to process approved submission: {string.Join(", ", result.SelectMany(r => r.Messages))}");
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Amendment request {RequestId} {Status} by reviewer {ReviewerId}", requestId, request.Status, reviewerId);
        }
    }
}
