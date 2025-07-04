using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DTInvestmentReturn : FormBase
    {
        public decimal CoreCapital { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal NonEarningAssets { get; set; }
        public decimal FinancialInvestments { get; set; }
        public decimal LandAndBuildings { get; set; }

        // 2. Land & Buildings Ratios
        public decimal LandBuildingsToTotalAssetsRatio { get; set; }
        public decimal MaxLandBuildingsToTotalAssetsRatio { get; set; } = 5; // 5%
        public decimal LandBuildingsRatioExcessDeficiency { get; set; }

        // 3. Financial Investment to Core Capital Ratios
        public decimal FinancialInvestmentsToCoreCapitalRatio { get; set; }
        public decimal MaxFinancialInvestmentsToCoreCapitalRatio { get; set; } = 40; // 40%
        public decimal FinancialInvestmentsToCoreCapitalExcessDeficiency { get; set; }

        // 4. Financial Investment to Deposits Ratios
        public decimal FinancialInvestmentsToDepositsRatio { get; set; }
        public decimal MaxFinancialInvestmentsToDepositsRatio { get; set; } = 5; // 5%
        public decimal FinancialInvestmentsToDepositsExcessDeficiency { get; set; }

        // 5. Non-Earning Assets Ratios
        public decimal NonEarningAssetsToTotalAssetsRatio { get; set; }
        public decimal MaxNonEarningAssetsToTotalAssetsRatio { get; set; } = 10; // 10%
        public decimal NonEarningAssetsRatioExcessDeficiency { get; set; }
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        public string Year { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy { get; set; }
        
        public Guid ReturnSubmissionId { get; set; }           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
