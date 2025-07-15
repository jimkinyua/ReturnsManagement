using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.DTOs.Returns.Returns_Submission
{
    public class ConsistencyCheckResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public List<string> ProcessingSummary { get; set; } = new List<string>();
        public List<ValidationError> ConsistencyErrors { get; set; } = new List<ValidationError>();
        public bool HasConsistencyBeenChecked { get; set; }
        public bool IsValid => !ConsistencyErrors.Any();
    }
}