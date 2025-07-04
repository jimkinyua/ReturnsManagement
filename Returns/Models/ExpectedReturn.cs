using Returns.Helpers.Enums;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ExpectedReturn : CommonFields
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;  // PK, GUID

        [ForeignKey("Period")]
        public string PeriodId { get; set; } = null!;  // FK → ReturnPeriods
        public virtual ReturnPeriods Period { get; set; } = null!;

        [ForeignKey("ReturnForm")]
        public string FormId { get; set; } = null!;  // FK → ReturnForm
        public virtual ReturnForm Form { get; set; } = null!;

        public DateTime DueDate { get; set; }
        
        // Only store if it's waived - other statuses are calculated
        public bool IsWaived { get; set; } = false;
        public string? WaivedReason { get; set; }
        public DateTime? WaivedDate { get; set; }
        public string? WaivedBy { get; set; }
        
        // Calculated property - not stored in database
        public ExpectedStatus Status 
        { 
            get 
            {
                if (IsWaived)
                    return ExpectedStatus.Waived;
                    
                // Check if filed by looking for associated return
                if (HasAssociatedReturn)
                    return ExpectedStatus.Filed;
                    
                // Check if late
                if (DateTime.Now > DueDate)
                    return ExpectedStatus.Late;
                    
                // Otherwise it's due
                return ExpectedStatus.Due;
            }
        }
        
        // This would be set by queries that join with Returns table
        public bool HasAssociatedReturn { get; set; }
        
        // Helper method to check if return can be filed
        public bool CanFileReturn()
        {
            return !IsWaived && !HasAssociatedReturn;
        }
        
        // Helper method to get days until due (negative if overdue)
        public int DaysUntilDue()
        {
            return (DueDate - DateTime.Now).Days;
        }
    }
}