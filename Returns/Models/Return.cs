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

        // DRAFT STATUS
        public bool IsDraft { get; set; } = false;

        // TRACKING 
        public int VersionNumber { get; set; } = 1;
        public bool IsActiveVersion { get; set; } = true;
        public DateTime? AmendmentDate { get; set; }
        public string? PreviousVersionId { get; set; }
        public Boolean CanReportBeViewed { get; set; } = false;

        public virtual Return? PreviousVersion { get; set; }

        public virtual ICollection<DTCapitalAdequacyReturn> CapitalAdequencies { get; set; } = new List<DTCapitalAdequacyReturn>();
        public virtual ICollection<DTLiquidityReturn> LiquidityReturns { get; set; } = new List<DTLiquidityReturn>();
        public virtual ICollection<DTRiskClassificationReturn> RiskClassifications { get; set; } = new List<DTRiskClassificationReturn>();
        public virtual ICollection<DTInvestmentReturn> InvestmentReturns { get; set; } = new List<DTInvestmentReturn>();
        public virtual ICollection<DTFinancialPositionReturn> StatementOfFinancialPositionReturns { get; set; } = new List<DTFinancialPositionReturn>();
        public virtual ICollection<DTComprehensiveIncomeReturn> StatementOfComprehensiveIncomeReturns { get; set; } = new List<DTComprehensiveIncomeReturn>();
        public virtual ICollection<SaccoAnalysis> SaccoAnalysis { get; set; } = new List<SaccoAnalysis>();
        public virtual ICollection<DepositReturn> DepositReturns { get; set; } = new List<DepositReturn>();


        // NWDT
        public virtual ICollection<NWDTCapitalAdequacyReturn> NDWTCapitalAdequacyReturns { get; set; } = new List<NWDTCapitalAdequacyReturn>();
        public virtual ICollection<NWDTLiquidityReturn> NWDTLiquidityReturns { get; set; } = new List<NWDTLiquidityReturn>();
        public virtual ICollection<NWDTDepositReturn> NWDTDepositReturns { get; set; } = new List<NWDTDepositReturn>();
        public virtual ICollection<NWDTInvestmentReturn> NWDTInvestmentReturns { get; set; } = new List<NWDTInvestmentReturn>();
        public virtual ICollection<NWDTFinancialPositionReturn> NWDTFinancialPositionReturns { get; set; } = new List<NWDTFinancialPositionReturn>();
        public virtual ICollection<NWDTComprehensiveIncomeReturn> NWDTComprehensiveIncomeReturns { get; set; } = new List<NWDTComprehensiveIncomeReturn>();
        public virtual ICollection<NWDTRiskClassificationReturn> NWDTRiskClassificationReturns { get; set; } = new List<NWDTRiskClassificationReturn>();


        public virtual ICollection<OtherReturn> OtherReturns { get; set; } = new List<OtherReturn>();
        public virtual ICollection<SectoralLendingReport> SectoralLendingReports { get; set; } = new List<SectoralLendingReport>();
        public virtual ICollection<DailyLiquidityReturn> DailyLiquidityReturns { get; set; } = new List<DailyLiquidityReturn>();
        public virtual ICollection<InsiderLendingHeader> InsiderLendingHeaders { get; set; } = new List<InsiderLendingHeader>();
        public virtual ICollection<ManagementReturn> ManagementReturns { get; set; } = new List<ManagementReturn>();

        // Assignments 
        public virtual ICollection<ReturnsAssigment> ReturnsAssigments { get; set; } = new List<ReturnsAssigment>();
        public virtual ICollection<AdditionalInformationRequest> ReturnsAdditionalInformationRequests { get; set; } = new List<AdditionalInformationRequest>();
        public virtual ICollection<WorkflowInstance> WorkflowInstances { get; set; } = new List<WorkflowInstance>();
    }
}
