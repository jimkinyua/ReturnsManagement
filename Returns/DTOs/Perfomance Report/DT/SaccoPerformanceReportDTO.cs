using Returns.Models;
namespace Returns.DTOs.Perfomance_Report
{
    public class SaccoPerformanceReportDTO
    {
        public string SaccoName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public List<PeriodData> Periods { get; set; } = new List<PeriodData>();
        public List<ApprovalAction> approvalActions { get; set; } = new List<ApprovalAction>();
        public class PeriodData
        {
            // Basic info
            public string PeriodLabel { get; set; }
            public string PeriodType { get; set; }
            public string PeriodEndDate { get; set; }
            public string PeriodStartDate { get; set; }
            public decimal CoreCapital { get; set; }
            // A. Capital Adequacy - Ratios that need multiplication
            public decimal CoreCapitalToTotalAssets { get; set; }
            public decimal InstitutionalCapitalToTotalAssets { get; set; }
            // B. Asset Quality - Ratios that need multiplication
            public decimal NPL { get; set; }
            public decimal NonEarningAssets { get; set; }
            public decimal EquityInvestmentsToDeposits { get; set; }
            public decimal EquityInvestmentsToCoreCapital { get; set; }
            // C. Earnings Rating - Ratios that need multiplication
            public decimal YieldOnGrossLoans { get; set; }
            public decimal TotalExpenseToTotalIncome { get; set; }
            public decimal NetIncomeToAverageAssets { get; set; }
            public decimal OpertatingExpenseToFinancialOpex { get; set; }
            public decimal ROA { get; set; }
            public decimal OPEX { get; set; }
            // D. Liquidity - Ratios that need multiplication
            public decimal LiquidAssetsToShortTermLiabilities { get; set; }
            public decimal ExternalBorrowingToTotalAssets { get; set; }
            public decimal LiquidAssetsToTotalAssets { get; set; }
            // E. Structure/Sensitivity to Risk - Ratios that need multiplication
            public decimal GrossLoansToTotalAssets { get; set; }
            public decimal GrossLoansToDeposits { get; set; }
            public decimal FinancialInvestmentsToTotalAssets { get; set; }
            public decimal DividendsAndInterestOnDepositsToTotalIncome { get; set; }
            // F. Key Financial Statistics - Monetary values (no multiplication needed)
            public decimal TotalAssets { get; set; }
            public decimal AverageAssets { get; set; }
            public decimal TotalDeposits { get; set; }
            public decimal GrossLoansForm4 { get; set; }
            public decimal GrossLoansForm6 { get; set; }
            public decimal InstitutionalCapital { get; set; }
            public decimal NonPerformingLoans { get; set; }
            public decimal PropertyAndEquipment { get; set; }
            public decimal EquityInvestments { get; set; }
            public decimal FinancialInvestments { get; set; }
            public decimal EquityInvestmentsInNACCOS { get; set; }
            public decimal LiquidAssets { get; set; }
            public decimal ShortTermLiabilities { get; set; }
            public decimal ExternalBorrowing { get; set; }
            public decimal AverageGrossLoans { get; set; }
            public decimal TotalIncome { get; set; }
            public decimal NetFinancialIncome { get; set; }
            public decimal DividendsAndInterestOnDeposits { get; set; }
            public decimal OperatingExpenses { get; set; }
            public decimal InterestOnLoanPortfolioAndFeesCommission { get; set; }
            public decimal TotalExpenses { get; set; }
            public decimal NetIncome { get; set; }
            public int ManagementScore { get; set; }
            public decimal MemberProtectionScore { get; set; }
            public decimal ComplianceWithLawsScore { get; set; }
            public decimal InternalControlsScore { get; set; }
            public decimal GovernanceStructureScore { get; set; }
            public decimal CoreCapitalToTotalDepositsRatio { get; internal set; }
        }
        public Dictionary<string, string> PrudentialStandards { get; set; } = new Dictionary<string, string>();
    }
}