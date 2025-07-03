using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Returns.DTOs.Forms;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Text.Json;

namespace Returns.Helpers
{
    public class ReturnFormAttachmentService : IReturnFormAttachmentService
    {
        private readonly ReturnsDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReturnFormAttachmentService> _logger;
        private const int DEFAULT_FILING_DEADLINE_DAYS = 30;
        private const int PREVIEW_CACHE_MINUTES = 10;

        public ReturnFormAttachmentService(
            ReturnsDbContext context,
            IMemoryCache cache,
            ILogger<ReturnFormAttachmentService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ReturnFormAttachmentPreviewDto> PreviewAttachmentAsync(AttachReturnFormsDto request)
        {
            var preview = new ReturnFormAttachmentPreviewDto();
            var expectedReturns = new List<ExpectedReturnPreviewDto>();

            // Validate request
           /* if (!request.PeriodIds.Any() && !request.ApplyToAllPeriodsInYear)
            {
                preview.Warnings.Add("No periods specified for attachment.");
                return preview;
            }*/

            if (!request.PeriodIds.Any())
            {
                preview.Warnings.Add("No periods specified for attachment.");
                return preview;
            }

            if (!request.ReturnFormIds.Any())
            {
                preview.Warnings.Add("No forms specified for attachment.");
                return preview;
            }

            // Get periods
            var periodQuery = _context.ReturnPeriods
                .Include(p => p.ReportingYear)
                .Include(p => p.FrequencyCatalog)
                .Where(p => p.IsLocked);

           /* if (request.ApplyToAllPeriodsInYear && !string.IsNullOrEmpty(request.YearId.ToString()))
            {
                periodQuery = periodQuery.Where(p => p.YearId == request.YearId.Value);
            }
            else
            {*/
                periodQuery = periodQuery.Where(p => request.PeriodIds.Contains(p.Id));
           /* }*/

            var periods = await periodQuery.ToListAsync();

            if (!periods.Any())
            {
                preview.Warnings.Add("No valid periods found for the specified criteria.");
                return preview;
            }

            // Get forms
            var forms = await _context.ReturnForms
                .Where(f => request.ReturnFormIds.Contains(f.Id) && f.IsActive)
                .ToListAsync();

            if (!forms.Any())
            {
                preview.Warnings.Add("No valid forms found for the specified IDs.");
                return preview;
            }

            // Get existing expected returns for conflict detection
            var existingReturns = await _context.ExpectedReturns
                .Where(er => periods.Select(p => p.Id).Contains(er.PeriodId) &&
                            forms.Select(f => f.Id).Contains(er.ReturnFormId))
                .ToListAsync();

            // Generate preview
            foreach (var period in periods)
            {
                foreach (var form in forms)
                {
                    var existing = existingReturns.FirstOrDefault(er =>
                        er.PeriodId == period.Id && er.ReturnFormId == form.Id);

                    var filingDeadline = CalculateFilingDeadline(
                        period.EndDate,
                         period.FrequencyCatalog.DefaultDeadlineOffset);

                    expectedReturns.Add(new ExpectedReturnPreviewDto
                    {
                        PeriodName = period.Name,
                        FormName = form.FormName,
                        FormCode = form.Code,
                        FilingDeadline = filingDeadline,
                        AlreadyExists = existing != null,
                        ExistingStatus = existing?.Status.ToString()
                    });

                    if (existing != null && existing.IsActive)
                    {
                        preview.Warnings.Add($"Form '{form.FormName}' already attached to period '{period.Name}' with status '{existing.Status}'");
                    }
                }
            }

            preview.ExpectedReturns = expectedReturns;
            preview.TotalExpectedReturns = expectedReturns.Count;

            // Generate and cache preview token
            preview.PreviewToken = Guid.NewGuid().ToString();
            var cacheKey = $"form_attachment_preview_{preview.PreviewToken}";
            var cacheData = new
            {
                Request = request,
                Preview = preview,
                Periods = periods.Select(p => p.Id).ToList(),
                Forms = forms.Select(f => f.Id).ToList()
            };

            _cache.Set(cacheKey, JsonSerializer.Serialize(cacheData), TimeSpan.FromMinutes(PREVIEW_CACHE_MINUTES));

            return preview;
        }
        public async Task<ReturnFormAttachmentResultDto> ConfirmAttachmentAsync(ConfirmReturnFormAttachmentDto request)
        {
            var result = new ReturnFormAttachmentResultDto();

            // Retrieve cached preview data
            var cacheKey = $"form_attachment_preview_{request.PreviewToken}";
            if (!_cache.TryGetValue<string>(cacheKey, out var cachedJson) || string.IsNullOrEmpty(cachedJson))
            {
                result.Errors.Add("Preview token expired or invalid. Please generate a new preview.");
                return result;
            }

            JsonDocument cacheDoc;
            try
            {
                cacheDoc = JsonDocument.Parse(cachedJson);
            }
            catch
            {
                result.Errors.Add("Invalid preview data. Please generate a new preview.");
                return result;
            }

            // Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var periodIds = JsonSerializer.Deserialize<List<string>>(cacheDoc.RootElement.GetProperty("Periods").GetRawText());
                var formIds = JsonSerializer.Deserialize<List<string>>(cacheDoc.RootElement.GetProperty("Forms").GetRawText());
                var originalRequest = JsonSerializer.Deserialize<AttachReturnFormsDto>(cacheDoc.RootElement.GetProperty("Request").GetRawText());

                // Get periods and forms
                var periods = await _context.ReturnPeriods
                    .Where(p => periodIds!.Contains(p.Id))
                    .ToListAsync();

                var forms = await _context.ReturnForms
                    .Where(f => formIds!.Contains(f.Id))
                    .ToListAsync();

                // Get existing expected returns
                var existingReturns = await _context.ExpectedReturns
                    .Where(er => periodIds!.Contains(er.PeriodId) && formIds!.Contains(er.ReturnFormId))
                    .ToListAsync();

                foreach (var period in periods)
                {
                    foreach (var form in forms)
                    {
                        var existing = existingReturns.FirstOrDefault(er =>
                            er.PeriodId == period.Id && er.ReturnFormId == form.Id);

                        if (existing != null)
                        {
                            if (request.ForceOverwrite)
                            {
                                // Update existing
                                existing.FilingDeadline = CalculateFilingDeadline(
                                    period.EndDate,
                                    period.FrequencyCatalog.DefaultDeadlineOffset);
                                existing.IsActive = true;
                                existing.Status = ExpectedStatus.Due;
                                result.UpdatedCount++;
                            }
                            else
                            {
                                result.SkippedCount++;
                                _logger.LogInformation($"Skipped existing attachment: Form {form.Code} for period {period.Name}");
                            }
                        }
                        else
                        {
                            // Create new
                            var expectedReturn = new ExpectedReturn
                            {
                                Id = Guid.NewGuid().ToString(),
                                PeriodId = period.Id,
                                ReturnFormId = form.Id,
                                FilingDeadline = CalculateFilingDeadline(
                                    period.EndDate,
                                    period.FrequencyCatalog.DefaultDeadlineOffset),
                                Status = ExpectedStatus.Due,
                                IsActive = true
                            };

                            _context.ExpectedReturns.Add(expectedReturn);
                            result.CreatedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Success = true;
                _logger.LogInformation($"Successfully attached forms: Created={result.CreatedCount}, Updated={result.UpdatedCount}, Skipped={result.SkippedCount}");

                // Clear the preview cache
                _cache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during form attachment confirmation");
                result.Success = false;
                result.Errors.Add($"Transaction failed: {ex.Message}");
            }

            return result;
        }

        public async Task<List<AttachedReturnFormDto>> GetAttachedFormsAsync(string periodId)
        {
            var attachedForms = await _context.ExpectedReturns
                .Include(er => er.ReturnForm)
                .Where(er => er.PeriodId == periodId && er.IsActive)
                .Select(er => new AttachedReturnFormDto
                {
                    Id = er.Id,
                    FormId = er.ReturnFormId,
                    FormName = er.ReturnForm.FormName,
                    FormCode = er.ReturnForm.Code,
                    FilingDeadline = er.FilingDeadline,
                    Status = er.Status,
                    IsActive = er.IsActive
                })
                .ToListAsync();

            return attachedForms;
        }

        public async Task<bool> RemoveAttachmentAsync(string expectedReturnId)
        {
            var expectedReturn = await _context.ExpectedReturns
                .FirstOrDefaultAsync(er => er.Id == expectedReturnId);

            if (expectedReturn == null)
            {
                _logger.LogWarning($"Expected return not found: {expectedReturnId}");
                return false;
            }

            // Check if already filed
            if (expectedReturn.Status == ExpectedStatus.Filed)
            {
                _logger.LogWarning($"Cannot remove filed return: {expectedReturnId}");
                return false;
            }

            // Soft delete
            expectedReturn.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Removed attachment: {expectedReturnId}");
            return true;
        }

        public async Task<bool> UpdateFilingDeadlinesAsync(UpdateFilingDeadlinesDto request)
        {
            if (!request.ExpectedReturnIds.Any())
            {
                return false;
            }

            var expectedReturns = await _context.ExpectedReturns
                .Where(er => request.ExpectedReturnIds.Contains(er.Id) && er.IsActive)
                .ToListAsync();

            if (!expectedReturns.Any())
            {
                return false;
            }

            foreach (var expectedReturn in expectedReturns)
            {
                expectedReturn.FilingDeadline = request.NewFilingDeadline;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Updated filing deadlines for {expectedReturns.Count} expected returns");

            return true;
        }

        private DateTime CalculateFilingDeadline(DateTime periodEndDate, int daysAfterEnd)
        {
            var deadline = periodEndDate.AddDays(daysAfterEnd);

            // If deadline falls on weekend, move to next Monday
            if (deadline.DayOfWeek == DayOfWeek.Saturday)
                deadline = deadline.AddDays(2);
            else if (deadline.DayOfWeek == DayOfWeek.Sunday)
                deadline = deadline.AddDays(1);

            return deadline;
        }
    }
}