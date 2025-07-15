using System.ComponentModel.DataAnnotations;
using Returns.DTOs.Returns_Analysis;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class CamelsAnalysisApprovalDTO
    {
        [Required]
        public string ReturnId { get; set; } = string.Empty;
        
        [Required]
        public CamelsRatingsDTO Analysis { get; set; } = new CamelsRatingsDTO();
        
        public string? Comments { get; set; }
        
        public string? Recommendation { get; set; } // "Approve", "Reject", "RequestChanges"
        
        public List<string>? RequestedChanges { get; set; } = new List<string>();
    }
    
    public class CamelsAnalysisApprovalResponseDTO
    {
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected"
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
    
    public class CamelsAnalysisStatusDTO
    {
        public string ReturnId { get; set; } = string.Empty;
        public bool HasAnalysis { get; set; }
        public DateTime? AnalysisDate { get; set; }
        public string? AnalyzedBy { get; set; }
        public string? ApprovalStatus { get; set; } // "Pending", "Approved", "Rejected"
        public DateTime? ApprovalDate { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Comments { get; set; }
        public CamelsRatingsDTO? Analysis { get; set; }
    }
}