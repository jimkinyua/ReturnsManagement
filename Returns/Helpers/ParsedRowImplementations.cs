using Returns.Helpers.Interfaces;
using Returns.Models;
using static Returns.Helpers.ExcelService;

namespace Returns.Helpers
{
    public class CapitalAdequacyParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form1Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new DTCapitalAdequacyReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                //Year = Data.Period,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.Index?.Trim())
                {
                    // CORE CAPITAL
                    case "1.1.1":
                        entity.ShareCapital = row.Amount ?? 0;
                        break;
                    case "1.1.2":
                        entity.StatutoryReserves = row.Amount ?? 0;
                        break;
                    case "1.1.3":
                        entity.RetainedEarningsAccumulatedLosses = row.Amount ?? 0;
                        break;
                    case "1.1.4":
                        entity.NetSurplusAfterTaxCurrentYearToDate = row.Amount ?? 0;
                        break;
                    case "1.1.5":
                        entity.CapitalGrantsEquityInNature = row.Amount ?? 0;
                        break;
                    case "1.1.6":
                        entity.GeneralReserves = row.Amount ?? 0;
                        break;
                    case "1.1.7":
                        entity.OtherReserves = row.Amount ?? 0;
                        break;
                    case "1.1.8":
                        entity.SubTotalCoreCapital = row.Amount ?? 0;
                        break;

                    // DEDUCTIONS
                    case "1.1.9":
                        entity.InvestmentsInSubsidiaryAndEquityInstruments = row.Amount ?? 0;
                        break;
                    case "1.1.10":
                        entity.OtherDeductions = row.Amount ?? 0;
                        break;
                    case "1.1.11":
                        entity.TotalDeductions = row.Amount ?? 0;
                        break;
                    case "1.1.12":
                        entity.CoreCapital = row.Amount ?? 0;
                        break;
                    case "1.1.13":
                        entity.InstitutionalCapital = row.Amount ?? 0;
                        break;

                    // ON-BALANCE SHEET ASSETS
                    case "2.1":
                        entity.CashLocalAndForeignCurrency = row.Amount ?? 0;
                        break;
                    case "2.2":
                        entity.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "2.3":
                        entity.DepositsAndBalancesAtOtherInstitutions = row.Amount ?? 0;
                        break;
                    case "2.4":
                        entity.LoansAndAdvances = row.Amount ?? 0;
                        break;
                    case "2.5":
                        entity.Investments = row.Amount ?? 0;
                        break;
                    case "2.6":
                        entity.PropertyAndEquipment = row.Amount ?? 0;
                        break;
                    case "2.7":
                        entity.OtherAssets = row.Amount ?? 0;
                        break;
                    case "2.8":
                        entity.TotalOnBalanceSheetAssets = row.Amount ?? 0;
                        break;
                    case "2.9":
                        entity.TotalAssetsPerBalanceSheet = row.Amount ?? 0;
                        break;
                    case "3.0":
                        entity.Difference = row.Amount ?? 0;
                        break;

                    // OFF-BALANCE SHEET
                    case "3":
                        entity.TotalOffBalanceSheetAssets = row.Amount ?? 0;
                        break;

                    // RATIOS
                    case "4.1":
                        entity.TotalOnBalanceSheetAssets = row.Amount ?? 0;
                        break;
                    case "4.2":
                        entity.TotalOffBalanceSheetAssets = row.Amount ?? 0;
                        break;
                    case "4.3":
                        entity.TotalAssets = row.Amount ?? 0;
                        break;
                    case "4.4":
                        entity.TotalDepositsLiabilities = row.Amount ?? 0;
                        break;
                    case "4.5":
                        entity.CoreCapitalToAssetsRatio = row.Amount ?? 0;
                        break;
                    case "4.7":
                        entity.CoreCapitalToAssetsRatioExcessDeficiency = row.Amount ?? 0;
                        break;
                    case "4.8":
                        entity.InstitutionalCapitalToAssetsRatio = row.Amount ?? 0;
                        break;
                    case "4.9":
                        entity.MinimumInstitutionalToAssetsRatio = row.Amount ?? 0;
                        break;
                    case "4.10":
                        entity.InstitutionalCapitalToAssetsRatioExcessDeficiency = row.Amount ?? 0;
                        break;
                    case "4.11":
                        entity.CoreCapitalToDepositsRatio = row.Amount ?? 0;
                        break;
                    case "4.12":
                        entity.MinimumCoreCapitalToDepositsRatio = row.Amount ?? 0;
                        break;
                    case "4.13":
                        entity.CoreCapitalToDepositsRatioExcessDeficiency = row.Amount ?? 0;
                        break;
                }
            }

            return entity;
        }
    }

    public class LiquidityParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new DTLiquidityReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                Year = Data.Period,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            // Map liquidity data based on row indices
            /* foreach (var row in Data.Rows)
             {
                 switch (row.Index?.Trim())
                 {
                     case "1.1":
                         entity.NotesAndCoins = row.Amount ?? 0;
                         break;
                     case "1.2":
                         entity.TillsAndVault = row.Amount ?? 0;
                         break;
                     case "1.3":
                         entity.TotalNotesAndCoins = row.Amount ?? 0;
                         break;
                     case "2.1":
                         entity.CurrentAccountBalancesInBanks = row.Amount ?? 0;
                         break;
                     case "2.2":
                         entity.FixedDepositAccountsInBanks90Days = row.Amount ?? 0;
                         break;
                     case "2.3":
                         entity.TotalBankBalances = row.Amount ?? 0;
                         break;
                     case "3.1":
                         entity.DepositsWithOtherFinancialInstitutions = row.Amount ?? 0;
                         break;
                     case "3.2":
                         entity.TotalOtherFinancialInstitutions = row.Amount ?? 0;
                         break;
                     case "4.1":
                         entity.TreasuryBills = row.Amount ?? 0;
                         break;
                     case "4.2":
                         entity.TreasuryBonds = row.Amount ?? 0;
                         break;
                     case "4.3":
                         entity.CorporateBonds = row.Amount ?? 0;
                         break;
                     case "4.4":
                         entity.TotalGovernmentSecurities = row.Amount ?? 0;
                         break;
                     case "5":
                         entity.NetLiquidAssets = row.Amount ?? 0;
                         break;
                     case "6.1":
                         entity.MemberDeposits = row.Amount ?? 0;
                         break;
                     case "6.2":
                         entity.ShortTermLiabilities = row.Amount ?? 0;
                         break;
                     case "6.3":
                         entity.TotalOtherLiabilities = row.Amount ?? 0;
                         break;
                     case "7.1":
                         entity.TotalLiquidAssets = row.Amount ?? 0;
                         break;
                     case "7.2":
                         entity.TotalCurrentLiabilities = row.Amount ?? 0;
                         break;
                     case "7.3":
                         entity.LiquidityRatio = row.Amount ?? 0;
                         break;
                     case "7.4":
                         entity.MinimumStatutoryRatio = row.Amount ?? 0;
                         break;
                     case "7.5":
                         entity.ExcessDeficit = row.Amount ?? 0;
                         break;
                 }
             }*/

            return entity;
        }
    }

    public class DepositReturnParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form3Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            var depositReturns = new List<DepositReturn>();
            string currentRangeName = string.Empty;

            foreach (var row in Data.Rows)
            {
                // Skip TOTAL row as it's a summary
                if (row.DepositType?.ToUpper() == "TOTAL")
                {
                    continue;
                }

                // Update current range name if it's not empty
                if (!string.IsNullOrEmpty(row.RangeName))
                {
                    currentRangeName = row.RangeName;
                }

                var entity = new DepositReturn
                {
                    ReturnSubmissionId = ReturnSubmissionId,
                    SaccoCsNumber = Data.SaccoCsNumber,
                    Year = Data.Period,
                    StartDate = Data.StartDate,
                    EndDate = Data.EndDate,
                    RangeName = currentRangeName,  // Use the current range name
                    DepositType = row.DepositType,
                    NumberOfAccounts = row.NumberOfAccounts,
                    AmountInKshs000 = row.AmountInKshs000,
                    CreatedAt = DateTime.Now
                };

                depositReturns.Add(entity);
            }

            // Return the list of deposit returns
            return depositReturns;
        }
    }

    public class NWDTCapitalAdequacyParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2AStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new NWDTCapitalAdequacyReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            // Map NWDT capital adequacy data
            foreach (var row in Data.Rows)
            {
                switch (row.Index?.Trim())
                {
                    // Core Capital items
                    case "1.1.1":
                        entity.ShareCapital = row.Amount ?? 0;
                        break;
                    case "1..1.2":
                        entity.CapitalGrants = row.Amount ?? 0;
                        break;
                    case "1.1.3":
                        entity.RetainedEarningsAccumulatedLosses = row.Amount ?? 0;
                        break;
                    case "1.1.4":
                        entity.NetSurplusAfterTaxCurrentYearToDate = row.Amount ?? 0;
                        break;
                    case "1.1.5":
                        entity.StatutoryReserves = row.Amount ?? 0;
                        break;
                    case "1.1.6":
                        entity.OtherReserves = row.Amount ?? 0;
                        break;
                    case "1.1.8":
                        entity.InvestmentsInSubsidiaryAndEquityInstruments = row.Amount ?? 0;
                        break;
                    case "1.1.9":
                        entity.OtherDeductions = row.Amount ?? 0;
                        break;
                    case "1.1.10":
                        entity.TotalDeductions = row.Amount ?? 0;
                        break;
                    case "1.1.11":
                        entity.CoreCapital = row.Amount ?? 0;
                        break;
                    case "1.1.12":
                        entity.RetainedEarningsAndDisclosedReserves = row.Amount ?? 0;
                        break;

                    // Balance Sheet
                    case "2.1":
                        entity.CashLocalAndForeignCurrency = row.Amount ?? 0;
                        break;
                    case "2.2":
                        entity.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "2.3":
                        entity.DepositsAndBalancesAtOtherInstitutions = row.Amount ?? 0;
                        break;
                    case "2.4":
                        entity.LoansAndAdvances = row.Amount ?? 0;
                        break;
                    case "2.5":
                        entity.Investments = row.Amount ?? 0;
                        break;
                    case "2.6":
                        entity.PropertyAndEquipment = row.Amount ?? 0;
                        break;
                    case "2.7":
                        entity.OtherAssets = row.Amount ?? 0;
                        break;
                    case "2.9":
                        entity.TotalAssetsPerBalanceSheet = row.Amount ?? 0;
                        break;
                   /* case "3.0":
                        entity.DifferenceInAssets = row.Amount ?? 0;
                        break;*/

                    // Total Deposits
                    case "3":
                        entity.TotalOffBalanceSheetAssets = row.Amount ?? 0;
                        break;

                    case "4.3":
                        entity.TotalAssets = row.Amount ?? 0;
                        break;
                    case "4.4":
                        entity.TotalDepositsLiabilitiesPerBalanceSheet = row.Amount ?? 0;
                        break;
                    case "4.5":
                        entity.CoreCapitalToAssetsRatio = row.Amount ?? 0;
                        break;
                    case "4.6":
                        entity.MinimumCoreCapitalToAssetsRatioRequirement = row.Amount ?? 0;
                        break;
                    case "4.8":
                        entity.RetainedEarningsAndDisclosedReservesToCoreCapital = row.Amount ?? 0;
                        break;
                    case "4.9":
                        entity.MinimumRetainedEarningsAndDisclosedReservesToCoreCapitalRequirement = row.Amount ?? 0;
                        break;
                    case "4.11":
                        entity.CoreCapitalToDepositsRatio = row.Amount ?? 0;
                        break;
                    case "4.12":
                        entity.MinimumCoreCapitalToDepositsRatioRequirement = row.Amount ?? 0;
                        break;
                }
            }

            return entity;
        }
    }

    // You would need to implement similar classes for other form types:
    // - RiskClassificationParsedRow
    // - InvestmentParsedRow
    // - FinancialPositionParsedRow
    // - ComprehensiveIncomeParsedRow
    // - NWDTLiquidityParsedRow
    // - NWDTDepositParsedRow
    // - NWDTRiskClassificationParsedRow
    // - NWDTInvestmentParsedRow
    // - NWDTComprehensiveIncomeParsedRow
    // - NWDTFinancialPositionParsedRow
    // - ManagementReturnParsedRow
    // - SectoralLendingParsedRow
    // - DailyLiquidityParsedRow
    // - InsiderLendingParsedRow

    public class RiskClassificationParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form4Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            var riskClassifications = new List<DTRiskClassificationReturn>();

            foreach (var row in Data.Rows)
            {
                var entity = new DTRiskClassificationReturn
                {
                    ReturnSubmissionId = ReturnSubmissionId,
                    SaccoCsNumber = Data.SaccoCsNumber,
                    Year = Data.Period,
                    StartDate = Data.StartDate,
                    EndDate = Data.EndDate,
                    LoanType = row.LoanType,
                    Classification = row.Classification,
                    NumberOfAccounts = row.NumberOfAccounts,
                    OutstandingLoanPortfolio = row.OutstandingLoanPortfolio,
                    RequiredProvision = row.RequiredProvision,
                    RequiredProvisionAmount = row.RequiredProvisionAmount,
                    CreatedAt = DateTime.Now
                };

                riskClassifications.Add(entity);
            }

            return riskClassifications;
        }
    }

    public class InvestmentParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form5Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new DTInvestmentReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                Year = Data.Period,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.Index?.Trim())
                {
                    case "1.1":
                        entity.CoreCapital = row.Amount ?? 0;
                        break;
                    case "1.2":
                        entity.TotalAssets = row.Amount ?? 0;
                        break;
                    case "1.3":
                        entity.TotalDeposits = row.Amount ?? 0;
                        break;
                    case "1.4":
                        entity.NonEarningAssets = row.Amount ?? 0;
                        break;
                    case "1.5":
                        entity.FinancialInvestments = row.Amount ?? 0;
                        break;
                    case "1.6":
                        entity.LandAndBuildings = row.Amount ?? 0;
                        break;
                }
            }

            // Calculate ratios
            if (entity.TotalAssets > 0)
            {
                entity.LandBuildingsToTotalAssetsRatio = (entity.LandAndBuildings / entity.TotalAssets) * 100;
                entity.NonEarningAssetsToTotalAssetsRatio = (entity.NonEarningAssets / entity.TotalAssets) * 100;
            }

            if (entity.CoreCapital > 0)
            {
                entity.FinancialInvestmentsToCoreCapitalRatio = (entity.FinancialInvestments / entity.CoreCapital) * 100;
            }

            if (entity.TotalDeposits > 0)
            {
                entity.FinancialInvestmentsToDepositsRatio = (entity.FinancialInvestments / entity.TotalDeposits) * 100;
            }

            return entity;
        }
    }

    public class FinancialPositionParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form6Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new DTFinancialPositionReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                Year = Data.Period,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.RefNumber?.Trim())
                {
                    // Cash & Cash Equivalent
                    case "1.1":
                        entity.CashInHand = row.Amount ?? 0;
                        break;
                    case "1.2":
                        entity.CashAtBank = row.Amount ?? 0;
                        break;

                    // Prepayments & Sundry Receivables
                    case "2":
                        entity.PrepaymentsAndSundryReceivables = row.Amount ?? 0;
                        break;

                    // Financial Investments
                    case "3.1":
                        entity.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "3.2":
                        entity.OtherSecurities = row.Amount ?? 0;
                        break;
                    case "3.3a":
                        entity.BalancesWithOtherSaccos = row.Amount ?? 0;
                        break;
                    case "3.3b":
                        entity.InvestmentsInCompanies = row.Amount ?? 0;
                        break;

                    // Net Loan Portfolio
                    case "4.1":
                        entity.GrossLoanPortfolio = row.Amount ?? 0;
                        break;
                    case "4.2":
                        entity.AllowanceForLoanLoss = row.Amount ?? 0;
                        break;

                    // Accounts Receivables
                    case "5.1":
                        entity.TaxRecoverable = row.Amount ?? 0;
                        break;
                    case "5.2":
                        entity.DeferredTaxAssets = row.Amount ?? 0;
                        break;
                    case "5.3":
                        entity.RetirementBenefitAssets = row.Amount ?? 0;
                        break;

                    // Property & Equipment & Other assets
                    case "6.1":
                        entity.InvestmentProperties = row.Amount ?? 0;
                        break;
                    case "6.2":
                        entity.PropertyAndEquipment = row.Amount ?? 0;
                        break;
                    case "6.3":
                        entity.PrepaidLeaseRentals = row.Amount ?? 0;
                        break;
                    case "6.4":
                        entity.IntangibleAssets = row.Amount ?? 0;
                        break;
                    case "6.5":
                        entity.OtherAssets = row.Amount ?? 0;
                        break;

                    // LIABILITIES
                    case "7":
                        entity.SavingsDeposits = row.Amount ?? 0;
                        break;
                    case "8":
                        entity.ShortTermDeposits = row.Amount ?? 0;
                        break;
                    case "9":
                        entity.NonWithdrawableDeposits = row.Amount ?? 0;
                        break;

                    // Accounts Payable & Other Liabilities
                    case "10.1":
                        entity.TaxPayable = row.Amount ?? 0;
                        break;
                    case "10.2":
                        entity.DividendsPayable = row.Amount ?? 0;
                        break;
                    case "10.3":
                        entity.DeferredTaxLiability = row.Amount ?? 0;
                        break;
                    case "10.4":
                        entity.RetirementBenefitsLiability = row.Amount ?? 0;
                        break;
                    case "10.5":
                        entity.OtherLiabilities = row.Amount ?? 0;
                        break;
                    case "10.6":
                        entity.ExternalBorrowings = row.Amount ?? 0;
                        break;

                    // EQUITY
                    case "11":
                        entity.ShareCapital = row.Amount ?? 0;
                        break;
                    case "12":
                        entity.CapitalGrants = row.Amount ?? 0;
                        break;

                    // Retained Earnings
                    case "13.1":
                        entity.PriorYearsRetainedEarnings = row.Amount ?? 0;
                        break;
                    case "13.2":
                        entity.CurrentYearSurplus = row.Amount ?? 0;
                        break;

                    // Other Equity Accounts
                    case "14":
                        entity.StatutoryReserve = row.Amount ?? 0;
                        break;
                }
            }

            return entity;
        }
    }

    public class ComprehensiveIncomeParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form7Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new DTComprehensiveIncomeReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                Year = Data.Period,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.RefNumber?.Trim())
                {
                    // Financial Income from Loans Portfolio
                    case "2.1":
                        entity.InterestOnLoanPortfolio = row.Amount ?? 0;
                        break;
                    case "2.2":
                        entity.FeesAndCommissionOnLoanPortfolio = row.Amount ?? 0;
                        break;

                    // Financial Income from Investments
                    case "3.1":
                        entity.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "3.2":
                        entity.DepositsWithBanks = row.Amount ?? 0;
                        break;
                    case "3.3":
                        entity.OtherInvestments = row.Amount ?? 0;
                        break;
                    case "3.4":
                        entity.OtherOperatingIncome = row.Amount ?? 0;
                        break;

                    // Financial Expense
                    case "4.2":
                        entity.InterestExpenseOnDeposits = row.Amount ?? 0;
                        break;
                    case "4.3":
                        entity.CostOfExternalBorrowings = row.Amount ?? 0;
                        break;
                    case "4.4":
                        entity.DividendExpenses = row.Amount ?? 0;
                        break;
                    case "4.5":
                        entity.OtherFinancialExpense = row.Amount ?? 0;
                        break;
                    case "4.6":
                        entity.FeesAndCommissionExpense = row.Amount ?? 0;
                        break;
                    case "4.7":
                        entity.OtherExpense = row.Amount ?? 0;
                        break;

                    // Allowance for Loan Loss
                    case "6.1":
                        entity.ProvisionForLoanLosses = row.Amount ?? 0;
                        break;
                    case "6.2":
                        entity.ValueOfLoansRecovered = row.Amount ?? 0;
                        break;

                    // Operating Expenses
                    case "7.1":
                        entity.PersonnelExpenses = row.Amount ?? 0;
                        break;
                    case "7.2":
                        entity.GovernanceExpenses = row.Amount ?? 0;
                        break;
                    case "7.3":
                        entity.MarketingExpenses = row.Amount ?? 0;
                        break;
                    case "7.4":
                        entity.DepreciationAndAmortization = row.Amount ?? 0;
                        break;
                    case "7.5":
                        entity.AdministrativeExpenses = row.Amount ?? 0;
                        break;

                    // Non-Operating Income/Expense
                    case "9.1":
                        entity.NonOperatingIncome = row.Amount ?? 0;
                        break;
                    case "9.2":
                        entity.NonOperatingExpense = row.Amount ?? 0;
                        break;

                    // Taxes and Donations
                    case "11":
                        entity.Taxes = row.Amount ?? 0;
                        break;
                    case "13":
                        entity.Donations = row.Amount ?? 0;
                        break;
                }
            }

            return entity;
        }
    }

    public class NWDTLiquidityParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2BStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new NWDTLiquidityReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                Period = Data.Period,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.Index?.Trim())
                {
                    case "1.1":
                        entity.LocalNotesAndCoins = row.Amount ?? 0;
                        break;
                    case "1.2":
                        entity.ForeignNotesAndCoins = row.Amount ?? 0;
                        break;
                    case "2.1":
                        entity.BalancesWithCommercialBanks = row.Amount ?? 0;
                        break;
                    case "2.2":
                        entity.TimeDepositsWithBanksMoreThan90Days = row.Amount ?? 0;
                        break;
                    case "2.3":
                        entity.OverdraftsAndMaturedLoans = row.Amount ?? 0;
                        break;
                    case "3.1":
                        entity.BalancesWithOtherSaccoSocieties = row.Amount ?? 0;
                        break;
                    case "3.2":
                        entity.BalancesWithOtherFinancialInstitutions = row.Amount ?? 0;
                        break;
                    case "3.3":
                        entity.BalancesDueToOtherSaccoSocieties = row.Amount ?? 0;
                        break;
                    case "3.4":
                        entity.BalancesDueToFinancialInstitutions = row.Amount ?? 0;
                        break;
                    case "3.5":
                        entity.MaturedLoansAndAdvances = row.Amount ?? 0;
                        break;
                    case "4.1":
                        entity.TreasuryBills = row.Amount ?? 0;
                        break;
                    case "4.2":
                        entity.TreasuryBondsBearerBonds = row.Amount ?? 0;
                        break;
                    case "6.1":
                        entity.MaturedLiabilities = row.Amount ?? 0;
                        break;
                    case "6.2":
                        entity.LiabilitiesMaturing91Days = row.Amount ?? 0;
                        break;
                }
            }

            entity.CalculateAndStoreTotals();

            return entity;
        }
    }

    public class NWDTDepositParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2CStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var depositReturns = new List<NWDTDepositReturn>();
            string currentRangeName = string.Empty;

            foreach (var row in Data.Rows)
            {
                // Skip TOTAL row as it's a summary
                if (row.DepositType?.ToUpper() == "TOTAL")
                {
                    continue;
                }

                // Update current range name if it's not empty
                if (!string.IsNullOrEmpty(row.Range))
                {
                    currentRangeName = row.Range;
                }

                var entity = new NWDTDepositReturn
                {
                    ReturnSubmissionId = ReturnSubmissionId,
                    StartDate = Data.StartDate,
                    EndDate = Data.EndDate,
                    //Period = Data.Period,
                    SaccoCsNumber = Data.SaccoCsNumber,
                    AmountInKshs000 = row.Amount,
                    RangeName = currentRangeName,  // Use the current range name
                    DepositType = row.DepositType,
                    NumberOfAccounts = row.NumberOfAccounts,
                    CreatedAt = DateTime.Now
                };

                depositReturns.Add(entity);
            }

            // Return the list of deposit returns
            return depositReturns;
        }
    }

    public class NWDTRiskClassificationParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2DStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var riskClassifications = new List<NWDTRiskClassificationReturn>();

            foreach (var row in Data.Rows)
            {
                var entity = new NWDTRiskClassificationReturn
                {
                    ReturnSubmissionId = ReturnSubmissionId,
                    LoanType = row.LoanType,
                    Classification = row.Classification,
                    NumberOfAccounts = row.NumberOfAccounts,
                    OutstandingLoanPortfolio = row.OutstandingLoanPortfolio,
                    RequiredProvision = row.RequiredProvision,
                    RequiredProvisionAmount = row.RequiredProvisionAmount,
                    //Period = Data.Period,
                    StartDate = Data.StartDate,
                    EndDate = Data.EndDate,
                    SaccoCsNumber = Data.CsNumber,
                    CreatedAt = DateTime.Now
                };

                riskClassifications.Add(entity);
            }

            return riskClassifications;
        }
    }

    public class NWDTInvestmentParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2EStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new NWDTInvestmentReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                //Period = Data.Period,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.Index)
                {
                    case "1.1":
                        entity.CoreCapital = row.Amount ?? 0;
                        break;
                    case "1.2":
                        entity.TotalAssets = row.Amount ?? 0;
                        break;
                    case "1.3":
                        entity.TotalDeposits = row.Amount ?? 0;
                        break;
                    case "1.4":
                        entity.NonEarningAssets = row.Amount ?? 0;
                        break;
                    case "1.5.1":
                        entity.SubsidiaryRelatedEntityInvestments = row.Amount ?? 0;
                        break;
                    case "1.5.2":
                        entity.EquityInvestments = row.Amount ?? 0;
                        break;
                    case "1.5.3":
                        entity.OtherInvestments = row.Amount ?? 0;
                        break;
                    case "1.6":
                        entity.OtherAssetsLandBuildingEquipment = row.Amount ?? 0;
                        break;
                    case "1.7":
                        entity.LandAndBuilding = row.Amount ?? 0;
                        break;
                    case "1.9":
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1) percentage /= 100;
                            entity.MaxLandBuildingEquipmentToTotalAssetRequirement = percentage;
                        }
                        break;
                    case "2.2":
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1) percentage /= 100;
                            entity.MaxLandBuildingToTotalAssetRequirement = percentage;
                        }
                        break;
                    case "2.5":
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1) percentage /= 100;
                            entity.MaxFinancialInvestmentsToCoreCapital = percentage;
                        }
                        break;
                    case "2.8":
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1) percentage /= 100;
                            entity.MaxEquityInvestmentsToTotalDeposits = percentage;
                        }
                        break;
                    case "3.1":
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1) percentage /= 100;
                            entity.MaxSubsidiaryInvestmentToTotalAssets = percentage;
                        }
                        break;
                    case "3.4":
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1) percentage /= 100;
                            entity.MaxOtherInvestmentsToCoreCapital = percentage;
                        }
                        break;
                }
            }

            entity.CalculateAndStoreTotals();

            return entity;
        }
    }

    public class NWDTComprehensiveIncomeParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2FStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new NWDTComprehensiveIncomeReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                //Period = Data.Period,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.RefNumber)
                {
                    // Financial Income from Loans Portfolio
                    case "2.1":
                        entity.InterestOnLoanPortfolio = row.Amount ?? 0;
                        break;
                    case "2.2":
                        entity.FeesCommissionOnLoanPortfolio = row.Amount ?? 0;
                        break;

                    // Financial Income from Investments
                    case "3.1":
                        entity.GovernmentSecuritiesIncome = row.Amount ?? 0;
                        break;
                    case "3.2":
                        entity.PlacementInBanksIncome = row.Amount ?? 0;
                        break;
                    case "3.3":
                        entity.CommercialPapersIncome = row.Amount ?? 0;
                        break;
                    case "3.4":
                        entity.CollectiveInvestmentSchemesIncome = row.Amount ?? 0;
                        break;
                    case "3.5":
                        entity.DerivativesIncome = row.Amount ?? 0;
                        break;
                    case "3.6":
                        entity.EquityInvestmentsIncome = row.Amount ?? 0;
                        break;
                    case "3.7":
                        entity.InvestmentInCompaniesIncome = row.Amount ?? 0;
                        break;

                    // Financial Expense
                    case "4.2":
                        entity.InterestExpenseOnDeposits = row.Amount ?? 0;
                        break;
                    case "4.3":
                        entity.CostOfExternalBorrowings = row.Amount ?? 0;
                        break;
                    case "4.4":
                        entity.DividendExpenses = row.Amount ?? 0;
                        break;
                    case "4.5":
                        entity.OtherFinancialExpense = row.Amount ?? 0;
                        break;
                    case "4.6":
                        entity.FeesCommissionExpense = row.Amount ?? 0;
                        break;
                    case "4.7":
                        entity.OtherExpense = row.Amount ?? 0;
                        break;

                    // Allowance for Loan Loss
                    case "6.1":
                        entity.ProvisionForLoanLosses = row.Amount ?? 0;
                        break;
                    case "6.2":
                        entity.ValueOfLoansRecovered = row.Amount ?? 0;
                        break;

                    // Operating Expenses
                    case "7.1":
                        entity.PersonnelExpenses = row.Amount ?? 0;
                        break;
                    case "7.2":
                        entity.GovernanceExpenses = row.Amount ?? 0;
                        break;
                    case "7.3":
                        entity.MarketingExpenses = row.Amount ?? 0;
                        break;
                    case "7.4":
                        entity.DepreciationAmortizationCharges = row.Amount ?? 0;
                        break;
                    case "7.5":
                        entity.AdministrativeExpenses = row.Amount ?? 0;
                        break;

                    // Non-Operating Income/Expense
                    case "9.1":
                        entity.NonOperatingIncome = row.Amount ?? 0;
                        break;
                    case "9.2":
                        entity.NonOperatingExpense = row.Amount ?? 0;
                        break;

                    // Taxes and Donations
                    case "11":
                        entity.Taxes = row.Amount ?? 0;
                        break;
                    case "13":
                        entity.Donations = row.Amount ?? 0;
                        break;
                }
            }

            entity.CalculateAndStoreTotals();

            return entity;
        }
    }

    public class NWDTFinancialPositionParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form2GStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new NWDTFinancialPositionReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                //Period = Data.Period,
                SaccoCsNumber = Data.SaccoCsNumber,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.Rows)
            {
                switch (row.RefNumber)
                {
                    // Cash & Cash Equivalent Section
                    case "1.1":
                        entity.CashInHand = row.Amount ?? 0;
                        break;
                    case "1.2":
                        entity.CashAtBank = row.Amount ?? 0;
                        break;

                    // Prepayments & Sundry Receivables
                    case "2.0":
                        entity.PrepaymentsAndSundryReceivables = row.Amount ?? 0;
                        break;

                    // Financial Investments
                    case "3.1":
                        entity.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "3.2":
                        entity.PlacementInFinancialInstitutions = row.Amount ?? 0;
                        break;
                    case "3.3":
                        entity.CommercialPapers = row.Amount ?? 0;
                        break;
                    case "3.4":
                        entity.CollectiveInvestmentSchemes = row.Amount ?? 0;
                        break;
                    case "3.5":
                        entity.Derivatives = row.Amount ?? 0;
                        break;
                    case "3.6":
                        entity.EquityInvestments = row.Amount ?? 0;
                        break;
                    case "3.7":
                        entity.InvestmentInCompanies = row.Amount ?? 0;
                        break;

                    // Loan Portfolio
                    case "4.1":
                        entity.GrossLoanPortfolio = row.Amount ?? 0;
                        break;
                    case "4.2":
                        entity.AllowanceForLoanLoss = row.Amount ?? 0;
                        break;

                    // Accounts Receivables
                    case "5.1":
                        entity.TaxRecoverable = row.Amount ?? 0;
                        break;
                    case "5.2":
                        entity.DeferredTaxAssets = row.Amount ?? 0;
                        break;
                    case "5.3":
                        entity.RetirementBenefitAssets = row.Amount ?? 0;
                        break;

                    // Property & Equipment Section
                    case "6.1":
                        entity.InvestmentProperties = row.Amount ?? 0;
                        break;
                    case "6.2":
                        entity.PropertyAndEquipment = row.Amount ?? 0;
                        break;
                    case "6.3":
                        entity.PrepaidLeaseRentals = row.Amount ?? 0;
                        break;
                    case "6.4":
                        entity.IntangibleAssets = row.Amount ?? 0;
                        break;
                    case "6.5":
                        entity.OtherAssets = row.Amount ?? 0;
                        break;

                    // Liabilities - Deposits
                    case "7":
                        entity.NonWithdrawableDeposits = row.Amount ?? 0;
                        break;

                    // Liabilities - Accounts Payable
                    case "8.1":
                        entity.TaxPayable = row.Amount ?? 0;
                        break;
                    case "8.2":
                        entity.DividendsPayable = row.Amount ?? 0;
                        break;
                    case "8.3":
                        entity.DeferredTaxLiability = row.Amount ?? 0;
                        break;
                    case "8.4":
                        entity.RetirementBenefitsLiability = row.Amount ?? 0;
                        break;
                    case "8.5":
                        entity.OtherLiabilities = row.Amount ?? 0;
                        break;
                    case "8.6":
                        entity.ExternalBorrowings = row.Amount ?? 0;
                        break;

                    // Equity
                    case "9":
                        entity.ShareCapital = row.Amount ?? 0;
                        break;
                    case "10":
                        entity.CapitalGrants = row.Amount ?? 0;
                        break;
                    case "11.1":
                        entity.PriorYearsRetainedEarnings = row.Amount ?? 0;
                        break;
                    case "11.2":
                        entity.CurrentYearSurplus = row.Amount ?? 0;
                        break;
                    case "12.1":
                        entity.StatutoryReserve = row.Amount ?? 0;
                        break;
                    case "12.2":
                        entity.OtherReserves = row.Amount ?? 0;
                        break;
                    case "12.3":
                        entity.RevaluationReserves = row.Amount ?? 0;
                        break;
                    case "12.4":
                        entity.ProposedDividends = row.Amount ?? 0;
                        break;
                    case "12.5":
                        entity.AdjustmentToEquity = row.Amount ?? 0;
                        break;
                }
            }

            entity.CalculateAndStoreTotals();

            return entity;
        }
    }

    public class ManagementReturnParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public ManagementData Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new ManagementReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                MRating = Data.MRating,
                CreatedAt = DateTime.Now
            };

            foreach (var row in Data.ManagementReports)
            {
                switch (row.Category?.Trim())
                {
                    case "GOVERNANCE, STRUCTURE AND ORGANIZATION":
                        entity.GorvenanceStructureScore = row.Score ?? 0;
                        entity.GorvenanceStructureWeight = row.Weight ?? 0;
                        entity.GorvenanceStructureWeightedScore = row.WeightedScore ?? 0;
                        break;
                    case "INTERNAL CONTROLS":
                        entity.InternalControlsScore = row.Score ?? 0;
                        entity.InternalControlsWeight = row.Weight ?? 0;
                        entity.InternalControlsWeightedScore = row.WeightedScore ?? 0;
                        break;
                    case "COMPLIANCE WITH LAWS AND REGULATIONS":
                        entity.ComplianceWithLawsAndRegulationsScore = row.Score ?? 0;
                        entity.ComplianceWithLawsAndRegulationsWeight = row.Weight ?? 0;
                        entity.ComplianceWithLawsAndRegulationsWeightedScore = row.WeightedScore ?? 0;
                        break;
                    case "MEMBER PROTECTION":
                        entity.MemberProtectionScore = row.Score ?? 0;
                        entity.MemberProtectionWeight = row.Weight ?? 0;
                        entity.MemberProtectionWeightedScore = row.WeightedScore ?? 0;
                        break;
                    case "ADEQUACY OF MIS":
                        entity.AdequacyOfMISScore = row.Score ?? 0;
                        entity.AdequacyOfMISWeight = row.Weight ?? 0;
                        entity.AdequacyOfMISWeightedScore = row.WeightedScore ?? 0;
                        break;
                    case "OVERALL RISK PROFILE":
                        entity.OverallRiskProfileScore = row.Score ?? 0;
                        entity.OverallRiskProfileWeight = row.Weight ?? 0;
                        entity.OverallRiskProfileWeightedScore = row.WeightedScore ?? 0;
                        break;
                }
            }

            return entity;
        }
    }

    public class SectoralLendingParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public SectoralLendingReportDto Data { get; set; } = null!;

        public object ToEntity()
        {
            var report = new SectoralLendingReport
            {
                ReturnSubmissionId = ReturnSubmissionId,
                Year = Data.Year,
                Month = Data.Month,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoName = Data.SaccoName,
                SaccoId = Data.SaccoCsNumber,
                FilePath = string.Empty, // Will be set by file upload service
                DaysLateBy = 0,
                Version = 1,
                CreatedAt = DateTime.Now
            };

            // Process categories and economic sector data
            var economicSectorDataList = new List<EconomicSectorData>();

            foreach (var category in Data.Categories)
            {
                foreach (var subCategory in category.SubCategories)
                {
                    foreach (var economicSector in subCategory.EconomicSectors)
                    {
                        var economicSectorData = new EconomicSectorData
                        {
                            ReturnSubmissionId = ReturnSubmissionId,
                            //ReturnId = ReturnSubmissionId, // This will be updated when linked to actual return
                            Amount = economicSector.Amount,
                            EconomicSectorCode = economicSector.EconomicSectorCode,
                            //EconomicSectorId = economicSector.EconomicSectorCode, // This should be a proper ID reference
                            Category = category.CategoryName,
                            SubCategory = subCategory.SubCategoryName,
                            SaccoType = Data.SaccoType,
                            EconomicSectorName = economicSector.EconomicSectorName,
                            CreatedAt = DateTime.Now
                        };

                        economicSectorDataList.Add(economicSectorData);
                    }
                }
            }

            // Return both the report and the economic sector data
            return new { Report = report, EconomicSectorData = economicSectorDataList };
        }
    }

    public class DailyLiquidityParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public DailyLiquidityStatement Data { get; set; } = null!;

        public object ToEntity()
        {
            var entity = new DailyLiquidityReturn
            {
                ReturnSubmissionId = ReturnSubmissionId,
                ReportDate = Data.ReportDate,
                SACCOName = Data.SACCOName,
                CSNO = Data.CSNO,
                DaysLateBy = 0,
                Version = 1,
                FilePath = string.Empty, // Will be set by file upload service

                // Opening Balances
                BankBalancesOpening = Data.BankBalancesOpening,
                ConsolidatedTreasuryCashBalancesOpening = Data.ConsolidatedTreasuryCashBalancesOpening,
                TellersBalancesOpening = Data.TellersBalancesOpening,
                MobileMoneyChannelsOpening = Data.MobileMoneyChannelsOpening,
                PlacementWithBanksOpening = Data.PlacementWithBanksOpening,
                SubTotalOpening = Data.SubTotalOpening,

                // Day Receipts
                DepositsFromMembers = Data.DepositsFromMembers,
                CashLoanRepayments = Data.CashLoanRepayments,
                OtherCashReceipts = Data.OtherCashReceipts,
                SubTotalReceipts = Data.SubTotalReceipts,
                TotalOpeningAndReceipts = Data.TotalOpeningAndReceipts,

                // Day Payments
                CashWithdrawalsByMembers = Data.CashWithdrawalsByMembers,
                CashPaymentsToMembers = Data.CashPaymentsToMembers,
                OtherCashPayments = Data.OtherCashPayments,
                SubTotalPayments = Data.SubTotalPayments,

                // Closing Balances
                BankBalancesClosing = Data.BankBalancesClosing,
                ConsolidatedTreasuryCashBalancesClosing = Data.ConsolidatedTreasuryCashBalancesClosing,
                TellersBalancesClosing = Data.TellersBalancesClosing,
                MobileMoneyChannelsClosing = Data.MobileMoneyChannelsClosing,
                PlacementWithBanksClosing = Data.PlacementWithBanksClosing,
                TotalClosingBalance = Data.TotalClosingBalance,

                // Deposit Liabilities
                BOSADeposits = Data.BOSADeposits,
                FOSADeposits = Data.FOSADeposits,
                TotalDeposits = Data.TotalDeposits,

                // Liquidity Ratios
                TotalClosingBalanceToTotalDepositsRatio = Data.TotalClosingBalanceToTotalDepositsRatio,
                TotalClosingBalanceToFOSADepositsRatio = Data.TotalClosingBalanceToFOSADepositsRatio,

                CreatedAt = DateTime.Now
            };

            return entity;
        }
    }

    public class InsiderLendingParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public InsiderLendingReportDTO Data { get; set; } = null!;

        public object ToEntity()
        {
            var header = new InsiderLendingHeader
            {
                ReturnSubmissionId = ReturnSubmissionId,
                StartDate = Data.StartDate,
                EndDate = Data.EndDate,
                SaccoName = Data.SaccoName,
                CSNO = Data.SaccoSocietyCsNumber,
                DaysLateBy = 0,
                Version = 1,
                FilePath = string.Empty, // Will be set by file upload service
                CreatedAt = DateTime.Now
            };

            // Process individual loans
            var insiderLoans = new List<InsiderLoan>();

            foreach (var loan in Data.Loans)
            {
                var insiderLoan = new InsiderLoan
                {
                    ReturnSubmissionId = ReturnSubmissionId,
                    NameOfBorrower = loan.NameOfBorrower,
                    LoanCategory = loan.LoanCategory,
                    MemberNumber = loan.MemberNumber,
                    PositionHeld = loan.PositionHeld,
                    LoanTypeName = loan.LoanTypeName,
                    AmountAppliedFor = loan.AmountAppliedFor,
                    AmountGranted = loan.AmountGranted,
                    DateApprovedOrRatified = loan.DateApprovedOrRatified,
                    AmountOfBosaDeposits = loan.AmountOfBosaDeposits,
                    NatureOfSecurity = loan.NatureOfSecurity,
                    RepaymentCommencementDate = loan.RepaymentCommencementDate,
                    RepaymentPeriod = loan.RepaymentPeriod,
                    OtherRemarks = loan.OtherRemarks,
                    OutstandingAmount = loan.OutstandingAmount,
                    PerfomanceCategory = loan.PerfomanceCategory,
                    RepaymentStatus = loan.RepaymentStatus,
                    CreatedAt = DateTime.Now
                };

                insiderLoans.Add(insiderLoan);
            }

            // Return both the header and the loans
            return new { Header = header, InsiderLoans = insiderLoans };
        }
    }
}