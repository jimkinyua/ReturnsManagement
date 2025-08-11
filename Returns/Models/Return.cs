using Returns.Models.Common;

namespace Returns.Models
{
    public class Return : CommonFields
    {
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public DateTime ReturnFor { get; set; }
        public string SaccoId { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public string SaccoName { get; set; } = "";
        public string Period { get; set; } = "";
        public string Year { get; set; } = null!;
        public Boolean IsNotConsistent { get; set; } = false;
        public string? ConsistentErrorMessage { get; set; }

        // TRACKING 
        public int VersionNumber { get; set; } = 1;
        public bool IsActiveVersion { get; set; } = true;
        public DateTime? AmendmentDate { get; set; }
        public string? PreviousVersionId { get; set; }
        public Boolean CanReportBeViewed { get; set; } = false;

  
    }
}
