using Returns.DTOs.Returns.Returns_Submission;
using Returns.Helpers.Enums;

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
        public ExpectedStatus Status { get; set; }
        public bool IsLate => DateTime.Now > FilingDeadline && Status != ExpectedStatus.Filed;
        public string? TemplateUrl { get; set; }
        public string SaccoTypeId { get; set; } = null!;

        // Enhanced properties for submission handling
        public bool IsSubmitted { get; set; } = false;
        public string? SubmissionId { get; set; }
        public SubmissionStatus? SubmissionStatus { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool CanSubmit => !IsSubmitted && !IsLate && Status != ExpectedStatus.Waived;
        public bool CanContinue => IsSubmitted  && !IsLate;
        public bool CanRequestEdit => IsSubmitted && IsLate;
        public string ActionType => IsSubmitted ? (IsLate ? "RequestEdit" : "Continue") : "Submit";
    }
}