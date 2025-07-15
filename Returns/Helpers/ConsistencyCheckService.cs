using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers
{
    public class ConsistencyCheckService : IConsistencyCheckService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<ConsistencyCheckService> _logger;
        private readonly IRatingDefinitionService _ratingDefinitionService;

        public ConsistencyCheckService(ReturnsDbContext context, ILogger<ConsistencyCheckService> logger,
            IRatingDefinitionService ratingDefinitionService)
        {
            _context = context;
            _logger = logger;
            _ratingDefinitionService = ratingDefinitionService;
        }

        Task<(bool IsValid, List<string> ProcessingSummary, List<ValidationError> ConsistencyErrors,bool HasConsistencyBeenChecked, List<object> FormData, string CommonPeriod)>IConsistencyCheckService.CheckConsistencyForDTAsync(NewReturnDTO createFormDTO, string ratingName)
        {
            return CheckConsistencyForDTAsyncInternal(createFormDTO, ratingName);
        }

        private async Task<(bool IsValid, List<string> ProcessingSummary, List<ValidationError> ConsistencyErrors,bool HasConsistencyBeenChecked, List<object> FormData, string CommonPeriod)>CheckConsistencyForDTAsyncInternal(NewReturnDTO createFormDTO, string ratingName)
        {
            var processingSummary = new List<string>();
            var consistencyErrors = new List<ValidationError>(); 
            var hasConsistencyBeenChecked = false;
            var formData = new List<object>();
            string commonPeriod = null;

            // Step 1: Validate attachments
            (bool hasAttachments, string attachmentError) = ValidateAttachments(createFormDTO);
            if (!hasAttachments)
            {
                processingSummary.Add(attachmentError);
                return (false, processingSummary, consistencyErrors, hasConsistencyBeenChecked, formData, commonPeriod);
            }

            // Step 2: Fetch RatingDefinition by name and SACCO type
            var saccoType = "0";
            var ratingDefinition = await FetchRatingDefinitionAsync(ratingName, saccoType);
            if (ratingDefinition == null)
            {
                processingSummary.Add($"No RatingDefinition found for {ratingName} and SACCO type {saccoType}.");
                return (false, processingSummary, consistencyErrors, hasConsistencyBeenChecked, formData, commonPeriod);
            }

            // Step 3: Parse and validate uploaded forms
            var requiredFormCodes = ratingDefinition.RatingForms.Select(rf => rf.FormCode).ToList();
            var formDataMap = await ParseFormsAsync(createFormDTO, requiredFormCodes, processingSummary);
            if (formDataMap == null || !formDataMap.Any())
            {
                return (false, processingSummary, consistencyErrors, hasConsistencyBeenChecked, formData, commonPeriod);
            }

            // Step 4: Check for missing required forms
            var missingForms = requiredFormCodes.Except(formDataMap.Keys).ToList();
            if (missingForms.Any())
            {
                processingSummary.Add($"Missing required forms: {string.Join(", ", missingForms)}");
                return (false, processingSummary, consistencyErrors, hasConsistencyBeenChecked, formData, commonPeriod);
            }

            // Step 5: Check period consistency
            /* var periodCheck = await CheckPeriodConsistencyAsync(formDataMap.Values.ToList());
            if (!periodCheck.IsValid)
            {
                processingSummary.Add(periodCheck.Message);
                commonPeriod = periodCheck.CommonPeriod;
                return (false, processingSummary, consistencyErrors, hasConsistencyBeenChecked, formData, commonPeriod);
            }
            commonPeriod = periodCheck.CommonPeriod;
            hasConsistencyBeenChecked = true;*/

            // Step 6: Perform consistency validation
            var validationResult = await ValidateConsistencyAsync(formDataMap);
            if (!validationResult.IsValid)
            {
                consistencyErrors.AddRange(validationResult.ValidationErrors);
                //await GenerateAndSaveReportAsync(validationResult, commonPeriod, saccoType);
            }

            formData.AddRange(formDataMap.Values);
            return (validationResult.IsValid, processingSummary, consistencyErrors, hasConsistencyBeenChecked, formData, commonPeriod);
        }

        private (bool HasAttachments, string ErrorMessage) ValidateAttachments(NewReturnDTO dto)
        {
            if (dto.FormUploads == null)
            {
                return (false, "No attachments found. Please attach at least one form.");
            }
            return (true, string.Empty);
        }

        private async Task<RatingDefination?> FetchRatingDefinitionAsync(string ratingName, string saccoType)
        {
            return await _context.RatingDefinations
                .Include(rd => rd.RatingForms)
                .FirstOrDefaultAsync(rd => rd.RatingName == ratingName && rd.SaccoType == saccoType);
        }

        private async Task<Dictionary<string, object>> ParseFormsAsync(NewReturnDTO dto, List<string> requiredFormCodes, List<string> processingSummary)
        {
            var formDataMap = new Dictionary<string, object>();
            foreach (var form in dto.FormUploads)
            {
                if (form.formFile == null) continue;

                var returnForm = await _context.ReturnForms
                    .FirstOrDefaultAsync(f => f.Id == form.FormId);
                if (returnForm == null)
                {
                    processingSummary.Add($"Form with ID {form.FormId} not found.");
                    continue;
                }

                try
                {
                    var formData = await ParseFormDataAsync(returnForm, form.formFile);
                    if (formData != null && requiredFormCodes.Contains(returnForm.Code))
                    {
                        formDataMap[returnForm.Code] = formData;
                    }
                }
                catch (Exception ex)
                {
                    processingSummary.Add($"Error processing {form.formFile.FileName}: {ex.Message}");
                }
            }
            return formDataMap;
        }

        private async Task<object> ParseFormDataAsync(ReturnForm returnForm, IFormFile file)
        {
            return returnForm.Category switch
            {
                FormCategory.CapitalAdequacy => ExcelService.ImportCapitalAdequacyRows(file, _logger),
                FormCategory.LiquidityStatement => ExcelService.ImportLiquidityStatementRows(file, _logger),
                FormCategory.DepositReturn => ExcelService.ImportDepositRangeDataRows(file, _logger),
                FormCategory.RiskClassification => ExcelService.ImportRiskClassificationRows(file, _logger),
                FormCategory.InvestmentReturn => ExcelService.ImportInvestmentRows(file, _logger),
                FormCategory.FinancialPosition => ExcelService.ImportFinancialPositionRows(file, _logger),
                FormCategory.StatementOfComprehensiveIncome => ExcelService.ImportStatementOfComprehensiveIncomeRows(file, _logger),
                _ => null
            };
        }

        /* private async Task<(bool IsValid, string Message, string CommonPeriod)> CheckPeriodConsistencyAsync(List<object> forms)
         {
             var helper = new ReturnsHelper(_context);
             var formStatements = forms.Cast<IFormStatement>().ToList(); // Assume IFormStatement interface
             return helper.AreAllFormsInSamePeriod(formStatements.ToArray());
         }*/

        private async Task<ValidationResult> ValidateConsistencyAsync(Dictionary<string, object> formDataMap)
        {
            // Fetch current form codes from ReturnForms, taking the first active record per Category
            var returnForms = await _context.ReturnForms
                .Where(f => f.IsActive)
                .GroupBy(f => (FormCategory)f.Category)
                .ToDictionaryAsync(g => g.Key, g => g.First().Code);

            var result = new ValidationResult { IsValid = true };
            // Map formDataMap keys dynamically
            var form1Rows = returnForms.TryGetValue(FormCategory.CapitalAdequacy, out var capitalAdequacyCode) && formDataMap.ContainsKey(capitalAdequacyCode)
            ? ((dynamic)formDataMap[capitalAdequacyCode]).Rows as List<object> ?? new List<object>()
            : null;
            var form2Rows = returnForms.TryGetValue(FormCategory.LiquidityStatement, out var liquidityCode) && formDataMap.ContainsKey(liquidityCode)
                ? ((dynamic)formDataMap[liquidityCode]).Rows as List<object> ?? new List<object>()
                : null;
            var form3Rows = returnForms.TryGetValue(FormCategory.DepositReturn, out var depositReturnCode) && formDataMap.ContainsKey(depositReturnCode)
                ? ((dynamic)formDataMap[depositReturnCode]).Rows as List<object> ?? new List<object>()
                : null;
            var form4Rows = returnForms.TryGetValue(FormCategory.RiskClassification, out var riskClassificationCode) && formDataMap.ContainsKey(riskClassificationCode)
                ? ((dynamic)formDataMap[riskClassificationCode]).Rows as List<object> ?? new List<object>()
                : null;
            var form5Rows = returnForms.TryGetValue(FormCategory.InvestmentReturn, out var investmentCode) && formDataMap.ContainsKey(investmentCode)
                ? ((dynamic)formDataMap[investmentCode]).Rows as List<object> ?? new List<object>()
                : null;
            var form6Rows = returnForms.TryGetValue(FormCategory.FinancialPosition, out var financialPositionCode) && formDataMap.ContainsKey(financialPositionCode)
                ? ((dynamic)formDataMap[financialPositionCode]).Rows as List<object> ?? new List<object>()
                : null;
            var form7Rows = returnForms.TryGetValue(FormCategory.StatementOfComprehensiveIncome, out var incomeCode) && formDataMap.ContainsKey(incomeCode)
                ? ((dynamic)formDataMap[incomeCode]).Rows as List<object> ?? new List<object>()
                : null;

            decimal GetValue(string cell, List<object> rows) =>
                rows?.Cast<dynamic>().FirstOrDefault()?.GetType().GetProperty(cell)?.GetValue(rows.First()) ?? 0m;

            if (form1Rows != null && form6Rows != null && Math.Abs(GetValue("D10", form1Rows) - GetValue("C57", form6Rows)) > 0.01m)
                result.AddError("Capital Mismatch", "Share Capital", new() { { "D10", GetValue("D10", form1Rows).ToString("C") }, { "C57", GetValue("C57", form6Rows).ToString("C") } });
            if (form1Rows != null && form5Rows != null && Math.Abs(GetValue("D22", form1Rows) - GetValue("C8", form5Rows)) > 0.01m)
                result.AddError("Capital Mismatch", "Core Capital", new() { { "D22", GetValue("D22", form1Rows).ToString("C") }, { "C8", GetValue("C8", form5Rows).ToString("C") } });
            if (form1Rows != null && form6Rows != null && Math.Abs(GetValue("D11", form1Rows) - GetValue("C65", form6Rows)) > 0.01m)
                result.AddError("Capital Mismatch", "Statutory Reserves", new() { { "D11", GetValue("D11", form1Rows).ToString("C") }, { "C65", GetValue("C65", form6Rows).ToString("C") } });
            if (form1Rows != null && form6Rows != null && Math.Abs(GetValue("D12", form1Rows) - GetValue("C61", form6Rows)) > 0.01m)
                result.AddError("Capital Mismatch", "Retained Earnings", new() { { "D12", GetValue("D12", form1Rows).ToString("C") }, { "C61", GetValue("C61", form6Rows).ToString("C") } });
            if (form1Rows != null && form7Rows != null && Math.Abs(GetValue("D13", form1Rows) - (GetValue("C56", form7Rows) * 0.5m)) > 0.01m)
                result.AddError("Income Mismatch", "Net Surplus after Tax", new() { { "D13", GetValue("D13", form1Rows).ToString("C") }, { "50% of C56", (GetValue("C56", form7Rows) * 0.5m).ToString("C") } });
            if (form1Rows != null && form6Rows != null && Math.Abs(GetValue("D14", form1Rows) - GetValue("C58", form6Rows)) > 0.01m)
                result.AddError("Capital Mismatch", "Capital Grants", new() { { "D14", GetValue("D14", form1Rows).ToString("C") }, { "C58", GetValue("C58", form6Rows).ToString("C") } });
            if (form1Rows != null && form6Rows != null && Math.Abs(GetValue("D16", form1Rows) - GetValue("C66", form6Rows)) > 0.01m)
                result.AddError("Capital Mismatch", "Other Reserves", new() { { "D16", GetValue("D16", form1Rows).ToString("C") }, { "C66", GetValue("C66", form6Rows).ToString("C") } });
            if (form1Rows != null && form6Rows != null && Math.Abs(GetValue("D19", form1Rows) - GetValue("C20", form6Rows)) > 0.01m)
                result.AddError("Asset Mismatch", "Investment in Subsidiary", new() { { "D19", GetValue("D19", form1Rows).ToString("C") }, { "C20", GetValue("C20", form6Rows).ToString("C") } });
            if (form1Rows != null && form2Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D26", form1Rows) - GetValue("D8", form2Rows)) > 0.01m ||
                 Math.Abs(GetValue("D26", form1Rows) - GetValue("C11", form6Rows)) > 0.01m))
                result.AddError("Asset Mismatch", "Cash", new() { { "D26", GetValue("D26", form1Rows).ToString("C") }, { "D8", GetValue("D8", form2Rows).ToString("C") }, { "C11", GetValue("C11", form6Rows).ToString("C") } });
            if (form1Rows != null && form2Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D28", form1Rows) - GetValue("D12", form2Rows)) > 0.01m ||
                 Math.Abs(GetValue("D28", form1Rows) - GetValue("C12", form6Rows)) > 0.01m))
                result.AddError("Asset Mismatch", "Bank Balances", new() { { "D28", GetValue("D28", form1Rows).ToString("C") }, { "D12", GetValue("D12", form2Rows).ToString("C") }, { "C12", GetValue("C12", form6Rows).ToString("C") } });
            if (form1Rows != null && form2Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D27", form1Rows) - GetValue("D26", form2Rows)) > 0.01m ||
                 Math.Abs(GetValue("D27", form1Rows) - GetValue("C17", form6Rows)) > 0.01m))
                result.AddError("Asset Mismatch", "Government Securities", new() { { "D27", GetValue("D27", form1Rows).ToString("C") }, { "D26", GetValue("D26", form2Rows).ToString("C") }, { "C17", GetValue("C17", form6Rows).ToString("C") } });
            if (form1Rows != null && form4Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D29", form1Rows) - GetValue("D23", form4Rows)) > 0.01m ||
                 Math.Abs(GetValue("D29", form1Rows) - GetValue("C23", form6Rows)) > 0.01m))
                result.AddError("Asset Mismatch", "Loans and Advances", new() { { "D29", GetValue("D29", form1Rows).ToString("C") }, { "D23", GetValue("D23", form4Rows).ToString("C") }, { "C23", GetValue("C23", form6Rows).ToString("C") } });
            if (form1Rows != null && form5Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D30", form1Rows) - GetValue("C12", form5Rows)) > 0.01m ||
                 Math.Abs(GetValue("D30", form1Rows) - GetValue("C16", form6Rows)) > 0.01m))
                result.AddError("Asset Mismatch", "Investments", new() { { "D30", GetValue("D30", form1Rows).ToString("C") }, { "C12", GetValue("C12", form5Rows).ToString("C") }, { "C16", GetValue("C16", form6Rows).ToString("C") } });
            if (form1Rows != null && form5Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D31", form1Rows) - GetValue("C13", form5Rows)) > 0.01m ||
                 Math.Abs(GetValue("D31", form1Rows) - GetValue("C33", form6Rows)) > 0.01m))
                result.AddError("Asset Mismatch", "Property and Equipment", new() { { "D31", GetValue("D31", form1Rows).ToString("C") }, { "C13", GetValue("C13", form5Rows).ToString("C") }, { "C33", GetValue("C33", form6Rows).ToString("C") } });
            if (form1Rows != null && form5Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D34", form1Rows) - GetValue("C9", form5Rows)) > 0.01m ||
                 Math.Abs(GetValue("D34", form1Rows) - GetValue("C38", form6Rows)) > 0.01m))
                result.AddError("Asset Mismatch", "Total Assets", new() { { "D34", GetValue("D34", form1Rows).ToString("C") }, { "C9", GetValue("C9", form5Rows).ToString("C") }, { "C38", GetValue("C38", form6Rows).ToString("C") } });
            if (form6Rows != null && Math.Abs(GetValue("C38", form6Rows) - GetValue("C72", form6Rows)) > 0.01m)
                result.AddError("Liability Mismatch", "Total Liabilities and Equity", new() { { "C38", GetValue("C38", form6Rows).ToString("C") }, { "C72", GetValue("C72", form6Rows).ToString("C") } });
            if (form2Rows != null && form3Rows != null && form5Rows != null && form6Rows != null &&
                (Math.Abs(GetValue("D32", form2Rows) - GetValue("E26", form3Rows)) > 0.01m ||
                 Math.Abs(GetValue("D32", form2Rows) - GetValue("C10", form5Rows)) > 0.01m ||
                 Math.Abs(GetValue("D32", form2Rows) - GetValue("C45", form6Rows)) > 0.01m))
                result.AddError("Liability Mismatch", "Total Deposit Liability", new() { { "D32", GetValue("D32", form2Rows).ToString("C") }, { "E26", GetValue("E26", form3Rows).ToString("C") }, { "C10", GetValue("C10", form5Rows).ToString("C") }, { "C45", GetValue("C45", form6Rows).ToString("C") } });
            if (form1Rows != null && form6Rows != null && Math.Abs(GetValue("D32", form1Rows) - GetValue("C36", form6Rows)) > 0.01m)
                result.AddError("Asset Mismatch", "Other Assets", new() { { "D32", GetValue("D32", form1Rows).ToString("C") }, { "C36", GetValue("C36", form6Rows).ToString("C") } });
            if (form6Rows != null && form7Rows != null && Math.Abs(GetValue("C62", form6Rows) - GetValue("C56", form7Rows)) > 0.01m)
                result.AddError("Income Mismatch", "Current Year Surplus", new() { { "C62", GetValue("C62", form6Rows).ToString("C") }, { "C56", GetValue("C56", form7Rows).ToString("C") } });
            decimal nonEarningForm5 = form5Rows != null ? GetValue("C11", form5Rows) : 0m;
            decimal nonEarningForm6 = form6Rows != null ? GetValue("C34", form6Rows) + GetValue("C35", form6Rows) +
                GetValue("C36", form6Rows) + GetValue("C26", form6Rows) + GetValue("C14", form6Rows) : 0m;
            if (Math.Abs(nonEarningForm5 - nonEarningForm6) > 0.01m)
                result.AddError("Asset Mismatch", "Non-Earning Assets", new() { { "C11", nonEarningForm5.ToString("C") }, { "C34+C35+C36+C26+C14", nonEarningForm6.ToString("C") } });

            // Note: Categories SectoralLending (5), DailyLiquidity (6), Management (7), InsiderLending (8) are not used. Confirm if additional rules are needed.
            return result;

        }
        /* private async Task GenerateAndSaveReportAsync(ValidationResult result, string period, string saccoType)
         {
             var report = new ConsistencyReport(result, period, saccoType);
             var pdfBytes = report.GeneratePdf();
             await FormsHelper.SaveReportAsync(pdfBytes, "ConsistencyReport", saccoType, period);
         }*/
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<ValidationError> ValidationErrors { get; } = new();

        public void AddError(string category, string description, Dictionary<string, string> details)
        {
            ValidationErrors.Add(new ValidationError { Category = category, Description = description, Details = details });
            IsValid = false;
        }
    }

    // Ensure this matches ReturnAnalysisHelper.ValidationError
    public class ValidationError
    {
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Dictionary<string, string> Details { get; set; } = new();
    }
}

