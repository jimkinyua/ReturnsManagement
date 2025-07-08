using ClosedXML.Excel;
using Returns.Helpers.Excel.Mappers;
using Returns.Interfaces;

namespace Returns.Helpers.Excel.Configurations
{
    // Configuration for Capital Adequacy Form
    public class CapitalAdequacyConfiguration : BaseFormConfiguration<CapitalAdequacyStatement>
    {
        public override string FormType => "CAPITAL_ADEQUACY";
        public override int DataStartRow => 10;
        public override int DataEndRow => 53;
        public override IRowMapper<CapitalAdequacyStatement> RowMapper => new CapitalAdequacyRowMapper();

        // Can override metadata locations if different
        public override FormMetadataLocation MetadataLocations => new FormMetadataLocation
        {
            SaccoCsNumberCell = "D3",
            PeriodCell = "D4", 
            StartDateCell = "D5",
            EndDateCell = "D6"
        };
    }

    // The strongly-typed DTO
    public class CapitalAdequacyStatement : BaseFormStatement, IFormWithRows<CapitalAdequacyRow>
    {
        public List<CapitalAdequacyRow> Rows { get; set; } = new List<CapitalAdequacyRow>();

        public void SetRows(List<object> rows)
        {
            Rows = rows.Cast<CapitalAdequacyRow>().ToList();
        }

        // Computed properties
        public decimal CoreCapital => GetRowAmount("1.8") ?? 0;
        public decimal TotalAssets => GetRowAmount("3.11") ?? 0;
        public decimal CoreCapitalToAssetsRatio => GetRowAmount("4.4") ?? 0;

        private decimal? GetRowAmount(string index)
        {
            return Rows.FirstOrDefault(r => r.Index == index)?.Amount;
        }
    }

    public class CapitalAdequacyRow
    {
        public string Index { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public string CellReference { get; set; }
    }

    // Row mapper for Capital Adequacy
    public class CapitalAdequacyRowMapper : IRowMapper<CapitalAdequacyStatement>
    {
        public CapitalAdequacyRow MapRow(IXLRow row, int rowNumber)
        {
            return new CapitalAdequacyRow
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

            // Skip empty rows
            if (string.IsNullOrWhiteSpace(index) && string.IsNullOrWhiteSpace(description))
                return true;

            // Skip section headers
            if (string.IsNullOrWhiteSpace(index) && 
                (description.StartsWith("LESS") || 
                 description == "OFF-BALANCE SHEET ASSETS" ||
                 description == "CAPITAL RATIO CALCULATIONS" || 
                 description == "CAPITAL COMPONENTS" ||
                 description == "ON - BALANCE SHEET ASSETS"))
                return true;

            return false;
        }

        CapitalAdequacyStatement IRowMapper<CapitalAdequacyStatement>.MapRow(IXLRow row, int rowNumber)
        {
            throw new NotImplementedException("Use MapRow without generic parameter");
        }
    }
}