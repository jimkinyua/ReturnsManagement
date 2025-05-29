using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs.Returns.Returns_Analysis
{
    public class SaccoResubmissionDto
    {
        [Required]
        public string FormId { get; set; } = null!;
        [Required]
        public string ReturnId { get; set; } = null!;
        [Required]
        public IFormFile FormFile { get; set; } = null!;
        public string? ResubmissionNotes { get; set; }
    }
}
