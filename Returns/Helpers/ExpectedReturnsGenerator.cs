using Microsoft.EntityFrameworkCore;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class ExpectedReturnsGenerator
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<ExpectedReturnsGenerator> _logger;

        public ExpectedReturnsGenerator(
            ReturnsDbContext context,
            ILogger<ExpectedReturnsGenerator> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Generate expected returns for all SACCOs for a specific year
        /// </summary>
        public async Task<int> GenerateExpectedReturnsForYearAsync(int year)
        {
            try
            {
                // Get all periods for the year
                var periods = await _context.ReturnPeriods
                    .Include(p => p.FrequencyCatalog)
                    .Where(p => p.ReportingYear.Year == year)
                    .ToListAsync();

                // Get all active forms grouped by frequency
                var formsByFrequency = await _context.ReturnForms
                    .Where(f => f.IsActive)
                    .GroupBy(f => f.Frequency)
                    .ToListAsync();

                // Get all SACCOs (this would normally come from a SACCO service)
                // For now, we'll get unique SACCOs from existing returns
                var saccos = await _context.Returns
                    .Select(r => new { r.SaccoId, r.SaccoType })
                    .Distinct()
                    .ToListAsync();

                int generatedCount = 0;

                foreach (var sacco in saccos)
                {
                    foreach (var period in periods)
                    {
                        // Get forms for this period's frequency
                        var formsForFrequency = formsByFrequency
                            .FirstOrDefault(g => g.Key == period.FrequencyCatalog.Code)?
                            .ToList() ?? new List<ReturnForm>();

                        // Also get forms that apply to all frequencies
                        var universalForms = formsByFrequency
                            .FirstOrDefault(g => g.Key == "ALL")?
                            .ToList() ?? new List<ReturnForm>();

                        var allForms = formsForFrequency.Concat(universalForms).Distinct();

                        foreach (var form in allForms)
                        {
                            // Check if expected return already exists
                            var exists = await _context.ExpectedReturns
                                .AnyAsync(e => e.SaccoId == sacco.SaccoId 
                                    && e.PeriodId == period.Id 
                                    && e.FormId == form.Id);

                            if (!exists)
                            {
                                var expectedReturn = new ExpectedReturn
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    SaccoId = sacco.SaccoId,
                                    SaccoType = sacco.SaccoType,
                                    PeriodId = period.Id,
                                    FormId = form.Id,
                                    DueDate = period.GetDueDate()
                                };

                                _context.ExpectedReturns.Add(expectedReturn);
                                generatedCount++;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Generated {Count} expected returns for year {Year}",
                    generatedCount, year);

                return generatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating expected returns for year {Year}", year);
                throw;
            }
        }

        /// <summary>
        /// Generate expected returns for a specific SACCO
        /// </summary>
        public async Task<int> GenerateExpectedReturnsForSaccoAsync(
            string saccoId, 
            string saccoType, 
            int year)
        {
            try
            {
                // Get all periods for the year
                var periods = await _context.ReturnPeriods
                    .Include(p => p.FrequencyCatalog)
                    .Where(p => p.ReportingYear.Year == year)
                    .ToListAsync();

                // Get all active forms
                var forms = await _context.ReturnForms
                    .Where(f => f.IsActive)
                    .ToListAsync();

                int generatedCount = 0;

                foreach (var period in periods)
                {
                    // Filter forms by frequency
                    var formsForPeriod = forms
                        .Where(f => f.Frequency == period.FrequencyCatalog.Code 
                                 || f.Frequency == "ALL")
                        .ToList();

                    foreach (var form in formsForPeriod)
                    {
                        // Check if expected return already exists
                        var exists = await _context.ExpectedReturns
                            .AnyAsync(e => e.SaccoId == saccoId 
                                && e.PeriodId == period.Id 
                                && e.FormId == form.Id);

                        if (!exists)
                        {
                            var expectedReturn = new ExpectedReturn
                            {
                                Id = Guid.NewGuid().ToString(),
                                SaccoId = saccoId,
                                SaccoType = saccoType,
                                PeriodId = period.Id,
                                FormId = form.Id,
                                DueDate = period.GetDueDate()
                            };

                            _context.ExpectedReturns.Add(expectedReturn);
                            generatedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Generated {Count} expected returns for SACCO {SaccoId} in year {Year}",
                    generatedCount, saccoId, year);

                return generatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error generating expected returns for SACCO {SaccoId}", saccoId);
                throw;
            }
        }

        /// <summary>
        /// Clean up duplicate or invalid expected returns
        /// </summary>
        public async Task<int> CleanupExpectedReturnsAsync()
        {
            try
            {
                // Remove expected returns where the return has already been filed
                var filedReturns = await _context.Returns
                    .Where(r => r.IsActiveVersion)
                    .Select(r => new { r.SaccoId, r.PeriodId })
                    .ToListAsync();

                var toRemove = new List<ExpectedReturn>();

                foreach (var filed in filedReturns)
                {
                    var expectedToRemove = await _context.ExpectedReturns
                        .Where(e => e.SaccoId == filed.SaccoId 
                                 && e.PeriodId == filed.PeriodId
                                 && !e.IsWaived)
                        .ToListAsync();

                    toRemove.AddRange(expectedToRemove);
                }

                _context.ExpectedReturns.RemoveRange(toRemove);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} expected returns that were already filed",
                    toRemove.Count);

                return toRemove.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expected returns");
                throw;
            }
        }
    }
}