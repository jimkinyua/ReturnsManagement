using Returns.DTOs.Returns.Return_Analysis_Result;
using SASRAXRBSS.Dto.Returns_Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Analysis
{
    public class CamelsRatingsDTO
    {
        public string PeriodId { get; set; } = string.Empty; 
        public string PeriodName { get; set; } = string.Empty; 
        public string SaccoId { get; set; } = string.Empty;   
        public string RatingDefinitionId { get; set; } = string.Empty;   

        // ─── headline ratings ─────────────────────────────────────────────────────
        public int CapitalRating { get; set; }
        public int AssetQualityRating { get; set; }
        public int ManagementRating { get; set; }
        public int EarningsRating { get; set; }
        public int LiquidityRating { get; set; }

        public int OverallRating { get; set; }
        public decimal Average { get; set; }  // mean of the five components
        public string RiskLevel { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;  

        // ─── drill‑down slices (current + 3 history) ───────────────────────────────
        public List<CapitalAnalysisResult> CapitalAnalysisResults { get; set; } = new();
        public List<ManagementRatingDetails> ManagementRatingResults { get; set; } = new();
        public List<AssetQualityRatingDetails> AssetQualityRatingResults { get; set; } = new();
        public List<EarningsRatingDetails> EarningsRatingResults { get; set; } = new();
        public List<LiquidityRatingDetails> LiquidityRatingResults { get; set; } = new();
        public List<StructureOfAssetsRatingDetails> StructureOfAssetsRatingResults { get; set; } = new();
    }
}
