using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class InvestmentReturnDTO: CommonFormDTO
    {
        public decimal CoreCapital { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal NonEarningAssets { get; set; }
        public decimal FinancialInvestments { get; set; }
        public decimal LandAndBuildings { get; set; }

        // Ratios
        public decimal LandBuildingsToTotalAssetsRatio { get; set; }
        public decimal LandBuildingsRatioExcessDeficiency { get; set; }
        public decimal NonEarningAssetsToTotalAssetsRatio { get; set; }
        public decimal NonEarningAssetsRatioExcessDeficiency { get; set; }
        public decimal FinancialInvestmentsToCoreCapitalRatio { get; set; }
        public decimal FinancialInvestmentsToCoreCapitalExcessDeficiency { get; set; }
        public decimal FinancialInvestmentsToDepositsRatio { get; set; }
        public decimal FinancialInvestmentsToDepositsExcessDeficiency { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public decimal MaxNonEarningAssetsToTotalAssetsRatio { get; internal set; }
    }
}
