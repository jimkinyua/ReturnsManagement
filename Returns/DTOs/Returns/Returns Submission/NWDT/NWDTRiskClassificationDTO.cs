namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTRiskClassificationDTO:CommonFormDTO
    {
        public List<NWDTRiskClassificationData> NWDTRiskClassificationData { get; set; } = new List<NWDTRiskClassificationData>();
    }
    public class NWDTRiskClassificationData
    {
        public string? LoanType { get; set; }
        public string? Classification { get; set; }
        public int? NumberOfAccounts { get; set; }
        public decimal? OutstandingLoanPortfolio { get; set; }
        public decimal? RequiredProvision { get; set; }
        public decimal? RequiredProvisionAmount { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
