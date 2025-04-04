namespace Returns.DTOs.Forms
{
    public class FormsToSubmitDTO
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string TemplateUrl { get; set; } = null!;
        public Boolean IsLate { get; set; }
        public Boolean IsSubmitted { get; set; }
        public DateTime ReportingPeriodStart { get; set; }
        public DateTime ReportingPeriodEnd { get; set; }
        public DateTime SubmissionDeadLine { get; set; }
    }
}
