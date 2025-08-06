using Returns.DTOs.Returns.Returns_Analysis;
using Returns.DTOs.Returns_Analysis;
using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.Returns_Submission.DT;
using Returns.DTOs.WorkFlow_Engine;
using Returns.Helpers;
using System;

namespace Returns.DTOs.Returns_Submission
{
    public class AdminGroupedReturnDetailsDTO
    {
        public string GroupId { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string PeriodName { get; set; } = null!;
        public int Year { get; set; }
        public string Frequency { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public bool IsComplete { get; set; }
        public int SubmittedForms { get; set; }
        public ReturnGroupType GroupType { get; set; }

        public object? CapitalAdequacy { get; set; }
        public object? Liquidity { get; set; }
        public object? RiskClassification { get; set; }
        public object? DepositReturn { get; set; }
        public object? FinancialPosition { get; set; }
        public object? ComprehensiveIncome { get; set; }
        public object? InvestmentReturn { get; set; }
        public List<ReturnAnalysisHelper.ValidationError> ConsistencyErrors { get; internal set; } = new List<ReturnAnalysisHelper.ValidationError>();
        public WorkflowStateDto? WorkflowState { get; set; }
        public SingleCAELSDTO Ratings { get; set; } = new SingleCAELSDTO();
        public List<CommentDetails> Comments { get; set; } = new List<CommentDetails>();
        public List<string> MissingForms { get; internal set; } = new List<string>();

        // Version-related properties
        public int CurrentVersion { get; set; } = 1;
        public List<ReturnVersionDTO> AvailableVersions { get; set; } = new List<ReturnVersionDTO>();
        public bool HasMultipleVersions { get; set; } = false;
    }

    public class ReturnVersionDTO
    {
        public int Version { get; set; }
        public string SubmissionId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public bool IsLatest { get; set; }
        public string? AmendsSubmissionId { get; set; }
        public string? AmendedBySubmissionId { get; set; }
    }
}