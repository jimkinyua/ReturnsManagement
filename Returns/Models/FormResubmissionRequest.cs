using Returns.Models.Common;

namespace Returns.Models
{
    public class FormResubmissionRequest:CommonFields
    {
        public string FormId { get; set; } = null!;
        public string ReturnId { get; set; } = null!;

        public string SaccoId { get; set; } = null!;
        public string SaccoType { get; set; } = null!; 

        public string RequestedBy { get; set; } = null!;
        public string RequestedByEmail { get; set; } = null!; 
        public string RequestedByName { get; set; } = null!; 

        public string ChildId { get; set; } = null!;
        public string? Reason { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = null!;
        public DateTime? UploadedAt { get; set; }
        public string? RespondedBy { get; set; }
        public string? ResubmissionNotes { get; set; } 

    }
}
