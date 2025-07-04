using Returns.Models.Common;

namespace Returns.Models
{
    public class WaivedReturn : CommonFields
    {
        public string SaccoId { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string FormId { get; set; } = null!;
        public string WaivedReason { get; set; } = null!;
        public DateTime WaivedDate { get; set; }
        public string WaivedBy { get; set; } = null!;
        
        // Navigation properties
        public virtual ReturnPeriods Period { get; set; } = null!;
        public virtual ReturnForm Form { get; set; } = null!;
    }
}