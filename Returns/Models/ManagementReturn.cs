using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ManagementReturn : FormBase
    {
        public string FilePath { get; set; } = null!;
        public string? Year { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }   
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return? Return { get; set; } = null!;
        public decimal GorvenanceStructureScore { get; set; }
        public decimal GorvenanceStructureWeight { get; set; }
        public decimal GorvenanceStructureWeightedScore { get; set; }
        public decimal InternalControlsScore { get; set; }
        public decimal InternalControlsWeight { get; set; }
        public decimal InternalControlsWeightedScore { get; set; }
        public decimal InternalScoreScore { get; set; }
        public decimal InternalScoreWeight { get; set; }
        public decimal InternalScoreWeightedScore { get; set; }
        public decimal ComplianceWithLawsAndRegulationsScore { get; set; }
        public decimal ComplianceWithLawsAndRegulationsWeight { get; set; }
        public decimal ComplianceWithLawsAndRegulationsWeightedScore { get; set; }
        public decimal MemberProtectionScore { get; set; }
        public decimal MemberProtectionWeight { get; set; }
        public decimal MemberProtectionWeightedScore { get; set; }
        public decimal AdequacyOfMISScore { get; set; }
        public decimal AdequacyOfMISWeight { get; set; }
        public decimal AdequacyOfMISWeightedScore { get; set; }
        public decimal OverallRiskProfileScore { get; set; }
        public decimal OverallRiskProfileWeight { get; set; }
        public decimal OverallRiskProfileWeightedScore { get; set; }
        public int MRating { get; set; }

    }
}
