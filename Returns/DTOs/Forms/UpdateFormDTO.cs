using Microsoft.AspNetCore.Http;

namespace Returns.DTOs.Forms
{
    public class UpdateFormDTO
    {
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string SaccoTypeId { get; set; } = null!;
        public IFormFile? Template { get; set; }  // Optional - only if updating template
        public bool UpdateTemplate { get; set; } = false;  // Flag to indicate if template should be updated
        public Boolean IsCapitalAdequencyForm { get; set; } = false;
        public Boolean IsLiquidityStatement { get; set; } = false;
        public Boolean IsRiskClassification { get; set; } = false;
        public Boolean IsInvestmentReturn { get; set; } = false;
        public Boolean IsFinancialPosition { get; set; } = false;
        public Boolean IsManagement { get; set; } = false;
        public Boolean IsStatementOfComprehensiveIncome { get; set; } = false;
        public Boolean IsDepositReturnForm { get; set; } = false;
        public Boolean IsSectoralLending { get; set; } = false;
        public Boolean IsDailyLiquidity { get; set; } = false;
        public Boolean IsInsiderLending { get; set; } = false;
        public Boolean IsOtherForm { get; set; } = false;
    }
}