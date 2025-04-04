public class CreateCamelIndicatorThresholdDTO
{
    public string IndicatorId { get; set; } = null!;
    public int RatingLevel { get; set; }
    public decimal ThresholdValue { get; set; }
}