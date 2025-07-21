using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.Returns_Submission.DT;
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
    }


}