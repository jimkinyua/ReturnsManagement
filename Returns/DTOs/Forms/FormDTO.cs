namespace Returns.DTOs.Forms
{
    public class FormDTO
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string SaccoTypeId { get; set; } = null!;
        public string TemplateUrl { get; set; } = null!;
        public Boolean IsCapitalAdequencyForm { get; set; } = false;
        public Boolean IsLiquidityStatement { get; set; } = false;
        public Boolean IsRiskClassification { get; set; } = false;
        public Boolean IsInvestmentReturn { get; set; } = false;
        public Boolean IsFinancialPosition { get; set; } = false;
        public Boolean IsStatementOfComprehensiveIncome { get; set; } = false;
        public Boolean IsDepositReturnForm { get; set; } = false;
        public Boolean IsOtherForm { get; set; } = false;
    }
}
