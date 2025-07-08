using ClosedXML.Excel;
using Returns.Interfaces;

namespace Returns.Helpers.Excel.Configurations
{
    // Configuration for Liquidity Statement Form
    public class LiquidityStatementConfiguration : BaseFormConfiguration<LiquidityStatement>
    {
        public override string FormType => "LIQUIDITY_STATEMENT";
        public override int DataStartRow => 7;
        public override int DataEndRow => 52;
        public override IRowMapper<LiquidityStatement> RowMapper => new LiquidityStatementRowMapper();
    }

    // The strongly-typed DTO
    public class LiquidityStatement : BaseFormStatement, IFormWithRows<LiquidityStatementRow>
    {
        public List<LiquidityStatementRow> Rows { get; set; } = new List<LiquidityStatementRow>();

        public void SetRows(List<object> rows)
        {
            Rows = rows.Cast<LiquidityStatementRow>().ToList();
        }

        // Computed properties
        public decimal TotalNotesAndCoins => GetSectionTotal("1");
        public decimal NetBankBalances => GetSectionTotal("2");
        public decimal NetFinancialInstitutionBalances => GetSectionTotal("3");
        public decimal TotalGovernmentSecurities => GetSectionTotal("4");
        public decimal NetLiquidAssets => GetItemAmount("5");
        public decimal LiquidityRatio => GetItemAmount("7.3");

        private decimal GetSectionTotal(string sectionId)
        {
            return Rows.FirstOrDefault(r => r.Index == sectionId && !string.IsNullOrEmpty(r.Index))?.Amount ?? 0;
        }

        private decimal GetItemAmount(string itemId)
        {
            return Rows.FirstOrDefault(r => r.Index == itemId)?.Amount ?? 0;
        }
    }

    public class LiquidityStatementRow
    {
        public string Index { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public string CellReference { get; set; }
    }

    // Row mapper for Liquidity Statement
    public class LiquidityStatementRowMapper : IRowMapper<LiquidityStatement>
    {
        public LiquidityStatementRow MapRow(IXLRow row, int rowNumber)
        {
            return new LiquidityStatementRow
            {
                Index = ExcelImportService.GetCellValueOrEmpty(row.Cell(2)),         // Column B
                Description = ExcelImportService.GetCellValueOrEmpty(row.Cell(3)),   // Column C
                Amount = ExcelImportService.GetDecimalOrNull(row.Cell(4)),          // Column D
                CellReference = row.Cell(4)?.Address.ToString() ?? string.Empty
            };
        }

        public bool ShouldSkipRow(IXLRow row)
        {
            string index = ExcelImportService.GetCellValueOrEmpty(row.Cell(2));
            string description = ExcelImportService.GetCellValueOrEmpty(row.Cell(3));

            // Include rows that have either index or description
            return string.IsNullOrWhiteSpace(index) && string.IsNullOrWhiteSpace(description);
        }

        LiquidityStatement IRowMapper<LiquidityStatement>.MapRow(IXLRow row, int rowNumber)
        {
            throw new NotImplementedException("Use MapRow without generic parameter");
        }
    }
}