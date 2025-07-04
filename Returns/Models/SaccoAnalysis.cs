using Returns.Models.Common;

namespace Returns.Models
{
    public class SaccoAnalysis : CommonFields
    {
        // Capital Analysis (C)
        public decimal CoreCapital { get; set; }  // Actual amount
        public decimal CoreCapitalToAssetsRatio { get; set; }  // Core Capital / Total Assets
        public decimal InstitutionalCapitalRatio { get; set; }  // Institutional Capital / Total Assets
        public int CapitalRating { get; set; }  // Final 1-5 rating for Capital

        // Asset Quality Analysis (A)
        public decimal NonPerformingLoans { get; set; }  // Bad loans amount
        public decimal TotalLoans { get; set; }  // Total loans amount
        public decimal NonPerformingLoanRatio { get; set; }  // NPL/Total Loans
        public int AssetQualityRating { get; set; }  // Final 1-5 rating for Assets

        // Management Analysis (M)
        public decimal GovernanceScore { get; set; }
        public decimal ControlsScore { get; set; }
        public decimal ComplianceScore { get; set; }
        public int ManagementRating { get; set; }  // Final 1-5 rating for Management

        // Earnings Analysis (E)
        public decimal NetIncome { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal ReturnOnAssets { get; set; }  // Net Income / Total Assets
        public int EarningsRating { get; set; }  // Final 1-5 rating for Earnings

        // Liquidity Analysis (L)
        public decimal LiquidAssets { get; set; }  // Cash and near-cash
        public decimal TotalDeposits { get; set; }  // Customer deposits
        public decimal LiquidityRatio { get; set; }  // Liquid Assets / Deposits
        public int LiquidityRating { get; set; }  // Final 1-5 rating for Liquidity

        // Final Results
        public int OverallRating { get; set; }  // 1-5 final CAMELS rating
        public string RiskLevel { get; set; }  // "Low", "Medium", or "High"
        public string ActionRequired { get; set; }  // What needs to be done

        public DateTime AnalysisDate { get; set; }
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        
        public Guid ReturnSubmissionId { get; set; }           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
