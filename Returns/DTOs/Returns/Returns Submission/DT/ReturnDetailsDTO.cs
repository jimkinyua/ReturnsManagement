using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Returns.DTOs.Returns.Returns_Submission.DT;
using Returns.DTOs.Returns.Returns_Submission.NWDT;
using Returns.DTOs.Returns_Analysis;
using Returns.DTOs.Returns_Submission;
using Returns.DTOs.Returns_Submission.Returns_Submission.DT;
using static Returns.Helpers.ReturnsHelper;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class ReturnDetailsDTO
    {
        public string Id { get; set; }
        public string SaccoName { get; set; }
        public string PeriodName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string ReturnStatus { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Year { get; set; }
        public Boolean IsConsistent { get; set; } = false;
        public string? ConsistentErrorMessage { get; set; }
        public int CapitalAdequacyDaysLate { get; set; }
        public int LiquidityReturnDaysLate { get; set; }
        public int RiskClassificationDaysLate { get; set; }
        public int InvestmentReturnDaysLate { get; set; }
        public int StatementOfFinancialPositionDaysLate { get; set; }
        public int StatementOfComprehensiveIncomeDaysLate { get; set; }
        public int SaccoAnalysisDaysLate { get; set; }
        public int DepositReturnDaysLate { get; set; }
        public int OtherReturnsDaysLate { get; set; }
        public CapitalAdequacyDTO? CapitalAdequacy { get; set; }
        public ManagementReturnDTO? ManagementReturn { get; set; }
        public LiquidityStatementDTO? LiquidityStatement { get; set; }
        public List<RiskClassificationDTO> RiskClassifications { get; set; } = new List<RiskClassificationDTO>();
        public InvestmentReturnDTO? Investment { get; set; }
        public ComprehesiveIncomeStatementDTO? IncomeStatement { get; set; }
        public FinancialPositionDTO? FinancialPosition { get; set; }
        public List<DepositReturnDto> DepositReturn { get; set; }
        public List<OtherReturnDTO> OtherReturns { get; set; } = new List<OtherReturnDTO>();
        public SectoralLendingDTO SectoralLending { get; set; } = new SectoralLendingDTO();

        public int VersionNumber { get; set; }
        public Boolean CanReportBeViewed { get; set; } = false;

        public bool IsActiveVersion { get; set; }
        public DateTime? AmendmentDate { get; set; }
        public string? PreviousVersionId { get; set; }
        public List<VersionChoice> PreviousVersionIds { get; set; } = new List<VersionChoice>();

    }
}
