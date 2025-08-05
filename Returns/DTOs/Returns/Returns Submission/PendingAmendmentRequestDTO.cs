using Returns.Helpers.Enums;
using Returns.Models;

namespace Returns.DTOs.Returns.Returns_Submission
{
    public class PendingAmendmentRequestDTO
    {
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public string Id { get; set; } = null!;
        public string RequestedById { get; set; } = null!;
        public string ReturnSubmissionId { get; set; } = null!;
        public string ExpectedReturnId { get; set; } = null!;
        public string RequestedAt { get; set; } = string.Empty;
        public AmendmentStatus Status { get; set; } = AmendmentStatus.PendingAdminApproval;
        public string Reason { get; set; } = string.Empty;
        public string OriginalYear { get; set; } = string.Empty;
        public string OriginalPeriod { get; set; } = string.Empty;
        public string PeriodStart { get; set; } = string.Empty;
        public string PeriodEnd { get; set; } = string.Empty;
        public string OriginalSubmittedAt { get; set; } = string.Empty;
        public string? ReturnType { get; set; }
    }
}
