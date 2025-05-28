namespace Returns.DTOs.Returns.Returns_Analysis
{
    public class ResubmissionRequestDto
    {
        public string FormId { get; set; } = null!;
        public string ReturnId { get; set; } = null!;
        public string? Reason { get; set; }
    }
}
