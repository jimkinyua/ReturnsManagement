using Returns.Helpers.Enums;
using Returns.Models;

namespace Returns.DTOs.Returns.Returns_Submission
{
    public class PendingAmendmentRequestDTO
    {
        public string SaccoId { get; set; } = null!;
        public string Id { get; set; } = null!;
        public string RequestedById { get; set; } = null!;
        public string ReturnSubmissionId { get; set; } = null!;
        public string ExpectedReturnId { get; set; } = null!;
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public AmendmentStatus Status { get; set; } = AmendmentStatus.Pending;
        public string Reason { get; set; } = string.Empty;
        public FormCategory? ReturnType { get; set; }

    }
}
