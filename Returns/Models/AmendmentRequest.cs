using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public enum AmendmentStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3   // closed after successful re‑upload
    }

    public class AmendmentRequest:CommonFields
    {
        public string SaccoId { get; set; } = null!;
        public string RequestedById { get; set; } = null!;
        public string ExpectedReturnId { get; set; } = null!;
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public AmendmentStatus Status { get; set; } = AmendmentStatus.Pending;
        public string? ReviewedById { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string FileUrl { get; set; } = null!;
        public string? ContentsJson { get; set; } // Parsed Excel data as JSON
        public bool ParseSuccess { get; set; } = true; // Default to true
        public string? ParseErrorsJson { get; set; } // Parsing errors as JSON
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;
        public bool IsAdminInitiated { get; set; } = false; 
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
        public ExpectedReturn ExpectedReturn { get; internal set; } = null!;
    }
}
