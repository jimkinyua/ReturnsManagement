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

        // Top-level properties for each form type
        public CapitalAdequacyDTO? CapitalAdequacy { get; set; }
        public LiquidityDTO? Liquidity { get; set; }
        public RiskClassificationDTO? RiskClassification { get; set; }
        public DepositReturnDTO? DepositReturn { get; set; }
        public FinancialPositionDTO? FinancialPosition { get; set; }
        public ComprehensiveIncomeDTO? ComprehensiveIncome { get; set; }
        // Add more as needed
    }

    // Example DTOs for each form type (replace with your actual DTOs)
    public class CapitalAdequacyDTO { }
    public class LiquidityDTO { }
    public class RiskClassificationDTO { }
    public class DepositReturnDTO { }
    public class FinancialPositionDTO { }
    public class ComprehensiveIncomeDTO { }
}