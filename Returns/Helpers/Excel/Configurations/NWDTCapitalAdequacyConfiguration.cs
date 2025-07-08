using ClosedXML.Excel;
using Returns.DTOs.Returns.Returns_Submission.NWDT;
using Returns.Interfaces;

namespace Returns.Helpers.Excel.Configurations
{
    // Configuration for NWDT Capital Adequacy Form
    public class NWDTCapitalAdequacyConfiguration : BaseFormConfiguration<NWDTCapitalAdequacyDTO>
    {
        public override string FormType => "NWDT_CAPITAL_ADEQUACY";
        public override string SheetName => "NWDT Capital Adequacy"; 
        public override int DataStartRow => 8;
        public override int DataEndRow => 45;
        public override IRowMapper<NWDTCapitalAdequacyDTO> RowMapper => new NWDTCapitalAdequacyRowMapper();

        // NWDT forms might have different metadata locations
        public override FormMetadataLocation MetadataLocations => new FormMetadataLocation
        {
            SaccoCsNumberCell = "E3",
            PeriodCell = "E4",
            StartDateCell = "E5",
            EndDateCell = "G5"
        };
    }

    // Row mapper for NWDT Capital Adequacy
    public class NWDTCapitalAdequacyRowMapper : IRowMapper<NWDTCapitalAdequacyDTO>
    {
        // Define the specific rows for NWDT form structure
        private static readonly Dictionary<int, Action<NWDTCapitalAdequacyDTO, IXLRow>> RowMappings = new()
        {
            { 8, (dto, row) => dto.ShareCapital = ParseDecimal(row.Cell("E")) },
            { 9, (dto, row) => dto.MemberDeposits = ParseDecimal(row.Cell("E")) },
            { 10, (dto, row) => dto.InstitutionalCapital = ParseDecimal(row.Cell("E")) },
            { 11, (dto, row) => dto.RetainedEarnings = ParseDecimal(row.Cell("E")) },
            { 12, (dto, row) => dto.Grants = ParseDecimal(row.Cell("E")) },
            { 13, (dto, row) => dto.OtherReserves = ParseDecimal(row.Cell("E")) },
            { 14, (dto, row) => dto.TotalCoreCapital = ParseDecimal(row.Cell("E")) },
            
            // Risk-weighted assets section
            { 20, (dto, row) => dto.CashAndCashEquivalents = ParseDecimal(row.Cell("E")) },
            { 21, (dto, row) => dto.GovernmentSecurities = ParseDecimal(row.Cell("E")) },
            { 22, (dto, row) => dto.LoansFullySecured = ParseDecimal(row.Cell("E")) },
            { 23, (dto, row) => dto.LoansPartiallySecured = ParseDecimal(row.Cell("E")) },
            { 24, (dto, row) => dto.LoansUnsecured = ParseDecimal(row.Cell("E")) },
            
            // Calculations
            { 40, (dto, row) => dto.TotalRiskWeightedAssets = ParseDecimal(row.Cell("E")) },
            { 41, (dto, row) => dto.CoreCapitalRatio = ParseDecimal(row.Cell("E")) },
            { 42, (dto, row) => dto.MinimumCoreCapitalRequired = ParseDecimal(row.Cell("E")) },
            { 43, (dto, row) => dto.ExcessDeficitCoreCapital = ParseDecimal(row.Cell("E")) }
        };

        public NWDTCapitalAdequacyDTO MapRow(IXLRow row, int rowNumber)
        {
            var dto = new NWDTCapitalAdequacyDTO();
            
            if (RowMappings.TryGetValue(rowNumber, out var mapping))
            {
                mapping(dto, row);
            }
            
            return dto;
        }

        public bool ShouldSkipRow(IXLRow row, int rowNumber)
        {
            // Skip rows not in our mapping and empty rows
            if (!RowMappings.ContainsKey(rowNumber))
                return true;
                
            // Check if the value cell is empty
            var valueCell = row.Cell("E").Value.ToString();
            return string.IsNullOrWhiteSpace(valueCell) && !valueCell.Equals("0");
        }

        private static decimal ParseDecimal(IXLCell cell)
        {
            if (cell == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                return 0m;

            var value = cell.Value.ToString()
                .Replace(",", "")
                .Replace("%", "")
                .Trim();

            if (decimal.TryParse(value, out var result))
                return result;

            return 0m;
        }
    }
}