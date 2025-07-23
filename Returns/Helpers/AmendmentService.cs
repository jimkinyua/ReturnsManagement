using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class AmendmentService : IAmendmentService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AmendmentService> _logger;
        private readonly IReturnSubmissionService _returnSubmissionService;
        private readonly IReturnAmendmentPolicy _returnAmendmentPolicy;

        public AmendmentService( ReturnsDbContext context,IEmailService emailService,ILogger<AmendmentService> logger,IReturnSubmissionService returnSubmissionService, IReturnAmendmentPolicy returnAmendmentPolicy)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _returnSubmissionService = returnSubmissionService;
            _returnAmendmentPolicy = returnAmendmentPolicy;
        }


        public Task CreateAmendRequestForSacco(string submissionId, string officerId, string reason)
        {
            throw new NotImplementedException();
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

        public Task ReviewAmendmentRequest(string requestId, bool approve, string reviewerId)
        {
            throw new NotImplementedException();
        }
    }
}
