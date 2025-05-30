namespace Returns.DTOs.Enforcement
{
    public class EnforcementCaseRequestDTO
    {
        public string Title { get; init; } = "";
        public string Description { get; init; } = "";
        public string SaccoId { get; init; }
        public string SaccoName { get; init; } = "";
        public string Source { get; init; } = "";
        public string SourceReferenceNo { get; init; } = "";
        public string Classification { get; init; } = "";
        public DateTime DateRequested { get; init; }
        public IFormFile? SupportingFile { get; init; }          
        public string Remarks { get; init; } = "";
    }
}
