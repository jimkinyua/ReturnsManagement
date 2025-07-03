using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ReportingYear
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Year { get; set; } // e.g., 2025

        public DateTime EndDateDate { get; set; }
        public DateTime StartDate { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = null!;

        // Navigation property
        public ICollection<ReturnPeriods> Periods { get; set; } = new List<ReturnPeriods>();
    }
}