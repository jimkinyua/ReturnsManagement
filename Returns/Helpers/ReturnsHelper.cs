using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.Controllers;
using Returns.DTOs;
using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.NWDT;
using Returns.Migrations;
using Returns.Models;
using Returns.Models.Data;
using System.Diagnostics;
using static Returns.Helpers.ExcelService;

namespace Returns.Helpers
{
    public static class ReturnsHelper
    {
        public static bool AreAllFormsPresent(params object[] forms)
        {
            return forms.All(form => form != null);
        }

        public static bool CheckLateReturns(Return returnItem)
        {
            var TotalLateReturns = CountLateReturns(returnItem);
            if (TotalLateReturns > 0)
            {
                return true;
            }
            return false;
        }

        public static async Task<List<string>> GetPreviousVersionIdsAsync(Return returnEntity)
        {
            // 1. Bulk load all (Id → PreviousVersionId) for this Sacco+Type+Year
            using var context = new ReturnsDbContext();
            var links = await context.Returns
                .AsNoTracking()
                .Where(r =>
                    r.SaccoId == returnEntity.SaccoId &&
                    r.SaccoType == returnEntity.SaccoType &&
                    r.ReturnFor.Year == returnEntity.ReturnFor.Year
                )
                .Select(r => new { r.Id, r.PreviousVersionId })
                .ToListAsync();

            // 2. Build the lookup map
            var map = links.ToDictionary(x => x.Id, x => x.PreviousVersionId);

            // 3. Walk the chain in a simple for‑loop
            var previousIds = new List<string>();
            for (var currentId = returnEntity.Id;
                 map.TryGetValue(currentId, out var prevId) && !string.IsNullOrEmpty(prevId);
                 currentId = prevId!)
            {
                previousIds.Add(prevId!);
            }

            return previousIds;
        }




        public static int CountPopulatedReturns(Return returnItem)
        {
            int count = 0;

            if (returnItem.CapitalAdequencies.Any()) count++;
            if (returnItem.LiquidityReturns.Any()) count++;
            if (returnItem.RiskClassifications.Any()) count++;
            if (returnItem.InvestmentReturns.Any()) count++;
            if (returnItem.StatementOfFinancialPositionReturns.Any()) count++;
            if (returnItem.StatementOfComprehensiveIncomeReturns.Any()) count++;
            if (returnItem.DepositReturns.Any()) count++;
            if (returnItem.OtherReturns.Any()) count++;

            return count;
        }

        public static int CountLateReturns(Return returnItem)
        {
            int count = 0;

            if (returnItem.CapitalAdequencies.Any(c => c.DaysLateBy > 0)) count++;
            if (returnItem.LiquidityReturns.Any(l => l.DaysLateBy > 0)) count++;
            if (returnItem.RiskClassifications.Any(r => r.DaysLateBy > 0)) count++;
            if (returnItem.InvestmentReturns.Any(i => i.DaysLateBy > 0)) count++;
            if (returnItem.StatementOfFinancialPositionReturns.Any(s => s.DaysLateBy > 0)) count++;
            if (returnItem.StatementOfComprehensiveIncomeReturns.Any(s => s.DaysLateBy > 0)) count++;
            if (returnItem.DepositReturns.Any(d => d.DaysLateBy > 0)) count++;

            return count;
        }
        public static int CountPopulatedReturnsNWDT(Return returnItem)
        {
            int count = 0;

            if (returnItem.NDWTCapitalAdequacyReturns.Any()) count++;
            if (returnItem.NWDTLiquidityReturns.Any()) count++;
            if (returnItem.NWDTRiskClassificationReturns.Any()) count++;
            if (returnItem.NWDTInvestmentReturns.Any()) count++;
            if (returnItem.NWDTFinancialPositionReturns.Any()) count++;
            if (returnItem.NWDTComprehensiveIncomeReturns.Any()) count++;
            if (returnItem.NWDTDepositReturns.Any()) count++;
            if (returnItem.OtherReturns.Any()) count++;

            return count;
        }

        public static int CountLateReturnsNWDT(Return returnItem)
        {
            int count = 0;

            if (returnItem.NDWTCapitalAdequacyReturns.Any(c => c.DaysLateBy > 0)) count++;
            if (returnItem.NWDTLiquidityReturns.Any(l => l.DaysLateBy > 0)) count++;
            if (returnItem.NWDTRiskClassificationReturns.Any(r => r.DaysLateBy > 0)) count++;
            if (returnItem.NWDTInvestmentReturns.Any(i => i.DaysLateBy > 0)) count++;
            if (returnItem.NWDTFinancialPositionReturns.Any(s => s.DaysLateBy > 0)) count++;
            if (returnItem.NWDTComprehensiveIncomeReturns.Any(s => s.DaysLateBy > 0)) count++;
            if (returnItem.NWDTDepositReturns.Any(d => d.DaysLateBy > 0)) count++;
            // if (returnItem.OtherReturns.Any(d => d.DaysLateBy > 0)) count++;

            return count;
        }

        public static bool CheckLateReturnsNWDT(Return returnItem)
        {
            var TotalLateReturns = CountLateReturnsNWDT(returnItem);
            if (TotalLateReturns > 0)
            {
                return true;
            }
            return false;
        }

        /*   public static (bool IsCapitalAdequacyLate, bool IsLiquidityReturnLate, bool IsRiskClassificationLate, bool IsInvestmentReturnLate, bool IsStatementOfFinancialPositionLate, bool IsStatementOfComprehensiveIncomeLate, bool IsSaccoAnalysisLate, bool IsDepositReturnLate) CheckLateReturns(Return returnItem)
           {
               bool isCapitalAdequacyLate = returnItem.CapitalAdequencies.Any() && HowLate(returnItem.CapitalAdequencies.First().RespondedAt, returnItem.ReturnFor) > 0;
               bool isLiquidityReturnLate = returnItem.DTLiquidityReturns.Any() && HowLate(returnItem.DTLiquidityReturns.First().RespondedAt, returnItem.ReturnFor) > 0;
               bool isRiskClassificationLate = returnItem.DTRiskClassificationReturns.Any() && HowLate(returnItem.DTRiskClassificationReturns.First().RespondedAt, returnItem.ReturnFor) > 0;
               bool isInvestmentReturnLate = returnItem.DTInvestmentReturns.Any() && HowLate(returnItem.DTInvestmentReturns.First().RespondedAt, returnItem.ReturnFor) > 0;
               bool isStatementOfFinancialPositionLate = returnItem.DTFinancialPositionReturns.Any() && HowLate(returnItem.DTFinancialPositionReturns.First().RespondedAt, returnItem.ReturnFor) > 0;
               bool isStatementOfComprehensiveIncomeLate = returnItem.DTComprehensiveIncomeReturns.Any() && HowLate(returnItem.DTComprehensiveIncomeReturns.First().RespondedAt, returnItem.ReturnFor) > 0;
               bool isSaccoAnalysisLate = returnItem.SaccoAnalysis.Any() && HowLate(returnItem.SaccoAnalysis.First().RespondedAt, returnItem.ReturnFor) > 0;
               bool isDepositReturnLate = returnItem.DepositReturns.Any() && HowLate(returnItem.DepositReturns.First().RespondedAt, returnItem.ReturnFor) > 0;

               return (isCapitalAdequacyLate, isLiquidityReturnLate, isRiskClassificationLate, isInvestmentReturnLate, isStatementOfFinancialPositionLate, isStatementOfComprehensiveIncomeLate, isSaccoAnalysisLate, isDepositReturnLate);
           }*/
        public static (int CapitalAdequacyDaysLate, int LiquidityReturnDaysLate, int RiskClassificationDaysLate, int InvestmentReturnDaysLate, int StatementOfFinancialPositionDaysLate, int StatementOfComprehensiveIncomeDaysLate, int SaccoAnalysisDaysLate, int DepositReturnDaysLate) CalculateDaysLateForChildren(Return returnItem)
        {
            int capitalAdequacyDaysLate = returnItem.CapitalAdequencies.Any() ? HowLate(returnItem.CapitalAdequencies.First().CreatedAt, returnItem.ReturnFor) : 0;
            int liquidityReturnDaysLate = returnItem.LiquidityReturns.Any() ? HowLate(returnItem.LiquidityReturns.First().CreatedAt, returnItem.ReturnFor) : 0;
            int riskClassificationDaysLate = returnItem.RiskClassifications.Any() ? HowLate(returnItem.RiskClassifications.First().CreatedAt, returnItem.ReturnFor) : 0;
            int investmentReturnDaysLate = returnItem.InvestmentReturns.Any() ? HowLate(returnItem.InvestmentReturns.First().CreatedAt, returnItem.ReturnFor) : 0;
            int statementOfFinancialPositionDaysLate = returnItem.StatementOfFinancialPositionReturns.Any() ? HowLate(returnItem.StatementOfFinancialPositionReturns.First().CreatedAt, returnItem.ReturnFor) : 0;
            int statementOfComprehensiveIncomeDaysLate = returnItem.StatementOfComprehensiveIncomeReturns.Any() ? HowLate(returnItem.StatementOfComprehensiveIncomeReturns.First().CreatedAt, returnItem.ReturnFor) : 0;
            int saccoAnalysisDaysLate = returnItem.SaccoAnalysis.Any() ? HowLate(returnItem.SaccoAnalysis.First().CreatedAt, returnItem.ReturnFor) : 0;
            int depositReturnDaysLate = returnItem.DepositReturns.Any() ? HowLate(returnItem.DepositReturns.First().CreatedAt, returnItem.ReturnFor) : 0;

            return (capitalAdequacyDaysLate, liquidityReturnDaysLate, riskClassificationDaysLate, investmentReturnDaysLate, statementOfFinancialPositionDaysLate, statementOfComprehensiveIncomeDaysLate, saccoAnalysisDaysLate, depositReturnDaysLate);
        }


        public static int HowLate(DateTime submittedAt, DateTime returnFor)
        {
            TimeSpan difference = submittedAt - returnFor;
            return difference.Days > 0 ? difference.Days : 0;
        }

        private static DateTime CalculateDueDate(DateTime reportingEndDate, string periodType)
        {
            switch (periodType)
            {
                case "Daily":
                    // Daily forms are due the same day
                    return reportingEndDate;

                case "Monthly":
                    // Monthly forms are due by the 15th of the next month
                    var monthlyDue = reportingEndDate.AddMonths(1);
                    return new DateTime(monthlyDue.Year, monthlyDue.Month, 15);

                case "Quarterly":
                    // Quarterly forms are due by the 15th of the month after the quarter ends
                    var quarter = (reportingEndDate.Month - 1) / 3 + 1;
                    var quarterEnd = new DateTime(reportingEndDate.Year, quarter * 3, 1).AddDays(-1);
                    var quarterlyDue = quarterEnd.AddMonths(1);
                    return new DateTime(quarterlyDue.Year, quarterlyDue.Month, 15);

                case "Annual":
                    // Annual forms are due by January 15th of the next year
                    return new DateTime(reportingEndDate.Year + 1, 1, 15);

                case "Semi-Annual":
                    // Semi-annual forms are due 15 days after the half-year ends
                    if (reportingEndDate.Month <= 6)
                    {
                        // First half ends June 30, due by July 15
                        return new DateTime(reportingEndDate.Year, 7, 15);
                    }
                    else
                    {
                        // Second half ends December 31, due by January 15
                        return new DateTime(reportingEndDate.Year + 1, 1, 15);
                    }

                case "Bi-Monthly":
                    // Bi-monthly forms are due on the 1st or 15th
                    if (reportingEndDate.Day <= 15)
                    {
                        return new DateTime(reportingEndDate.Year, reportingEndDate.Month, 15);
                    }
                    else
                    {
                        var nextMonth = reportingEndDate.AddMonths(1);
                        return new DateTime(nextMonth.Year, nextMonth.Month, 1);
                    }

                default:
                    // Default to 15 days after the end date if unknown period type
                    return reportingEndDate.AddDays(15);
            }
        }


        public static int CalculateDaysLate(ReturnForm form, DateTime currentDate, DateTime EndDate)
        {
            var periodName = form.Period.Name;
            DateTime dueDate = CalculateDueDate(EndDate, form.Period.Name);

            // Calculate the difference in days
            TimeSpan difference = currentDate - dueDate;
            int daysLate = difference.Days;

            // If the form is not late, return 0
            return daysLate > 0 ? daysLate : 0;
        }

        public static bool HasValidUploads(NewReturnDTO createFormDTO)
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            return createFormDTO.FormUploads != null && createFormDTO.FormUploads.Any(f => f.formFile != null);
        }


        public static bool IsFormDueForSubmission(ReturnForm form, DateTime currentDate)
        {
            var periodName = form.Period.Name;

            switch (periodName)
            {
                case "Daily":
                    // Daily forms can be submitted on the same day
                    return true;

                case "Monthly":
                    // Current month's form isn't due yet
                    // Previous month's form is due until the 15th of the current month
                    int daysInCurrentMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                    // return currentDate.Day >= 15;
                    return true;

                case "Quarterly":
                    // Quarterly returns are due in the first 15 days after the quarter ends
                    // int currentQuarter = (currentDate.Month - 1) / 3 + 1;
                    // int currentQuarterStartMonth = ((currentQuarter - 1) * 3) + 1;

                    // // Forms are only due in the first month of the new quarter, within the first 15 days
                    // return currentDate.Month == currentQuarterStartMonth && currentDate.Day <= 15;
                    return true;

                case "Annual":
                    // Annual returns are due in the first 15 days of the new year
                    // return currentDate.Month == 1 && currentDate.Day <= 15;
                    return true;


                case "Semi-Annual":
                    // Semi-annual returns are due in the first 15 days after the half-year ends
                    // First half: January 1 to June 30, due by July 15
                    // Second half: July 1 to December 31, due by January 15
                    // return (currentDate.Month == 1 || currentDate.Month == 7) && currentDate.Day <= 15;
                    return true;


                case "Bi-Monthly":
                    // Bi-monthly returns are due on the 1st and 15th of each month
                    // 1st half of month (1-15): Submit on the 15th
                    // 2nd half of month (16-end): Submit on the 1st of next month
                    // return currentDate.Day == 1 || currentDate.Day == 15;
                    return true;


                default:
                    return false;
            }
        }

        public static async Task ProcessCapitalAdequacyForm(IFormFile file, string returnId, ILogger _logger, ReturnForm form, Boolean IsAmendMent, string PrevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            var Form1Statement = ExcelService.ImportCapitalAdequacyRows(file, _logger);
            if (Form1Statement == null || !Form1Statement.Rows.Any())
                throw new Exception("No data found in Capital Adequacy Form");

            // save the Excel
            var Path = await FormsHelper.SaveFileAsync(file, "Capital Adequacy Returns", "");
            if (Path == null)
            {
                throw new Exception("Error saving file");
            }
            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, Form1Statement.EndDate);
            string EffectiveReturnId = returnId;
            string PreviousReturnId = string.Empty;
            DTCapitalAdequacyReturn? capitalAdequacy = null;
            if (!IsAmendMent)
            {
                capitalAdequacy = new DTCapitalAdequacyReturn
                {
                    ReturnId = returnId,
                    FilePath = Path,
                    Year = Form1Statement.Period,
                    StartDate = Form1Statement.StartDate,
                    EndDate = Form1Statement.EndDate,
                    Frequency = form.Period.Name,
                    DaysLateBy = DaysLateBy,
                    IsCurrent = true,
                    IsAmended = false,
                };
            }
            else
            {
                capitalAdequacy = await _context.DTCapitalAdequacyReturns.FirstOrDefaultAsync(x => x.ReturnId == EffectiveReturnId);
                if (capitalAdequacy == null)
                {
                    throw new Exception(
                        "Not Fond");
                }
                capitalAdequacy.PreviousReturnId = PreviousReturnId;
                capitalAdequacy.IsCurrent = false;
                capitalAdequacy.IsAmended = true;
            }


            foreach (var row in Form1Statement.Rows)
            {
                switch (row.Index?.Trim())
                {
                    // CORE CAPITAL
                    case "1.1.1":
                        capitalAdequacy.ShareCapital = row.Amount ?? 0;
                        break;
                    case "1.1.2":
                        capitalAdequacy.StatutoryReserves = row.Amount ?? 0;
                        break;
                    case "1.1.3":
                        capitalAdequacy.RetainedEarningsAccumulatedLosses = row.Amount ?? 0;
                        break;
                    case "1.1.4":
                        capitalAdequacy.NetSurplusAfterTaxCurrentYearToDate = row.Amount ?? 0;
                        break;
                    case "1.1.5":
                        capitalAdequacy.CapitalGrantsEquityInNature = row.Amount ?? 0;
                        break;
                    case "1.1.6":
                        capitalAdequacy.GeneralReserves = row.Amount ?? 0;
                        break;
                    case "1.1.7":
                        capitalAdequacy.OtherReserves = row.Amount ?? 0;
                        break;
                    case "1.1.8":
                        capitalAdequacy.SubTotalCoreCapital = row.Amount ?? 0;
                        break;

                    // DEDUCTIONS
                    case "1.1.9":
                        capitalAdequacy.InvestmentsInSubsidiaryAndEquityInstruments = row.Amount ?? 0;
                        break;
                    case "1.1.10":
                        capitalAdequacy.OtherDeductions = row.Amount ?? 0;
                        break;
                    case "1.1.11":
                        capitalAdequacy.TotalDeductions = row.Amount ?? 0;
                        break;
                    case "1.1.12":
                        capitalAdequacy.CoreCapital = row.Amount ?? 0;
                        break;
                    case "1.1.13":
                        capitalAdequacy.InstitutionalCapital = row.Amount ?? 0;
                        break;

                    // ON-BALANCE SHEET ASSETS
                    case "2.1":
                        capitalAdequacy.CashLocalAndForeignCurrency = row.Amount ?? 0;
                        break;
                    case "2.2":
                        capitalAdequacy.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "2.3":
                        capitalAdequacy.DepositsAndBalancesAtOtherInstitutions = row.Amount ?? 0;
                        break;
                    case "2.4":
                        capitalAdequacy.LoansAndAdvances = row.Amount ?? 0;
                        break;
                    case "2.5":
                        capitalAdequacy.Investments = row.Amount ?? 0;
                        break;
                    case "2.6":
                        capitalAdequacy.PropertyAndEquipment = row.Amount ?? 0;
                        break;
                    case "2.7":
                        capitalAdequacy.OtherAssets = row.Amount ?? 0;
                        break;
                    case "2.8":
                        capitalAdequacy.TotalOnBalanceSheetAssets = row.Amount ?? 0;
                        break;

                    case "2.9":
                        capitalAdequacy.TotalAssetsPerBalanceSheet = row.Amount ?? 0;
                        break;

                    case "3.0":
                        capitalAdequacy.Difference = row.Amount ?? 0;
                        break;

                    // OFF-BALANCE SHEET
                    case "3":
                        capitalAdequacy.TotalOffBalanceSheetAssets = row.Amount ?? 0;
                        break;


                    // RATIOS, ETC.
                    case "4.1":
                        capitalAdequacy.TotalOnBalanceSheetAssets = row.Amount ?? 0;
                        break;
                    case "4.2":
                        capitalAdequacy.TotalOffBalanceSheetAssets = row.Amount ?? 0;
                        break;

                    //( 4.1 + 4.2)
                    case "4.3":
                        capitalAdequacy.TotalAssets = row.Amount ?? 0;
                        break;

                    case "4.4":
                        capitalAdequacy.TotalDepositsLiabilities = row.Amount ?? 0;
                        break;

                    // 1.1.12/4.3
                    case "4.5":
                        capitalAdequacy.CoreCapitalToAssetsRatio = row.Amount ?? 0;
                        break;

                    case "4.7":
                        capitalAdequacy.CoreCapitalToAssetsRatioExcessDeficiency = row.Amount ?? 0;
                        break;

                    // 1.1.13/4.3
                    case "4.8":
                        capitalAdequacy.InstitutionalCapitalToAssetsRatio = row.Amount ?? 0;
                        break;

                    case "4.9":
                        capitalAdequacy.MinimumInstitutionalToAssetsRatio = row.Amount ?? 0;
                        break;

                    case "4.10":
                        capitalAdequacy.InstitutionalCapitalToAssetsRatioExcessDeficiency = row.Amount ?? 0;
                        break;

                    // 1.1.12/4.4
                    case "4.11":
                        capitalAdequacy.CoreCapitalToDepositsRatio = row.Amount ?? 0;
                        break;

                    case "4.12":
                        capitalAdequacy.MinimumCoreCapitalToDepositsRatio = row.Amount ?? 0;
                        break;

                    case "4.13":
                        capitalAdequacy.CoreCapitalToDepositsRatioExcessDeficiency = row.Amount ?? 0;
                        break;
                }
            }
            await _context.DTCapitalAdequacyReturns.AddAsync(capitalAdequacy);
            await _context.SaveChangesAsync();
        }

        public static async Task ProcessInsiderLendingForm(IFormFile file, string returnId, ILogger _logger, ReturnForm form, Boolean IsAmendment, string PrevId = "")
        {
            try
            {
                ReturnsDbContext _context = new ReturnsDbContext();

                var ImportedLendingReport = ExcelService.ImportInsiderLendingReport(file, _logger);
                if (ImportedLendingReport == null)
                    throw new Exception("No data found in InsiderLendingReport Form");

                // Save the Excel file
                var Path = await FormsHelper.SaveFileAsync(file, "InsiderLendingReport", "");
                if (Path == null)
                {
                    throw new Exception("Error saving file");
                }

                // Calculate days late
                var DaysLateBy = CalculateDaysLate(form, DateTime.Now, ImportedLendingReport.EndDate);

                string EffectiveReturnId = returnId;
                string PreviousReturnId = string.Empty;
                InsiderLendingHeader? insiderLendingHeader = null;
                List<InsiderLoan> loanEntities = new List<InsiderLoan>();

                if (!IsAmendment)
                {
                    insiderLendingHeader = new InsiderLendingHeader
                    {
                        ReturnId = returnId,
                        FilePath = Path,
                        StartDate = ImportedLendingReport.StartDate,
                        EndDate = ImportedLendingReport.EndDate,
                        DaysLateBy = DaysLateBy,
                        IsCurrent = true,
                        IsAmended = false,
                        SaccoName = ImportedLendingReport.SaccoName,
                        CSNO = ImportedLendingReport.SaccoSocietyCsNumber,
                    };
                }
                else
                {
                    insiderLendingHeader = await _context.InsiderLendingHeaders.FirstOrDefaultAsync(x => x.ReturnId == EffectiveReturnId);
                    if (insiderLendingHeader == null)
                    {
                        throw new Exception("Return not found for amendment");
                    }

                    insiderLendingHeader.PreviousReturnId = PrevId;
                    insiderLendingHeader.ReturnId = returnId;
                    insiderLendingHeader.Version = insiderLendingHeader.Version;// + 1;
                    insiderLendingHeader.IsCurrent = true;
                    insiderLendingHeader.IsAmended = false;
                    insiderLendingHeader.StartDate = ImportedLendingReport.StartDate;
                    insiderLendingHeader.EndDate = ImportedLendingReport.EndDate;
                    insiderLendingHeader.FilePath = Path;
                    insiderLendingHeader.DaysLateBy = DaysLateBy;
                    insiderLendingHeader.DaysLateBy = DaysLateBy;
                    insiderLendingHeader.SaccoName = ImportedLendingReport.SaccoName;
                    insiderLendingHeader.CSNO = ImportedLendingReport.SaccoSocietyCsNumber;

                }


                if (!IsAmendment)
                {
                    await _context.InsiderLendingHeaders.AddAsync(insiderLendingHeader);
                }
                else
                {
                    _context.InsiderLendingHeaders.Update(insiderLendingHeader);
                }

                foreach (var loanDTO in ImportedLendingReport.Loans)
                {
                    var loan = new InsiderLoan
                    {
                        InsiderLendingHeaderId = insiderLendingHeader.Id,
                        LoanCategory = loanDTO.LoanCategory,
                        NameOfBorrower = loanDTO.NameOfBorrower,
                        MemberNumber = loanDTO.MemberNumber,
                        PositionHeld = loanDTO.PositionHeld,
                        LoanTypeName = loanDTO.LoanTypeName,
                        AmountAppliedFor = loanDTO.AmountAppliedFor,
                        AmountGranted = loanDTO.AmountGranted,
                        DateApprovedOrRatified = loanDTO.DateApprovedOrRatified,
                        AmountOfBosaDeposits = loanDTO.AmountOfBosaDeposits,
                        NatureOfSecurity = loanDTO.NatureOfSecurity,
                        RepaymentCommencementDate = loanDTO.RepaymentCommencementDate,
                        RepaymentPeriod = loanDTO.RepaymentPeriod,
                        OtherRemarks = loanDTO.OtherRemarks,
                        OutstandingAmount = loanDTO.OutstandingAmount,
                        PerfomanceCategory = loanDTO.PerfomanceCategory,
                        RepaymentStatus = loanDTO.RepaymentStatus,
                        PreviousReturnId = PrevId,
                        IsCurrent = true,
                        IsAmended = false,
                        ReturnId = EffectiveReturnId
                    };

                    loanEntities.Add(loan);
                    await _context.InsiderLoans.AddAsync(loan);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully processed Insider Lending Report with {loanEntities.Count} loans for return ID: {returnId}");

            }
            catch (Exception ex)
            {
                throw ex;
                _logger.LogError(ex, $"Error processing Form 2B for return ID: {returnId}");

            }
        }




        public static async Task ProcessDailyLiquidityForm(IFormFile file, string returnId, ILogger _logger, ReturnForm form, Boolean IsAmendment, string PrevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            // Import the Excel data using the ExcelService
            var liquidityData = ExcelService.ImportDailyLiquidityRows(file, _logger);
            if (liquidityData == null)
                throw new Exception("No data found in Daily Liquidity Form");

            // Save the Excel file
            var Path = await FormsHelper.SaveFileAsync(file, "Daily Liquidity Returns", "");
            if (Path == null)
            {
                throw new Exception("Error saving file");
            }

            // Calculate days late
            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, liquidityData.ReportDate);

            string EffectiveReturnId = returnId;
            string PreviousReturnId = string.Empty;
            DailyLiquidityReturn? dailyLiquidity = null;

            if (!IsAmendment)
            {
                // Create new record
                dailyLiquidity = new DailyLiquidityReturn
                {
                    ReturnId = returnId,
                    FilePath = Path,
                    ReportDate = liquidityData.ReportDate,
                    DaysLateBy = DaysLateBy,
                    IsCurrent = true,
                    IsAmended = false,
                    SACCOName = liquidityData.SACCOName,
                    CSNO = liquidityData.CSNO,
                };
            }
            else
            {
                dailyLiquidity = await _context.DailyLiquidityReturns.FirstOrDefaultAsync(x => x.ReturnId == EffectiveReturnId);
                if (dailyLiquidity == null)
                {
                    throw new Exception("Return not found for amendment");
                }

                // Update amendment information
                dailyLiquidity.PreviousReturnId = PrevId;
                dailyLiquidity.ReturnId = returnId;
                dailyLiquidity.Version = dailyLiquidity.Version + 1;
                dailyLiquidity.IsCurrent = true;
                dailyLiquidity.IsAmended = false;
                dailyLiquidity.ReportDate = liquidityData.ReportDate;
                dailyLiquidity.FilePath = Path;
                dailyLiquidity.DaysLateBy = DaysLateBy;
                dailyLiquidity.DaysLateBy = DaysLateBy;
                dailyLiquidity.SACCOName = liquidityData.SACCOName;
                dailyLiquidity.CSNO = liquidityData.CSNO;
    
            }

            // Opening Balances
            dailyLiquidity.BankBalancesOpening = liquidityData.BankBalancesOpening;
            dailyLiquidity.ConsolidatedTreasuryCashBalancesOpening = liquidityData.ConsolidatedTreasuryCashBalancesOpening;
            dailyLiquidity.TellersBalancesOpening = liquidityData.TellersBalancesOpening;
            dailyLiquidity.MobileMoneyChannelsOpening = liquidityData.MobileMoneyChannelsOpening;
            dailyLiquidity.PlacementWithBanksOpening = liquidityData.PlacementWithBanksOpening;
            dailyLiquidity.SubTotalOpening = liquidityData.SubTotalOpening;

            // Day Receipts
            dailyLiquidity.DepositsFromMembers = liquidityData.DepositsFromMembers;
            dailyLiquidity.CashLoanRepayments = liquidityData.CashLoanRepayments;
            dailyLiquidity.OtherCashReceipts = liquidityData.OtherCashReceipts;
            dailyLiquidity.SubTotalReceipts = liquidityData.SubTotalReceipts;
            dailyLiquidity.TotalOpeningAndReceipts = liquidityData.TotalOpeningAndReceipts;

            // Day Payments
            dailyLiquidity.CashWithdrawalsByMembers = liquidityData.CashWithdrawalsByMembers;
            dailyLiquidity.CashPaymentsToMembers = liquidityData.CashPaymentsToMembers;
            dailyLiquidity.OtherCashPayments = liquidityData.OtherCashPayments;
            dailyLiquidity.SubTotalPayments = liquidityData.SubTotalPayments;

            // Closing Balances
            dailyLiquidity.BankBalancesClosing = liquidityData.BankBalancesClosing;
            dailyLiquidity.ConsolidatedTreasuryCashBalancesClosing = liquidityData.ConsolidatedTreasuryCashBalancesClosing;
            dailyLiquidity.TellersBalancesClosing = liquidityData.TellersBalancesClosing;
            dailyLiquidity.MobileMoneyChannelsClosing = liquidityData.MobileMoneyChannelsClosing;
            dailyLiquidity.PlacementWithBanksClosing = liquidityData.PlacementWithBanksClosing;
            dailyLiquidity.TotalClosingBalance = liquidityData.TotalClosingBalance;

            // Deposit Liabilities
            dailyLiquidity.BOSADeposits = liquidityData.BOSADeposits;
            dailyLiquidity.FOSADeposits = liquidityData.FOSADeposits;
            dailyLiquidity.TotalDeposits = liquidityData.TotalDeposits;

            // Liquidity Ratios
            dailyLiquidity.TotalClosingBalanceToTotalDepositsRatio = liquidityData.TotalClosingBalanceToTotalDepositsRatio;
            dailyLiquidity.TotalClosingBalanceToFOSADepositsRatio = liquidityData.TotalClosingBalanceToFOSADepositsRatio;

            //ValidateDailyLiquidityData(dailyLiquidity, _logger);

            // Save to database
            if (IsAmendment)
            {
                // Mark old record as amended
                var oldRecord = await _context.DailyLiquidityReturns.FirstOrDefaultAsync(x => x.ReturnId == PrevId);
                if (oldRecord != null)
                {
                    oldRecord.IsCurrent = false;
                    oldRecord.IsAmended = true;
                }
            }

            if (!IsAmendment)
            {
                await _context.DailyLiquidityReturns.AddAsync(dailyLiquidity);
            }
            else
            {
                _context.DailyLiquidityReturns.Update(dailyLiquidity);
            }

            await _context.SaveChangesAsync();
        }



        public static async Task<List<string>> GetMissingRequiredFormsAsync(NewReturnDTO createFormDTO, string SaccoType)
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            var forms = await _context.ReturnForms.ToListAsync();
            // List any form flagged as required
            var requiredForms = forms.Where(f =>
                f.IsFinancialPosition ||
                f.IsStatementOfComprehensiveIncome ||
                f.IsCapitalAdequencyForm ||
                f.IsInvestmentReturn ||
                f.IsDepositReturnForm ||
                f.IsRiskClassification ||
                f.IsLiquidityStatement
            ).ToList();

            // filter out those that are not required for this Sacco type
            requiredForms = requiredForms.Where(f => f.SaccoTypeId == createFormDTO.SaccoId).ToList();

            var missingForms = new List<string>();
            foreach (var requiredForm in requiredForms)
            {
                if (!createFormDTO.FormUploads.Any(f => f.FormId == requiredForm.Id && f.formFile != null))
                    missingForms.Add(requiredForm.FormName);
            }
            return missingForms;
        }

        public static async Task ValidateUploadedForms(NewReturnDTO createFormDTO)
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            // Get all required forms for this Sacco type
            var requiredForms = await _context.ReturnForms
                    .Where(f => f.SaccoTypeId == createFormDTO.SaccoId &&
                                (f.IsCapitalAdequencyForm ||
                                 f.IsLiquidityStatement ||
                                 f.IsRiskClassification ||
                                 f.IsInvestmentReturn ||
                                 f.IsFinancialPosition ||
                                 f.IsStatementOfComprehensiveIncome ||
                                 f.IsDepositReturnForm))
                    .ToListAsync();

            // Check for missing forms
            var missingForms = new List<string>();

            foreach (var requiredForm in requiredForms)
            {
                bool isFormUploaded = false;

                // Check if this form exists in uploads
                foreach (var upload in createFormDTO.FormUploads)
                {
                    if (upload.FormId == requiredForm.Id && upload.formFile != null)
                    {
                        isFormUploaded = true;
                        break;
                    }
                }

                if (!isFormUploaded)
                {
                    missingForms.Add(requiredForm.Code);
                }
            }

            if (missingForms.Count > 0)
            {
                string missingFormsList = string.Join(", ", missingForms);
                throw new Exception(
                    $"Missing required forms: {missingFormsList}. " +
                    "All required forms must be submitted."
                );
            }
        }

        public static async Task NWDTValidateUploadedForms(NWDTNewReturnDTO createFormNWDTDTO)
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            // Get all required forms for this Sacco type
            var requiredForms = await _context.ReturnForms
                    .Where(f => f.SaccoTypeId == createFormNWDTDTO.SaccoId &&
                                (f.IsCapitalAdequencyForm ||
                                 f.IsLiquidityStatement ||
                                 f.IsRiskClassification ||
                                 f.IsInvestmentReturn ||
                                 f.IsFinancialPosition ||
                                 f.IsStatementOfComprehensiveIncome ||
                                 f.IsDepositReturnForm))
                    .ToListAsync();

            // Check for missing forms
            var missingForms = new List<string>();

            foreach (var requiredForm in requiredForms)
            {
                bool isFormUploaded = false;

                // Check if this form exists in uploads
                foreach (var upload in createFormNWDTDTO.FormUploads)
                {
                    if (upload.FormId == requiredForm.Id && upload.formFile != null)
                    {
                        isFormUploaded = true;
                        break;
                    }
                }

                if (!isFormUploaded)
                {
                    missingForms.Add(requiredForm.Code);
                }
            }

            if (missingForms.Count > 0)
            {
                string missingFormsList = string.Join(", ", missingForms);
                throw new Exception(
                    $"Missing required forms: {missingFormsList}. " +
                    "All required forms must be submitted."
                );
            }
        }
        public static async Task ProcessForm2B(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form, Boolean IsAmendMent, string PrevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            try
            {
                _logger.LogInformation($"Processing Form 2B for return ID: {returnId}");

                // Import data using your Form2BStatement importer
                var form2BData = ExcelService.ImportForm2BStatement(formFile, _logger);
                if (form2BData == null || form2BData.Rows == null || !form2BData.Rows.Any())
                    throw new Exception("No data found in Form 2B");

                var Path = await FormsHelper.SaveFileAsync(formFile, "Form 2B Returns");
                if (Path == null)
                {
                    throw new Exception("Error saving file");
                }
                NWDTLiquidityReturn? liquidityStatement;
                // Calculate days late
                var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form2BData.EndDate);
                // Create new Liquidity return object
                if (IsAmendMent)
                {
                    liquidityStatement = _context.NDWTLiquidityReturns.FirstOrDefault(X => X.ReturnId == returnId);
                    if (liquidityStatement == null)
                    {
                        throw new Exception(
                            "Wah");
                    }
                    liquidityStatement.PreviousReturnId = PrevId;
                }
                else
                {
                    liquidityStatement = new NWDTLiquidityReturn
                    {
                        ReturnId = returnId,
                        Period = form2BData.Period,
                        Frequency = form.Period.Name,
                        DaysLateBy = DaysLateBy
                    };
                    liquidityStatement.StartDate = form2BData.StartDate;
                    liquidityStatement.EndDate = form2BData.EndDate;
                }

                if (liquidityStatement is null)
                {
                    throw new Exception("Prev Return not Found");
                }


                // Extract values from Form1Statement based on index
                foreach (var row in form2BData.Rows)
                {
                    switch (row.Index?.Trim())
                    {
                        case "1.1":
                            liquidityStatement.LocalNotesAndCoins = row.Amount ?? 0;
                            break;
                        case "1.2":
                            liquidityStatement.ForeignNotesAndCoins = row.Amount ?? 0;
                            break;
                        case "2.1":
                            liquidityStatement.BalancesWithCommercialBanks = row.Amount ?? 0;
                            break;
                        case "2.2":
                            liquidityStatement.TimeDepositsWithBanksMoreThan90Days = row.Amount ?? 0;
                            break;
                        case "2.3":
                            liquidityStatement.OverdraftsAndMaturedLoans = row.Amount ?? 0;
                            break;
                        case "3.1":
                            liquidityStatement.BalancesWithOtherSaccoSocieties = row.Amount ?? 0;
                            break;
                        case "3.2":
                            liquidityStatement.BalancesWithOtherFinancialInstitutions = row.Amount ?? 0;
                            break;
                        case "3.3":
                            liquidityStatement.BalancesDueToOtherSaccoSocieties = row.Amount ?? 0;
                            break;
                        case "3.4":
                            liquidityStatement.BalancesDueToFinancialInstitutions = row.Amount ?? 0;
                            break;
                        case "3.5":
                            liquidityStatement.MaturedLoansAndAdvances = row.Amount ?? 0;
                            break;
                        case "4.1":
                            liquidityStatement.TreasuryBills = row.Amount ?? 0;
                            break;
                        case "4.2":
                            liquidityStatement.TreasuryBondsBearerBonds = row.Amount ?? 0;
                            break;
                        case "6.1":
                            liquidityStatement.MaturedLiabilities = row.Amount ?? 0;
                            break;
                        case "6.2":
                            liquidityStatement.LiabilitiesMaturing91Days = row.Amount ?? 0;
                            break;

                    }
                }

                liquidityStatement.CalculateAndStoreTotals();

                if (!IsAmendMent)
                {
                    await _context.NDWTLiquidityReturns.AddAsync(liquidityStatement);
                }
                else
                {
                    _context.NDWTLiquidityReturns.Update(liquidityStatement);
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully processed Form 2B for return ID: {returnId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing Form 2B for return ID: {returnId}");
                throw;
            }
        }

        // Helper method to extract value after colon in strings like "Name of Sacco Society: ABCD"
        private static string GetStringValueAfterColon(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains(":"))
                return string.Empty;

            return text.Substring(text.IndexOf(":") + 1).Trim();
        }


        public static async Task ProcessLiquidityForm(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form)
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            var form2 = ExcelService.ImportLiquidityStatementRows(formFile, _logger);
            if (form2.Rows == null || !form2.Rows.Any())
                throw new Exception("No data found in Liquidity Statement Form");

            var Path = await FormsHelper.SaveFileAsync(formFile, "Liquidity Statement Returns");
            if (Path == null)
            {
                throw new Exception("Error saving file");
            }

            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form2.EndDate);

            var liquidityStatement = new DTLiquidityReturn
            {
                ReturnId = returnId,
                Year = form2.Period,
                StartDate = form2.StartDate,
                EndDate = form2.EndDate,
                Frequency = form.Period.Name,
                FilePath = Path,
                DaysLateBy = DaysLateBy
            };
            foreach (var liquidityRow in form2.Rows)
            {
                switch (liquidityRow.Index?.Trim())
                {
                    case "1.1":
                        liquidityStatement.LocalNotesAndCoins = liquidityRow.Amount ?? 0;
                        break;
                    case "1.2":
                        liquidityStatement.ForeignNotesAndCoins = liquidityRow.Amount ?? 0;
                        break;
                    case "2.1":
                        liquidityStatement.BalancesWithCommercialBanks = liquidityRow.Amount ?? 0;
                        break;
                    case "2.2":
                        liquidityStatement.TimeDepositsWithBanksMoreThan90Days = liquidityRow.Amount ?? 0;
                        break;
                    case "2.3":
                        liquidityStatement.OverdraftsAndMaturedLoans = liquidityRow.Amount ?? 0;
                        break;
                    case "3.1":
                        liquidityStatement.BalancesWithOtherSaccoSocieties = liquidityRow.Amount ?? 0;
                        break;
                    case "3.2":
                        liquidityStatement.BalancesWithOtherFinancialInstitutions = liquidityRow.Amount ?? 0;
                        break;
                    case "3.3":
                        liquidityStatement.BalancesDueToOtherSaccoSocieties = liquidityRow.Amount ?? 0;
                        break;
                    case "3.4":
                        liquidityStatement.BalancesDueToFinancialInstitutions = liquidityRow.Amount ?? 0;
                        break;
                    case "4.1":
                        liquidityStatement.TreasuryBills = liquidityRow.Amount ?? 0;
                        break;
                    case "4.2":
                        liquidityStatement.TreasuryBonds = liquidityRow.Amount ?? 0;
                        break;
                    case "6.1":
                        liquidityStatement.DepositsFromMembers = liquidityRow.Amount ?? 0;
                        break;
                    case "6.2":
                        liquidityStatement.DepositsFromOtherSources = liquidityRow.Amount ?? 0;
                        break;
                    case "7.1":
                        liquidityStatement.MaturedLiabilities = liquidityRow.Amount ?? 0;
                        break;
                    case "7.2":
                        liquidityStatement.LiabilitiesMaturing91Days = liquidityRow.Amount ?? 0;
                        break;
                }

            }

            // Calculate totals and ratios
            liquidityStatement.TotalNotesAndCoins = liquidityStatement.LocalNotesAndCoins + liquidityStatement.ForeignNotesAndCoins;
            liquidityStatement.TotalGovernmentSecurities = liquidityStatement.TreasuryBills + liquidityStatement.TreasuryBonds;
            liquidityStatement.NetLiquidAssets = liquidityStatement.TotalNotesAndCoins + liquidityStatement.NetBankBalances +
                                      liquidityStatement.NetFinancialInstitutionBalances + liquidityStatement.TotalGovernmentSecurities;

            liquidityStatement.TotalDeposits = liquidityStatement.DepositsFromMembers + liquidityStatement.DepositsFromOtherSources;
            liquidityStatement.TotalOtherLiabilities = liquidityStatement.MaturedLiabilities + liquidityStatement.LiabilitiesMaturing91Days;
            if (liquidityStatement.TotalDeposits > 0)
            {
                liquidityStatement.LiquidityRatio = (liquidityStatement.NetLiquidAssets / liquidityStatement.TotalDeposits) * 100;
                liquidityStatement.LiquidityRatioExcessDeficit = liquidityStatement.LiquidityRatio - liquidityStatement.MinimumLiquidityRequirement;
            }

            await _context.DTLiquidityReturns.AddAsync(liquidityStatement);
            await _context.SaveChangesAsync();
        }

        public static async Task ProcessDepositReturnForm(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form)
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            var form3 = ExcelService.ImportDepositRangeDataRows(formFile, _logger);
            var rows = form3.Rows;
            if (rows == null || !rows.Any())
                throw new Exception("No data found in Deposit Return Form");

            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form3.EndDate);
            var FilePath = await FormsHelper.SaveFileAsync(formFile, "Deposit Return Forms");

            foreach (var row in rows)
            {
                var depositReturn = new DepositReturn
                {
                    ReturnId = returnId,
                    RangeName = row.RangeName,
                    DepositType = row.DepositType,
                    NumberOfAccounts = row.NumberOfAccounts,
                    AmountInKshs000 = row.AmountInKshs000,
                    Year = form3.Period,
                    StartDate = form3.StartDate,
                    EndDate = form3.EndDate,
                    Frequency = form.Period.Name,
                    FilePath = FilePath,
                    DaysLateBy = DaysLateBy
                };
                await _context.DepositReturns.AddAsync(depositReturn);
            }
            await _context.SaveChangesAsync();
        }


        public static async Task ProcessForm2C(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form, Boolean IsAmendMent, string PrevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            var form2CData = ExcelService.ImportForm2CDataRows(formFile, _logger);
            if (form2CData.Rows == null || !form2CData.Rows.Any())
            {
                throw new Exception("No data found in Deposit Return Form");
            }

            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form2CData.EndDate);
            var FilePath = await FormsHelper.SaveFileAsync(formFile, "Deposit Return Forms");
            NWDTDepositReturn? depositReturn; ;

            foreach (var row in form2CData.Rows)
            {
                if (IsAmendMent)
                {
                    depositReturn = _context.NWDTDepositReturns.FirstOrDefault(x => x.ReturnId == returnId);
                    if (depositReturn is null)
                    {
                        continue;
                    }
                    depositReturn.PreviousReturnId = PrevId;
                }
                else
                {
                    depositReturn = new NWDTDepositReturn
                    {
                        StartDate = form2CData.StartDate,
                        EndDate = form2CData.EndDate,
                        Period = form2CData.Period,
                        Frequency = form.Period.Name,
                        DaysLateBy = DaysLateBy,
                        FilePath = FilePath
                    };
                    depositReturn.ReturnId = returnId;
                    depositReturn.AmountInKshs000 = row.Amount;
                    depositReturn.RangeName = row.Range;
                    depositReturn.DepositType = row.DepositType;
                    depositReturn.NumberOfAccounts = row.NumberOfAccounts;
                }

                if (depositReturn is null)
                {
                    continue;
                }
                if (!IsAmendMent)
                {
                    await _context.NWDTDepositReturns.AddAsync(depositReturn);
                }
                else
                {
                    _context.NWDTDepositReturns.Update(depositReturn);
                }
            }

            await _context.SaveChangesAsync();
        }

        public static async Task ProcessForm2D(IFormFile formFile, string returnId, ILogger logger, ReturnForm form, bool isAmendment, string prevId = "")
        {
            try
            {
                using var context = new ReturnsDbContext();

                // Import and validate data
                var form2DData = ExcelService.ImportForm2DRows(formFile, logger);
                if (form2DData.Rows == null || !form2DData.Rows.Any())
                {
                    throw new ArgumentException("No data found in Deposit Return Form");
                }

                // Calculate days late and save file
                var daysLateBy = CalculateDaysLate(form, DateTime.Now, form2DData.EndDate);
                var filePath = await FormsHelper.SaveFileAsync(formFile, "Risk Classification Returns");
                if (filePath == null)
                {
                    throw new IOException("Error saving file");
                }

                // Process rows
                var entitiesToAdd = new List<NWDTRiskClassificationReturn>();
                var entitiesToUpdate = new List<NWDTRiskClassificationReturn>();

                foreach (var row in form2DData.Rows)
                {
                    if (isAmendment)
                    {
                        var riskClassification = await context.NWDTRiskClassificationReturns
                            .FirstOrDefaultAsync(x => x.Id == returnId);

                        if (riskClassification != null)
                        {
                            riskClassification.PreviousReturnId = prevId;
                            entitiesToUpdate.Add(riskClassification);
                        }
                    }
                    else
                    {
                        var riskClassification = new NWDTRiskClassificationReturn
                        {
                            LoanType = row.LoanType,
                            Classification = row.Classification,
                            NumberOfAccounts = row.NumberOfAccounts,
                            OutstandingLoanPortfolio = row.OutstandingLoanPortfolio,
                            RequiredProvision = row.RequiredProvision,
                            RequiredProvisionAmount = row.RequiredProvisionAmount,
                            ReturnId = returnId,
                            Period = form2DData.Period,
                            Frequency = form.Period.Name,
                            StartDate = form2DData.StartDate,
                            EndDate = form2DData.EndDate,
                            FilePath = filePath,
                            DaysLateBy = daysLateBy,
                            PreviousReturnId = prevId
                        };
                        entitiesToAdd.Add(riskClassification);
                    }
                }

                if (entitiesToAdd.Any())
                {
                    await context.NWDTRiskClassificationReturns.AddRangeAsync(entitiesToAdd);
                }

                if (entitiesToUpdate.Any())
                {
                    context.NWDTRiskClassificationReturns.UpdateRange(entitiesToUpdate);
                }

                await context.SaveChangesAsync();

                logger.LogInformation($"Successfully processed Form 2D with {entitiesToAdd.Count + entitiesToUpdate.Count} entries for ReturnId: {returnId}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing Form 2D for ReturnId: {returnId}");
                throw;
            }
        }

        public static async Task ProcessRiskClassificationForm(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form)
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            var form4 = ExcelService.ImportRiskClassificationRows(formFile, _logger);
            var rows = form4.Rows;
            if (rows == null || !rows.Any())
                throw new Exception("No data found in Risk Classification Form");

            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form4.EndDate);
            var FilePath = await FormsHelper.SaveFileAsync(formFile, "Risk Classification Returns");

            foreach (var row in rows)
            {
                var riskClassification = new DTRiskClassificationReturn
                {
                    LoanType = row.LoanType,
                    Classification = row.Classification,
                    NumberOfAccounts = row.NumberOfAccounts,
                    OutstandingLoanPortfolio = row.OutstandingLoanPortfolio,
                    RequiredProvision = row.RequiredProvision,
                    RequiredProvisionAmount = row.RequiredProvisionAmount,
                    ReturnId = returnId,
                    Year = form4.Period,
                    StartDate = form4.StartDate,
                    EndDate = form4.EndDate,
                    Frequency = form.Period.Name,
                    FilePath = FilePath,
                    DaysLateBy = DaysLateBy
                };
                await _context.DTRiskClassificationReturns.AddAsync(riskClassification);
            }
            await _context.SaveChangesAsync();
        }

        internal static async Task ProcessInvestmentReturnForm(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form)
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            var form5 = ExcelService.ImportInvestmentRows(formFile, _logger);
            var rows = form5.Rows;
            if (rows == null || !rows.Any())
            {
                throw new Exception("No data found in Investment Return Form");
            }

            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form5.EndDate);
            var FilePath = await FormsHelper.SaveFileAsync(formFile, "Investment Returns");

            var investmentReturn = new DTInvestmentReturn
            {
                ReturnId = returnId,
                Year = form5.Period,
                StartDate = form5.StartDate,
                EndDate = form5.EndDate,
                Frequency = form.Period.Name,
                FilePath = FilePath,
                DaysLateBy = DaysLateBy
            };
            foreach (var row in rows)
            {
                switch (row.Index?.Trim())
                {
                    case "1.1":
                        investmentReturn.CoreCapital = row.Amount ?? 0;
                        break;
                    case "1.2":
                        investmentReturn.TotalAssets = row.Amount ?? 0;
                        break;
                    case "1.3":
                        investmentReturn.TotalDeposits = row.Amount ?? 0;
                        break;
                    case "1.4":
                        investmentReturn.NonEarningAssets = row.Amount ?? 0;
                        break;
                    case "1.5":
                        investmentReturn.FinancialInvestments = row.Amount ?? 0;
                        break;
                    case "1.6":
                        investmentReturn.LandAndBuildings = row.Amount ?? 0;
                        break;
                }
            }

            if (investmentReturn.TotalAssets > 0)
            {
                investmentReturn.LandBuildingsToTotalAssetsRatio = (investmentReturn.LandAndBuildings / investmentReturn.TotalAssets) * 100;
                investmentReturn.LandBuildingsRatioExcessDeficiency = investmentReturn.LandBuildingsToTotalAssetsRatio - investmentReturn.MaxLandBuildingsToTotalAssetsRatio;

                investmentReturn.NonEarningAssetsToTotalAssetsRatio = (investmentReturn.NonEarningAssets / investmentReturn.TotalAssets) * 100;
                investmentReturn.NonEarningAssetsRatioExcessDeficiency = investmentReturn.NonEarningAssetsToTotalAssetsRatio - investmentReturn.MaxNonEarningAssetsToTotalAssetsRatio;
            }

            if (investmentReturn.CoreCapital > 0)
            {
                investmentReturn.FinancialInvestmentsToCoreCapitalRatio = (investmentReturn.FinancialInvestments / investmentReturn.CoreCapital) * 100;
                investmentReturn.FinancialInvestmentsToCoreCapitalExcessDeficiency = investmentReturn.FinancialInvestmentsToCoreCapitalRatio - investmentReturn.MaxFinancialInvestmentsToCoreCapitalRatio;
            }

            if (investmentReturn.TotalDeposits > 0)
            {
                investmentReturn.FinancialInvestmentsToDepositsRatio = (investmentReturn.FinancialInvestments / investmentReturn.TotalDeposits) * 100;
                investmentReturn.FinancialInvestmentsToDepositsExcessDeficiency = investmentReturn.FinancialInvestmentsToDepositsRatio - investmentReturn.MaxFinancialInvestmentsToDepositsRatio;
            }

            await _context.DTInvestmentReturns.AddAsync(investmentReturn);
            await _context.SaveChangesAsync();
        }


        internal static async Task ProcessForm2E(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form, bool isAmendment, string prevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            var Form2E = ExcelService.ImportForm2ERows(formFile, _logger);
            if (Form2E.Rows == null || !Form2E.Rows.Any())
            {
                throw new Exception("No data found in Investment Return Form");
            }
            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, Form2E.EndDate);
            var FilePath = await FormsHelper.SaveFileAsync(formFile, "Investment Returns");
            if (FilePath == null)
            {
                throw new Exception("Error saving file");
            }

            NWDTInvestmentReturn? investmentReturn;

            if (isAmendment)
            {
                investmentReturn = await _context.NWDTInvestmentReturns.FirstOrDefaultAsync(x => x.ReturnId == returnId);

                if (investmentReturn == null)
                {
                    _logger.LogWarning($"Amendment requested but no existing record found for ReturnId: {prevId}");
                    investmentReturn = new NWDTInvestmentReturn();
                }
                investmentReturn.PreviousReturnId = prevId;
            }
            else
            {
                investmentReturn = new NWDTInvestmentReturn
                {
                    StartDate = Form2E.StartDate,
                    EndDate = Form2E.EndDate,
                    Period = Form2E.Period,
                    Frequency = form.Period.Name,
                    FilePath = FilePath,
                    DaysLateBy = DaysLateBy,
                    ReturnId = returnId
                };
            }


            foreach (var row in Form2E.Rows)
            {
                switch (row.Index)
                {
                    case "1.1": // Core Capital
                        investmentReturn.CoreCapital = row.Amount ?? 0;
                        break;
                    case "1.2": // Total Assets
                        investmentReturn.TotalAssets = row.Amount ?? 0;
                        break;
                    case "1.3": // Total Deposits
                        investmentReturn.TotalDeposits = row.Amount ?? 0;
                        break;
                    case "1.4": // Non-earning Assets
                        investmentReturn.NonEarningAssets = row.Amount ?? 0;
                        break;
                    case "1.5.1": // Subsidiary and Related Entity investments
                        investmentReturn.SubsidiaryRelatedEntityInvestments = row.Amount ?? 0;
                        break;
                    case "1.5.2": // Equity investment
                        investmentReturn.EquityInvestments = row.Amount ?? 0;
                        break;
                    case "1.5.3": // Other investments
                        investmentReturn.OtherInvestments = row.Amount ?? 0;
                        break;
                    case "1.6": // Other assets - Land & Building, equipment
                        investmentReturn.OtherAssetsLandBuildingEquipment = row.Amount ?? 0;
                        break;
                    case "1.7": // Land & Building
                        investmentReturn.LandAndBuilding = row.Amount ?? 0;
                        break;
                    case "1.9": // Maximum Land & Building and equipment to Total Asset requirement
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            // If it's a percentage (e.g., 10%), convert to decimal value (0.10)
                            if (percentage > 1)
                                percentage /= 100;
                            investmentReturn.MaxLandBuildingEquipmentToTotalAssetRequirement = percentage;
                        }
                        break;
                    case "2.2": // Maximum Land & Building to Total Asset requirement
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1)
                                percentage /= 100;
                            investmentReturn.MaxLandBuildingToTotalAssetRequirement = percentage;
                        }
                        break;
                    case "2.5": // Maximum financial investments to Core capital
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1)
                                percentage /= 100;
                            investmentReturn.MaxFinancialInvestmentsToCoreCapital = percentage;
                        }
                        break;
                    case "2.8": // Maximum financial investments to Total Deposits liabilities Ratio
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1)
                                percentage /= 100;
                            investmentReturn.MaxEquityInvestmentsToTotalDeposits = percentage;
                        }
                        break;
                    case "3.1": // Maximum Subsidiary investment to Total assets Ratio
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1)
                                percentage /= 100;
                            investmentReturn.MaxSubsidiaryInvestmentToTotalAssets = percentage;
                        }
                        break;
                    case "3.4": // Maximum Other investments to Core Capital
                        if (row.Amount.HasValue)
                        {
                            var percentage = row.Amount.Value;
                            if (percentage > 1)
                                percentage /= 100;
                            investmentReturn.MaxOtherInvestmentsToCoreCapital = percentage;
                        }
                        break;
                }
            }
            investmentReturn.CalculateAndStoreTotals();

            if (isAmendment && investmentReturn.Id != string.Empty)
            {
                _context.NWDTInvestmentReturns.Update(investmentReturn);
            }
            else
            {
                await _context.NWDTInvestmentReturns.AddAsync(investmentReturn);
            }

            await _context.SaveChangesAsync();
        }

        public static async Task ProcessFinancialPositionForm(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form)
        {
            ReturnsDbContext _context = new ReturnsDbContext();

            var form6 = ExcelService.ImportFinancialPositionRows(formFile, _logger);
            var rows = form6.Rows;
            if (rows == null || !rows.Any())
            {
                throw new Exception("No data found in Statement of Financial Position");
            }
            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form6.EndDate);
            var FilePath = await FormsHelper.SaveFileAsync(formFile, "Statement of Financial Position Returns");
            var statement = new DTFinancialPositionReturn
            {
                ReturnId = returnId,
                Year = form6.Period,
                StartDate = form6.StartDate,
                EndDate = form6.EndDate,
                Frequency = form.Period.Name,
                FilePath = FilePath,
                DaysLateBy = DaysLateBy
            };
            foreach (var row in rows)
            {
                switch (row.RefNumber?.Trim())
                {
                    // Cash & Cash Equivalent
                    case "1.1":
                        statement.CashInHand = row.Amount ?? 0;
                        break;
                    case "1.2":
                        statement.CashAtBank = row.Amount ?? 0;
                        break;

                    // Prepayments & Sundry Receivables
                    case "2":
                        statement.PrepaymentsAndSundryReceivables = row.Amount ?? 0;
                        break;

                    // Financial Investments
                    case "3.1":
                        statement.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "3.2":
                        statement.OtherSecurities = row.Amount ?? 0;
                        break;
                    case "3.3a":
                        statement.BalancesWithOtherSaccos = row.Amount ?? 0;
                        break;
                    case "3.3b":
                        statement.InvestmentsInCompanies = row.Amount ?? 0;
                        break;

                    // Net Loan Portfolio
                    case "4.1":
                        statement.GrossLoanPortfolio = row.Amount ?? 0;
                        break;
                    case "4.2":
                        statement.AllowanceForLoanLoss = row.Amount ?? 0;
                        break;

                    // Accounts Receivables
                    case "5.1":
                        statement.TaxRecoverable = row.Amount ?? 0;
                        break;
                    case "5.2":
                        statement.DeferredTaxAssets = row.Amount ?? 0;
                        break;
                    case "5.3":
                        statement.RetirementBenefitAssets = row.Amount ?? 0;
                        break;

                    // Property & Equipment & Other assets
                    case "6.1":
                        statement.InvestmentProperties = row.Amount ?? 0;
                        break;
                    case "6.2":
                        statement.PropertyAndEquipment = row.Amount ?? 0;
                        break;
                    case "6.3":
                        statement.PrepaidLeaseRentals = row.Amount ?? 0;
                        break;
                    case "6.4":
                        statement.IntangibleAssets = row.Amount ?? 0;
                        break;
                    case "6.5":
                        statement.OtherAssets = row.Amount ?? 0;
                        break;

                    // LIABILITIES
                    case "7":
                        statement.SavingsDeposits = row.Amount ?? 0;
                        break;
                    case "8":
                        statement.ShortTermDeposits = row.Amount ?? 0;
                        break;
                    case "9":
                        statement.NonWithdrawableDeposits = row.Amount ?? 0;
                        break;

                    // Accounts Payable & Other Liabilities
                    case "10.1":
                        statement.TaxPayable = row.Amount ?? 0;
                        break;
                    case "10.2":
                        statement.DividendsPayable = row.Amount ?? 0;
                        break;
                    case "10.3":
                        statement.DeferredTaxLiability = row.Amount ?? 0;
                        break;
                    case "10.4":
                        statement.RetirementBenefitsLiability = row.Amount ?? 0;
                        break;
                    case "10.5":
                        statement.OtherLiabilities = row.Amount ?? 0;
                        break;
                    case "10.6":
                        statement.ExternalBorrowings = row.Amount ?? 0;
                        break;

                    // EQUITY
                    case "11":
                        statement.ShareCapital = row.Amount ?? 0;
                        break;
                    case "12":
                        statement.CapitalGrants = row.Amount ?? 0;
                        break;

                    // Retained Earnings
                    case "13.1":
                        statement.PriorYearsRetainedEarnings = row.Amount ?? 0;
                        break;
                    case "13.2":
                        statement.CurrentYearSurplus = row.Amount ?? 0;
                        break;

                    // Other Equity Accounts
                    case "14":
                        // This is a section total, actual values come from sub-items
                        statement.StatutoryReserve = row.Amount ?? 0;
                        break;
                }
            }
            await _context.DTFinancialPositionReturns.AddAsync(statement);
            await _context.SaveChangesAsync();
        }

        public static async Task ProcessForm2A(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form, Boolean IsAmendMent, string PrevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            try
            {
                _logger.LogInformation($"Processing Form 2A for return ID: {returnId}");

                // Import data using the Form2AStatement
                var form2A = ExcelService.ImportForm2ARows(formFile, _logger);
                if (form2A == null || form2A.Rows == null || !form2A.Rows.Any())
                {
                    throw new Exception("No data found in Capital Adequacy Form");
                }
                var Path = await FormsHelper.SaveFileAsync(formFile, "NWDT Capital Adequacy Returns");
                if (Path == null)
                {
                    throw new Exception("Error saving file");
                }

                var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form2A.EndDate);
                string EffectiveReturnId = returnId;
                string PreviousReturnId = PrevId;
                NWDTCapitalAdequacyReturn? capitalAdequacy = null;

                if (!IsAmendMent)
                {
                    capitalAdequacy = new NWDTCapitalAdequacyReturn
                    {
                        ReturnId = returnId,
                        StartDate = form2A.StartDate,
                        EndDate = form2A.EndDate,
                        FilePath = Path,
                        Period = form2A.Period,
                        Frequency = form.Period.Name,
                        DaysLateBy = DaysLateBy,
                        IsAmended = false,
                        IsCurrent = true
                    };

                }
                else
                {
                    capitalAdequacy = await _context.NWDTCapitalAdequacyReturns.FirstOrDefaultAsync(x => x.ReturnId == EffectiveReturnId);
                    if (capitalAdequacy == null)
                    {
                        throw new Exception(
                            "Not Fond");
                    }
                    capitalAdequacy.PreviousReturnId = PreviousReturnId;
                    capitalAdequacy.IsAmended = false;
                    capitalAdequacy.IsCurrent = true;
                }


                // Create new Capital Adequacy return object

                // Map data from Form1Statement to entity properties based on index values
                foreach (var row in form2A.Rows)
                {
                    switch (row.Index)
                    {
                        // Core Capital Components
                        case "1.1.1":
                            capitalAdequacy.ShareCapital = row.Amount ?? 0;
                            break;
                        case "1.1.2":
                            capitalAdequacy.CapitalGrants = row.Amount ?? 0;
                            break;
                        case "1.1.3":
                            capitalAdequacy.RetainedEarnings = row.Amount ?? 0;
                            break;
                        case "1.1.4":
                            capitalAdequacy.NetSurplusAfterTax = row.Amount ?? 0;
                            break;
                        case "1.1.5":
                            capitalAdequacy.StatutoryReserves = row.Amount ?? 0;
                            break;
                        case "1.1.6":
                            capitalAdequacy.OtherReserves = row.Amount ?? 0;
                            break;

                        // Deductions
                        case "1.1.8":
                            capitalAdequacy.InvestmentsInSubsidiary = row.Amount ?? 0;
                            break;
                        case "1.1.9":
                            capitalAdequacy.OtherDeductions = row.Amount ?? 0;
                            break;

                        // On-Balance Sheet Assets
                        case "2.1":
                            capitalAdequacy.CashLocalForeign = row.Amount ?? 0;
                            break;
                        case "2.2":
                            capitalAdequacy.GovernmentSecurities = row.Amount ?? 0;
                            break;
                        case "2.3":
                            capitalAdequacy.DepositsBalancesAtOtherInstitutions = row.Amount ?? 0;
                            break;
                        case "2.4":
                            capitalAdequacy.LoansAndAdvances = row.Amount ?? 0;
                            break;
                        case "2.5":
                            capitalAdequacy.Investments = row.Amount ?? 0;
                            break;
                        case "2.6":
                            capitalAdequacy.PropertyAndEquipment = row.Amount ?? 0;
                            break;
                        case "2.7":
                            capitalAdequacy.OtherAssets = row.Amount ?? 0;
                            break;
                        case "2.9":
                            capitalAdequacy.TotalAssetsPerBalanceSheet = row.Amount ?? 0;
                            break;

                        // Off-Balance Sheet Assets
                        case "3":
                            capitalAdequacy.OffBalanceSheetAssets = row.Amount ?? 0;
                            break;

                        // Capital Ratio Calculations
                        case "4.4":
                            capitalAdequacy.TotalDepositsLiabilities = row.Amount ?? 0;
                            break;

                        // Minimum Requirements
                        case "4.6":
                            string minCapitalToAssetsStr = row.Description;
                            if (!string.IsNullOrEmpty(minCapitalToAssetsStr) && row.Amount.HasValue)
                            {
                                // Extract percentage value (handle "8%" format)
                                capitalAdequacy.MinimumCoreCapitalToAssetsRatio = row.Amount.Value;
                            }
                            break;
                        case "4.9":
                            string minRetainedEarningsStr = row.Description;
                            if (!string.IsNullOrEmpty(minRetainedEarningsStr) && row.Amount.HasValue)
                            {
                                // Extract percentage value (handle "50%" format)
                                capitalAdequacy.MinimumRetainedEarningsToCoreCaptialRequirement = row.Amount.Value;
                            }
                            break;
                        case "4.12":
                            string minCapitalToDepositsStr = row.Description;
                            if (!string.IsNullOrEmpty(minCapitalToDepositsStr) && row.Amount.HasValue)
                            {
                                // Extract percentage value (handle "5%" format)
                                capitalAdequacy.MinimumCoreCapitalToDepositsRequirement = row.Amount.Value;
                            }
                            break;
                    }
                }

                // Calculate and store all computed totals
                capitalAdequacy.CalculateAndStoreTotals();


                if (!IsAmendMent)
                {
                    await _context.NWDTCapitalAdequacyReturns.AddAsync(capitalAdequacy);
                }
                else
                {
                    _context.NWDTCapitalAdequacyReturns.Update(capitalAdequacy);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully processed Form 2A for return ID: {returnId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing Form 2A for return ID: {returnId}");
                throw;
            }
        }

        public static async Task ProcessForm2F(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form, bool isAmendment, string prevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            try
            {
                _logger.LogInformation($"Processing Form 2F for return ID: {returnId}");

                // Import data using the Form2FStatement
                var form2F = ExcelService.ImportForm2FRows(formFile, _logger);
                if (form2F == null || form2F.Rows == null || !form2F.Rows.Any())
                {
                    throw new Exception("No data found in Statement of Comprehensive Income Form");
                }

                // Save file to disk
                var Path = await FormsHelper.SaveFileAsync(formFile, "NWDT Comprehensive Income Returns");
                if (Path == null)
                {
                    throw new Exception("Error saving file");
                }
                var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form2F.EndDate);

                NWDTComprehensiveIncomeReturn? comprehensiveIncome;

                if (isAmendment)
                {
                    comprehensiveIncome = await _context.NWDTComprehensiveIncomeReturns.FirstOrDefaultAsync(x => x.ReturnId == returnId);

                    if (comprehensiveIncome == null)
                    {
                        _logger.LogWarning($"Amendment requested but no existing record found for ReturnId: {prevId}");
                        comprehensiveIncome = new NWDTComprehensiveIncomeReturn();
                    }

                    comprehensiveIncome.PreviousReturnId = prevId;
                }
                else
                {
                    comprehensiveIncome = new NWDTComprehensiveIncomeReturn
                    {
                        ReturnId = returnId,
                        StartDate = form2F.StartDate,
                        EndDate = form2F.EndDate,
                        Period = form2F.Period,
                        Frequency = form.Period.Name,
                        FilePath = Path,
                        DaysLateBy = DaysLateBy
                    };
                }


                // Map data from Form1Statement to entity properties based on reference numbers
                foreach (var row in form2F.Rows)
                {
                    switch (row.RefNumber)
                    {
                        // Financial Income from Loans Portfolio
                        case "2.1":
                            comprehensiveIncome.InterestOnLoanPortfolio = row.Amount ?? 0;
                            break;
                        case "2.2":
                            comprehensiveIncome.FeesCommissionOnLoanPortfolio = row.Amount ?? 0;
                            break;

                        // Financial Income from Investments
                        case "3.1":
                            comprehensiveIncome.GovernmentSecuritiesIncome = row.Amount ?? 0;
                            break;
                        case "3.2":
                            comprehensiveIncome.PlacementInBanksIncome = row.Amount ?? 0;
                            break;
                        case "3.3":
                            comprehensiveIncome.CommercialPapersIncome = row.Amount ?? 0;
                            break;
                        case "3.4":
                            comprehensiveIncome.CollectiveInvestmentSchemesIncome = row.Amount ?? 0;
                            break;
                        case "3.5":
                            comprehensiveIncome.DerivativesIncome = row.Amount ?? 0;
                            break;
                        case "3.6":
                            comprehensiveIncome.EquityInvestmentsIncome = row.Amount ?? 0;
                            break;
                        case "3.7":
                            comprehensiveIncome.InvestmentInCompaniesIncome = row.Amount ?? 0;
                            break;

                        // Financial Expense
                        case "4.2":
                            comprehensiveIncome.InterestExpenseOnDeposits = row.Amount ?? 0;
                            break;
                        case "4.3":
                            comprehensiveIncome.CostOfExternalBorrowings = row.Amount ?? 0;
                            break;
                        case "4.4":
                            comprehensiveIncome.DividendExpenses = row.Amount ?? 0;
                            break;
                        case "4.5":
                            comprehensiveIncome.OtherFinancialExpense = row.Amount ?? 0;
                            break;
                        case "4.6":
                            comprehensiveIncome.FeesCommissionExpense = row.Amount ?? 0;
                            break;
                        case "4.7":
                            comprehensiveIncome.OtherExpense = row.Amount ?? 0;
                            break;

                        // Allowance for Loan Loss
                        case "6.1":
                            comprehensiveIncome.ProvisionForLoanLosses = row.Amount ?? 0;
                            break;
                        case "6.2":
                            comprehensiveIncome.ValueOfLoansRecovered = row.Amount ?? 0;
                            break;

                        // Operating Expenses
                        case "7.1":
                            comprehensiveIncome.PersonnelExpenses = row.Amount ?? 0;
                            break;
                        case "7.2":
                            comprehensiveIncome.GovernanceExpenses = row.Amount ?? 0;
                            break;
                        case "7.3":
                            comprehensiveIncome.MarketingExpenses = row.Amount ?? 0;
                            break;
                        case "7.4":
                            comprehensiveIncome.DepreciationAmortizationCharges = row.Amount ?? 0;
                            break;
                        case "7.5":
                            comprehensiveIncome.AdministrativeExpenses = row.Amount ?? 0;
                            break;

                        // Non-Operating Income/Expense
                        case "9.1":
                            comprehensiveIncome.NonOperatingIncome = row.Amount ?? 0;
                            break;
                        case "9.2":
                            comprehensiveIncome.NonOperatingExpense = row.Amount ?? 0;
                            break;

                        // Taxes and Donations
                        case "11":
                            comprehensiveIncome.Taxes = row.Amount ?? 0;
                            break;
                        case "13":
                            comprehensiveIncome.Donations = row.Amount ?? 0;
                            break;
                    }
                }

                // Calculate and store all computed totals
                comprehensiveIncome.CalculateAndStoreTotals();

                if (isAmendment && comprehensiveIncome.Id != string.Empty)
                {
                    _context.NWDTComprehensiveIncomeReturns.Update(comprehensiveIncome);
                    _logger.LogInformation($"Updated existing comprehensive income record for amendment, ReturnId: {returnId}");
                }
                else
                {
                    await _context.NWDTComprehensiveIncomeReturns.AddAsync(comprehensiveIncome);
                    _logger.LogInformation($"Added new comprehensive income record, ReturnId: {returnId}");
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully processed Form 2F for return ID: {returnId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing Form 2F for return ID: {returnId}");
                throw;
            }
        }

        public static async Task ProcessForm2G(IFormFile formFile, string returnId, ILogger _logger, ReturnForm form, bool isAmendment, string prevId = "")
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            try
            {
                _logger.LogInformation($"Processing Form 2G for return ID: {returnId}");

                var form2G = ExcelService.ImportForm2GRows(formFile, _logger);
                if (form2G == null || form2G.Rows == null || !form2G.Rows.Any())
                {
                    throw new Exception("No data found in Statement of Financial Position Form");
                }

                var Path = await FormsHelper.SaveFileAsync(formFile, "NWDT Financial Position Returns");
                if (Path == null)
                {
                    throw new Exception("Error saving file");
                }

                var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form2G.EndDate);

                NWDTFinancialPositionReturn? financialPosition;

                if (isAmendment)
                {
                    financialPosition = await _context.NWDTFinancialPositionReturns.FirstOrDefaultAsync(x => x.ReturnId == returnId);

                    if (financialPosition == null)
                    {
                        _logger.LogWarning($"Amendment requested but no existing record found for ReturnId: {prevId}");
                        financialPosition = new NWDTFinancialPositionReturn();
                    }

                    financialPosition.PreviousReturnId = prevId;
                }
                else
                {
                    financialPosition = new NWDTFinancialPositionReturn
                    {
                        ReturnId = returnId,
                        StartDate = form2G.StartDate,
                        EndDate = form2G.EndDate,
                        Period = form2G.Period,
                        Frequency = form.Period.Name,
                        FilePath = Path,
                        DaysLateBy = DaysLateBy
                    };
                }

                // Map data from Form1Statement to entity properties based on reference numbers
                foreach (var row in form2G.Rows)
                {
                    switch (row.RefNumber)
                    {
                        // Cash & Cash Equivalent Section
                        case "1.1":
                            financialPosition.CashInHand = row.Amount ?? 0;
                            break;
                        case "1.2":
                            financialPosition.CashAtBank = row.Amount ?? 0;
                            break;

                        // Prepayments & Sundry Receivables
                        case "2.0":
                            financialPosition.PrepaymentsAndSundryReceivables = row.Amount ?? 0;
                            break;

                        // Financial Investments
                        case "3.1":
                            financialPosition.GovernmentSecurities = row.Amount ?? 0;
                            break;
                        case "3.2":
                            financialPosition.PlacementInFinancialInstitutions = row.Amount ?? 0;
                            break;
                        case "3.3":
                            financialPosition.CommercialPapers = row.Amount ?? 0;
                            break;
                        case "3.4":
                            financialPosition.CollectiveInvestmentSchemes = row.Amount ?? 0;
                            break;
                        case "3.5":
                            financialPosition.Derivatives = row.Amount ?? 0;
                            break;
                        case "3.6":
                            financialPosition.EquityInvestments = row.Amount ?? 0;
                            break;
                        case "3.7":
                            financialPosition.InvestmentInCompanies = row.Amount ?? 0;
                            break;

                        // Loan Portfolio
                        case "4.1":
                            financialPosition.GrossLoanPortfolio = row.Amount ?? 0;
                            break;
                        case "4.2":
                            financialPosition.AllowanceForLoanLoss = row.Amount ?? 0;
                            break;

                        // Accounts Receivables
                        case "5.1":
                            financialPosition.TaxRecoverable = row.Amount ?? 0;
                            break;
                        case "5.2":
                            financialPosition.DeferredTaxAssets = row.Amount ?? 0;
                            break;
                        case "5.3":
                            financialPosition.RetirementBenefitAssets = row.Amount ?? 0;
                            break;

                        // Property & Equipment Section
                        case "6.1":
                            financialPosition.InvestmentProperties = row.Amount ?? 0;
                            break;
                        case "6.2":
                            financialPosition.PropertyAndEquipment = row.Amount ?? 0;
                            break;
                        case "6.3":
                            financialPosition.PrepaidLeaseRentals = row.Amount ?? 0;
                            break;
                        case "6.4":
                            financialPosition.IntangibleAssets = row.Amount ?? 0;
                            break;
                        case "6.5":
                            financialPosition.OtherAssets = row.Amount ?? 0;
                            break;

                        // Liabilities - Deposits
                        case "7":
                            financialPosition.NonWithdrawableDeposits = row.Amount ?? 0;
                            break;

                        // Liabilities - Accounts Payable
                        case "8.1":
                            financialPosition.TaxPayable = row.Amount ?? 0;
                            break;
                        case "8.2":
                            financialPosition.DividendsPayable = row.Amount ?? 0;
                            break;
                        case "8.3":
                            financialPosition.DeferredTaxLiability = row.Amount ?? 0;
                            break;
                        case "8.4":
                            financialPosition.RetirementBenefitsLiability = row.Amount ?? 0;
                            break;
                        case "8.5":
                            financialPosition.OtherLiabilities = row.Amount ?? 0;
                            break;
                        case "8.6":
                            financialPosition.ExternalBorrowings = row.Amount ?? 0;
                            break;

                        // Equity
                        case "9":
                            financialPosition.ShareCapital = row.Amount ?? 0;
                            break;
                        case "10":
                            financialPosition.CapitalGrants = row.Amount ?? 0;
                            break;
                        case "11.1":
                            financialPosition.PriorYearsRetainedEarnings = row.Amount ?? 0;
                            break;
                        case "11.2":
                            financialPosition.CurrentYearSurplus = row.Amount ?? 0;
                            break;
                        case "12.1":
                            financialPosition.StatutoryReserve = row.Amount ?? 0;
                            break;
                        case "12.2":
                            financialPosition.OtherReserves = row.Amount ?? 0;
                            break;
                        case "12.3":
                            financialPosition.RevaluationReserves = row.Amount ?? 0;
                            break;
                        case "12.4":
                            financialPosition.ProposedDividends = row.Amount ?? 0;
                            break;
                        case "12.5":
                            financialPosition.AdjustmentToEquity = row.Amount ?? 0;
                            break;
                    }
                }

                financialPosition.CalculateAndStoreTotals();

                if (isAmendment && financialPosition.Id != string.Empty)
                {
                    _context.NWDTFinancialPositionReturns.Update(financialPosition);
                    _logger.LogInformation($"Updated existing financial position record for amendment, ReturnId: {returnId}");
                }
                else
                {
                    await _context.NWDTFinancialPositionReturns.AddAsync(financialPosition);
                    _logger.LogInformation($"Added new financial position record, ReturnId: {returnId}");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully processed Form 2G for return ID: {returnId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing Form 2G for return ID: {returnId}");
                throw;
            }
        }

        public static async Task ProcessComprehensiveIncomeForm(IFormFile file, string returnId, ILogger _logger, ReturnForm form)
        {
            ReturnsDbContext _context = new ReturnsDbContext();
            var form7 = ExcelService.ImportStatementOfComprehensiveIncomeRows(file, _logger);
            var rows = form7.Rows;
            if (rows == null || !rows.Any())
            {
                throw new Exception("No data found in Statement of Comprehensive Income Form");
            }

            var DaysLateBy = CalculateDaysLate(form, DateTime.Now, form7.EndDate);
            var FilePath = await FormsHelper.SaveFileAsync(file, "Statement of Comprehensive Income Returns");
            var statement = new DTComprehensiveIncomeReturn
            {
                ReturnId = returnId,
                Year = form7.Period,
                StartDate = form7.StartDate,
                EndDate = form7.EndDate,
                Frequency = form.Period.Name,
                FilePath = FilePath,
                DaysLateBy = DaysLateBy
            };
            foreach (var row in rows)
            {
                switch (row.RefNumber?.Trim())
                {
                    // Financial Income from Loans Portfolio
                    case "2.1":
                        statement.InterestOnLoanPortfolio = row.Amount ?? 0;
                        break;
                    case "2.2":
                        statement.FeesAndCommissionOnLoanPortfolio = row.Amount ?? 0;
                        break;

                    // Financial Income from Investments
                    case "3.1":
                        statement.GovernmentSecurities = row.Amount ?? 0;
                        break;
                    case "3.2":
                        statement.DepositsWithBanks = row.Amount ?? 0;
                        break;
                    case "3.3":
                        statement.OtherInvestments = row.Amount ?? 0;
                        break;
                    case "3.4":
                        statement.OtherOperatingIncome = row.Amount ?? 0;
                        break;

                    // Financial Expense
                    case "4.2":
                        statement.InterestExpenseOnDeposits = row.Amount ?? 0;
                        break;
                    case "4.3":
                        statement.CostOfExternalBorrowings = row.Amount ?? 0;
                        break;
                    case "4.4":
                        statement.DividendExpenses = row.Amount ?? 0;
                        break;
                    case "4.5":
                        statement.OtherFinancialExpense = row.Amount ?? 0;
                        break;
                    case "4.6":
                        statement.FeesAndCommissionExpense = row.Amount ?? 0;
                        break;
                    case "4.7":
                        statement.OtherExpense = row.Amount ?? 0;
                        break;

                    // Allowance for Loan Loss
                    case "6.1":
                        statement.ProvisionForLoanLosses = row.Amount ?? 0;
                        break;
                    case "6.2":
                        statement.ValueOfLoansRecovered = row.Amount ?? 0;
                        break;

                    // Operating Expenses
                    case "7.1":
                        statement.PersonnelExpenses = row.Amount ?? 0;
                        break;
                    case "7.2":
                        statement.GovernanceExpenses = row.Amount ?? 0;
                        break;
                    case "7.3":
                        statement.MarketingExpenses = row.Amount ?? 0;
                        break;
                    case "7.4":
                        statement.DepreciationAndAmortization = row.Amount ?? 0;
                        break;
                    case "7.5":
                        statement.AdministrativeExpenses = row.Amount ?? 0;
                        break;

                    // Non-Operating Income/Expense
                    case "9.1":
                        statement.NonOperatingIncome = row.Amount ?? 0;
                        break;
                    case "9.2":
                        statement.NonOperatingExpense = row.Amount ?? 0;
                        break;

                    // Taxes and Donations
                    case "11":
                        statement.Taxes = row.Amount ?? 0;
                        break;
                    case "13":
                        statement.Donations = row.Amount ?? 0;
                        break;
                }
            }
            await _context.DTComprehensiveIncomeReturns.AddAsync(statement);
            await _context.SaveChangesAsync();
        }

        public static async Task ProcessSectoralLendingForm(IFormFile file, string returnId, ILogger _logger, ReturnForm form, Boolean IsAmendMent, string PrevId = "")
        {
            try
            {
                ReturnsDbContext _context = new ReturnsDbContext();
                var ImportedSectoralReport = ExcelService.ImportSectoralLendingReport(file, _logger);
                var DaysLateBy = CalculateDaysLate(form, DateTime.Now, ImportedSectoralReport.EndDate);
                var FilePath = await FormsHelper.SaveFileAsync(file, "Statement of Comprehensive Income Returns");
                string EffectiveReturnId = returnId;
                string PreviousReturnId = string.Empty;
                List<EconomicSectorData> econDataList = new List<EconomicSectorData>();

                SectoralLendingReport? sectoralLendingReport = null!;
                if (!IsAmendMent)
                {
                    sectoralLendingReport = new SectoralLendingReport
                    {
                        ReturnId = returnId,
                        FilePath = FilePath,
                        Year = ImportedSectoralReport.Year,
                        Month = ImportedSectoralReport.Month,
                        StartDate = ImportedSectoralReport.StartDate,
                        EndDate = ImportedSectoralReport.EndDate,
                        DaysLateBy = DaysLateBy,
                        SaccoName = ImportedSectoralReport.SaccoName,
                        SaccoId = ImportedSectoralReport.SaccoId,
                        IsCurrent = true,
                        IsAmended = false
                    };
                }
                else
                {
                    // For an amendment, update the existing report.
                    sectoralLendingReport = await _context.SectoralLendingReports.FirstOrDefaultAsync(x => x.ReturnId == EffectiveReturnId);
                    if (sectoralLendingReport == null)
                    {
                        throw new Exception("Existing report not found for amendment.");
                    }
                    sectoralLendingReport.PreviousReturnId = PrevId;
                    sectoralLendingReport.IsCurrent = true;
                    sectoralLendingReport.IsAmended = false;
                    sectoralLendingReport.FilePath = FilePath;
                    sectoralLendingReport.ReturnId = EffectiveReturnId;
                }

                if (!IsAmendMent)
                {
                    await _context.SectoralLendingReports.AddAsync(sectoralLendingReport);
                }
                else
                {
                    _context.SectoralLendingReports.Update(sectoralLendingReport);
                }
                foreach (var catDto in ImportedSectoralReport.Categories)
                {
                    // Upsert Category: check if a Category with the same code exists.
                    var categoryEntity = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryCode == catDto.CategoryCode);

                    if (categoryEntity == null)
                    {
                        categoryEntity = new Category
                        {
                            CategoryCode = catDto.CategoryCode,
                            CategoryName = catDto.CategoryName
                        };
                        _context.Categories.Add(categoryEntity);
                    }
                    else
                    {
                        // Update properties if needed.
                        categoryEntity.CategoryName = catDto.CategoryName;
                        _context.Categories.Update(categoryEntity);
                    }
                    await _context.SaveChangesAsync();

                    // 3. Process each SubCategoryDto within the Category.
                    foreach (var subDto in catDto.SubCategories)
                    {
                        var subCategoryEntity = await _context.SubCategories
                            .FirstOrDefaultAsync(sc => sc.Code == subDto.SubCategoryCode && sc.CategoryId == categoryEntity.Id);

                        if (subCategoryEntity == null)
                        {
                            subCategoryEntity = new SubCategory
                            {
                                Code = subDto.SubCategoryCode,
                                Name = subDto.SubCategoryName,
                                CategoryId = categoryEntity.Id
                            };
                            _context.SubCategories.Add(subCategoryEntity);
                        }
                        else
                        {
                            subCategoryEntity.Name = subDto.SubCategoryName;
                            _context.SubCategories.Update(subCategoryEntity);
                        }
                        await _context.SaveChangesAsync();

                        // 4. Process each EconomicSectorDto within the SubCategory.
                        foreach (var econDto in subDto.EconomicSectors)
                        {
                            var econEntity = await _context.EconomicSectors.FirstOrDefaultAsync(es => es.Code == econDto.EconomicSectorCode && es.SubCategoryId == subCategoryEntity.Id);

                            if (econEntity == null)
                            {
                                econEntity = new EconomicSector
                                {
                                    Code = econDto.EconomicSectorCode,
                                    Name = econDto.EconomicSectorName,
                                    SubCategoryId = subCategoryEntity.Id
                                };
                                _context.EconomicSectors.Add(econEntity);
                            }
                            else
                            {
                                econEntity.Name = econDto.EconomicSectorName;
                                _context.EconomicSectors.Update(econEntity);
                            }
                            await _context.SaveChangesAsync();

                            // 5. Create an EconomicSectorData record for this economic sector.
                            var econData = new EconomicSectorData
                            {
                                Amount = econDto.Amount,
                                Category = categoryEntity.CategoryName,
                                SubCategory = subCategoryEntity.Name,
                                EconomicSectorName = econEntity.Name,
                                ReturnId = sectoralLendingReport.ReturnId,   
                                IsCurrent = sectoralLendingReport.IsCurrent,
                                PreviousReturnId = sectoralLendingReport.PreviousReturnId,
                                IsAmended = sectoralLendingReport.IsAmended,
                                EconomicSectorId = econEntity.Id,
                                SectoralLendingReportId = sectoralLendingReport.Id
                            };
                            econDataList.Add(econData);
                            _context.SectoralLendingData.Add(econData);
                            await _context.SaveChangesAsync();

                        }


                        await _context.SaveChangesAsync();
                    }
                }

            }
            catch (Exception Ex)
            {

                throw Ex;
            }
        }
        public static (bool IsValid, string Message, string CommonPeriod) AreAllFormsInSamePeriodNWDT(
            Form2AStatement? capital_adequacy_form1,
            Form2BStatement? liquidityStatement_form_2,
            Form2CStatement? depositreturn_form_3,
            Form2DStatement? riskClassification_form_4,
            Form2EStatement? inverstment_return_form_5,
            Form2GStatement? financialPositionStatement_form_6,
            Form2FStatement? comprehensiveStatement_form7)
        {
            var formsToValidate = new List<(object? Form, string FormName)>
                {
                    (capital_adequacy_form1, "Capital Adequacy Form"),
                    (liquidityStatement_form_2, "Liquidity Statement Form"),
                    (depositreturn_form_3, "Deposit Return Form"),
                    (riskClassification_form_4, "Risk Classification Form"),
                    (inverstment_return_form_5, "Investment Return Form"),
                    (financialPositionStatement_form_6, "Financial Position Statement Form"),
                    (comprehensiveStatement_form7, "Comprehensive Statement Form")
                };

            // Check if any form is null
            foreach (var (form, formName) in formsToValidate)
            {
                if (form == null)
                {
                    return (false, $"{formName} is null.", "");
                }
            }

            // Get the period of the first form
            var firstFormPeriod = (formsToValidate[0].Form as dynamic)?.Period;

            // Check if all forms have the same period
            foreach (var (form, formName) in formsToValidate)
            {
                var currentFormPeriod = (form as dynamic)?.Period;
                if (currentFormPeriod != firstFormPeriod)
                {
                    return (false, $"{formName} has a different period. Expected: {firstFormPeriod}, Actual: {currentFormPeriod}", "");
                }
            }

            // All forms have the same period, return it
            return (true, "All forms are in the same period.", firstFormPeriod?.ToString() ?? "");
        }
        public static (bool IsValid, string Message, string CommonPeriod) AreAllFormsInSamePeriod(
        Form1Statement? capital_adequacy_form1,
        Form2Statement? liquidityStatement_form_2,
        Form3Statement? depositreturn_form_3,
        Form4Statement? riskClassification_form_4,
        Form5Statement? inverstment_return_form_5,
        Form6Statement? financialPositionStatement_form_6,
        Form7Statement? comprehensiveStatement_form7)
        {
            var formsToValidate = new List<(object? Form, string FormName)>
            {
                (capital_adequacy_form1, "Capital Adequacy Form"),
                (liquidityStatement_form_2, "Liquidity Statement Form"),
                (depositreturn_form_3, "Deposit Return Form"),
                (riskClassification_form_4, "Risk Classification Form"),
                (inverstment_return_form_5, "Investment Return Form"),
                (financialPositionStatement_form_6, "Financial Position Statement Form"),
                (comprehensiveStatement_form7, "Comprehensive Statement Form")
            };

            // Check if any form is null
            foreach (var (form, formName) in formsToValidate)
            {
                if (form == null)
                {
                    return (false, $"{formName} is null.", "");
                }
            }

            // Get the period of the first form
            var firstFormPeriod = (formsToValidate[0].Form as dynamic)?.Period;

            // Check if all forms have the same period
            foreach (var (form, formName) in formsToValidate)
            {
                var currentFormPeriod = (form as dynamic)?.Period;
                if (currentFormPeriod != firstFormPeriod)
                {
                    return (false, $"{formName} has a different period. Expected: {firstFormPeriod}, Actual: {currentFormPeriod}", "");
                }
            }

            return (true, "All forms are in the same period.", firstFormPeriod?.ToString() ?? "");
        }
    }
}
