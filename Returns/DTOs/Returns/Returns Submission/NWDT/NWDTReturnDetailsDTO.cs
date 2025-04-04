using Returns.DTOs.Returns.Returns_Submission.DT;

namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTReturnDetailsDTO
    {
        public string? Id { get; set; }
        public string? SaccoName { get; set; }
        public string? PeriodName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? ReturnStatus { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Year { get; set; }
        public Boolean IsConsistent { get; set; } = false;
        public string? ConsistentErrorMessage { get; set; }
        public int CapitalAdequacyDaysLate { get; set; }
        public int LiquidityReturnDaysLate { get; set; }
        public int RiskClassificationDaysLate { get; set; }
        public int InvestmentReturnDaysLate { get; set; }
        public int StatementOfFinancialPositionDaysLate { get; set; }
        public int StatementOfComprehensiveIncomeDaysLate { get; set; }
        public int DepositReturnDaysLate { get; set; }
        public NWDTCapitalAdequacyDTO? NWDTCapitalAdequacy { get; set; }
        public NWDTLiquidityStatementDTO? NWDTLiquidityStatement { get; set; }
        public List<NWDTRiskClassificationDTO> NWDTRiskClassifications { get; set; } = new List<NWDTRiskClassificationDTO>();
        public NWDTInvestmentReturnDTO? NWDTInvestment { get; set; }
        public NWDTComprehesiveIncomeStatementDTO? NWDTIncomeStatement { get; set; }
        public NWDTFinancialPositionDTO? NWDTFinancialPosition { get; set; }
        public List<NWDTDepositReturnDto> NWDTDepositReturn { get; set; } = new List<NWDTDepositReturnDto>();
        public List<OtherReturnDTO> OtherReturns { get; set; } = new List<OtherReturnDTO>();
        public int VersionNumber { get; set; }
        public bool IsActiveVersion { get; set; }
        public DateTime? AmendmentDate { get; set; }
        public string? PreviousVersionId { get; set; }
        public List<string> PreviousVersionIds { get; set; } = new List<string>();
    }
}
