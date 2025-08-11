using Returns.DTOs.Returns.Returns_Submission;
using Returns.Helpers.Enums;
using SubmissionStatusEnum = Returns.DTOs.Returns.Returns_Submission.SubmissionStatus;

namespace Returns.DTOs.Forms
{
    public class FormsDueByMonthDTO
    {
        public string FormId { get; set; } = null!;
        public string ExpectedReturnId { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string PeriodName { get; set; } = null!;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime FilingDeadline { get; set; }
        public bool IsLate => DateTime.Now > FilingDeadline && SubmissionStatus != SubmissionStatusEnum.Submitted.ToString();
        public string? TemplateUrl { get; set; }
        public string? UploadUrl { get; set; }
        public string SaccoTypeId { get; set; } = null!;

        // Enhanced properties for submission handling
        //public bool IsSubmitted { get; set; } = false;
        public string? SubmissionId { get; set; }
        public string? SubmissionStatus { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsDraft => SubmissionStatus == SubmissionStatusEnum.Draft.ToString();

        public ExpectedStatus Status { get; internal set; }
    }
}