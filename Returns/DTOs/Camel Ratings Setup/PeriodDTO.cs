namespace Returns.DTOs
{
    public class PeriodDTO
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        //public bool IsQuartely { get; set; } = false;
        //public string Deadline { get; set; } = null!;
        //// Optional: You can add lists if you need related data.
        //public List<QuarterDatesDTO> QuarterDates { get; set; } = new List<QuarterDatesDTO>();
        //public List<ReturnFormDTO> ReturnForms { get; set; } = new List<ReturnFormDTO>();
    }
    public class ReturnFormDTO
    {
        public string Id { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string TemplateUrl { get; set; } = null!;
        public string SaccoTypeId { get; set; } = null!;
        public bool IsCapitalAdequencyForm { get; set; } = false;
        public bool IsLiquidityStatement { get; set; } = false;
        public bool IsRiskClassification { get; set; } = false;
        public bool IsInvestmentReturn { get; set; } = false;
        public bool IsFinancialPosition { get; set; } = false;
        public bool IsStatementOfComprehensiveIncome { get; set; } = false;
        public bool IsDepositReturnForm { get; set; } = false;
        public string PeriodId { get; set; } = null!; // Foreign Key Reference
        public DateTime CreatedAt { get; set; }
    }

   public class QuarterDatesDTO
    {
        public string? Id { get; set; } = null!;
        public string StartDate { get; set; } = null!;
        public string EndDate { get; set; } = null!;
        public string? Deadline { get; set; } = null!;
        public string? PeriodId { get; set; } = null!; // Foreign Key Reference
        public DateTime CreatedAt { get; set; }

        // Parameterless constructor (Required for deserialization)
        public QuarterDatesDTO() { }

        // Overloaded constructor for conversion from entity to DTO
        public QuarterDatesDTO(int startMonth, int startDay, int endMonth, int endDay, int deadlineMonth, int deadlineDay)
        {
            StartDate = $"{GetMonthName(startMonth)} {startDay}";
            EndDate = $"{GetMonthName(endMonth)} {endDay}";
            Deadline = $"{GetMonthName(deadlineMonth)} {deadlineDay}";
        }

        private static string GetMonthName(int month)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }
    }


}