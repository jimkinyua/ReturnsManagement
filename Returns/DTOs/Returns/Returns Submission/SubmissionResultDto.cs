namespace Returns.DTOs.Returns.Returns_Submission
{
    public class SubmissionResultDto
    {
        public string SubmissionId { get; set; }
        public string FormType { get; set; }
        public string FormFileName { get; set; }
        public SubmissionStatus Status { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public object Data { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
    }

    public enum SubmissionStatus
    {
        Processing,
        Draft,
        Success,
        Failed,
        ValidationError
    }
}
