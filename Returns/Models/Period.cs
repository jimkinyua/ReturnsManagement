using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ReturnPeriods
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public int YearId { get; set; }

        [Required]
        public int FrequencyId { get; set; }

        public int? SequenceNo { get; set; } // Monotonic counter within year+frequency

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!; // Display label like "Q1 2025", "Jan-2025"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime FilingDeadline { get; set; } // Calculated as EndDate + DefaultDeadlineOffset

        [Required]
        public bool IsLocked { get; set; } = false; // Regulator can freeze period

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = null!;

        // Navigation properties
        [ForeignKey("YearId")]
        public virtual ReportingYear ReportingYear { get; set; } = null!;

        [ForeignKey("FrequencyId")]
        public virtual FrequencyCatalog FrequencyCatalog { get; set; } = null!;

        public ICollection<ReturnForm> ReturnForms { get; set; } = new List<ReturnForm>();
    }
}
