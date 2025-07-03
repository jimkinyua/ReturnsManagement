using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class FrequencyCatalog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = null!; // MTH, QTR, FY, WK, BWK, DAY
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!; // Monthly, Quarterly, etc.
        
        [Required]
        public int IntervalDays { get; set; } // Length of each period block
        
        [Required]
        public int DefaultDeadlineOffset { get; set; } // Filing grace period (days after end_date)
        
        [Required]
        [MaxLength(20)]
        public string LabelStrategy { get; set; } = null!; // MONTH, QUARTER, ISO_WEEK, YEAR, DATE, BI_WEEK
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = null!;
        
        // Navigation property
        public ICollection<Period> Periods { get; set; } = new List<Period>();
    }
}