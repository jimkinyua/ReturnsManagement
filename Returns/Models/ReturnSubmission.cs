using Returns.Helpers.Enums;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ReturnSubmission : CommonFields
    {
        [ForeignKey("ExpectedReturn")]
        public string ExpectedReturnId { get; set; } = null!;
        public ExpectedReturn ExpectedReturn { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public string FileUrl { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public string? AmendsSubmissionId { get; set; }
        public string? AmendedBySubmissionId { get; set; }
        public bool IsLatest { get; set; } = true;
        public int Version { get; set; } = 1;
        public string Status { get; set; } = ExpectedStatus.Draft.ToString(); // Default status is Draft

        //DT Returns
        public ICollection<DTCapitalAdequacyReturn> DTCapitalAdequacyReturns { get; set; } = new List<DTCapitalAdequacyReturn>();
        public ICollection<DTComprehensiveIncomeReturn> DTComprehensiveIncomeReturns { get; set; } = new List<DTComprehensiveIncomeReturn>();
        public ICollection<DTFinancialPositionReturn> DTFinancialPositionReturns { get; set; } = new List<DTFinancialPositionReturn>();
        public ICollection<DTInvestmentReturn> DTInvestmentReturns { get; set; } = new List<DTInvestmentReturn>();
        public ICollection<DTLiquidityReturn> DTLiquidityReturns { get; set; } = new List<DTLiquidityReturn>();
        public ICollection<DTRiskClassificationReturn> DTRiskClassificationReturns { get; set; } = new List<DTRiskClassificationReturn>();
        public ICollection<DepositReturn> DepositReturns { get; set; } = new List<DepositReturn>();

        //NWDT Returns
        public ICollection<NWDTCapitalAdequacyReturn> NWDTCapitalAdequacyReturns { get; set; } = new List<NWDTCapitalAdequacyReturn>();
        public ICollection<NWDTLiquidityReturn> NWDTLiquidityReturns { get; set; } = new List<NWDTLiquidityReturn>();
        public ICollection<NWDTDepositReturn> NWDTDepositReturns { get; set; } = new List<NWDTDepositReturn>();
        public ICollection<NWDTInvestmentReturn> NWDTInvestmentReturns { get; set; } = new List<NWDTInvestmentReturn>();
        public ICollection<NWDTFinancialPositionReturn> NWDTFinancialPositionReturns { get; set; } = new List<NWDTFinancialPositionReturn>();
        public ICollection<NWDTComprehensiveIncomeReturn> NWDTComprehensiveIncomeReturns { get; set; } = new List<NWDTComprehensiveIncomeReturn>();
        public ICollection<NWDTRiskClassificationReturn> NWDTRiskClassificationReturns { get; set; } = new List<NWDTRiskClassificationReturn>();

        public ICollection<AmendmentRequest> AmendmentRequests { get; set; } = new List<AmendmentRequest>();
        public virtual ICollection<AdditionalInformationRequest> ReturnsAdditionalInformationRequests { get; set; } = new List<AdditionalInformationRequest>();
        public virtual ICollection<AuditedFinancialPosition> AuditedFinancialPositions { get; set; } = new List<AuditedFinancialPosition>();
        public virtual ICollection<AuditedComprehensiveIncome> AuditedComprehensiveIncomes { get; set; } = new List<AuditedComprehensiveIncome>();
        public virtual ICollection<AuditedRiskClassification> AuditedRiskClassifications { get; set; } = new List<AuditedRiskClassification>();


    }
}
