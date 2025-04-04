namespace Returns.DTOs
{
    public class CamelIndicatorThresholdDTO
    {
        public string Id { get; set; } = null!;
        public string IndicatorId { get; set; } = null!;
        public int RatingLevel { get; set; }
        public decimal ThresholdValue { get; set; }
    }
}
