using Returns.Models;
using SASRAXRBSS.Dto.Returns_Analysis;
using static Returns.Helpers.ExcelService;
using System.Linq;
using System.Text;
using Returns.Migrations;
using static Returns.Helpers.ExcelService.Form2CStatement;
using Returns.DTOs.Returns.Return_Analysis_Result;

namespace Returns.Helpers
{
    public class ReturnAnalysisHelper
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
        }

        public class ValidationError
        {
            public string Category { get; set; } // e.g., "Capital Mismatch"
            public string Description { get; set; } // e.g., "Share Capital Mismatch"
            public Dictionary<string, string> Details { get; set; } // Key-value pairs for mismatched values
        }

        private static string FormatErrorMessage(string title, Dictionary<string, string> details)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"*** {title} ***");
            foreach (var kvp in details)
            {
                sb.AppendLine($" • {kvp.Key}: {kvp.Value}");
            }
            sb.AppendLine();
            return sb.ToString();
        }


        public static ValidationResult ValidateNWDTReturns(
       List<CapitalAdequacyRow> form1Rows,
       List<LiquidityStatementRow> form2Rows,
       List<DepositRangeRow> form3Rows,
       List<RiskClassificationRow> form4Rows,
       List<Form2ERow> form5Rows,
       List<StatementOfFinancialPositionRow> form6Rows,
       List<StatementOfComprehensiveIncomeRow> form7Rows)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {

                decimal shareCapitalForm1 = GetCapitalAdequacyAmount("D11", form1Rows);
                decimal shareCapitalForm6 = GetFinancialPositionAmount("C60", form6Rows);

                if (Math.Abs(shareCapitalForm1 - shareCapitalForm6) > 0.01m)
                {
                    if (Math.Abs(shareCapitalForm1 - shareCapitalForm6) > 0.01m)
                    {
                        result.ValidationErrors.Add(new ValidationError
                        {
                            Category = "Capital Mismatch", // Group errors by category
                            Description = "Share Capital Mismatch", // Specific error description
                            Details = new Dictionary<string, string>
                                {
                                    { "Form 1 (D11)", shareCapitalForm1.ToString("C") }, // Mismatched values
                                    { "Form 6 (C60)", shareCapitalForm6.ToString("C") }
                                }
                        });
                        result.IsValid = false;
                    }
                }

                decimal coreCapitalForm1 = GetCapitalAdequacyAmount("D22", form1Rows);
                decimal coreCapitalForm5 = GetInvestmentAmountNWDT("C8", form5Rows);

                if (Math.Abs(coreCapitalForm1 - coreCapitalForm5) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Core Capital Mismatch",
                        Details = new Dictionary<string, string>
                            {
                                { "Form 1 (D22)", coreCapitalForm1.ToString("C") },
                                { "Form 5 (C8)", coreCapitalForm5.ToString("C") }
                            }
                    });
                    result.IsValid = false;
                }

                // Statutory Reserves Validation (Form 1 D11 = Form 6 C65)
                decimal statutoryReservesForm1 = GetCapitalAdequacyAmount("D15", form1Rows);
                decimal statutoryReservesForm6 = GetFinancialPositionAmount("C68", form6Rows);

                if (Math.Abs(statutoryReservesForm1 - statutoryReservesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Statutory Reserves Mismatch",
                        Details = new Dictionary<string, string>
                        {
                            { "Form 1 (D11)", statutoryReservesForm1.ToString("C") },
                            { "Form 6 (C65)", statutoryReservesForm6.ToString("C") }
                        }
                    });
                    result.IsValid = false;
                }

                // Retained Earnings Validation (Form 1 D12 = Form 6 C60)
                decimal retainedEarningsForm1 = GetCapitalAdequacyAmount("D14", form1Rows);
                decimal retainedEarningsForm6 = GetFinancialPositionAmount("C63", form6Rows);
                if (Math.Abs(retainedEarningsForm1 - retainedEarningsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Retained Earnings Mismatch",
                        Details = new Dictionary<string, string>
                        {
                            { "Form 1 (D12)", retainedEarningsForm1.ToString("C") },
                            { "Form 6 (C60)", retainedEarningsForm6.ToString("C") }
                        }
                    });
                    result.IsValid = false;
                }

                // Net Surplus after Tax Validation (Form 1 D13 = Form 6 C62)
                decimal netSurplusForm1 = GetCapitalAdequacyAmount("D13", form1Rows);
                decimal netSurplusForm6 = GetFinancialPositionAmount("C62", form6Rows);
                if (Math.Abs(netSurplusForm1 - netSurplusForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Income Mismatch",
                        Description = "Net Surplus after Tax Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D13)", netSurplusForm1.ToString("C") },
            { "Form 6 (C62)", netSurplusForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Capital Grants Validation (Form 1 D14 = Form 6 C58)
                decimal capitalGrantsForm1 = GetCapitalAdequacyAmount("D12", form1Rows);
                decimal capitalGrantsForm6 = GetFinancialPositionAmount("C61", form6Rows);
                if (Math.Abs(capitalGrantsForm1 - capitalGrantsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Capital Grants Mismatch",
                        Details = new Dictionary<string, string>
                        {
                            { "Form 1 (D12)", capitalGrantsForm1.ToString("C") },
                            { "Form 6 (C61)", capitalGrantsForm6.ToString("C") }
                        }
                    });
                    result.IsValid = false;
                }

                // Other Reserves Validation (Form 1 D16 = Form 6 C66)
                decimal otherReservesForm1 = GetCapitalAdequacyAmount("D16", form1Rows);
                decimal otherReservesForm6 = GetFinancialPositionAmount("C69", form6Rows);
                if (Math.Abs(otherReservesForm1 - otherReservesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Other Reserves Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D16)", otherReservesForm1.ToString("C") },
            { "Form 6 (C69)", otherReservesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Investment in Subsidiary Validation (Form 1 D19 = Form 6 C20)
                decimal investmentSubsidiaryForm1 = GetCapitalAdequacyAmount("D19", form1Rows);
                decimal investmentSubsidiaryForm6 = GetFinancialPositionAmount("C23", form6Rows);
                if (Math.Abs(investmentSubsidiaryForm1 - investmentSubsidiaryForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Investment in Subsidiary Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D19)", investmentSubsidiaryForm1.ToString("C") },
            { "Form 6 (C23)", investmentSubsidiaryForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }


                // Cash Validation (Form 1 D26 = Form 2 D8 = Form 6 C11)
                decimal cashForm1 = GetCapitalAdequacyAmount("D26", form1Rows);
                decimal cashForm2 = GetLiquidityStatementAmount("D9", form2Rows);
                decimal cashForm6 = GetFinancialPositionAmount("C12", form6Rows);

                if (Math.Abs(cashForm1 - cashForm2) > 0.01m || Math.Abs(cashForm1 - cashForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Cash Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D26)", cashForm1.ToString("C") },
            { "Form 2 (D9)", cashForm2.ToString("C") },
            { "Form 6 (C12)", cashForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Bank Balances Validation (Form 1 D28 = Form 2 D12 = Form 6 C12)
                decimal bankBalancesForm1 = GetCapitalAdequacyAmount("D28", form1Rows);
                decimal bankBalancesForm2 = GetLiquidityStatementAmount("D13", form2Rows);
                decimal bankBalancesForm6 = GetFinancialPositionAmount("C13", form6Rows);

                if (Math.Abs(bankBalancesForm1 - bankBalancesForm2) > 0.01m || Math.Abs(bankBalancesForm1 - bankBalancesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Bank Balances Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D28)", bankBalancesForm1.ToString("C") },
            { "Form 2 (D13)", bankBalancesForm2.ToString("C") },
            { "Form 6 (C13)", bankBalancesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Government Securities Validation (Form 1 D27 = Form 2 D26 = Form 6 C17)
                decimal govtSecuritiesForm1 = GetCapitalAdequacyAmount("D27", form1Rows);
                decimal govtSecuritiesForm2 = GetLiquidityStatementAmount("D27", form2Rows);
                decimal govtSecuritiesForm6 = GetFinancialPositionAmount("C18", form6Rows);

                if (Math.Abs(govtSecuritiesForm1 - govtSecuritiesForm2) > 0.01m || Math.Abs(govtSecuritiesForm1 - govtSecuritiesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Government Securities Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D27)", govtSecuritiesForm1.ToString("C") },
            { "Form 2 (D27)", govtSecuritiesForm2.ToString("C") },
            { "Form 6 (C18)", govtSecuritiesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Loans and Advances Validation (Form 1 D29 = Form 4 D23 = Form 6 C23)
                decimal loansAdvancesForm1 = GetCapitalAdequacyAmount("D29", form1Rows);
                decimal loansAdvancesForm4 = GetRiskClassificationAmount("D24", form4Rows);
                decimal loansAdvancesForm6 = GetFinancialPositionAmount("C27", form6Rows);

                if (Math.Abs(loansAdvancesForm1 - loansAdvancesForm4) > 0.01m || Math.Abs(loansAdvancesForm1 - loansAdvancesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Loans and Advances Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D29)", loansAdvancesForm1.ToString("C") },
            { "Form 4 (D24)", loansAdvancesForm4.ToString("C") },
            { "Form 6 (C27)", loansAdvancesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Investments Validation (Form 1 D30 = Form 5 C12 = Form 6 C16)
                decimal investmentsForm1 = GetCapitalAdequacyAmount("D30", form1Rows);
                decimal investmentsForm5 = GetInvestmentAmountNWDT("C12", form5Rows);
                decimal investmentsForm6 = GetFinancialPositionAmount("C17", form6Rows);

                if (Math.Abs(investmentsForm1 - investmentsForm5) > 0.01m || Math.Abs(investmentsForm1 - investmentsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Investments Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D30)", investmentsForm1.ToString("C") },
            { "Form 5 (C12)", investmentsForm5.ToString("C") },
            { "Form 6 (C17)", investmentsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Property and Equipment Validation (Form 1 D31 = Form 5 C13 = Form 6 C33)
                decimal propertyEquipmentForm1 = GetCapitalAdequacyAmount("D31", form1Rows);
                decimal propertyEquipmentForm5 = GetInvestmentAmountNWDT("C13", form5Rows);
                decimal propertyEquipmentForm6 = GetFinancialPositionAmount("C33", form6Rows);

                if (Math.Abs(propertyEquipmentForm1 - propertyEquipmentForm5) > 0.01m || Math.Abs(propertyEquipmentForm1 - propertyEquipmentForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Property and Equipment Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D31)", propertyEquipmentForm1.ToString("C") },
            { "Form 5 (C13)", propertyEquipmentForm5.ToString("C") },
            { "Form 6 (C33)", propertyEquipmentForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Total Assets Validation (Form 1 D34 = Form 5 C9 = Form 6 C38)
                decimal totalAssetsForm1 = GetCapitalAdequacyAmount("D33", form1Rows);
                decimal totalAssetsForm5 = GetInvestmentAmountNWDT("C9", form5Rows);
                decimal totalAssetsForm6 = GetFinancialPositionAmount("C32", form6Rows);

                if (Math.Abs(totalAssetsForm1 - totalAssetsForm5) > 0.01m || Math.Abs(totalAssetsForm1 - totalAssetsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Total Assets Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D34)", totalAssetsForm1.ToString("C") },
            { "Form 5 (C9)", totalAssetsForm5.ToString("C") },
            { "Form 6 (C38)", totalAssetsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Total Liabilities and Equity (Form 6 C72 should match Total Assets)
                /* decimal totalLiabilitiesEquityForm6 = GetFinancialPositionAmount("C72", form6Rows);
                 if (Math.Abs(totalAssetsForm6 - totalLiabilitiesEquityForm6) > 0.01m)
                 {
                     result.ValidationMessages.Add(
                         FormatErrorMessage("Total Liabilities and Equity Mismatch",
                             new Dictionary<string, string>
                             {
                     { "Form 6 (C72)", totalLiabilitiesEquityForm6.ToString("C") },
                     { "Form 6 (C38)", totalAssetsForm6.ToString("C") }
                             }));
                     result.IsValid = false;
                 }*/

                // Total Deposit Liability (Form 2 D32 = Form 3 E26 = Form 5 C10 = Form 6 C45)
                // Total Deposit Liability Validation
                decimal depositLiabilityForm2 = GetLiquidityStatementAmount("D32", form2Rows);
                decimal depositLiabilityForm3 = GetDepositRangeAmountNWDT("20", form3Rows);
                decimal depositLiabilityForm5 = GetInvestmentAmountNWDT("C10", form5Rows);
                decimal depositLiabilityForm6 = GetFinancialPositionAmount("C46", form6Rows);

                if (Math.Abs(depositLiabilityForm2 - depositLiabilityForm3) > 0.01m ||
                    Math.Abs(depositLiabilityForm2 - depositLiabilityForm5) > 0.01m ||
                    Math.Abs(depositLiabilityForm2 - depositLiabilityForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Liability Mismatch",
                        Description = "Total Deposit Liability Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 2 (D32)", depositLiabilityForm2.ToString("C") },
            { "Form 3 (E26)", depositLiabilityForm3.ToString("C") },
            { "Form 5 (C10)", depositLiabilityForm5.ToString("C") },
            { "Form 6 (C45)", depositLiabilityForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Other Assets Validation
                decimal otherAssetsForm1 = GetCapitalAdequacyAmount("D32", form1Rows);
                decimal otherAssetsForm6 = GetFinancialPositionAmount("C40", form6Rows);

                if (Math.Abs(otherAssetsForm1 - otherAssetsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Other Assets Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D32)", otherAssetsForm1.ToString("C") },
            { "Form 6 (C40)", otherAssetsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Current Year Surplus Validation
                decimal surplusForm6 = GetFinancialPositionAmount("C65", form6Rows);
                decimal surplusForm7 = GetComprehensiveIncomeAmount("C58", form7Rows);

                if (Math.Abs(surplusForm6 - surplusForm7) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Income Mismatch",
                        Description = "Current Year Surplus Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 6 (C65)", surplusForm6.ToString("C") },
            { "Form 7 (C58)", surplusForm7.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Non-Earning Assets Validation
                decimal nonEarningAssetsForm5 = GetInvestmentAmountNWDT("C11", form5Rows);
                decimal computedNonEarningAssetsForm6 =
                    GetFinancialPositionAmount("C39", form6Rows) +
                    GetFinancialPositionAmount("C38", form6Rows) +
                    GetFinancialPositionAmount("C37", form6Rows) +
                    GetFinancialPositionAmount("C30", form6Rows) +
                    GetFinancialPositionAmount("C15", form6Rows);

                if (Math.Abs(nonEarningAssetsForm5 - computedNonEarningAssetsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Non-Earning Assets Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 5 (C11)", nonEarningAssetsForm5.ToString("C") },
            { "Form 6 (C39+C38+C37+C30+C15)", computedNonEarningAssetsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Final check for validation result
                result.IsValid = !result.ValidationErrors.Any();
            }
            catch (Exception ex)
            {
                result.ValidationErrors.Add(new ValidationError
                {
                    Category = "System Error",
                    Description = "An unexpected error occurred during validation.",
                    Details = new Dictionary<string, string>
                    {
                        { "Message", ex.Message },
                        { "StackTrace", ex.StackTrace }
                    }
                });
                result.IsValid = false;
            }

            return result;
        }

        public static ValidationResult ValidateReturns(
            List<CapitalAdequacyRow> form1Rows,
            List<LiquidityStatementRow> form2Rows,
            List<DepositRangeData> form3Rows,
            List<RiskClassificationRow> form4Rows,
            List<InvestmentRow> form5Rows,
            List<StatementOfFinancialPositionRow> form6Rows,
            List<StatementOfComprehensiveIncomeRow> form7Rows)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {

                decimal shareCapitalForm1 = GetCapitalAdequacyAmount("D10", form1Rows);
                decimal shareCapitalForm6 = GetFinancialPositionAmount("C57", form6Rows);

                // Share Capital Validation
                if (Math.Abs(shareCapitalForm1 - shareCapitalForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Share Capital Mismatch",
                        Details = new Dictionary<string, string>
                        {
                            { "Form 1 (D10)", shareCapitalForm1.ToString("C") },
                            { "Form 6 (C57)", shareCapitalForm6.ToString("C") }
                        }
                    });
                    result.IsValid = false;
                }

                // Core Capital Validation
                decimal coreCapitalForm1 = GetCapitalAdequacyAmount("D22", form1Rows);
                decimal coreCapitalForm5 = GetInvestmentAmount("C8", form5Rows);

                if (Math.Abs(coreCapitalForm1 - coreCapitalForm5) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Core Capital Mismatch",
                        Details = new Dictionary<string, string>
                        {
                            { "Form 1 (D22)", coreCapitalForm1.ToString("C") },
                            { "Form 5 (C8)", coreCapitalForm5.ToString("C") }
                        }
                    });
                    result.IsValid = false;
                }

                // Statutory Reserves Validation
                decimal statutoryReservesForm1 = GetCapitalAdequacyAmount("D11", form1Rows);
                decimal statutoryReservesForm6 = GetFinancialPositionAmount("C65", form6Rows);

                if (Math.Abs(statutoryReservesForm1 - statutoryReservesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Statutory Reserves Mismatch",
                        Details = new Dictionary<string, string>
                        {
                            { "Form 1 (D11)", statutoryReservesForm1.ToString("C") },
                            { "Form 6 (C65)", statutoryReservesForm6.ToString("C") }
                        }
                    });
                    result.IsValid = false;
                }

                // Retained Earnings Validation
                decimal retainedEarningsForm1 = GetCapitalAdequacyAmount("D12", form1Rows);
                decimal retainedEarningsForm6 = GetFinancialPositionAmount("C60", form6Rows);

                if (Math.Abs(retainedEarningsForm1 - retainedEarningsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Retained Earnings Mismatch",
                        Details = new Dictionary<string, string>
                            {
                                { "Form 1 (D12)", retainedEarningsForm1.ToString("C") },
                                { "Form 6 (C60)", retainedEarningsForm6.ToString("C") }
                            }
                    });
                    result.IsValid = false;
                }

                // Net Surplus after Tax Validation
                decimal netSurplusForm1 = GetCapitalAdequacyAmount("D13", form1Rows);
                decimal netSurplusForm6 = GetFinancialPositionAmount("C62", form6Rows);

                if (Math.Abs(netSurplusForm1 - netSurplusForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Income Mismatch",
                        Description = "Net Surplus after Tax Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D13)", netSurplusForm1.ToString("C") },
            { "Form 6 (C62)", netSurplusForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Capital Grants Validation
                decimal capitalGrantsForm1 = GetCapitalAdequacyAmount("D14", form1Rows);
                decimal capitalGrantsForm6 = GetFinancialPositionAmount("C58", form6Rows);

                if (Math.Abs(capitalGrantsForm1 - capitalGrantsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Capital Grants Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D14)", capitalGrantsForm1.ToString("C") },
            { "Form 6 (C58)", capitalGrantsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Other Reserves Validation
                decimal otherReservesForm1 = GetCapitalAdequacyAmount("D16", form1Rows);
                decimal otherReservesForm6 = GetFinancialPositionAmount("C66", form6Rows);

                if (Math.Abs(otherReservesForm1 - otherReservesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Capital Mismatch",
                        Description = "Other Reserves Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D16)", otherReservesForm1.ToString("C") },
            { "Form 6 (C66)", otherReservesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Investment in Subsidiary Validation
                decimal investmentSubsidiaryForm1 = GetCapitalAdequacyAmount("D19", form1Rows);
                decimal investmentSubsidiaryForm6 = GetFinancialPositionAmount("C20", form6Rows);

                if (Math.Abs(investmentSubsidiaryForm1 - investmentSubsidiaryForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Investment in Subsidiary Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D19)", investmentSubsidiaryForm1.ToString("C") },
            { "Form 6 (C20)", investmentSubsidiaryForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }
                // Cash Validation (Form 1 D26 = Form 2 D8 = Form 6 C11)
                decimal cashForm1 = GetCapitalAdequacyAmount("D26", form1Rows);
                decimal cashForm2 = GetLiquidityStatementAmount("D8", form2Rows);
                decimal cashForm6 = GetFinancialPositionAmount("C11", form6Rows);

                // Cash Validation
                if (Math.Abs(cashForm1 - cashForm2) > 0.01m || Math.Abs(cashForm1 - cashForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Cash Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D26)", cashForm1.ToString("C") },
            { "Form 2 (D8)", cashForm2.ToString("C") },
            { "Form 6 (C11)", cashForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Bank Balances Validation
                decimal bankBalancesForm1 = GetCapitalAdequacyAmount("D28", form1Rows);
                decimal bankBalancesForm2 = GetLiquidityStatementAmount("D12", form2Rows);
                decimal bankBalancesForm6 = GetFinancialPositionAmount("C12", form6Rows);

                if (Math.Abs(bankBalancesForm1 - bankBalancesForm2) > 0.01m || Math.Abs(bankBalancesForm1 - bankBalancesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Bank Balances Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D28)", bankBalancesForm1.ToString("C") },
            { "Form 2 (D12)", bankBalancesForm2.ToString("C") },
            { "Form 6 (C12)", bankBalancesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Government Securities Validation
                decimal govtSecuritiesForm1 = GetCapitalAdequacyAmount("D27", form1Rows);
                decimal govtSecuritiesForm2 = GetLiquidityStatementAmount("D26", form2Rows);
                decimal govtSecuritiesForm6 = GetFinancialPositionAmount("C17", form6Rows);

                if (Math.Abs(govtSecuritiesForm1 - govtSecuritiesForm2) > 0.01m || Math.Abs(govtSecuritiesForm1 - govtSecuritiesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Government Securities Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D27)", govtSecuritiesForm1.ToString("C") },
            { "Form 2 (D26)", govtSecuritiesForm2.ToString("C") },
            { "Form 6 (C17)", govtSecuritiesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Loans and Advances Validation
                decimal loansAdvancesForm1 = GetCapitalAdequacyAmount("D29", form1Rows);
                decimal loansAdvancesForm4 = GetRiskClassificationAmount("D23", form4Rows);
                decimal loansAdvancesForm6 = GetFinancialPositionAmount("C23", form6Rows);

                if (Math.Abs(loansAdvancesForm1 - loansAdvancesForm4) > 0.01m || Math.Abs(loansAdvancesForm1 - loansAdvancesForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Loans and Advances Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D29)", loansAdvancesForm1.ToString("C") },
            { "Form 4 (D23)", loansAdvancesForm4.ToString("C") },
            { "Form 6 (C23)", loansAdvancesForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Investments Validation
                decimal investmentsForm1 = GetCapitalAdequacyAmount("D30", form1Rows);
                decimal investmentsForm5 = GetInvestmentAmount("C12", form5Rows);
                decimal investmentsForm6 = GetFinancialPositionAmount("C16", form6Rows);

                if (Math.Abs(investmentsForm1 - investmentsForm5) > 0.01m || Math.Abs(investmentsForm1 - investmentsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Investments Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D30)", investmentsForm1.ToString("C") },
            { "Form 5 (C12)", investmentsForm5.ToString("C") },
            { "Form 6 (C16)", investmentsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Property and Equipment Validation
                decimal propertyEquipmentForm1 = GetCapitalAdequacyAmount("D31", form1Rows);
                decimal propertyEquipmentForm5 = GetInvestmentAmount("C13", form5Rows);
                decimal propertyEquipmentForm6 = GetFinancialPositionAmount("C33", form6Rows);

                if (Math.Abs(propertyEquipmentForm1 - propertyEquipmentForm5) > 0.01m || Math.Abs(propertyEquipmentForm1 - propertyEquipmentForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Property and Equipment Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D31)", propertyEquipmentForm1.ToString("C") },
            { "Form 5 (C13)", propertyEquipmentForm5.ToString("C") },
            { "Form 6 (C33)", propertyEquipmentForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Total Assets Validation
                decimal totalAssetsForm1 = GetCapitalAdequacyAmount("D34", form1Rows);
                decimal totalAssetsForm5 = GetInvestmentAmount("C9", form5Rows);
                decimal totalAssetsForm6 = GetFinancialPositionAmount("C38", form6Rows);

                if (Math.Abs(totalAssetsForm1 - totalAssetsForm5) > 0.01m || Math.Abs(totalAssetsForm1 - totalAssetsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Total Assets Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D34)", totalAssetsForm1.ToString("C") },
            { "Form 5 (C9)", totalAssetsForm5.ToString("C") },
            { "Form 6 (C38)", totalAssetsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Total Liabilities and Equity Validation
                decimal totalLiabilitiesEquityForm6 = GetFinancialPositionAmount("C72", form6Rows);
                if (Math.Abs(totalAssetsForm6 - totalLiabilitiesEquityForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Liability Mismatch",
                        Description = "Total Liabilities and Equity Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 6 (C72)", totalLiabilitiesEquityForm6.ToString("C") },
            { "Form 6 (C38)", totalAssetsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Total Deposit Liability Validation
                decimal depositLiabilityForm2 = GetLiquidityStatementAmount("D32", form2Rows);
                decimal depositLiabilityForm3 = GetDepositRangeAmount("E26", form3Rows);
                decimal depositLiabilityForm5 = GetInvestmentAmount("C10", form5Rows);
                decimal depositLiabilityForm6 = GetFinancialPositionAmount("C45", form6Rows);

                if (Math.Abs(depositLiabilityForm2 - depositLiabilityForm3) > 0.01m ||
                    Math.Abs(depositLiabilityForm2 - depositLiabilityForm5) > 0.01m ||
                    Math.Abs(depositLiabilityForm2 - depositLiabilityForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Liability Mismatch",
                        Description = "Total Deposit Liability Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 2 (D32)", depositLiabilityForm2.ToString("C") },
            { "Form 3 (E26)", depositLiabilityForm3.ToString("C") },
            { "Form 5 (C10)", depositLiabilityForm5.ToString("C") },
            { "Form 6 (C45)", depositLiabilityForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Other Assets Validation
                decimal otherAssetsForm1 = GetCapitalAdequacyAmount("D32", form1Rows);
                decimal otherAssetsForm6 = GetFinancialPositionAmount("C36", form6Rows);

                if (Math.Abs(otherAssetsForm1 - otherAssetsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Other Assets Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 1 (D32)", otherAssetsForm1.ToString("C") },
            { "Form 6 (C36)", otherAssetsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Current Year Surplus Validation
                decimal surplusForm6 = GetFinancialPositionAmount("C62", form6Rows);
                decimal surplusForm7 = GetComprehensiveIncomeAmount("C56", form7Rows);

                if (Math.Abs(surplusForm6 - surplusForm7) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Income Mismatch",
                        Description = "Current Year Surplus Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 6 (C62)", surplusForm6.ToString("C") },
            { "Form 7 (C56)", surplusForm7.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Non-Earning Assets Validation
                decimal nonEarningAssetsForm5 = GetInvestmentAmount("C11", form5Rows);
                decimal computedNonEarningAssetsForm6 =
                    GetFinancialPositionAmount("C34", form6Rows) +
                    GetFinancialPositionAmount("C35", form6Rows) +
                    GetFinancialPositionAmount("C36", form6Rows) +
                    GetFinancialPositionAmount("C26", form6Rows) +
                    GetFinancialPositionAmount("C14", form6Rows);

                if (Math.Abs(nonEarningAssetsForm5 - computedNonEarningAssetsForm6) > 0.01m)
                {
                    result.ValidationErrors.Add(new ValidationError
                    {
                        Category = "Asset Mismatch",
                        Description = "Non-Earning Assets Mismatch",
                        Details = new Dictionary<string, string>
        {
            { "Form 5 (C11)", nonEarningAssetsForm5.ToString("C") },
            { "Form 6 (C34+C35+C36+C26+C14)", computedNonEarningAssetsForm6.ToString("C") }
        }
                    });
                    result.IsValid = false;
                }

                // Final check for validation result
                result.IsValid = !result.ValidationErrors.Any();
            }
            catch (Exception ex)
            {
                result.ValidationErrors.Add(new ValidationError
                {
                    Category = "System Error",
                    Description = "An unexpected error occurred during validation.",
                    Details = new Dictionary<string, string>
        {
            { "Message", ex.Message },
            { "StackTrace", ex.StackTrace }
        }
                });
                result.IsValid = false;
            }

            return result;
        }

        private static decimal GetCapitalAdequacyAmount(string cellRef, List<CapitalAdequacyRow> capitalAdequacyRows)
        {
            return capitalAdequacyRows?.FirstOrDefault(r => r.CellNumberWithFigures == cellRef)?.Amount ?? 0;

        }

        private static decimal GetLiquidityStatementAmount(string cellRef, List<LiquidityStatementRow> liquidityRows)
        {
            return liquidityRows?.FirstOrDefault(r => r.CellNumberWithFigures == cellRef)?.Amount ?? 0;

        }

        private static decimal GetDepositRangeAmount(string cellRef, List<DepositRangeData> depositRows)
        {
            return depositRows?.FirstOrDefault(r => r.CellNumber == cellRef)?.AmountInKshs000 ?? 0;
        }
        private static decimal GetDepositRangeAmountNWDT(string cellRef, List<DepositRangeRow> depositRows)
        {
            return depositRows?.FirstOrDefault(r => r.CellNumberWithAmount == cellRef)?.Amount ?? 0;
        }

        private static decimal GetRiskClassificationAmount(string cellRef, List<RiskClassificationRow> riskRows)
        {
            return riskRows?.FirstOrDefault(r => r.OutstandingLoanPortfolioCellAddress == cellRef)?.OutstandingLoanPortfolio ?? 0;
        }
        private static decimal GetInvestmentAmount(string cellRef, List<InvestmentRow> investmentRows)
        {
            return investmentRows?.FirstOrDefault(r => r.cellNumberWithFigures == cellRef)?.Amount ?? 0;
        }


        private static decimal GetInvestmentAmountNWDT(string cellRef, List<Form2ERow> investmentRows)
        {
            return investmentRows?.FirstOrDefault(r => r.CellReference == cellRef)?.Amount ?? 0;
        }

        private static decimal GetFinancialPositionAmount(string cellRef, List<StatementOfFinancialPositionRow> financialPositionRows)
        {
            return financialPositionRows?.FirstOrDefault(r => r.CellNumberWithFigures == cellRef)?.Amount ?? 0;
        }

        private static decimal GetComprehensiveIncomeAmount(string cellRef, List<StatementOfComprehensiveIncomeRow> comprehensiveIncomeRows)
        {
            return comprehensiveIncomeRows?.FirstOrDefault(r => r.CellNumberWithFigures == cellRef)?.Amount ?? 0;
        }

        public static decimal CalculateAdjustedCCA(DTCapitalAdequacyReturn form1, DTFinancialPositionReturn form6)
        {
            decimal increasedProvisions = form6.AllowanceForLoanLoss * 1.5m;
            decimal stressedCoreCapital = form1.CoreCapital - (increasedProvisions - form6.AllowanceForLoanLoss);

            return form1.TotalAssets != 0 ? (stressedCoreCapital / form1.TotalAssets) * 100 : 0;
        }

        public static decimal CalculateNwdtAdjustedCCA(NWDTCapitalAdequacyReturn form1, NWDTFinancialPositionReturn form6)
        {
            decimal increasedProvisions = form6.AllowanceForLoanLoss * 1.5m;
            decimal stressedCoreCapital = form1.CoreCapital - (increasedProvisions - form6.AllowanceForLoanLoss);

            return form1.TotalAssets != 0 ? (stressedCoreCapital / form1.TotalAssets) * 100 : 0;
        }

        public class CapitalAnalysisData
        {
            public decimal CoreCapital { get; set; }
            public decimal CoreCapitalToAssetsRatio { get; set; }
            public decimal InstitutionalCapitalRatio { get; set; }
            public decimal CoreCapitalToDepositsRatio { get; set; }
            public decimal AdjustedCCARatio { get; set; }
        }


        public static decimal CalculateNonPerformingLoans(List<DTRiskClassificationReturn> riskClassificationData)
        {
            decimal nonPerformingLoans = riskClassificationData
                .Where(r => r.Classification == "Substandard" ||
                            r.Classification == "Doubtful" ||
                            r.Classification == "Loss")
                .Sum(r => r.OutstandingLoanPortfolio ?? 0);

            return nonPerformingLoans;
        }

        public static decimal CalculateNwdtNonPerformingLoans(List<NWDTRiskClassificationReturn> riskClassificationData)
        {
            decimal nonPerformingLoans = riskClassificationData
                .Where(r => r.Classification == "Substandard" ||
                            r.Classification == "Doubtful" ||
                            r.Classification == "Loss")
                .Sum(r => r.OutstandingLoanPortfolio ?? 0);

            return nonPerformingLoans;
        }

        public static decimal CalculateRegularLoans(List<DTRiskClassificationReturn> riskClassificationData)
        {
            return riskClassificationData
                .Where(r => r.LoanType == "Regular")
                .Sum(r => r.OutstandingLoanPortfolio ?? 0);
        }
        public static decimal CalculateNwdtRegularLoans(List<NWDTRiskClassificationReturn> riskClassificationData)
        {
            return riskClassificationData
                .Where(r => r.LoanType == "Regular")
                .Sum(r => r.OutstandingLoanPortfolio ?? 0);
        }

        public static decimal CalculateGrossLoans(List<DTRiskClassificationReturn> riskClassificationData)
        {
            return riskClassificationData
                .Where(r => r.LoanType == "Total")
                .Sum(r => r.OutstandingLoanPortfolio ?? 0);
        }

        public static decimal CalculateRescheduledLoans(List<DTRiskClassificationReturn> riskClassificationData)
        {
            return riskClassificationData
                .Where(r => r.LoanType == "Rescheduled/Renegotiated")
                .Sum(r => r.OutstandingLoanPortfolio ?? 0);
        }
        public static decimal CalculateNwdtRescheduledLoans(List<NWDTRiskClassificationReturn> riskClassificationData)
        {
            return riskClassificationData
                .Where(r => r.LoanType == "Rescheduled/Renegotiated")
                .Sum(r => r.OutstandingLoanPortfolio ?? 0);
        }

        public static decimal CalculateTotalLoans(List<DTRiskClassificationReturn> riskClassificationData)
        {
            return CalculateRegularLoans(riskClassificationData) +
                   CalculateRescheduledLoans(riskClassificationData);
        }
        public static decimal CalculateNwdtTotalLoans(List<NWDTRiskClassificationReturn> riskClassificationData)
        {
            return CalculateNwdtRegularLoans(riskClassificationData) +
                   CalculateNwdtRescheduledLoans(riskClassificationData);
        }



        public static async Task<CapitalAnalysisResult> AnalyzeCapitalWithDetails(CapitalAnalysisData data)
        {
            var result = new CapitalAnalysisResult();

            // 1. Minimum Core Capital (CC)
            result.MinimumCC.ActualValue = data.CoreCapital;
            var (minCcRating, minCcWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Minimum CC", data.CoreCapital);
            result.MinimumCC.Rating = minCcRating;
            result.MinimumCC.Weight = minCcWeight;
            result.MinimumCC.WeightedScore = result.MinimumCC.Rating * result.MinimumCC.Weight;

            // 2. Core Capital to Assets Ratio (CCA)
            result.CCA.ActualValue = data.CoreCapitalToAssetsRatio;
            var (ccaRating, ccaWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("CCA", data.CoreCapitalToAssetsRatio);
            result.CCA.Rating = ccaRating;
            result.CCA.Weight = ccaWeight;
            result.CCA.WeightedScore = result.CCA.Rating * result.CCA.Weight;

            // 3. Institutional Capital Ratio (ICA)
            result.ICA.ActualValue = data.InstitutionalCapitalRatio;
            var (icaRating, icaWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("ICA", data.InstitutionalCapitalRatio);
            result.ICA.Rating = icaRating;
            result.ICA.Weight = icaWeight;
            result.ICA.WeightedScore = result.ICA.Rating * result.ICA.Weight;

            // 4. Core Capital to Deposits Ratio (CCD)
            result.CCD.ActualValue = data.CoreCapitalToDepositsRatio;
            var (ccdRating, ccdWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("CCD", data.CoreCapitalToDepositsRatio);
            result.CCD.Rating = ccdRating;
            result.CCD.Weight = ccdWeight;
            result.CCD.WeightedScore = result.CCD.Rating * result.CCD.Weight;

            // 5. Adjusted CCA
            result.AdjustedCCA.ActualValue = data.AdjustedCCARatio;
            var (adjCcaRating, adjCcaWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Adjusted CCA", data.AdjustedCCARatio);
            result.AdjustedCCA.Rating = adjCcaRating;
            result.AdjustedCCA.Weight = adjCcaWeight;
            result.AdjustedCCA.WeightedScore = result.AdjustedCCA.Rating * result.AdjustedCCA.Weight;

            // Final weighted rating
            decimal totalWeightedScore =
                result.MinimumCC.WeightedScore +
                result.CCA.WeightedScore +
                result.ICA.WeightedScore +
                result.CCD.WeightedScore +
                result.AdjustedCCA.WeightedScore;

            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);

          
            int worstSub = new[]
            {
                result.MinimumCC.Rating,
                result.CCA.Rating,
                result.ICA.Rating,
                result.CCD.Rating,
                result.AdjustedCCA.Rating
            }.Max();


            // "no more than one level better than the worst
            //          (i.e. numerically lowest) sub-ratio"
            result.FinalRating = Math.Max(baseRating, worstSub - 1);

            //result.FinalRating = (int)Math.Round(totalWeightedScore);

            return result;
        }


        public static async Task<EarningsRatingDetails> AnalyzeEarnings(DTComprehensiveIncomeReturn incomeStatement, DTFinancialPositionReturn balanceSheet)
        {
            var result = new EarningsRatingDetails();

            // Calculate ratios
            result.ROAValue = balanceSheet.TotalAssets != 0 ? incomeStatement.TotalFinancialIncome / balanceSheet.TotalAssets : 0;
            result.CostToIncomeValue = incomeStatement.NetFinancialIncome != 0 ? incomeStatement.TotalOperatingExpenses / incomeStatement.NetFinancialIncome : 0;
            result.OEValue = balanceSheet.TotalAssets != 0 ? incomeStatement.TotalOperatingExpenses / balanceSheet.TotalAssets : 0;


            // Return on Assets (assume higher is better)
            var (roaRating, roaWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Return on Assets", result.ROAValue);
            result.ROARating = roaRating;
            result.ROAWeight = roaWeight;


            // Cost-Income Ratio (assume lower is better)
            var (costRating, costWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Cost- Income ratio", result.CostToIncomeValue);
            result.CostToIncomeRating = costRating;
            result.CostToIncomeWeight = costWeight;

            // Operating Expense Ratio (assume lower is better)
            var (oeRating, oeWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Operating expense ratio", result.OEValue);
            result.OERating = oeRating;
            result.OEWeight = oeWeight;

            var ROAWeightedScore = result.ROARating * result.ROAWeight;
            var OEWeightedScore = result.OERating * result.OEWeight;
            var CostToIncomeWeightedScore = result.CostToIncomeRating * result.CostToIncomeWeight;


            decimal totalWeightedScore = ROAWeightedScore + CostToIncomeWeightedScore + OEWeightedScore;
            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);
            int worstSub = new[]
            {
                result.ROARating,
                result.CostToIncomeRating,
                result.OERating
            }.Max();

            //result.FinalRating = (int)Math.Round(totalWeightedScore);
            result.FinalRating = Math.Max(baseRating, worstSub - 1);

            return result;
        }

        public static async Task<EarningsRatingDetails> AnalyzeNwdtEarnings(NWDTComprehensiveIncomeReturn incomeStatement, NWDTFinancialPositionReturn balanceSheet)
        {
            var result = new EarningsRatingDetails();

            // Calculate ratios
            // result.ROAValue = balanceSheet.TotalAssets != 0 ? incomeStatement.TotalFinancialIncome / balanceSheet.TotalAssets : 0;
            // result.CostToIncomeValue = incomeStatement.NetFinancialIncome != 0 ? incomeStatement.TotalOperatingExpenses / incomeStatement.NetFinancialIncome : 0;
            // result.OEValue = balanceSheet.TotalAssets != 0 ? incomeStatement.TotalOperatingExpenses / balanceSheet.TotalAssets : 0;


            // Return on Assets (assume higher is better)
            var (roaRating, roaWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Return on Assets", result.ROAValue);
            result.ROARating = roaRating;
            result.ROAWeight = roaWeight;


            // Cost-Income Ratio (assume lower is better)
            var (costRating, costWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Cost- Income ratio", result.CostToIncomeValue);
            result.CostToIncomeRating = costRating;
            result.CostToIncomeWeight = costWeight;

            // Operating Expense Ratio (assume lower is better)
            var (oeRating, oeWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Operating expense ratio", result.OEValue);
            result.OERating = oeRating;
            result.OEWeight = oeWeight;

            var ROAWeightedScore = result.ROARating * result.ROAWeight;
            var OEWeightedScore = result.OERating * result.OEWeight;
            var CostToIncomeWeightedScore = result.CostToIncomeRating * result.CostToIncomeWeight;

            decimal totalWeightedScore = ROAWeightedScore + CostToIncomeWeightedScore + OEWeightedScore;
            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);
            int worst = new[] { result.ROARating, result.CostToIncomeRating, result.OERating }.Max();
             result.FinalRating = Math.Max(baseRating, worst - 1);

            //result.FinalRating = (int)Math.Round(totalWeightedScore);
            return result;
        }


        private static int GetROARating(decimal roa)
        {
            // Need actual thresholds - using placeholders
            if (roa >= 3) return 1;  // Excellent
            if (roa >= 2) return 2;  // Good
            if (roa >= 1) return 3;  // Fair
            if (roa >= 0) return 4;  // Poor
            return 5;                // Very Poor
        }

        private static int GetCostToIncomeRating(decimal costToIncome)
        {
            // Need actual thresholds - using placeholders
            if (costToIncome <= 40) return 1;  // Excellent
            if (costToIncome <= 45) return 2;  // Good
            if (costToIncome <= 50) return 3;  // Fair
            if (costToIncome <= 55) return 4;  // Poor
            return 5;                          // Very Poor
        }

        private static int GetOERating(decimal oe)
        {
            // Need actual thresholds - using placeholders
            if (oe <= 2) return 1;   // Excellent
            if (oe <= 3) return 2;   // Good
            if (oe <= 4) return 3;   // Fair
            if (oe <= 5) return 4;   // Poor
            return 5;                // Very Poor
        }

        public static async Task<AssetQualityRatingDetails> AnalyzeAssetQuality(
        List<DTRiskClassificationReturn> currentQuarterData,
        List<DTRiskClassificationReturn> previousQuarterData)
        {
            var result = new AssetQualityRatingDetails();

            // Calculate ratios
            decimal npl30 = CalculateNonPerformingLoans(currentQuarterData);
            decimal totalLoans = CalculateTotalLoans(currentQuarterData);
            result.NPL30Value = totalLoans > 0 ? (npl30 / totalLoans) : 0;

            decimal rescheduledLoans = CalculateRescheduledLoans(currentQuarterData);
            decimal previousTotalLoans = CalculateTotalLoans(previousQuarterData);
            result.AdjustedNPL30Value = previousTotalLoans > 0 ? ((npl30 + rescheduledLoans) / previousTotalLoans) : 0;

            // NPL30 Indicator
            var (npl30Rating, npl30Weight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("NPL30", result.NPL30Value);
            result.NPL30Rating = npl30Rating;
            result.NPL30Weight = npl30Weight;
            result.NPL30WeightedScore = result.NPL30Rating * result.NPL30Weight;

            // Adjusted NPL30 Indicator (assumed name "Adj/lagged NPL30")
            var (adjNplRating, adjNplWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Adj/lagged NPL30", result.AdjustedNPL30Value);
            result.AdjustedNPL30Rating = adjNplRating;
            result.AdjustedNPL30Weight = adjNplWeight;
            result.AdjustedNPL30WeightedScore = result.AdjustedNPL30Rating * result.AdjustedNPL30Weight;

            // Final rating calculation
            decimal totalWeightedScore = result.NPL30WeightedScore + result.AdjustedNPL30WeightedScore;
            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);
            int worst = Math.Max(result.NPL30Rating, result.AdjustedNPL30Rating);
            result.FinalRating = Math.Max(baseRating, worst - 1);
            //result.FinalRating = (int)Math.Round(totalWeightedScore);

            return result;
        }

        public static async Task<AssetQualityRatingDetails> AnalyzeNwdtAssetQuality(
       List<NWDTRiskClassificationReturn> currentQuarterData,
       List<NWDTRiskClassificationReturn> previousQuarterData)
        {
            var result = new AssetQualityRatingDetails();

            // Calculate ratios
            decimal npl30 = CalculateNwdtNonPerformingLoans(currentQuarterData);
            decimal totalLoans = CalculateNwdtTotalLoans(currentQuarterData);
            result.NPL30Value = totalLoans > 0 ? (npl30 / totalLoans) * 100 : 0;

            decimal rescheduledLoans = CalculateNwdtRescheduledLoans(currentQuarterData);
            decimal previousTotalLoans = CalculateNwdtTotalLoans(previousQuarterData);
            result.AdjustedNPL30Value = previousTotalLoans > 0 ? ((npl30 + rescheduledLoans) / previousTotalLoans) * 100 : 0;

            // NPL30 Indicator
            var (npl30Rating, npl30Weight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("NPL30", result.NPL30Value);
            result.NPL30Rating = npl30Rating;
            result.NPL30Weight = npl30Weight;
            result.NPL30WeightedScore = result.NPL30Rating * result.NPL30Weight;

            // Adjusted NPL30 Indicator (assumed name "Adj/lagged NPL30")
            var (adjNplRating, adjNplWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Adj/lagged NPL30", result.AdjustedNPL30Value);
            result.AdjustedNPL30Rating = adjNplRating;
            result.AdjustedNPL30Weight = adjNplWeight;
            result.AdjustedNPL30WeightedScore = result.AdjustedNPL30Rating * result.AdjustedNPL30Weight;

            // Final rating calculation
            decimal totalWeightedScore = result.NPL30WeightedScore + result.AdjustedNPL30WeightedScore;
            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);
            int worst = Math.Max(result.NPL30Rating, result.AdjustedNPL30Rating);   // worst sub-rating
            result.FinalRating = Math.Max(baseRating, worst - 1);

            //result.FinalRating = (int)Math.Round(totalWeightedScore);

            return result;
        }



        private static int GetNPLRating(decimal nplRatio)
        {
            if (nplRatio < 4) return 1;     // Excellent: < 4%
            if (nplRatio < 8) return 2;     // Good: 4% to < 8%
            if (nplRatio < 12) return 3;    // Fair: 8% to < 12%
            if (nplRatio < 15) return 4;    // Poor: 12% to < 15%
            return 5;                        // Very Poor: 15% or more
        }

        public static int CalculateOverallRating(SaccoAnalysis analysis)
        {
            // Get individual ratings
            var ratings = new[]
            {
        analysis.CapitalRating,
        analysis.AssetQualityRating,
        analysis.ManagementRating,
        analysis.EarningsRating,
        analysis.LiquidityRating
        };

            // Calculate average rating
            double averageRating = ratings.Average();

            // Find worst rating
            int worstRating = ratings.Max();

            // Overall rating can't be more than one level better than worst rating
            int suggestedRating = (int)Math.Round(averageRating);
            int finalRating = Math.Min(suggestedRating, worstRating - 1);

            // Determine risk level and required actions
            analysis.RiskLevel = finalRating switch
            {
                1 or 2 => "Low Risk",
                3 => "Medium Risk",
                _ => "High Risk"
            };

            analysis.ActionRequired = finalRating switch
            {
                1 or 2 => "No action needed",
                3 => "Needs additional supervision",
                4 => "Immediate corrective action required",
                _ => "Urgent intervention needed"
            };

            return finalRating;
        }



        public static ManagementRatingDetails AnalyzeManagement(ManagementReturn managementReturn)
        {
            var result = new ManagementRatingDetails();
            result.MRating = managementReturn.MRating;
            result.Period = managementReturn.Year;
            return result;
        }

        public static decimal CalculateWNLIQRatio(DTFinancialPositionReturn balanceSheet)
        {
            var C10 = balanceSheet.TotalCashAndCashEquivalent;
            var C17 = balanceSheet.GovernmentSecurities;
            var C47 = balanceSheet.TotalAccountsPayable;
            var C53 = balanceSheet.ExternalBorrowings;
            var C42 = balanceSheet.SavingsDeposits;
            var C43 = balanceSheet.ShortTermDeposits;

            decimal numerator = C10 + C17;
            decimal denominator = (C47 - C53) + C42 + C43;

            if (denominator == 0)
                return 0;

            return (numerator / denominator);
        }
        public static decimal CalculateNwdtWNLIQRatio(NWDTFinancialPositionReturn balanceSheet)
        {
            var C10 = 0; //Revisit: TotalCashAndCashEquivalent not defined in balanceSheet
            var C17 = balanceSheet.GovernmentSecurities;
            var C47 = 0; //Revisit: TotalAccountsPayable not defined in balancesSheet
            var C53 = balanceSheet.ExternalBorrowings;
            var C42 = 0; //Revisit: SavingsDeposits not defined in balanceSheet
            var C43 = 0; //Revisit: ShortTermDeposits not defined in balanceSheet

            decimal numerator = C10 + C17;
            decimal denominator = (C47 - C53) + C42 + C43;

            if (denominator == 0)
                return 0;

            return (numerator / denominator);
        }

        public static decimal CalculateTNLIQRatio(DTFinancialPositionReturn balanceSheet)
        {
            var C10 = balanceSheet.TotalCashAndCashEquivalent;
            var C17 = balanceSheet.GovernmentSecurities;
            var C47 = balanceSheet.TotalAccountsPayable;
            var C53 = balanceSheet.ExternalBorrowings;
            var C19 = balanceSheet.BalancesWithOtherSaccos;
            var C45 = balanceSheet.TotalDepositLiabilities;

            decimal numerator = C10 + C17 + C19;
            decimal denominator = C45 + (C47 - C53);

            if (denominator == 0)
                return 0;

            return (numerator / denominator);
        }
        public static decimal CalculateNwdtTNLIQRatio(NWDTFinancialPositionReturn balanceSheet)
        {
            var C10 = 0; //Revisit: TotalCashAndCashEquivalent not defined in balanceSheet
            var C17 = balanceSheet.GovernmentSecurities;
            var C47 = 0; //Revisit: TotalAccountsPayable not defined in balancesSheet
            var C53 = balanceSheet.ExternalBorrowings;
            var C19 = 0; //Revisit: BalancesWithOtherSaccos not defined in balanceSheet
            var C45 = balanceSheet.TotalDepositLiabilities;

            decimal numerator = C10 + C17 + C19;
            decimal denominator = C45 + (C47 - C53);

            if (denominator == 0)
                return 0;

            return (numerator / denominator);
        }

        public static decimal CalculateEBRatio(DTFinancialPositionReturn balanceSheet)
        {
            var C53 = balanceSheet.ExternalBorrowings;
            var C38 = balanceSheet.TotalAssets;
            //EB ratio = C53 / C38
            return C38 != 0 ? (C53 / C38) : 0;

        }
        public static decimal CalculateNwdtEBRatio(NWDTFinancialPositionReturn balanceSheet)
        {
            var C53 = balanceSheet.ExternalBorrowings;
            var C38 = balanceSheet.TotalAssets;
            //EB ratio = C53 / C38
            return C38 != 0 ? (C53 / C38) : 0;

        }

        public static decimal CalculateFICCRatio(DTFinancialPositionReturn balanceSheet)
        {
            var C19 = balanceSheet.BalancesWithOtherSaccos;
            var C20 = balanceSheet.InvestmentsInCompanies;

            var C57 = balanceSheet.ShareCapital;
            var C60 = balanceSheet.TotalRetainedEarnings;
            var C65 = balanceSheet.StatutoryReserve;
            var C58 = balanceSheet.CapitalGrants;
            var C66 = balanceSheet.OtherReserves;
            //(C19+C20)/(C57+C60+C65+C58+C66)

            var numerator = C19 + C20;
            var denominator = C57 + C60 + C65 + C58 + C66;
            return denominator != 0 ? (numerator / denominator) : 0;
        }
        public static decimal CalculateNwdtFICCRatio(NWDTFinancialPositionReturn balanceSheet)
        {
            var C19 = 0; //Todo: BalancesWithOtherSaccos NOT DEFINED IN balanceSheet
            var C20 = 0; //Todo: InvestmentsInCompanies NOT DEFINED IN balanceSheet

            var C57 = balanceSheet.ShareCapital;
            var C60 = 0; //Todo: TotalRetainedEarnings NOT DEFINED IN balanceSheet
            var C65 = balanceSheet.StatutoryReserve;
            var C58 = balanceSheet.CapitalGrants;
            var C66 = balanceSheet.OtherReserves;
            //(C19+C20)/(C57+C60+C65+C58+C66)

            var numerator = C19 + C20;
            var denominator = C57 + C60 + C65 + C58 + C66;
            return denominator != 0 ? (numerator / denominator) : 0;
        }

        public static decimal CalculateNEARatio(DTFinancialPositionReturn balanceSheet)
        {
            var C14 = balanceSheet.PrepaymentsAndSundryReceivables;
            var C31 = balanceSheet.PropertyAndEquipment;
            var C32 = balanceSheet.InvestmentProperties;

            var C38 = balanceSheet.TotalAssets;

            // (C14+C31-C32)/C38

            var numerator = C14 + C31 + C32;
            var denominator = C32;
            return denominator != 0 ? (numerator / denominator) : 0;
        }
        public static decimal CalculateNwdtNEARatio(NWDTFinancialPositionReturn balanceSheet)
        {
            var C14 = balanceSheet.PrepaymentsAndSundryReceivables;
            var C31 = balanceSheet.PropertyAndEquipment;
            var C32 = balanceSheet.InvestmentProperties;

            var C38 = balanceSheet.TotalAssets;

            // (C14+C31-C32)/C38

            var numerator = C14 + C31 + C32;
            var denominator = C32;
            return denominator != 0 ? (numerator / denominator) : 0;
        }

        public static decimal CalculateFITDRatio(DTFinancialPositionReturn balanceSheet)
        {
            var C19 = balanceSheet.BalancesWithOtherSaccos;
            var C20 = balanceSheet.InvestmentsInCompanies;

            var C45 = balanceSheet.TotalDepositLiabilities;


            //(C19+C20)/C45

            var numerator = C19 + C20;
            var denominator = C45;
            return denominator != 0 ? (numerator / denominator) : 0;
        }
        public static decimal CalculateNwdtFITDRatio(NWDTFinancialPositionReturn balanceSheet)
        {
            var C19 = 0; //TODO: BalancesWithOtherSaccos NOT DEFINED IN balanceSheet
            var C20 = 0; //TODO: InvestmentsInCompanies not defined in balanceSheet

            var C45 = balanceSheet.TotalDepositLiabilities;


            //(C19+C20)/C45

            var numerator = C19 + C20;
            var denominator = C45;
            return denominator != 0 ? (numerator / denominator) : 0;
        }

        public static decimal CalculateLIQtoTARatio(DTFinancialPositionReturn balanceSheet)
        {
            // LIQ to TA = (C10 + C17) / C38
            var C10 = balanceSheet.TotalCashAndCashEquivalent;
            var C17 = balanceSheet.GovernmentSecurities;
            var C38 = balanceSheet.TotalAssets;
            return C38 != 0 ? ((C10 + C17) / C38) * 100 : 0;
        }
        public static decimal CalculateNwdtLIQtoTARatio(NWDTFinancialPositionReturn balanceSheet)
        {
            // LIQ to TA = (C10 + C17) / C38
            var C10 = 0; //Revisit: TotalCashAndCashEquivalent not defined in balanceSheet
            var C17 = balanceSheet.GovernmentSecurities;
            var C38 = balanceSheet.TotalAssets;
            return C38 != 0 ? ((C10 + C17) / C38) * 100 : 0;
        }


        public static async Task<LiquidityRatingDetails> AnalyzeLiquidity(DTFinancialPositionReturn balanceSheet)
        {
            var result = new LiquidityRatingDetails();

            result.WNLIQValue = CalculateWNLIQRatio(balanceSheet);
            result.TNLIQValue = CalculateTNLIQRatio(balanceSheet);
            result.EBValue = CalculateEBRatio(balanceSheet);
            result.LIQtoTAValue = CalculateLIQtoTARatio(balanceSheet);

            var (minLiqRating, minLiqWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Minimum liquidity", result.WNLIQValue);
            result.WNLIQRating = minLiqRating;
            result.WNLIQWeight = minLiqWeight;
            var liqWeightScore = result.WNLIQRating * result.WNLIQWeight;

            var (techLiqRating, techLiqWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Technical liquidity", result.TNLIQValue);
            result.TNLIQRating = techLiqRating;
            result.TNLIQWeight = techLiqWeight;
            var techLiqWeightScore = result.TNLIQRating * result.TNLIQWeight;

            var (extBorrowRating, extBorrowWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("External borrowing to Total Assets Ratio", result.EBValue);
            result.EBRating = extBorrowRating;
            result.EBWeight = extBorrowWeight;
            var extBorrowWeightScore = result.EBRating * result.EBWeight;

            var (liqToTARating, liqToTAWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Liquid Assets to Total Assets Ratio", result.LIQtoTAValue);
            result.LIQtoTARating = liqToTARating;
            result.LIQtoTAWeight = liqToTAWeight;
            var liqToTAWeightScore = result.LIQtoTARating * result.LIQtoTAWeight;

            decimal totalWeightedScore = liqWeightScore + techLiqWeightScore + extBorrowWeightScore + liqToTAWeightScore;
            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);
            int worst = new[] { result.WNLIQRating, result.TNLIQRating, result.EBRating, result.LIQtoTARating }.Max();
            result.FinalRating = Math.Max(baseRating, worst - 1);
            //result.FinalRating = (int)Math.Round(totalWeightedScore);
            return result;
        }

        public static async Task<LiquidityRatingDetails> AnalyzeNwdtLiquidity(NWDTFinancialPositionReturn balanceSheet)
        {
            var result = new LiquidityRatingDetails();

            result.WNLIQValue = CalculateNwdtWNLIQRatio(balanceSheet);
            result.TNLIQValue = CalculateNwdtTNLIQRatio(balanceSheet);
            result.EBValue = CalculateNwdtEBRatio(balanceSheet);
            result.LIQtoTAValue = CalculateNwdtLIQtoTARatio(balanceSheet);

            var (minLiqRating, minLiqWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Minimum liquidity", result.WNLIQValue);
            result.WNLIQRating = minLiqRating;
            result.WNLIQWeight = minLiqWeight;
            var liqWeightScore = result.WNLIQRating * result.WNLIQWeight;

            var (techLiqRating, techLiqWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Technical liquidity", result.TNLIQValue);
            result.TNLIQRating = techLiqRating;
            result.TNLIQWeight = techLiqWeight;
            var techLiqWeightScore = result.TNLIQRating * result.TNLIQWeight;

            var (extBorrowRating, extBorrowWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("External borrowing to Total Assets Ratio", result.EBValue);
            result.EBRating = extBorrowRating;
            result.EBWeight = extBorrowWeight;
            var extBorrowWeightScore = result.EBRating * result.EBWeight;

            var (liqToTARating, liqToTAWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("Liquid Assets to Total Assets Ratio", result.LIQtoTAValue);
            result.LIQtoTARating = liqToTARating;
            result.LIQtoTAWeight = liqToTAWeight;
            var liqToTAWeightScore = result.LIQtoTARating * result.LIQtoTAWeight;

            decimal totalWeightedScore = liqWeightScore + techLiqWeightScore + extBorrowWeightScore + liqToTAWeightScore;
            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);

            int worst = new[] { result.WNLIQRating, result.TNLIQRating, result.EBRating, result.LIQtoTARating }.Max();
            result.FinalRating = Math.Max(baseRating, worst - 1);

            //result.FinalRating = (int)Math.Round(totalWeightedScore);
            return result;
        }


        public static async Task<StructureOfAssetsRatingDetails> AnalyzeStructureOfAssets(DTFinancialPositionReturn balanceSheet)
        {
            // Create the result object.
            var result = new StructureOfAssetsRatingDetails();


            // Avoid division by zero.
            result.LBRatioValue = balanceSheet.TotalAssets != 0
                ? balanceSheet.PropertyAndEquipment / balanceSheet.TotalAssets
                : 0;
            var (lbRating, lbWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("LB Ratio", result.LBRatioValue);
            result.LBRatioRating = lbRating;
            result.LBRatioWeight = lbWeight;
            result.LBRatioWeightedScore = result.LBRatioRating * result.LBRatioWeight;


            result.FICCValue = CalculateFICCRatio(balanceSheet);
            var (ficcRating, ficcWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("FICC Ratio", result.FICCValue);
            result.FICCRating = ficcRating;
            result.FICCWeight = ficcWeight;
            result.FICCWeightedScore = result.FICCRating * result.FICCWeight;


            result.FITDValue = CalculateFITDRatio(balanceSheet);
            var (fitdRating, fitdWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("FITD Ratio", result.FITDValue);
            result.FITDRating = fitdRating;
            result.FITDWeight = fitdWeight;
            result.FITDWeightedScore = result.FITDRating * result.FITDWeight;


            result.NEAValue = CalculateNEARatio(balanceSheet);
            var (neaRating, neaWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("NEA Ratio", result.NEAValue);
            result.NEARating = neaRating;
            result.NEAWeight = neaWeight;
            result.NEAWeightedScore = result.NEARating * result.NEAWeight;

            decimal totalWeightedScore = result.LBRatioWeightedScore +
                                          result.FICCWeightedScore +
                                          result.FITDWeightedScore +
                                          result.NEAWeightedScore;

            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);
            int worst = new[] { result.LBRatioRating, result.FICCRating, result.FITDRating, result.NEARating }.Max();
            result.FinalRating = Math.Max(baseRating, worst - 1);
            //result.FinalRating = (int)Math.Round(totalWeightedScore);

            return result;
        }

        public static async Task<StructureOfAssetsRatingDetails> AnalyzeNwdtStructureOfAssets(NWDTFinancialPositionReturn balanceSheet)
        {
            // Create the result object.
            var result = new StructureOfAssetsRatingDetails();


            // Avoid division by zero.
            result.LBRatioValue = balanceSheet.TotalAssets != 0
                ? balanceSheet.PropertyAndEquipment / balanceSheet.TotalAssets
                : 0;
            var (lbRating, lbWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("LB Ratio", result.LBRatioValue);
            result.LBRatioRating = lbRating;
            result.LBRatioWeight = lbWeight;
            result.LBRatioWeightedScore = result.LBRatioRating * result.LBRatioWeight;


            result.FICCValue = CalculateNwdtFICCRatio(balanceSheet);
            var (ficcRating, ficcWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("FICC Ratio", result.FICCValue);
            result.FICCRating = ficcRating;
            result.FICCWeight = ficcWeight;
            result.FICCWeightedScore = result.FICCRating * result.FICCWeight;


            result.FITDValue = CalculateNwdtFITDRatio(balanceSheet);
            var (fitdRating, fitdWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("FITD Ratio", result.FITDValue);
            result.FITDRating = fitdRating;
            result.FITDWeight = fitdWeight;
            result.FITDWeightedScore = result.FITDRating * result.FITDWeight;


            result.NEAValue = CalculateNwdtNEARatio(balanceSheet);
            var (neaRating, neaWeight) = await DynamicRatingHelper.GetRatingForIndicatorAsync("NEA Ratio", result.NEAValue);
            result.NEARating = neaRating;
            result.NEAWeight = neaWeight;
            result.NEAWeightedScore = result.NEARating * result.NEAWeight;

            decimal totalWeightedScore = result.LBRatioWeightedScore +
                                          result.FICCWeightedScore +
                                          result.FITDWeightedScore +
                                          result.NEAWeightedScore;

            int baseRating = (int)Math.Round(totalWeightedScore, MidpointRounding.AwayFromZero);
            int worst = new[] { result.LBRatioRating, result.FICCRating, result.FITDRating, result.NEARating }.Max();
            result.FinalRating = Math.Max(baseRating, worst - 1);


            //result.FinalRating = (int)Math.Round(totalWeightedScore);

            return result;
        }



        public static int CalculateOverallRating(int capitalRating, int assetRating,
            int earningsRating, int liquidityRating, int managementRating)
        {
            var CapitalWeighted = capitalRating * 0.20m;
            var AssetWeighted = assetRating * 0.20m;
            var EarningsWeighted = earningsRating * 0.20m;
            var LiquidityWeighted = liquidityRating * 0.20m;
            var ManagementWeighted = managementRating * 0.20m;
            // Calculate weighted average rating
            decimal weightedAverage = (CapitalWeighted + AssetWeighted + EarningsWeighted + LiquidityWeighted + ManagementWeighted) / 5.0m;
            // Round to nearest whole number
            int baseRating = (int)Math.Round(weightedAverage);
            // Find worst component rating
            int worstRating = new[] { capitalRating, assetRating, earningsRating, liquidityRating }.Max();
            // Overall rating can't be more than one level better than worst component
            return Math.Max(baseRating, worstRating - 1);

            /*// Calculate average rating
            decimal averageRating = (capitalRating + assetRating + earningsRating + liquidityRating + managementRating) / 5.0m;

            // Round to nearest whole number
            int baseRating = (int)Math.Round(averageRating);

            // Find worst component rating
            int worstRating = new[] { capitalRating, assetRating, earningsRating, liquidityRating }.Max();

            // Overall rating can't be more than one level better than worst component
            return Math.Min(baseRating, worstRating - 1);*/
        }

    }

}
