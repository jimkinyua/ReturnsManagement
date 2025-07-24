using Returns.Helpers.Enums;
using Returns.Models;

namespace Returns.DTOs.Returns
{
    public class AmendmentRequestDetailsDTO
    {
        public string Id { get; set; } = string.Empty;
        public string ExpectedReturnId { get; set; } = string.Empty;
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public string SaccoId { get; set; } = string.Empty;
        public string RequestedById { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string? ReviewedById { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AmendmentStatus Status { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public List<object> Rows { get; set; } // Parsed contents (e.g., DTCapitalAdequacyReturns)
        public FormCategory? ReturnType { get; set; }
    }
}
