using Returns.Models.Common;

namespace Returns.Models
{
    public class ConsistencyCheckResult:CommonFields
    {
        public string SaccoId { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string? RatingDefinitionId { get; set; } 
        public bool IsValid { get; set; } = true;
        public string ErrorsJson { get; set; } = "[]";  // JSON-serialized List<ValidationError>
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
