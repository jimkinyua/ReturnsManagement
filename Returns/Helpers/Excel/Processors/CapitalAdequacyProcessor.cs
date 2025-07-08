using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.Helpers.Excel.Configurations;
using Returns.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers.Excel.Processors
{
    public class CapitalAdequacyProcessor : BaseFormProcessor<CapitalAdequacyConfiguration, CapitalAdequacyStatement>
    {
        public override string FormType => "CAPITAL_ADEQUACY";

        public CapitalAdequacyProcessor(
            ReturnsDbContext context,
            ILogger<CapitalAdequacyProcessor> logger,
            IExcelImportService excelService) 
            : base(context, logger, excelService)
        {
        }

        protected override async Task SaveStatementAsync(
            CapitalAdequacyStatement statement, 
            string returnId, 
            string filePath,
            ReturnForm form)
        {
            var returnRecord = await _context.Returns.FindAsync(returnId);
            if (returnRecord == null)
            {
                throw new InvalidOperationException($"Return {returnId} not found");
            }

            var entity = new DTCapitalAdequacyReturn
            {
                ReturnId = returnId,
                FormId = form.Id,
                
                // Map key values from specific row indices
                ShareCapital = statement.Rows.FirstOrDefault(r => r.Index == "1.1")?.Amount ?? 0,
                StatutoryReserves = statement.Rows.FirstOrDefault(r => r.Index == "1.2")?.Amount ?? 0,
                RetainedEarningsAccumulatedLosses = statement.Rows.FirstOrDefault(r => r.Index == "1.3")?.Amount ?? 0,
                NetSurplusAfterTaxCurrentYearToDate = statement.Rows.FirstOrDefault(r => r.Index == "1.4")?.Amount ?? 0,
                CapitalGrantsEquityInNature = statement.Rows.FirstOrDefault(r => r.Index == "1.5")?.Amount ?? 0,
                GeneralReserves = statement.Rows.FirstOrDefault(r => r.Index == "1.6")?.Amount ?? 0,
                OtherReserves = statement.Rows.FirstOrDefault(r => r.Index == "1.7")?.Amount ?? 0,
                SubTotalCoreCapital = statement.Rows.FirstOrDefault(r => r.Index == "1.8")?.Amount ?? 0,
                
                // Deductions
                InvestmentsInSubsidiaryAndEquityInstruments = statement.Rows.FirstOrDefault(r => r.Index == "2.1")?.Amount ?? 0,
                OtherDeductions = statement.Rows.FirstOrDefault(r => r.Index == "2.2")?.Amount ?? 0,
                TotalDeductions = statement.Rows.FirstOrDefault(r => r.Index == "2.3")?.Amount ?? 0,
                
                // Computed values
                CoreCapital = statement.CoreCapital,
                TotalAssets = statement.TotalAssets,
                CoreCapitalToAssetsRatio = statement.CoreCapitalToAssetsRatio,
                
                // Metadata
                Year = statement.Period,
                StartDate = statement.StartDate,
                EndDate = statement.EndDate,
                Frequency = form.Period.Name,
                FilePath = filePath,
                DaysLateBy = CalculateDaysLate(form, DateTime.Now),
                
                CreatedAt = DateTime.Now,
                SaccoName = returnRecord.SaccoName,
                SaccoCsNumber = returnRecord.SaccoId
            };

            _context.DTCapitalAdequacyReturns.Add(entity);
            await _context.SaveChangesAsync();
        }

        protected override async Task<bool> CheckDuplicateSubmissionAsync(string returnId, DateTime endDate)
        {
            return await _context.DTCapitalAdequacyReturns
                .AnyAsync(c => c.ReturnId == returnId && c.EndDate.Date == endDate.Date);
        }
    }
}