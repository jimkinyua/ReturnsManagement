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

            foreach (var row in Data.Rows)
            {
                var entity = new DepositReturn
                {
                    ReturnSubmissionId = ReturnSubmissionId,
                    SaccoCsNumber = Data.SaccoCsNumber,
                    Year = Data.Period,
                    StartDate = Data.StartDate,
                    EndDate = Data.EndDate,
                    //Range = row.RangeName,
                    DepositType = row.DepositType,
                    NumberOfAccounts = row.NumberOfAccounts,
                    //Amount = row.AmountInKshs000,
                    CreatedAt = DateTime.Now
                };

                depositReturns.Add(entity);
            }

            // Return the first one for now, but in practice you might need to handle multiple entities
            return depositReturns.FirstOrDefault() ?? new DepositReturn();
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
            /*foreach (var row in Data.Rows)
            {
                switch (row.Index?.Trim())
                {
                    // Core Capital items
                    case "1.1":
                        entity.ShareCapital = row.Amount ?? 0;
                        break;
                    case "1.2":
                        entity.InstitutionalCapital = row.Amount ?? 0;
                        break;
                    case "1.3":
                        entity.StatutoryReserves = row.Amount ?? 0;
                        break;
                    case "1.4":
                        entity.OtherRevenue = row.Amount ?? 0;
                        break;
                    case "1.5":
                        entity.RetainedEarnings = row.Amount ?? 0;
                        break;
                    case "1.6":
                        entity.Grants = row.Amount ?? 0;
                        break;
                    case "1.7":
                        entity.ProposedDividends = row.Amount ?? 0;
                        break;
                    case "1.8":
                        entity.SubTotalCoreCapital = row.Amount ?? 0;
                        break;
                    case "1.9":
                        entity.AllowanceLoanLoss = row.Amount ?? 0;
                        break;
                    case "1.10":
                        entity.TotalCoreCapital = row.Amount ?? 0;
                        break;

                    // Total Assets
                    case "2":
                        entity.TotalAssets = row.Amount ?? 0;
                        break;

                    // Total Deposits
                    case "3":
                        entity.TotalDeposits = row.Amount ?? 0;
                        break;

                    // Ratios
                    case "4.1":
                        entity.CoreCapitalToTotalAssetsRatio = row.Amount ?? 0;
                        break;
                    case "4.2":
                        entity.CoreCapitalToTotalAssetsMinStatRatio = row.Amount ?? 0;
                        break;
                    case "4.3":
                        entity.ExcessShortfall = row.Amount ?? 0;
                        break;
                    case "4.4":
                        entity.CoreCapitalToTotalDepositsRatio = row.Amount ?? 0;
                        break;
                    case "4.5":
                        entity.CoreCapitalToTotalDepositsMinStatRatio = row.Amount ?? 0;
                        break;
                    case "4.6":
                        entity.ExcessShortfall2 = row.Amount ?? 0;
                        break;
                }
            }*/

            return entity;
        }
    }

    public class RiskClassificationParsedRow : IParsedRow
    {
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public Form4Statement Data { get; set; } = null!;

        public object ToEntity()
        {
            // Since Risk Classification has multiple rows (Regular + Rescheduled/Renegotiated loans),
            // we need to return a list of entities
            var riskClassificationReturns = new List<DTRiskClassificationReturn>();

            foreach (var row in Data.Rows)
            {
                // Skip the "Total" row as it's just a summary
                if (row.LoanType == "Total")
                    continue;

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

                riskClassificationReturns.Add(entity);
            }

            // Return the first one for now, but in practice you might need to handle multiple entities
            return riskClassificationReturns.FirstOrDefault() ?? new DTRiskClassificationReturn();
        }
    }

    // You would need to implement similar classes for other form types:
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
}