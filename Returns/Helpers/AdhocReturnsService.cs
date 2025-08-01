using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Adhoc;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Text.Json;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public class AdhocReturnsService : IAdhocReturnsService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AmendmentService> _logger;
        private readonly IReturnSubmissionService _returnSubmissionService;
        private readonly IReturnAmendmentPolicy _returnAmendmentPolicy;
        private readonly IExcelParser _excelParser;
        private readonly IComplianceService _complianceService;
        private readonly IWorkflowEngineService _workflowEngineService;
        public AdhocReturnsService(
                 ReturnsDbContext context,
                 IEmailService emailService,
                 ILogger<AmendmentService> logger,
                 IReturnSubmissionService returnSubmissionService,
                 IReturnAmendmentPolicy returnAmendmentPolicy,
                 IComplianceService complianceService,
                 IWorkflowEngineService workflowEngineService,
                 IExcelParser excelParser)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _complianceService = complianceService;
            _returnSubmissionService = returnSubmissionService;
            _returnAmendmentPolicy = returnAmendmentPolicy;
            _excelParser = excelParser;
            _workflowEngineService = workflowEngineService;
        }

        public async Task<IList<PendingAdHocReturnRequestDTO>> GetPendingAdHocReturnRequestsAsync(string? saccoId = null)
        {
            var query = _context.AdHocReturnRequests
                .Where(r => r.Status == AdHocReturnRequestStatus.Pending);

           /* if (!string.IsNullOrEmpty(saccoId))
            {
                query = query.Where(r => r.SaccoId == saccoId);
            }*/

            var rawData = await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new
            {
                r.Id,
                r.SaccoId,
                r.RequestedById,
                r.RequestedAt,
                r.Description,
                r.AttachmentUrlsJson,
                r.Status
            })
            .ToListAsync();

            var requests = rawData.Select(r => new PendingAdHocReturnRequestDTO
            {
                Id = r.Id,
                SaccoId = r.SaccoId,
                RequestedById = r.RequestedById,
                RequestedAt = r.RequestedAt,
                Description = r.Description,
                AttachmentUrls = string.IsNullOrEmpty(r.AttachmentUrlsJson)
         ? new List<string>()
         : JsonSerializer.Deserialize<List<string>>(r.AttachmentUrlsJson)!,
                Status = r.Status.ToString()
            }).ToList();

            return requests;
        }
        public async Task<AdHocReturnRequest> CreateAdHocReturnRequestAsync(AdHocReturnRequestDTO dto, LoggedInEntity admin)
        {
         
            var sacco = await _complianceService.GetSaccoByIdAsync(dto.SaccoId);
            if (sacco == null)
            {
                _logger.LogWarning("SACCO {SaccoId} not found.", dto.SaccoId);
                throw new InvalidOperationException("SACCO not found.");
            }

            List<string> attachmentUrls = new List<string>();

            foreach (var item in dto?.AttachmentFiles)
            {
                var attachmentUrl = await FormsHelper.SaveFileAsync(item, "AdHocReturnRequests");
                if (attachmentUrl == null)
                {
                    _logger.LogWarning("Failed to save attachment file {FileName} for SACCO {SaccoId}", item.FileName, dto.SaccoId);
                    throw new InvalidOperationException("Failed to save attachment file.");
                }
                attachmentUrls.Add(attachmentUrl);
            }

            // Create new ad hoc return request
            var newRequest = new AdHocReturnRequest
            {
                SaccoId = dto.SaccoId,
                RequestedById = admin.UserId,
                RequestedAt = DateTime.UtcNow,
                Description = dto.Description.Trim(),
                AttachmentUrlsJson = JsonSerializer.Serialize(attachmentUrls),
                Status = AdHocReturnRequestStatus.Pending,
            };

            await _context.AdHocReturnRequests.AddAsync(newRequest);
            await _context.SaveChangesAsync();

            // Notify SACCO
            return newRequest;
        }
        public async Task<AdHocReturnRequestDetailsDTO> GetAdHocReturnRequestDetailsAsync(string requestId, LoggedInEntity admin)
        {
            var request = await _context.AdHocReturnRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogWarning("Ad hoc return request {RequestId} not found.", requestId);
                throw new InvalidOperationException("Ad hoc return request not found.");
            }

            List<string> attachmentUrls = new List<string>();
            List<string> responseFileUrls = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(request.AttachmentUrlsJson))
                {
                    attachmentUrls = JsonSerializer.Deserialize<List<string>>(request.AttachmentUrlsJson) ?? new List<string>();
                }
                if (!string.IsNullOrEmpty(request.ResponseFileUrlsJson))
                {
                    responseFileUrls = JsonSerializer.Deserialize<List<string>>(request.ResponseFileUrlsJson) ?? new List<string>();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON for ad hoc return request {RequestId}", requestId);
                throw new InvalidOperationException("Failed to deserialize request contents.");
            }

            return new AdHocReturnRequestDetailsDTO
            {
                Id = request.Id,
                SaccoId = request.SaccoId,
                RequestedById = request.RequestedById,
                RequestedAt = request.RequestedAt,
                Description = request.Description,
                AttachmentUrls = attachmentUrls,
                ResponseDescription = request.ResponseDescription,
                ResponseFileUrls = responseFileUrls,
                RespondedById = request.RespondedById,
                RespondedAt = request.RespondedAt,
                Status = request.Status.ToString()
            };
        }
        public async Task<AdHocReturnRequest> RespondToAdHocReturnRequestAsync(AdHocReturnResponseDTO dto, LoggedInEntity loggedInEntity)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var request = await _context.AdHocReturnRequests
                .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.Status == AdHocReturnRequestStatus.Pending);

            if (request == null)
            {
                _logger.LogWarning("Ad hoc return request {RequestId} not found or not pending.", dto.RequestId);
                throw new InvalidOperationException("Ad hoc return request not found or not pending.");
            }

            if (loggedInEntity.SaccoId != request.SaccoId)
            {
                _logger.LogWarning("Unauthorized attempt to respond to ad hoc request {RequestId} by SACCO {SaccoId}", dto.RequestId, loggedInEntity.SaccoId);
                throw new UnauthorizedAccessException("You do not have permission to respond to this request.");
            }

            List<string> attachmentUrls = new List<string>();
             
            foreach (var item in dto.ResponseFiles)
            {
                var attachmentUrl = await FormsHelper.SaveFileAsync(item, "AdHocReturnRequests");
                if (attachmentUrl == null)
                {
                    throw new InvalidOperationException("Failed to save attachment file.");
                }
                attachmentUrls.Add(attachmentUrl);
            }

            // Update request
            request.ResponseDescription = dto.ResponseDescription.Trim();
            request.ResponseFileUrlsJson = JsonSerializer.Serialize(attachmentUrls);
            request.RespondedById = loggedInEntity.UserId;
            request.RespondedAt = DateTime.UtcNow;
            request.Status = AdHocReturnRequestStatus.Responded;

            await _context.SaveChangesAsync();
                
            return request;
        }
        private async Task<ReturnSubmission> GetSubmissionAsync(string submissionId, string saccoId)
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

        public async Task<IList<PendingAdHocReturnRequestDTO>> GetCompletedAdHocReturnRequestsAsync(string? saccoId = null)
        {
            var query = _context.AdHocReturnRequests
                          .Where(r => r.Status == AdHocReturnRequestStatus.Completed);

            if (!string.IsNullOrEmpty(saccoId))
            {
                query = query.Where(r => r.SaccoId == saccoId);
            }

            var rawData = await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new
            {
                r.Id,
                r.SaccoId,
                r.RequestedById,
                r.RequestedAt,
                r.Description,
                r.AttachmentUrlsJson,
                r.Status
            })
            .ToListAsync();

            var requests = rawData.Select(r => new PendingAdHocReturnRequestDTO
            {
                Id = r.Id,
                SaccoId = r.SaccoId,
                RequestedById = r.RequestedById,
                RequestedAt = r.RequestedAt,
                Description = r.Description,
                AttachmentUrls = string.IsNullOrEmpty(r.AttachmentUrlsJson)
         ? new List<string>()
         : JsonSerializer.Deserialize<List<string>>(r.AttachmentUrlsJson)!,
                Status = r.Status.ToString()
            }).ToList();

            return requests;
        }


        public async Task<IList<PendingAdHocReturnRequestDTO>> GetRespondedAdHocReturnRequestsAsync(string? saccoId = null)
        {
            var query = _context.AdHocReturnRequests
                          .Where(r => r.Status == AdHocReturnRequestStatus.Responded);

            if (!string.IsNullOrEmpty(saccoId))
            {
                query = query.Where(r => r.SaccoId == saccoId);
            }

            var rawData = await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new
            {
                r.Id,
                r.SaccoId,
                r.RequestedById,
                r.RequestedAt,
                r.Description,
                r.AttachmentUrlsJson,
                r.Status
            })
            .ToListAsync();

            var requests = rawData.Select(r => new PendingAdHocReturnRequestDTO
            {
                Id = r.Id,
                SaccoId = r.SaccoId,
                RequestedById = r.RequestedById,
                RequestedAt = r.RequestedAt,
                Description = r.Description,
                AttachmentUrls = string.IsNullOrEmpty(r.AttachmentUrlsJson)
         ? new List<string>()
         : JsonSerializer.Deserialize<List<string>>(r.AttachmentUrlsJson)!,
                Status = r.Status.ToString()
            }).ToList();

            return requests;
        }

        public async Task<AdHocReturnRequest> CloseRequest(string RequestId, LoggedInEntity admin)
        {
            var request = await _context.AdHocReturnRequests
                .FirstOrDefaultAsync(r => r.Id == RequestId);

            if (request == null)
            {
                _logger.LogWarning("Ad hoc return request {RequestId} not found or not pending.", RequestId);
                throw new InvalidOperationException("Ad hoc return request not found or not pending.");
            }
            request.Status = AdHocReturnRequestStatus.Completed;
            request.RespondedById = admin.UserId;
            await  _context.SaveChangesAsync();
            return request;
        }
    }
}   
