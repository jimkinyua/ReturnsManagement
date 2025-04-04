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
        public Boolean IsConsistent { get; set; } = false;
        public string? ConsistentErrorMessage { get; set; }

        // TRACKING 
        public int VersionNumber { get; set; } = 1;
        public bool IsActiveVersion { get; set; } = true;
        public DateTime? AmendmentDate { get; set; }
        public string? PreviousVersionId { get; set; }

        public virtual Return? PreviousVersion { get; set; }

        public virtual ICollection<CapitalAdequacy> CapitalAdequencies { get; set; } = new List<CapitalAdequacy>();
        public virtual ICollection<LiquidityReturn> LiquidityReturns { get; set; } = new List<LiquidityReturn>();
        public virtual ICollection<RiskClassificationReturn> RiskClassifications { get; set; } = new List<RiskClassificationReturn>();
        public virtual ICollection<InvestmentReturn> InvestmentReturns { get; set; } = new List<InvestmentReturn>();
        public virtual ICollection<StatementOfFinancialPositionReturn> StatementOfFinancialPositionReturns { get; set; } = new List<StatementOfFinancialPositionReturn>();
        public virtual ICollection<StatementOfComprehensiveIncomeReturn> StatementOfComprehensiveIncomeReturns { get; set; } = new List<StatementOfComprehensiveIncomeReturn>();
        public virtual ICollection<SaccoAnalysis> SaccoAnalysis { get; set; } = new List<SaccoAnalysis>();
        public virtual ICollection<DepositReturn> DepositReturns { get; set; } = new List<DepositReturn>();


        // NWDT
        public virtual ICollection<NDWTCapitalAdequacyReturn> NDWTCapitalAdequacyReturns { get; set; } = new List<NDWTCapitalAdequacyReturn>();
        public virtual ICollection<NWDTLiquidityReturn> NWDTLiquidityReturns { get; set; } = new List<NWDTLiquidityReturn>();
        public virtual ICollection<NWDTDepositReturn> NWDTDepositReturns { get; set; } = new List<NWDTDepositReturn>();
        public virtual ICollection<NWDTInvestmentReturn> NWDTInvestmentReturns { get; set; } = new List<NWDTInvestmentReturn>();
        public virtual ICollection<NWDTFinancialPositionReturn> NWDTFinancialPositionReturns { get; set; } = new List<NWDTFinancialPositionReturn>();
        public virtual ICollection<NWDTComprehensiveIncomeReturn> NWDTComprehensiveIncomeReturns { get; set; } = new List<NWDTComprehensiveIncomeReturn>();
        public virtual ICollection<NWDTRiskClassificationReturn> NWDTRiskClassificationReturns { get; set; } = new List<NWDTRiskClassificationReturn>();


        public virtual ICollection<OtherReturn> OtherReturns { get; set; } = new List<OtherReturn>();


        // Assignments 
        public virtual ICollection<ReturnsAssigment> ReturnsAssigments { get; set; } = new List<ReturnsAssigment>();
        public virtual ICollection<AdditionalInformationRequest> ReturnsAdditionalInformationRequests { get; set; } = new List<AdditionalInformationRequest>();
    }
}
