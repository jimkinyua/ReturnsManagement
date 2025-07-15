using Returns.Models.Common;

namespace Returns.Models
{
    public class SaccoAnalysis : CommonFields
    {
        public string ReturnId { get; set; } = string.Empty;
        public string AnalysisData { get; set; } = string.Empty; // JSON serialized CAMELS analysis
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? Comments { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        
        // Navigation property
        public virtual Return Return { get; set; } = null!;
    }
}
