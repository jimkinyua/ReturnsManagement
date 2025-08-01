namespace Returns.DTOs.Returns.Returns_Analysis
{
    public class SingleCAELSDTO
    {
        public int CapitalRating { get; set; }
        public int AssetQualityRating { get; set; }
        public int ManagementRating { get; set; }
        public int EarningsRating { get; set; }
        public int LiquidityRating { get; set; }
        public int OverallRating { get; set; }
        public decimal Average { get; set; }  // mean of the five components
        public string RiskLevel { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
    }
}
