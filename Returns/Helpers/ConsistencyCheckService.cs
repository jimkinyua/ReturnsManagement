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

        public async Task<(bool IsValid, List<string> ProcessingSummary, List<ValidationError> ConsistencyErrors, bool HasConsistencyBeenChecked, List<object> FormData, string CommonPeriod)> CheckConsistencyForDTAsync(NewReturnDTO createFormDTO, string ratingName)
        {
            var processingSummary = new List<string>();
            var consistencyErrors = new List<ValidationError>();
            var formData = new List<object>();
            string commonPeriod = null;

            try
            {
                // Step 1: Validate input
                if (createFormDTO?.FormUploads == null || !createFormDTO.FormUploads.Any())
                {
                    processingSummary.Add("No files uploaded. Please attach at least one form.");
                    return (false, processingSummary, consistencyErrors, false, formData, commonPeriod);
                }

                // Step 2: Get rating definition
                var ratingDefinition = await GetRatingDefinitionAsync(ratingName, processingSummary);
                if (ratingDefinition == null)
                {
                    return (false, processingSummary, consistencyErrors, false, formData, commonPeriod);
                }

                // Step 3: Parse uploaded forms
                var parsedForms = await ParseUploadedFormsAsync(createFormDTO, ratingDefinition, processingSummary);
                if (!parsedForms.Any())
                {
                    return (false, processingSummary, consistencyErrors, false, formData, commonPeriod);
                }

                // Step 4: Check for missing required forms
                var missingForms = CheckMissingRequiredForms(parsedForms, ratingDefinition, processingSummary);
                if (missingForms.Any())
                {
                    return (false, processingSummary, consistencyErrors, false, formData, commonPeriod);
                }

                // Step 5: Perform consistency validation
                var validationResult = await PerformConsistencyValidationAsync(parsedForms);
                consistencyErrors.AddRange(validationResult.ValidationErrors);

                formData.AddRange(parsedForms.Values);
                return (validationResult.IsValid, processingSummary, consistencyErrors, true, formData, commonPeriod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during consistency check");
                processingSummary.Add($"Unexpected error: {ex.Message}");
                return (false, processingSummary, consistencyErrors, false, formData, commonPeriod);
            }
        }

        private async Task<RatingDefination?> GetRatingDefinitionAsync(string ratingName, List<string> processingSummary)
        {
            try
            {
                var ratingDefinition = await _context.RatingDefinations
                    .Include(rd => rd.RatingForms)
                    .FirstOrDefaultAsync(rd => rd.RatingName == ratingName && rd.SaccoType == "0");

                if (ratingDefinition == null)
                {
                    processingSummary.Add($"No rating definition found for '{ratingName}' and SACCO type '0'.");
                    return null;
                }

                return ratingDefinition;
            }
            catch (Exception ex)
            {
                processingSummary.Add($"Error fetching rating definition: {ex.Message}");
                return null;
            }
        }

        private async Task<Dictionary<string, object>> ParseUploadedFormsAsync(NewReturnDTO dto, RatingDefination ratingDefinition, List<string> processingSummary)
        {
            var parsedForms = new Dictionary<string, object>();
            var requiredFormCodes = ratingDefinition.RatingForms.Select(rf => rf.FormCode).ToList();

            foreach (var formUpload in dto.FormUploads)
            {
                if (formUpload.formFile == null) continue;

                try
                {
                    var returnForm = await _context.ReturnForms
                        .FirstOrDefaultAsync(f => f.Id == formUpload.FormId);

                    if (returnForm == null)
                    {
                        processingSummary.Add($"Form with ID {formUpload.FormId} not found in database.");
                        continue;
                    }

                    if (!requiredFormCodes.Contains(returnForm.Code))
                    {
                        processingSummary.Add($"Form {returnForm.Code} is not required for this rating definition.");
                        continue;
                    }

                    var formData = await ParseFormDataAsync(returnForm, formUpload.formFile);
                    if (formData != null)
                    {
                        parsedForms[returnForm.Code] = formData;
                        processingSummary.Add($"Successfully parsed {returnForm.Code} from {formUpload.formFile.FileName}");
                    }
                    else
                    {
                        processingSummary.Add($"Failed to parse data from {formUpload.formFile.FileName}");
                    }
                }
                catch (Exception ex)
                {
                    processingSummary.Add($"Error processing {formUpload.formFile?.FileName}: {ex.Message}");
                }
            }

            return parsedForms;
        }

        private async Task<object?> ParseFormDataAsync(ReturnForm returnForm, IFormFile file)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing form {returnForm.Code}");
                return null;
            }
        }

        private List<string> CheckMissingRequiredForms(Dictionary<string, object> parsedForms, RatingDefination ratingDefinition, List<string> processingSummary)
        {
            var requiredFormCodes = ratingDefinition.RatingForms.Select(rf => rf.FormCode).ToList();
            var missingForms = requiredFormCodes.Except(parsedForms.Keys).ToList();

            if (missingForms.Any())
            {
                processingSummary.Add($"Missing required forms: {string.Join(", ", missingForms)}");
            }

            return missingForms;
        }

        private async Task<ValidationResult> PerformConsistencyValidationAsync(Dictionary<string, object> formDataMap)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // Get form data by category
                var capitalAdequacy = GetFormDataByCategory(formDataMap, FormCategory.CapitalAdequacy);
                var liquidity = GetFormDataByCategory(formDataMap, FormCategory.LiquidityStatement);
                var depositReturn = GetFormDataByCategory(formDataMap, FormCategory.DepositReturn);
                var riskClassification = GetFormDataByCategory(formDataMap, FormCategory.RiskClassification);
                var investment = GetFormDataByCategory(formDataMap, FormCategory.InvestmentReturn);
                var financialPosition = GetFormDataByCategory(formDataMap, FormCategory.FinancialPosition);
                var comprehensiveIncome = GetFormDataByCategory(formDataMap, FormCategory.StatementOfComprehensiveIncome);

                // Perform consistency checks
                PerformCapitalConsistencyChecks(capitalAdequacy, financialPosition, comprehensiveIncome, result);
                PerformAssetConsistencyChecks(capitalAdequacy, liquidity, riskClassification, investment, financialPosition, result);
                PerformLiabilityConsistencyChecks(liquidity, depositReturn, investment, financialPosition, result);
                PerformIncomeConsistencyChecks(financialPosition, comprehensiveIncome, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during consistency validation");
                result.AddError("System Error", "An error occurred during consistency validation", new Dictionary<string, string> { { "Error", ex.Message } });
            }

            return result;
        }

        private dynamic? GetFormDataByCategory(Dictionary<string, object> formDataMap, FormCategory category)
        {
            var formCode = GetFormCodeByCategory(category);
            return formDataMap.TryGetValue(formCode, out var data) ? data : null;
        }

        private string GetFormCodeByCategory(FormCategory category)
        {
            // This should be fetched from database, but for now using hardcoded mapping
            return category switch
            {
                FormCategory.CapitalAdequacy => "FORM1",
                FormCategory.LiquidityStatement => "FORM2",
                FormCategory.DepositReturn => "FORM3",
                FormCategory.RiskClassification => "FORM4",
                FormCategory.InvestmentReturn => "FORM5",
                FormCategory.FinancialPosition => "FORM6",
                FormCategory.StatementOfComprehensiveIncome => "FORM7",
                _ => string.Empty
            };
        }

        private void PerformCapitalConsistencyChecks(dynamic? capitalAdequacy, dynamic? financialPosition, dynamic? comprehensiveIncome, ValidationResult result)
        {
            if (capitalAdequacy?.Rows == null || financialPosition?.Rows == null) return;

            var capitalRows = capitalAdequacy.Rows as List<object>;
            var financialRows = financialPosition.Rows as List<object>;

            if (capitalRows == null || financialRows == null) return;

            // Share Capital check
            CheckConsistency("Share Capital", "D10", "C57", capitalRows, financialRows, result);
            
            // Statutory Reserves check
            CheckConsistency("Statutory Reserves", "D11", "C65", capitalRows, financialRows, result);
            
            // Retained Earnings check
            CheckConsistency("Retained Earnings", "D12", "C61", capitalRows, financialRows, result);
            
            // Capital Grants check
            CheckConsistency("Capital Grants", "D14", "C58", capitalRows, financialRows, result);
            
            // Other Reserves check
            CheckConsistency("Other Reserves", "D16", "C66", capitalRows, financialRows, result);

            // Net Surplus after Tax check (50% of comprehensive income)
            if (comprehensiveIncome?.Rows != null)
            {
                var incomeRows = comprehensiveIncome.Rows as List<object>;
                if (incomeRows != null)
                {
                    var capitalValue = GetValue("D13", capitalRows);
                    var incomeValue = GetValue("C56", incomeRows) * 0.5m;
                    
                    if (Math.Abs(capitalValue - incomeValue) > 0.01m)
                    {
                        result.AddError("Income Mismatch", "Net Surplus after Tax", 
                            new Dictionary<string, string> 
                            { 
                                { "D13", capitalValue.ToString("C") }, 
                                { "50% of C56", incomeValue.ToString("C") } 
                            });
                    }
                }
            }
        }

        private void PerformAssetConsistencyChecks(dynamic? capitalAdequacy, dynamic? liquidity, dynamic? riskClassification, 
            dynamic? investment, dynamic? financialPosition, ValidationResult result)
        {
            if (capitalAdequacy?.Rows == null || financialPosition?.Rows == null) return;

            var capitalRows = capitalAdequacy.Rows as List<object>;
            var financialRows = financialPosition.Rows as List<object>;
            var liquidityRows = liquidity?.Rows as List<object>;
            var riskRows = riskClassification?.Rows as List<object>;
            var investmentRows = investment?.Rows as List<object>;

            if (capitalRows == null || financialRows == null) return;

            // Investment in Subsidiary
            CheckConsistency("Investment in Subsidiary", "D19", "C20", capitalRows, financialRows, result);

            // Cash (check against both liquidity and financial position)
            if (liquidityRows != null)
            {
                var cashCapital = GetValue("D26", capitalRows);
                var cashLiquidity = GetValue("D8", liquidityRows);
                var cashFinancial = GetValue("C11", financialRows);

                if (Math.Abs(cashCapital - cashLiquidity) > 0.01m || Math.Abs(cashCapital - cashFinancial) > 0.01m)
                {
                    result.AddError("Asset Mismatch", "Cash", 
                        new Dictionary<string, string> 
                        { 
                            { "D26", cashCapital.ToString("C") }, 
                            { "D8", cashLiquidity.ToString("C") }, 
                            { "C11", cashFinancial.ToString("C") } 
                        });
                }
            }

            // Bank Balances
            if (liquidityRows != null)
            {
                CheckConsistency("Bank Balances", "D28", "D12", capitalRows, liquidityRows, result);
                CheckConsistency("Bank Balances", "D28", "C12", capitalRows, financialRows, result);
            }

            // Government Securities
            if (liquidityRows != null)
            {
                CheckConsistency("Government Securities", "D27", "D26", capitalRows, liquidityRows, result);
                CheckConsistency("Government Securities", "D27", "C17", capitalRows, financialRows, result);
            }

            // Loans and Advances
            if (riskRows != null)
            {
                CheckConsistency("Loans and Advances", "D29", "D23", capitalRows, riskRows, result);
                CheckConsistency("Loans and Advances", "D29", "C23", capitalRows, financialRows, result);
            }

            // Investments
            if (investmentRows != null)
            {
                CheckConsistency("Investments", "D30", "C12", capitalRows, investmentRows, result);
                CheckConsistency("Investments", "D30", "C16", capitalRows, financialRows, result);
            }

            // Property and Equipment
            if (investmentRows != null)
            {
                CheckConsistency("Property and Equipment", "D31", "C13", capitalRows, investmentRows, result);
                CheckConsistency("Property and Equipment", "D31", "C33", capitalRows, financialRows, result);
            }

            // Total Assets
            if (investmentRows != null)
            {
                CheckConsistency("Total Assets", "D34", "C9", capitalRows, investmentRows, result);
                CheckConsistency("Total Assets", "D34", "C38", capitalRows, financialRows, result);
            }

            // Other Assets
            CheckConsistency("Other Assets", "D32", "C36", capitalRows, financialRows, result);
        }

        private void PerformLiabilityConsistencyChecks(dynamic? liquidity, dynamic? depositReturn, dynamic? investment, 
            dynamic? financialPosition, ValidationResult result)
        {
            if (financialPosition?.Rows == null) return;

            var financialRows = financialPosition.Rows as List<object>;
            var liquidityRows = liquidity?.Rows as List<object>;
            var depositRows = depositReturn?.Rows as List<object>;
            var investmentRows = investment?.Rows as List<object>;

            if (financialRows == null) return;

            // Total Liabilities and Equity
            var totalAssets = GetValue("C38", financialRows);
            var totalLiabilities = GetValue("C72", financialRows);
            if (Math.Abs(totalAssets - totalLiabilities) > 0.01m)
            {
                result.AddError("Liability Mismatch", "Total Liabilities and Equity", 
                    new Dictionary<string, string> 
                    { 
                        { "C38", totalAssets.ToString("C") }, 
                        { "C72", totalLiabilities.ToString("C") } 
                    });
            }

            // Total Deposit Liability
            if (liquidityRows != null && depositRows != null && investmentRows != null)
            {
                var liquidityDeposits = GetValue("D32", liquidityRows);
                var depositReturnTotal = GetValue("E26", depositRows);
                var investmentDeposits = GetValue("C10", investmentRows);
                var financialDeposits = GetValue("C45", financialRows);

                if (Math.Abs(liquidityDeposits - depositReturnTotal) > 0.01m || 
                    Math.Abs(liquidityDeposits - investmentDeposits) > 0.01m || 
                    Math.Abs(liquidityDeposits - financialDeposits) > 0.01m)
                {
                    result.AddError("Liability Mismatch", "Total Deposit Liability", 
                        new Dictionary<string, string> 
                        { 
                            { "D32", liquidityDeposits.ToString("C") }, 
                            { "E26", depositReturnTotal.ToString("C") }, 
                            { "C10", investmentDeposits.ToString("C") }, 
                            { "C45", financialDeposits.ToString("C") } 
                        });
                }
            }
        }

        private void PerformIncomeConsistencyChecks(dynamic? financialPosition, dynamic? comprehensiveIncome, ValidationResult result)
        {
            if (financialPosition?.Rows == null || comprehensiveIncome?.Rows == null) return;

            var financialRows = financialPosition.Rows as List<object>;
            var incomeRows = comprehensiveIncome.Rows as List<object>;

            if (financialRows == null || incomeRows == null) return;

            // Current Year Surplus
            var financialSurplus = GetValue("C62", financialRows);
            var incomeSurplus = GetValue("C56", incomeRows);
            
            if (Math.Abs(financialSurplus - incomeSurplus) > 0.01m)
            {
                result.AddError("Income Mismatch", "Current Year Surplus", 
                    new Dictionary<string, string> 
                    { 
                        { "C62", financialSurplus.ToString("C") }, 
                        { "C56", incomeSurplus.ToString("C") } 
                    });
            }

            // Non-Earning Assets
            var nonEarningForm5 = GetValue("C11", incomeRows);
            var nonEarningForm6 = GetValue("C34", financialRows) + GetValue("C35", financialRows) + 
                                 GetValue("C36", financialRows) + GetValue("C26", financialRows) + GetValue("C14", financialRows);
            
            if (Math.Abs(nonEarningForm5 - nonEarningForm6) > 0.01m)
            {
                result.AddError("Asset Mismatch", "Non-Earning Assets", 
                    new Dictionary<string, string> 
                    { 
                        { "C11", nonEarningForm5.ToString("C") }, 
                        { "C34+C35+C36+C26+C14", nonEarningForm6.ToString("C") } 
                    });
            }
        }

        private void CheckConsistency(string description, string cell1, string cell2, List<object> rows1, List<object> rows2, ValidationResult result)
        {
            var value1 = GetValue(cell1, rows1);
            var value2 = GetValue(cell2, rows2);
            
            if (Math.Abs(value1 - value2) > 0.01m)
            {
                result.AddError("Consistency Error", description, 
                    new Dictionary<string, string> 
                    { 
                        { cell1, value1.ToString("C") }, 
                        { cell2, value2.ToString("C") } 
                    });
            }
        }

        private decimal GetValue(string cell, List<object> rows)
        {
            try
            {
                var firstRow = rows.FirstOrDefault();
                if (firstRow == null) return 0m;

                var property = firstRow.GetType().GetProperty(cell);
                if (property == null) return 0m;

                var value = property.GetValue(firstRow);
                return value is decimal d ? d : 0m;
            }
            catch
            {
                return 0m;
            }
        }
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

    public class ValidationError
    {
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Dictionary<string, string> Details { get; set; } = new();
    }
}

