using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Text.Json;
using static Returns.Helpers.ReturnAnalysisHelper;
using static Returns.Helpers.TokenHelper;

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

        public async Task<(bool IsValid, List<string> ProcessingSummary, List<ValidationError> ConsistencyErrors, bool HasConsistencyBeenChecked, List<object> FormData, string? CommonPeriod)> CheckConsistencyAsync(NewReturnDTO createFormDTO, string ratingName, LoggedInEntity  loggedInEntity)
        {
            var processingSummary = new List<string>();
            var consistencyErrors = new List<ValidationError>();
            var formData = new List<object>();
            string? commonPeriod = null; 

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
                var validationResult = PerformConsistencyValidation(parsedForms, loggedInEntity.SaccoType);
                consistencyErrors.AddRange(validationResult.ValidationErrors);

                // Step 6: Save or update the consistency check result (upsert)
                await SaveOrUpdateConsistencyCheckAsync(createFormDTO, loggedInEntity, validationResult.IsValid, consistencyErrors, ratingDefinition.Id);

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

        private async Task SaveOrUpdateConsistencyCheckAsync(NewReturnDTO createFormDTO, LoggedInEntity  loggedInEntity, bool isValid, List<ValidationError> errors, string ratingDefinitionId)
        {
            // Assume the first form's ExpectedReturn gives us the PeriodId (since all forms are for the same period in consistency check)
            var firstForm = createFormDTO.FormUploads.FirstOrDefault();
            if (firstForm == null) return;

            var expectedReturn = await _context.ExpectedReturns.FindAsync(firstForm.ExpectedReturnId);
            if (expectedReturn == null) return;

            var periodId = expectedReturn.PeriodId;
            var saccoId = loggedInEntity.SaccoId;

            // Serialize errors to JSON
            var errorsJson = JsonSerializer.Serialize(errors);

            // Check if existing record for this SaccoId + PeriodId
            var existingCheck = await _context.ConsistencyCheckResults
                .FirstOrDefaultAsync(cc => cc.SaccoId == saccoId && cc.PeriodId == periodId);

            if (existingCheck != null)
            {
                existingCheck.IsValid = isValid;
                existingCheck.ErrorsJson = errorsJson;
                existingCheck.CheckedAt = DateTime.UtcNow;
                existingCheck.RatingDefinitionId = ratingDefinitionId;
                _context.ConsistencyCheckResults.Update(existingCheck);
            }
            else
            {
                var newCheck = new ConsistencyCheckResult
                {
                    SaccoId = saccoId,
                    PeriodId = periodId,
                    RatingDefinitionId = ratingDefinitionId,
                    IsValid = isValid,
                    ErrorsJson = errorsJson,
                    CheckedAt = DateTime.UtcNow
                };
                _context.ConsistencyCheckResults.Add(newCheck);
            }

            await _context.SaveChangesAsync();
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

                    // Check if this form is required for the rating definition
                    /* if (!requiredFormCodes.Contains(returnForm.Code))
                     {
                         processingSummary.Add($"Form {returnForm.Code} is not required for this rating definition.");
                         continue;
                     }*/

                    var formData = await ParseFormDataAsync(returnForm, formUpload.formFile);
                    if (formData != null)
                    {
                        // Use FormCategory as key for consistency
                        var formCategory = returnForm.Category.ToString();
                        parsedForms[formCategory] = formData;
                        processingSummary.Add($"Successfully parsed {formCategory} from {formUpload.formFile.FileName}");
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
            // Get required form codes from the rating definition
            var requiredFormCodes = ratingDefinition.RatingForms.Select(rf => rf.FormCode).ToList();

            // Get the corresponding ReturnForms to check their categories
            var requiredFormCategories = new List<string>();
            foreach (var formCode in requiredFormCodes)
            {
                var returnForm = _context.ReturnForms.FirstOrDefault(f => f.Code == formCode);
                if (returnForm != null)
                {
                    requiredFormCategories.Add(returnForm.Category.ToString());
                }
            }

            var missingForms = requiredFormCategories.Except(parsedForms.Keys).ToList();

            if (missingForms.Any())
            {
                processingSummary.Add($"Missing required forms: {string.Join(", ", missingForms)}");
            }

            return missingForms;
        }

        private ValidationResult PerformConsistencyValidation(Dictionary<string, object> formDataMap,string SaccoType)
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

           
                if (string.Equals(SaccoType, "1"))
                {
                    // Perform NWDT consistency checks
                    PerformNWDTCapitalConsistencyChecks(capitalAdequacy, financialPosition, comprehensiveIncome, result);
                    PerformNWDTAssetConsistencyChecks(capitalAdequacy, liquidity, riskClassification, investment, financialPosition, result);
                    PerformNWDTLiabilityConsistencyChecks(liquidity, depositReturn, investment, financialPosition, result);
                    PerformNWDTIncomeConsistencyChecks(financialPosition, comprehensiveIncome, result);
                }
                else
                {
                    // Perform DT consistency checks
                    PerformCapitalConsistencyChecks(capitalAdequacy, financialPosition, comprehensiveIncome, result);
                    PerformAssetConsistencyChecks(capitalAdequacy, liquidity, riskClassification, investment, financialPosition, result);
                    PerformLiabilityConsistencyChecks(liquidity, depositReturn, investment, financialPosition, result);
                    PerformIncomeConsistencyChecks(financialPosition, comprehensiveIncome, result);
                }
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
            // Use FormCategory directly instead of hardcoded form codes
            return formDataMap.TryGetValue(category.ToString(), out var data) ? data : null;
        }

        private void PerformCapitalConsistencyChecks(dynamic? capitalAdequacy, dynamic? financialPosition, dynamic? comprehensiveIncome, ValidationResult result)
        {
            if (capitalAdequacy?.Rows == null || financialPosition?.Rows == null) return;

            var capitalRows = capitalAdequacy.Rows;
            var financialRows = financialPosition.Rows;

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
                var incomeRows = comprehensiveIncome.Rows;
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

        private void PerformNWDTCapitalConsistencyChecks(dynamic? capitalAdequacy, dynamic? financialPosition, dynamic? comprehensiveIncome, ValidationResult result)
        {
            if (capitalAdequacy?.Rows == null || financialPosition?.Rows == null) return;

            var capitalRows = capitalAdequacy.Rows;
            var financialRows = financialPosition.Rows;

            if (capitalRows == null || financialRows == null) return;

            // Share Capital check - D10 (Form 2A) should match C60 (Form 2G)
            CheckConsistency("Share Capital", "D10", "C60", capitalRows, financialRows, result);

            // Statutory Reserves check - D11 (Form 2A) should match C68 (Form 2G)
            CheckConsistency("Statutory Reserves", "D11", "C68", capitalRows, financialRows, result);

            // Retained Earnings check - D12 (Form 2A) should match C63 (Form 2G)
            CheckConsistency("Retained Earnings", "D12", "C63", capitalRows, financialRows, result);

            // Capital Grants check - D14 (Form 2A) should match C58 (Form 2G)
            CheckConsistency("Capital Grants", "D14", "C58", capitalRows, financialRows, result);

            // Other Reserves check - D16 (Form 2A) should match C66 (Form 2G)
            CheckConsistency("Other Reserves", "D16", "C66", capitalRows, financialRows, result);

            // Net Surplus after Tax check (50% of comprehensive income)
            if (comprehensiveIncome?.Rows != null)
            {
                var incomeRows = comprehensiveIncome.Rows;
                if (incomeRows != null)
                {
                    var capitalValue = GetValue("D13", capitalRows);
                    var incomeValue = GetValue("C58", incomeRows) * 0.5m;
                    var financialValue = GetValue("C65", financialRows);

                    // Check D13 (Form 2A) against 50% of C58 (Form 2F)
                    if (Math.Abs(capitalValue - incomeValue) > 0.01m)
                    {
                        result.AddError("Income Mismatch", "Net Surplus after Tax (50%)",
                            new Dictionary<string, string>
                            {
                                { "D13 (Form 2A)", capitalValue.ToString("C") },
                                { "50% of C58 (Form 2F)", incomeValue.ToString("C") }
                            });
                    }

                    // Check D13 (Form 2A) against C65 (Form 2G)
                    if (Math.Abs(capitalValue - financialValue) > 0.01m)
                    {
                        result.AddError("Income Mismatch", "Net Surplus after Tax",
                            new Dictionary<string, string>
                            {
                                { "D13 (Form 2A)", capitalValue.ToString("C") },
                                { "C65 (Form 2G)", financialValue.ToString("C") }
                            });
                    }
                }
            }
        }

        private void PerformAssetConsistencyChecks(dynamic? capitalAdequacy, dynamic? liquidity, dynamic? riskClassification,
            dynamic? investment, dynamic? financialPosition, ValidationResult result)
        {
            if (capitalAdequacy?.Rows == null || financialPosition?.Rows == null) return;

            var capitalRows = capitalAdequacy.Rows;
            var financialRows = financialPosition.Rows;
            var liquidityRows = liquidity?.Rows;
            var riskRows = riskClassification?.Rows;
            var investmentRows = investment?.Rows;

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

        private void PerformNWDTAssetConsistencyChecks(dynamic? capitalAdequacy, dynamic? liquidity, dynamic? riskClassification,
            dynamic? investment, dynamic? financialPosition, ValidationResult result)
        {
            if (capitalAdequacy?.Rows == null || financialPosition?.Rows == null) return;

            var capitalRows = capitalAdequacy.Rows;
            var financialRows = financialPosition.Rows;
            var liquidityRows = liquidity?.Rows;
            var riskRows = riskClassification?.Rows;
            var investmentRows = investment?.Rows;

            if (capitalRows == null || financialRows == null) return;

            // Core Capital check - D22 (Form 2A) should match C8 (Form 2E)
            if (investmentRows != null)
            {
                CheckConsistency("Core Capital", "D22", "C8", capitalRows, investmentRows, result);
            }

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
                            { "D26 (Form 2A)", cashCapital.ToString("C") },
                            { "D8 (Form 2B)", cashLiquidity.ToString("C") },
                            { "C11 (Form 2G)", cashFinancial.ToString("C") }
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

            var financialRows = financialPosition.Rows;
            var liquidityRows = liquidity?.Rows;
            var depositRows = depositReturn?.Rows;
            var investmentRows = investment?.Rows;

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

        private void PerformNWDTLiabilityConsistencyChecks(dynamic? liquidity, dynamic? depositReturn, dynamic? investment,
            dynamic? financialPosition, ValidationResult result)
        {
            if (financialPosition?.Rows == null) return;

            var financialRows = financialPosition.Rows;
            var liquidityRows = liquidity?.Rows;
            var depositRows = depositReturn?.Rows;
            var investmentRows = investment?.Rows;

            if (financialRows == null) return;

            // Total Liabilities and Equity
            var totalAssets = GetValue("C38", financialRows);
            var totalLiabilities = GetValue("C72", financialRows);
            if (Math.Abs(totalAssets - totalLiabilities) > 0.01m)
            {
                result.AddError("Liability Mismatch", "Total Liabilities and Equity",
                    new Dictionary<string, string>
                    {
                        { "C38 (Form 2G)", totalAssets.ToString("C") },
                        { "C72 (Form 2G)", totalLiabilities.ToString("C") }
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
                            { "D32 (Form 2B)", liquidityDeposits.ToString("C") },
                            { "E26 (Form 2C)", depositReturnTotal.ToString("C") },
                            { "C10 (Form 2E)", investmentDeposits.ToString("C") },
                            { "C45 (Form 2G)", financialDeposits.ToString("C") }
                        });
                }
            }
        }

        private void PerformIncomeConsistencyChecks(dynamic? financialPosition, dynamic? comprehensiveIncome, ValidationResult result)
        {
            if (financialPosition?.Rows == null || comprehensiveIncome?.Rows == null) return;

            var financialRows = financialPosition.Rows;
            var incomeRows = comprehensiveIncome.Rows;

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

        private void PerformNWDTIncomeConsistencyChecks(dynamic? financialPosition, dynamic? comprehensiveIncome, ValidationResult result)
        {
            if (financialPosition?.Rows == null || comprehensiveIncome?.Rows == null) return;

            var financialRows = financialPosition.Rows;
            var incomeRows = comprehensiveIncome.Rows;

            if (financialRows == null || incomeRows == null) return;

            // Current Year Surplus
            var financialSurplus = GetValue("C62", financialRows);
            var incomeSurplus = GetValue("C58", incomeRows);

            if (Math.Abs(financialSurplus - incomeSurplus) > 0.01m)
            {
                result.AddError("Income Mismatch", "Current Year Surplus",
                    new Dictionary<string, string>
                    {
                        { "C62 (Form 2G)", financialSurplus.ToString("C") },
                        { "C58 (Form 2F)", incomeSurplus.ToString("C") }
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
                        { "C11 (Form 2F)", nonEarningForm5.ToString("C") },
                        { "Sum of C34+C35+C36+C26+C14 (Form 2G)", nonEarningForm6.ToString("C") }
                    });
            }
        }

        private void CheckConsistency(string description, string cell1, string cell2, dynamic rows1, dynamic rows2, ValidationResult result)
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

        private decimal GetValue(string cell, dynamic rows)
        {
            try
            {
                if (rows == null) return 0m;

                // Search through all rows to find the one with matching CellNumberWithFigures
                foreach (var row in rows)
                {
                    var cellNumberProperty = row.GetType().GetProperty("CellNumberWithFigures");
                    var amountProperty = row.GetType().GetProperty("Amount");

                    if (cellNumberProperty != null && amountProperty != null)
                    {
                        var cellNumber = cellNumberProperty.GetValue(row)?.ToString();
                        if (cellNumber == cell)
                        {
                            var amount = amountProperty.GetValue(row);
                            return amount is decimal d ? d : 0m;
                        }
                    }
                }

                return 0m;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error getting value for cell {cell}: {ex.Message}");
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
}

