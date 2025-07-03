using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs.PeriodManagement
{
    public class CreateYearDto
    {
        [Required]
        [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100")]
        public int Year { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}