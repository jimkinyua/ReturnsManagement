using ClosedXML.Excel;
using Returns.Interfaces;
using Returns.DTOs.Returns.Returns_Submission.DT;

namespace Returns.Helpers.Excel.Configurations
{
    // Configuration for Deposit Return Form
    public class DepositReturnConfiguration : BaseFormConfiguration<DepositReturnDto>
    {
        public override string FormType => "DEPOSIT_RETURN";
        public override int DataStartRow => 6;
        public override int DataEndRow => 20;
        public override IRowMapper<DepositReturnDto> RowMapper => new DepositReturnRowMapper();

        // Override metadata locations if they differ from defaults
        public override FormMetadataLocation MetadataLocations => new FormMetadataLocation
        {
            SaccoCsNumberCell = "D3",
            PeriodCell = "D4",
            StartDateCell = "D5",
            EndDateCell = "F5"
        };
    }

    // Row mapper for Deposit Return
    public class DepositReturnRowMapper : IRowMapper<DepositReturnDto>
    {
        private static readonly Dictionary<int, string> RowDescriptions = new()
        {
            { 6, "Members Deposits" },
            { 7, "Deposit Accounts With Banks" },
            { 8, "Fixed Deposits With Banks" },
            { 9, "Cash" },
            { 10, "Total Liquid Assets" },
            { 11, "Loans To Members" },
            { 12, "External Borrowings" },
            { 13, "Total Illiquid Assets" },
            { 14, "Total Assets" },
            { 15, "Withdrawable Deposits" },
            { 16, "Non-Withdrawable Deposits" },
            { 17, "Total Deposits" },
            { 18, "Liquidity Ratio" },
            { 19, "Core Capital" },
            { 20, "Total Assets" }
        };

        public DepositReturnDto MapRow(IXLRow row, int rowNumber)
        {
            // Initialize DTO
            var dto = new DepositReturnDto();

            // Map based on row number
            if (RowDescriptions.TryGetValue(rowNumber, out var description))
            {
                switch (rowNumber)
                {
                    case 6:
                        dto.MembersDeposits = ParseDecimal(row.Cell("D"));
                        break;
                    case 7:
                        dto.DepositAccountsWithBanks = ParseDecimal(row.Cell("D"));
                        break;
                    case 8:
                        dto.FixedDepositsWithBanks = ParseDecimal(row.Cell("D"));
                        break;
                    case 9:
                        dto.Cash = ParseDecimal(row.Cell("D"));
                        break;
                    case 10:
                        dto.TotalLiquidAssets = ParseDecimal(row.Cell("D"));
                        break;
                    case 11:
                        dto.LoansToMembers = ParseDecimal(row.Cell("D"));
                        break;
                    case 12:
                        dto.ExternalBorrowings = ParseDecimal(row.Cell("D"));
                        break;
                    case 13:
                        dto.TotalIlliquidAssets = ParseDecimal(row.Cell("D"));
                        break;
                    case 14:
                        dto.TotalAssets = ParseDecimal(row.Cell("D"));
                        break;
                    case 15:
                        dto.WithdrawableDeposits = ParseDecimal(row.Cell("D"));
                        break;
                    case 16:
                        dto.NonWithdrawableDeposits = ParseDecimal(row.Cell("D"));
                        break;
                    case 17:
                        dto.TotalDeposits = ParseDecimal(row.Cell("D"));
                        break;
                    case 18:
                        dto.LiquidityRatio = ParseDecimal(row.Cell("D"));
                        break;
                    case 19:
                        dto.CoreCapital = ParseDecimal(row.Cell("D"));
                        break;
                    case 20:
                        dto.TotalAssetsEnd = ParseDecimal(row.Cell("D"));
                        break;
                }
            }

            return dto;
        }

        public bool ShouldSkipRow(IXLRow row, int rowNumber)
        {
            // Skip rows not in our mapping
            return !RowDescriptions.ContainsKey(rowNumber);
        }

        private decimal ParseDecimal(IXLCell cell)
        {
            if (cell == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                return 0m;

            if (decimal.TryParse(cell.Value.ToString(), out var result))
                return result;

            return 0m;
        }
    }
}