using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ReturnForm : CommonFields
    {
        public string FormName { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string TemplateUrl { get; set; } = null!;
        public string SaccoTypeId { get; set; } = null!;
        public bool IsCapitalAdequencyForm { get; set; } = false;
        public bool IsLiquidityStatement { get; set; } = false;
        public bool IsRiskClassification { get; set; } = false;
        public bool IsInvestmentReturn { get; set; } = false;
        public bool IsFinancialPosition { get; set; } = false;
        public bool IsSectoralLending { get; set; } = false;
        public bool IsDailyLiquidity { get; set; } = false;
        public bool IsManagement { get; set; } = false;
        public bool IsInsiderLending { get; set; } = false;
        public bool IsStatementOfComprehensiveIncome { get; set; } = false;
        public Boolean IsOtherForm { get; set; } = false;
        public bool IsDepositReturnForm { get; set; } = false;
        [ForeignKey("Period")]
        public string PeriodId { get; set; } = null!;
        public ReturnPeriods Period { get; set; } = null!;
        //[ForeignKey("ReturnPeriods")]
        //public string PeriodId { get; set; } = null!;
        //public ReturnPeriods ReturnPeriods { get; set; } = null!;
    }
}
