using Microsoft.EntityFrameworkCore;
using Returns.DTOs.PeriodManagement;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Globalization;

namespace Returns.Helpers
{
    public class PeriodGenerator : IPeriodGenerator
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<PeriodGenerator> _logger;

        public PeriodGenerator(ReturnsDbContext context, ILogger<PeriodGenerator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PeriodGenerationPreviewDto> PreviewPeriodsAsync(int yearId, List<int> frequencyIds)
        {
            var reportingYear = await _context.ReportingYears
                .FirstOrDefaultAsync(y => y.Id == yearId);

            if (reportingYear == null)
            {
                throw new ArgumentException($"Reporting year with ID {yearId} not found.");
            }

            var frequencies = await _context.FrequencyCatalogs
                .Where(f => frequencyIds.Contains(f.Id) && f.IsActive)
                .ToListAsync();

            var preview = new PeriodGenerationPreviewDto
            {
                YearId = yearId,
                Year = reportingYear.Year,
                StartDate = reportingYear.StartDate,
                EndDate = reportingYear.EndDateDate
            };

            // Check for any existing periods
            var existingPeriods = await _context.ReturnPeriods
                .Where(p => p.YearId == yearId && frequencyIds.Contains(p.FrequencyId))
                .ToListAsync();

            foreach (var frequency in frequencies)
            {
                var frequencyPreview = GenerateFrequencyPreview(reportingYear, frequency, existingPeriods);
                preview.Frequencies.Add(frequencyPreview);
                preview.TotalPeriodsToGenerate += frequencyPreview.NewPeriodsCount;
            }

            // Add warnings if needed
            if (reportingYear.StartDate >= reportingYear.EndDateDate)
            {
                preview.Warnings.Add("Warning: Reporting year start date is after or equal to end date.");
            }

            return preview;
        }

        public async Task<PeriodGenerationResultDto> GeneratePeriodsAsync(int yearId, List<int> frequencyIds, string createdBy)
        {
            var result = new PeriodGenerationResultDto
            {
                YearId = yearId,
                Success = true
            };

            try
            {
                var reportingYear = await _context.ReportingYears
                    .FirstOrDefaultAsync(y => y.Id == yearId);

                if (reportingYear == null)
                {
                    result.Success = false;
                    result.Message = $"Reporting year with ID {yearId} not found.";
                    return result;
                }

                var frequencies = await _context.FrequencyCatalogs
                    .Where(f => frequencyIds.Contains(f.Id) && f.IsActive)
                    .ToListAsync();

                // Get existing periods to ensure idempotency
                var existingPeriods = await _context.ReturnPeriods
                    .Where(p => p.YearId == yearId && frequencyIds.Contains(p.FrequencyId))
                    .ToListAsync();

                foreach (var frequency in frequencies)
                {
                    var frequencyResult = await GeneratePeriodsForFrequency(reportingYear, frequency, existingPeriods, createdBy);
                    result.FrequencyResults.Add(frequencyResult);
                    result.TotalPeriodsCreated += frequencyResult.PeriodsCreated;
                    result.TotalPeriodsSkipped += frequencyResult.PeriodsSkipped;
                }

                await _context.SaveChangesAsync();
                result.Message = $"Successfully generated {result.TotalPeriodsCreated} periods. Skipped {result.TotalPeriodsSkipped} existing periods.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating periods for year {YearId}", yearId);
                result.Success = false;
                result.Message = $"Error generating periods: {ex.Message}";
            }

            return result;
        }

        private FrequencyPreviewDto GenerateFrequencyPreview(ReportingYear year, FrequencyCatalog frequency, List<ReturnPeriods> existingPeriods)
        {
            var preview = new FrequencyPreviewDto
            {
                FrequencyId = frequency.Id,
                FrequencyCode = frequency.Code,
                FrequencyName = frequency.Name
            };

            var periods = GeneratePeriodList(year, frequency);
            var existingForFrequency = existingPeriods.Where(p => p.FrequencyId == frequency.Id).ToList();

            foreach (var period in periods)
            {
                var exists = existingForFrequency.Any(p => 
                    p.StartDate.Date == period.StartDate.Date && 
                    p.EndDate.Date == period.EndDate.Date);

                preview.Periods.Add(new PeriodPreviewDto
                {
                    SequenceNo = period.SequenceNo,
                    Name = period.Name,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    FilingDeadline = period.FilingDeadline,
                    AlreadyExists = exists
                });

                if (exists)
                    preview.ExistingPeriodsCount++;
                else
                    preview.NewPeriodsCount++;
            }

            return preview;
        }

        private async Task<FrequencyResultDto> GeneratePeriodsForFrequency(
            ReportingYear year, 
            FrequencyCatalog frequency, 
            List<ReturnPeriods> existingPeriods,
            string createdBy)
        {
            var result = new FrequencyResultDto
            {
                FrequencyId = frequency.Id,
                FrequencyName = frequency.Name
            };

            var periods = GeneratePeriodList(year, frequency);
            var existingForFrequency = existingPeriods.Where(p => p.FrequencyId == frequency.Id).ToList();

            foreach (var period in periods)
            {
                // Check if period already exists (idempotency)
                var exists = existingForFrequency.Any(p => 
                    p.StartDate.Date == period.StartDate.Date && 
                    p.EndDate.Date == period.EndDate.Date);

                if (!exists)
                {
                    period.CreatedBy = createdBy;
                    period.CreatedAt = DateTime.UtcNow;
                    _context.ReturnPeriods.Add(period);
                    result.PeriodsCreated++;
                    result.CreatedPeriodNames.Add(period.Name);
                }
                else
                {
                    result.PeriodsSkipped++;
                }
            }

            return result;
        }

        private List<ReturnPeriods> GeneratePeriodList(ReportingYear year, FrequencyCatalog frequency)
        {
            var periods = new List<ReturnPeriods>();
            var currentDate = year.StartDate;
            int sequenceNo = 1;

            while (currentDate < year.EndDateDate)
            {
                var period = new ReturnPeriods
                {
                    Id = Guid.NewGuid().ToString(),
                    YearId = year.Id,
                    FrequencyId = frequency.Id,
                    SequenceNo = sequenceNo,
                    StartDate = currentDate
                };

                // Calculate end date based on label strategy
                switch (frequency.LabelStrategy.ToUpperInvariant())
                {
                    case "DATE":
                        period.EndDate = currentDate.AddDays(frequency.IntervalDays - 1);
                        period.Name = $"{currentDate:yyyy-MM-dd} to {period.EndDate:yyyy-MM-dd}";
                        break;

                    case "ISO_WEEK":
                        var isoWeek = GetIsoWeekOfYear(currentDate);
                        period.EndDate = currentDate.AddDays(6);
                        period.Name = $"Week {isoWeek} {currentDate.Year}";
                        break;

                    case "BI_WEEK":
                        period.EndDate = currentDate.AddDays(13);
                        var biWeekNum = (GetIsoWeekOfYear(currentDate) + 1) / 2;
                        period.Name = $"Bi-Week {biWeekNum} {currentDate.Year}";
                        break;

                    case "MONTH":
                        period.EndDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1).AddDays(-1);
                        period.Name = $"{currentDate:MMMM yyyy}";
                        break;

                    case "QUARTER":
                        var quarter = (currentDate.Month - 1) / 3 + 1;
                        var quarterStart = new DateTime(currentDate.Year, (quarter - 1) * 3 + 1, 1);
                        period.EndDate = quarterStart.AddMonths(3).AddDays(-1);
                        period.Name = $"Q{quarter} {currentDate.Year}";
                        break;

                    case "SEMI_ANNUAL":
                        var half = currentDate.Month <= 6 ? 1 : 2;
                        var halfStart = new DateTime(currentDate.Year, half == 1 ? 1 : 7, 1);
                        period.EndDate = halfStart.AddMonths(6).AddDays(-1);
                        period.Name = $"H{half} {currentDate.Year}";
                        break;

                    case "YEAR":
                        period.EndDate = new DateTime(currentDate.Year, 12, 31);
                        period.Name = $"FY {currentDate.Year}";
                        break;

                    default:
                        // Default to interval days
                        period.EndDate = currentDate.AddDays(frequency.IntervalDays - 1);
                        period.Name = $"Period {sequenceNo} {currentDate.Year}";
                        break;
                }

                // Ensure end date doesn't exceed year end date
                if (period.EndDate > year.EndDateDate)
                {
                    period.EndDate = year.EndDateDate;
                }

                // Calculate filing deadline
                period.FilingDeadline = period.EndDate.AddDays(frequency.DefaultDeadlineOffset);

                periods.Add(period);

                // Move to next period
                currentDate = period.EndDate.AddDays(1);
                sequenceNo++;

                // Break if we've reached the end of the year
                if (currentDate > year.EndDateDate)
                {
                    break;
                }
            }

            return periods;
        }

        private int GetIsoWeekOfYear(DateTime date)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;
            var day = cal.GetDayOfWeek(date);
            
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}