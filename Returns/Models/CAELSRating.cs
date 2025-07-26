using Returns.DTOs.Compliance;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class CAELSRating : CommonFields
    {
        public string SaccoId { get; set; } = null!;

        [ForeignKey("ReturnPeriod")]
        public string PeriodId { get; set; } = null!;
        public ReturnPeriods ReturnPeriod { get; set; } = null!;

        [ForeignKey("RatingDefinition")]
        public string RatingDefinitionId { get; set; } = null!;
        public RatingDefination RatingDefinition { get; set; } = null!;

        public decimal CapitalRating { get; set; }
        public decimal AssetQualityRating { get; set; }
        public decimal EarningsRating { get; set; }
        public decimal LiquidityRating { get; set; }
        public decimal StructureRating { get; set; }  
        public decimal ManagementRating { get; set; }  

        public decimal OverallRating { get; set; }  // Composite score
        public decimal AverageRating { get; set; }  // Simple average of components
        public string RiskLevel { get; set; } = null!;
        public string? CalculationDetails { get; set; }
        public bool IsCurrent { get; set; } = true;  // Mark as latest rating for the period
    }
}