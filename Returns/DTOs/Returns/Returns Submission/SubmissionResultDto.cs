namespace Returns.DTOs.Returns.Returns_Submission
{
    public class SubmissionResultDto
    {
        public string? SubmissionId { get; set; }
        public string FormFileName { get; set; } = "";
        public SubmissionStatus Status { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }
    public enum SubmissionStatus
    {
        NotSubmitted,   // No submission exists
        Draft,           // Saved but not yet submitted
        Pending,
        Submitted,
        WithWarnings,
        Failed
    }
}
