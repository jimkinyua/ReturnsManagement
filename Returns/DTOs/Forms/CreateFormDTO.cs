using System.ComponentModel.DataAnnotations;

public class CreateFormDTO : IValidatableObject
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string PeriodId { get; set; } = null!;
    [Required]
    public string DisplayName { get; set; } = null!;
    [Required]
    public string SaccoTypeId { get; set; } = null!;
    public Boolean IsCapitalAdequencyForm { get; set; } = false;
    public Boolean IsDepositReturnForm { get; set; } = false;
    public Boolean IsLiquidityStatement { get; set; } = false;
    public Boolean IsRiskClassification { get; set; } = false;
    public Boolean IsInvestmentReturn { get; set; } = false;
    public Boolean IsFinancialPosition { get; set; } = false;
    public Boolean IsStatementOfComprehensiveIncome { get; set; } = false;
    [Required]
    public Boolean IsOtherForm { get; set; } = false;
    public IFormFile? Template { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsOtherForm && Template == null)
        {
            yield return new ValidationResult("Template is required if the form is not of type OtherForm.", new[] { nameof(Template) });
        }
    }
}