using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.VisualBasic;

namespace Returns.DTOs.Perfomance_Report.NWDT
{
    public class NWDTPerformanceReportDTO
    {
        public string SaccoName { get; set; }
        public DateTime ReportDate { get; set; }
        public List<NWDTPeriodData> Periods { get; set; } = new List<NWDTPeriodData>();

        public class NWDTPeriodData
        {
            // Period Information
            public string PeriodLabel { get; set; }
            public string PeriodType { get; set; }
            public DateTime PeriodDate { get; set; }

            // Capital Metrics
            public decimal CoreCapital { get; set; }
            public decimal CoreCapitalToTotalAssetsRatio { get; set; }
            public decimal CoreCapitalToTotalDepositsRatio { get; set; }

            // Asset Quality
            public decimal NPL { get; set; }
            public decimal NonEarningAssets { get; set; }
            public decimal TotalFinancialInvestmentToCoreCapitalRatio { get; set; }
            public decimal SubsidiaryAndRelatedInvestmentToCoreCapitalRatio { get; set; }
            public decimal EquityInvestmentsToCoreCapitalRatio { get; set; }
            public decimal EquityInvestmentsToDeposits { get; set; }
            public decimal OtherFinancialInvestmentsToCoreCapitalRatio { get; set; }

            // Earnings
            public decimal YieldOnGrossLoans { get; set; }
            public decimal TotalExpenseToTotalIncomeRatio { get; set; }
            public decimal NetIncomeToAverageAssetsRatio { get; set; }
            public decimal OperatingExpenseToFinancialIncomeRatio { get; set; }

            // Liquidity
            public decimal LiquidAssetsToShortTermLiabilitiesRatio { get; set; }
            public decimal ExternalBorrowingToTotalAssetsRatio { get; set; }
            public decimal LiquidAssetsToTotalAssetsRatio { get; set; }

            // Stucture of Assets
            public decimal FinancialInvestmentsToTotalAssetsRatio { get; set; }

            // Key Financial Statistics
            public decimal TotalAssets { get; set; }
            public decimal AverageAssets { get; set; }
            public decimal TotalDeposits { get; set; }
            public decimal GrossLoansForm4 { get; set; }
            public decimal GrossLoansForm6 { get; set; }
            public decimal NonPerformingLoans { get; set; }
            public decimal PropertyAndEquipment { get; set; }
            public decimal EquityInvestments { get; set; }
            public decimal FinancialInvestments { get; set; }
            public decimal EquityInvestmentsInNaccos { get; set; }
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
        }
        public Dictionary<string, string> PrudentialStandards { get; set; } = new Dictionary<string, string>();
    }
}
