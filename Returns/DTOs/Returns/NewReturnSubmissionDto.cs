using Microsoft.AspNetCore.Http;

namespace Returns.DTOs.Returns
{
    public class NewReturnSubmissionDto
    {
        public string SaccoId { get; set; } = "";
        public string SaccoType { get; set; } = "";
        public string PeriodId { get; set; } = "";
        public List<FormSubmission> FormSubmissions { get; set; } = new();
        public string? Notes { get; set; }
    }
    
    public class FormSubmission
    {
        public string FormId { get; set; } = "";
        public IFormFile? FormFile { get; set; }
        public Dictionary<string, object>? Metadata { get; set; } // For any form-specific data
    }
}