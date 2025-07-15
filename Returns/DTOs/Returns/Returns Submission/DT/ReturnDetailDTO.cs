using Returns.DTOs.Returns_Analysis;
using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.NWDT;
using Returns.DTOs.WorkFlow_Engine;
using Returns.Models;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class ReturnDetailDTO
    {
        // Basic return information
        public string Id { get; set; } = string.Empty;
        public string SaccoId { get; set; } = string.Empty;
        public string SaccoName { get; set; } = string.Empty;
        public string SaccoType { get; set; } = string.Empty;
        public DateTime ReturnFor { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string Period { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public bool IsActiveVersion { get; set; }
        public DateTime? AmendmentDate { get; set; }
        public string? PreviousVersionId { get; set; }
        
        // Status information
        public bool IsNotConsistent { get; set; }
        public string? ConsistentErrorMessage { get; set; }
        public string LateNessStatus { get; set; } = string.Empty;
        public int DaysLateBy { get; set; }
        public int TotalReturns { get; set; }
        public int TotalLateReturns { get; set; }
        
        // Form data - DT
        public DTCapitalAdequacyReturn? CapitalAdequacy { get; set; }
        public DTLiquidityReturn? LiquidityReturn { get; set; }
        public List<DTRiskClassificationReturn> RiskClassifications { get; set; } = new List<DTRiskClassificationReturn>();
        public DTInvestmentReturn? InvestmentReturn { get; set; }
        public DTFinancialPositionReturn? FinancialPosition { get; set; }
        public DTComprehensiveIncomeReturn? ComprehensiveIncome { get; set; }
        public List<DepositReturn> DepositReturns { get; set; } = new List<DepositReturn>();
        
        // Form data - NWDT
        public NWDTCapitalAdequacyReturn? NWDTCapitalAdequacy { get; set; }
        public NWDTLiquidityReturn? NWDTLiquidityReturn { get; set; }
        public List<NWDTRiskClassificationReturn> NWDTRiskClassifications { get; set; } = new List<NWDTRiskClassificationReturn>();
        public NWDTInvestmentReturn? NWDTInvestmentReturn { get; set; }
        public NWDTFinancialPositionReturn? NWDTFinancialPosition { get; set; }
        public NWDTComprehensiveIncomeReturn? NWDTComprehensiveIncome { get; set; }
        public List<NWDTDepositReturn> NWDTDepositReturns { get; set; } = new List<NWDTDepositReturn>();
        
        // Other returns
        public List<OtherReturn> OtherReturns { get; set; } = new List<OtherReturn>();
        public List<SectoralLendingReport> SectoralLendingReports { get; set; } = new List<SectoralLendingReport>();
        public List<DailyLiquidityReturn> DailyLiquidityReturns { get; set; } = new List<DailyLiquidityReturn>();
        public List<ManagementReturn> ManagementReturns { get; set; } = new List<ManagementReturn>();
        
        // Analysis results
        public CamelsRatingsDTO? CamelsAnalysis { get; set; }
        public bool HasCamelsAnalysis { get; set; }
        public DateTime? CamelsAnalysisDate { get; set; }
        public string? CamelsAnalysisBy { get; set; }
        
        // Workflow information
        public WorkflowStateDto? CurrentWorkflowState { get; set; }
        public List<CommentDetails> WorkflowComments { get; set; } = new List<CommentDetails>();
        
        // Additional information requests
        public List<AdditionalInformationRequest> AdditionalInfoRequests { get; set; } = new List<AdditionalInformationRequest>();
        
        // File attachments
        public List<ReturnFormAttachment> Attachments { get; set; } = new List<ReturnFormAttachment>();
    }
}