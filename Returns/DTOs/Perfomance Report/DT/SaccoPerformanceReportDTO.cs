using Returns.Models;

namespace Returns.DTOs.Perfomance_Report
{
    public class SaccoPerformanceReportDTO
    {
        public string SaccoName { get; set; }
        public DateTime ReportDate { get; set; }
        public List<PeriodData> Periods { get; set; } = new List<PeriodData>();
        public List<ApprovalAction> approvalActions { get; set; } = new List<ApprovalAction>();

        public class PeriodData
        {
            // Basic info
            public string PeriodLabel { get; set; }
            public string PeriodType { get; set; }
            public DateTime PeriodDate { get; set; }

            // Monetary values (no multiplication needed)
            public decimal CoreCapital { get; set; }

            // A. Capital Adequacy - Ratios that need multiplication
            private decimal _coreCapitalToTotalAssets;
            public decimal CoreCapitalToTotalAssets
            {
                get { return _coreCapitalToTotalAssets; }
                set { _coreCapitalToTotalAssets = value * 100; }
            }

            private decimal _institutionalCapitalToTotalAssets;
            public decimal InstitutionalCapitalToTotalAssets
            {
                get { return _institutionalCapitalToTotalAssets; }
                set { _institutionalCapitalToTotalAssets = value * 100; }
            }

            // B. Asset Quality - Ratios that need multiplication
            private decimal _npl;
            public decimal NPL
            {
                get { return _npl; }
                set { _npl = value * 100; }
            }

            private decimal _nonEarningAssets;
            public decimal NonEarningAssets
            {
                get { return _nonEarningAssets; }
                set { _nonEarningAssets = value * 100; }
            }

            private decimal _equityInvestmentsToDeposits;
            public decimal EquityInvestmentsToDeposits
            {
                get { return _equityInvestmentsToDeposits; }
                set { _equityInvestmentsToDeposits = value * 100; }
            }

            private decimal _equityInvestmentsToCoreCapital;
            public decimal EquityInvestmentsToCoreCapital
            {
                get { return _equityInvestmentsToCoreCapital; }
                set { _equityInvestmentsToCoreCapital = value * 100; }
            }

            // C. Earnings Rating - Ratios that need multiplication
            private decimal _yieldOnGrossLoans;
            public decimal YieldOnGrossLoans
            {
                get { return _yieldOnGrossLoans; }
                set { _yieldOnGrossLoans = value * 100; }
            }

            private decimal _totalExpenseToTotalIncome;
            public decimal TotalExpenseToTotalIncome
            {
                get { return _totalExpenseToTotalIncome; }
                set { _totalExpenseToTotalIncome = value * 100; }
            }

            private decimal _netIncomeToAverageAssets;
            public decimal NetIncomeToAverageAssets
            {
                get { return _netIncomeToAverageAssets; }
                set { _netIncomeToAverageAssets = value * 100; }
            }

            private decimal _opertatingExpenseToFinancialOpex;
            public decimal OpertatingExpenseToFinancialOpex
            {
                get { return _opertatingExpenseToFinancialOpex; }
                set { _opertatingExpenseToFinancialOpex = value * 100; }
            }

            private decimal _roa;
            public decimal ROA
            {
                get { return _roa; }
                set { _roa = value * 100; }
            }

            private decimal _opex;
            public decimal OPEX
            {
                get { return _opex; }
                set { _opex = value * 100; }
            }

            // D. Liquidity - Ratios that need multiplication
            private decimal _liquidAssetsToShortTermLiabilities;
            public decimal LiquidAssetsToShortTermLiabilities
            {
                get { return _liquidAssetsToShortTermLiabilities; }
                set { _liquidAssetsToShortTermLiabilities = value * 100; }
            }

            private decimal _externalBorrowingToTotalAssets;
            public decimal ExternalBorrowingToTotalAssets
            {
                get { return _externalBorrowingToTotalAssets; }
                set { _externalBorrowingToTotalAssets = value * 100; }
            }

            private decimal _liquidAssetsToTotalAssets;
            public decimal LiquidAssetsToTotalAssets
            {
                get { return _liquidAssetsToTotalAssets; }
                set { _liquidAssetsToTotalAssets = value * 100; }
            }

            // E. Structure/Sensitivity to Risk - Ratios that need multiplication
            private decimal _grossLoansToTotalAssets;
            public decimal GrossLoansToTotalAssets
            {
                get { return _grossLoansToTotalAssets; }
                set { _grossLoansToTotalAssets = value * 100; }
            }

            private decimal _grossLoansToDeposits;
            public decimal GrossLoansToDeposits
            {
                get { return _grossLoansToDeposits; }
                set { _grossLoansToDeposits = value * 100; }
            }

            private decimal _financialInvestmentsToTotalAssets;
            public decimal FinancialInvestmentsToTotalAssets
            {
                get { return _financialInvestmentsToTotalAssets; }
                set { _financialInvestmentsToTotalAssets = value * 100; }
            }

            private decimal _dividendsAndInterestOnDepositsToTotalIncome;
            public decimal DividendsAndInterestOnDepositsToTotalIncome
            {
                get { return _dividendsAndInterestOnDepositsToTotalIncome; }
                set { _dividendsAndInterestOnDepositsToTotalIncome = value * 100; }
            }

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
        }

        public Dictionary<string, string> PrudentialStandards { get; set; } = new Dictionary<string, string>();
    }
}