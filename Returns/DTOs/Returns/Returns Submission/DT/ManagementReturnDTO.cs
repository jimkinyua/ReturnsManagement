namespace Returns.DTOs.Returns.Returns_Submission.DT
{
    public class ManagementReturnDTO
    {
        public string ReturnId { get; set; } = null!;
        public string? SaccoCsNumber { get; set; }
        public decimal GovernanceStructureScore { get; set; }
        public decimal GovernanceStructureWeight { get; set; }
        public decimal GovernanceStructureWeightedScore { get; set; }

        public decimal InternalControlsScore { get; set; }
        public decimal InternalControlsWeight { get; set; }
        public decimal InternalControlsWeightedScore { get; set; }

        public decimal ComplianceWithLawsScore { get; set; }
        public decimal ComplianceWithLawsWeight { get; set; }
        public decimal ComplianceWithLawsWeightedScore { get; set; }

        public decimal MemberProtectionScore { get; set; }
        public decimal MemberProtectionWeight { get; set; }
        public decimal MemberProtectionWeightedScore { get; set; }

        public decimal AdequacyOfMISScore { get; set; }
        public decimal AdequacyOfMISWeight { get; set; }
        public decimal AdequacyOfMISWeightedScore { get; set; }

        public decimal OverallRiskProfileScore { get; set; }
        public decimal OverallRiskProfileWeight { get; set; }
        public decimal OverallRiskProfileWeightedScore { get; set; }

        public decimal CompositeScore { get; set; }   
        public int MRating { get; set; }
    }
}
