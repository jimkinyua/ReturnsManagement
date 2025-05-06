using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using static Returns.Helpers.ExcelService.Form2CStatement;
using System;
using Microsoft.IdentityModel.Tokens;
using static Returns.Helpers.ExcelService;
using Returns.Models.Common;
using Returns.Models;
using Returns.DTOs.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace Returns.Helpers
{
    public class ExcelService
    {
        private readonly ILogger<ExcelService> _logger;
        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger;
        }

        public class SectoralLendingReportDto
        {
            public string SaccoId { get; set; } = string.Empty;
            public string SaccoName { get; set; } = string.Empty;
            public string Year { get; set; } = string.Empty;
            public string Month { get; set; } = string.Empty;
            public DateTime StartDate { get; set; } 
            public DateTime EndDate { get; set; }
            public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        }

        public class CategoryDto
        {
            public string CategoryCode { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public List<SubCategoryDto> SubCategories { get; set; } = new List<SubCategoryDto>();
        }

        public class SubCategoryDto
        {
            public string SubCategoryCode { get; set; } = string.Empty;
            public string SubCategoryName { get; set; } = string.Empty;
            public List<EconomicSectorDto> EconomicSectors { get; set; } = new List<EconomicSectorDto>();
        }

        public class EconomicSectorDto
        {
            public string EconomicSectorCode { get; set; } = string.Empty;
            public string EconomicSectorName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        public class CapitalAdequacyRow
        {
            public string Index { get; set; }
            public string? Description { get; set; }
            public decimal? Amount { get; set; }
            public string CellNumberWithFigures { get; set; }
        }
        public class Form1Statement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<CapitalAdequacyRow> Rows { get; set; } = new List<CapitalAdequacyRow>();
        }

        public class DailyLiquidityStatement
        {
            public string SACCOName { get; set; } = string.Empty;
            public string CSNO { get; set; } = string.Empty;
            public DateTime ReportDate { get; set; }

            // Opening Balances
            public decimal BankBalancesOpening { get; set; }
            public decimal ConsolidatedTreasuryCashBalancesOpening { get; set; }
            public decimal TellersBalancesOpening { get; set; }
            public decimal MobileMoneyChannelsOpening { get; set; }
            public decimal PlacementWithBanksOpening { get; set; }
            public decimal SubTotalOpening { get; set; }

            // Day Receipts
            public decimal DepositsFromMembers { get; set; }
            public decimal CashLoanRepayments { get; set; }
            public decimal OtherCashReceipts { get; set; }
            public decimal SubTotalReceipts { get; set; }
            public decimal TotalOpeningAndReceipts { get; set; }

            // Day Payments
            public decimal CashWithdrawalsByMembers { get; set; }
            public decimal CashPaymentsToMembers { get; set; }
            public decimal OtherCashPayments { get; set; }
            public decimal SubTotalPayments { get; set; }

            // Closing Balances
            public decimal BankBalancesClosing { get; set; }
            public decimal ConsolidatedTreasuryCashBalancesClosing { get; set; }
            public decimal TellersBalancesClosing { get; set; }
            public decimal MobileMoneyChannelsClosing { get; set; }
            public decimal PlacementWithBanksClosing { get; set; }
            public decimal TotalClosingBalance { get; set; }

            // Deposit Liabilities
            public decimal BOSADeposits { get; set; }
            public decimal FOSADeposits { get; set; }
            public decimal TotalDeposits { get; set; }

            // Liquidity Ratios
            public decimal TotalClosingBalanceToTotalDepositsRatio { get; set; }
            public decimal TotalClosingBalanceToFOSADepositsRatio { get; set; }
        }


        public class LiquidityStatementRow
        {
            public string Index { get; set; }
            public string Description { get; set; }
            public decimal? Amount { get; set; }
            public string CellNumberWithFigures { get; set; }
        }

        public class Form2Statement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<LiquidityStatementRow> Rows { get; set; } = new List<LiquidityStatementRow>();
        }

        public class Form4Statement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<RiskClassificationRow> Rows { get; set; } = new List<RiskClassificationRow>();
        }

        public class RiskClassificationRow
        {
            public string LoanType { get; set; } // "Regular" or "Rescheduled/Renegotiated"
            public string Classification { get; set; } // "Performing", "Watch", "Substandard", etc.
            public string OutstandingLoanPortfolioCellAddress { get; set; }
            public int? NumberOfAccounts { get; set; }
            public decimal? OutstandingLoanPortfolio { get; set; }
            public decimal? RequiredProvision { get; set; }
            public decimal? RequiredProvisionAmount { get; set; }
            public decimal TotalOutStandingLoanPortifolio { get; set; }
            public decimal RequiredProvisionPercentage { get; set; }
            public string? CellNumberWithAccounts { get; internal set; }
            public string? CellNumberWithPortfolio { get; internal set; }
            public string? CellNumberWithProvisionAmount { get; internal set; }
            public string RequiredPercentage { get; set; }  // Store as string to preserve "1%" format

            public void CalculateProvisionAmount()
            {
                RequiredProvisionAmount = OutstandingLoanPortfolio * ProvisionPercentageValue;
            }
            public decimal ProvisionPercentageValue
            {
                get
                {
                    if (string.IsNullOrEmpty(RequiredPercentage))
                        return 0;

                    string percentString = RequiredPercentage.Replace("%", "").Trim();
                    if (decimal.TryParse(percentString, out decimal result))
                        return result / 100;

                    return 0;
                }
            }
        }

        public class InvestmentRow
        {
            public string Index { get; set; }
            public string Description { get; set; }
            public string cellNumberWithFigures { get; set; }
            public decimal? Amount { get; set; }
        }
        public class Form5Statement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<InvestmentRow> Rows { get; set; } = new List<InvestmentRow>();
        }
        public class Form2EStatement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<Form2ERow> Rows { get; set; } = new List<Form2ERow>();
        }

        public class Form2AStatement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<CapitalAdequacyRow> Rows { get; set; } = new List<CapitalAdequacyRow>();
        }

        public class Form2ERow
        {
            public string Index { get; set; }
            public string Description { get; set; }
            public decimal? Amount { get; set; }
            public string CellReference { get; set; }

        }
        public class StatementOfFinancialPositionRow
        {
            public string RefNumber { get; set; }
            public string Description { get; set; }
            public string CellNumberWithFigures { get; set; }
            public decimal? Amount { get; set; }
        }

        public class Form6Statement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<StatementOfFinancialPositionRow> Rows { get; set; } = new List<StatementOfFinancialPositionRow>();
        }


        public class Form7Statement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<StatementOfComprehensiveIncomeRow> Rows { get; set; } = new List<StatementOfComprehensiveIncomeRow>();
        }
        public class StatementOfComprehensiveIncomeRow
        {
            public string RefNumber { get; set; }
            public string Description { get; set; }
            public string CellNumberWithFigures { get; set; }
            public decimal? Amount { get; set; }
        }

        public class DepositRangeData
        {
            public string RangeName { get; set; }
            public string CellNumber { get; set; }
            public string DepositType { get; set; } = string.Empty;
            public int NumberOfAccounts { get; set; }
            public decimal AmountInKshs000 { get; set; }
        }

        public class Form3Statement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<DepositRangeData> Rows { get; set; } = new List<DepositRangeData>();
        }

        public class Form2DStatement
        {
            public string SaccoName { get; set; }
            public string CsNumber { get; set; }
            public string FinancialYear { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<RiskClassificationRow> Rows { get; set; } = new List<RiskClassificationRow>();
        }

        public class Form2FStatement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<StatementOfComprehensiveIncomeRow> Rows { get; set; } = new List<StatementOfComprehensiveIncomeRow>();
        }

        public class Form2GStatement
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period { get; set; } = string.Empty;
            public List<StatementOfFinancialPositionRow> Rows { get; set; } = new List<StatementOfFinancialPositionRow>();
        }

        public class EconomicSectorRow
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
        public class SectoralLendingRecord
        {
            public string SaccoSocietyCSNumber { get; set; } = string.Empty;
            public string Financial_Year { get; set; } = string.Empty;
            public DateTime start_date { get; set; }
            public DateTime end_date { get; set; }
            public string Month { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string EconomicSector { get; set; } = string.Empty;
            public decimal LoansAndAdvancesAmount { get; set; }
            public string CATEGORY { get; set; } = string.Empty;
            public string SUB_CATEGORY { get; set; } = string.Empty;
        }

        public class SectoralLendingDataRow
        {
            public string SectorCode { get; set; } = null!;
            public string SectorName { get; set; } = null!;
            public decimal Amount { get; set; }
            public SectorLevel Level { get; set; }
            public string? ParentCode { get; set; }
        }

        public enum SectorLevel
        {
            MainSector = 1,
            SubSector = 2,
            DetailItem = 3
        }


        public class Form2BStatement
        {
            // Form metadata
            public string SaccoName { get; set; }
            public string CsNumber { get; set; }
            public string Period { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

            // The rows collection
            public List<LiquidityStatementRow> Rows { get; set; } = new List<LiquidityStatementRow>();

            // Computed properties
            public decimal TotalNotesAndCoins => GetSectionTotal("1");
            public decimal TotalBankBalances => GetSectionTotal("2");
            public decimal TotalOtherFinancialInstitutions => GetSectionTotal("3");
            public decimal TotalGovernmentSecurities => GetSectionTotal("4");
            public decimal NetLiquidAssets => GetItemAmount("5");
            public decimal TotalOtherLiabilities => GetItemAmount("6.3");
            public decimal LiquidityRatio => GetItemAmount("7.3");
            public decimal MinimumRequirement => 10.0m; // Default 10%
            public decimal ExcessDeficit => GetItemAmount("7.5");

            // Helper method to get section totals
            private decimal GetSectionTotal(string sectionId)
            {
                return Rows.FirstOrDefault(r => r.Index == sectionId && !string.IsNullOrEmpty(r.Index))?.Amount ?? 0;
            }

            // Helper method to get a specific item amount
            private decimal GetItemAmount(string itemId)
            {
                return Rows.FirstOrDefault(r => r.Index == itemId)?.Amount ?? 0;
            }
        }

        public class Form2CStatement
        {

            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Period = string.Empty;

            public List<DepositRangeRow> Rows { get; set; } = new List<DepositRangeRow>();

            public int TotalNumberOfAccounts => GetTotalNumberOfAccounts();
            public decimal TotalAmount => GetTotalAmount();

            private int GetTotalNumberOfAccounts()
            {
                int total = 0;
                foreach (var row in Rows)
                {
                    total += row.NumberOfAccounts;
                }
                return total;
            }

            // Helper method to calculate total amount
            private decimal GetTotalAmount()
            {
                decimal total = 0;
                foreach (var row in Rows)
                {
                    total += row.Amount;
                }
                return total;
            }

            public class DepositRangeRow
            {
                public string Number { get; set; }
                public string Range { get; set; }
                public string DepositType { get; set; }
                public int NumberOfAccounts { get; set; }
                public decimal Amount { get; set; }
                public string CellNumberWithAccounts { get; set; }
                public string CellNumberWithAmount { get; set; }
            }
        }


        public class InsiderLendingReportDTO
        {
            public string SaccoName { get; set; } = null!;
            public string SaccoSocietyCsNumber { get; set; } =null!;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime SubmissionDate { get; set; }
            public string Year { get; set; } = null!;
            public string Quarter { get; set; } =null!;
            public decimal TotalNewLoansAmount { get; set; }
            public decimal TotalOutstandingLoansAmount { get; set; }
            public int TotalLoansCount { get; set; }
            public List<InsiderLoanDTO> Loans { get; set; } = new List<InsiderLoanDTO>();
        }

        public class InsiderLoanDTO
        {
            public string LoanCategory { get; set; } = string.Empty; // "New" or "Outstanding" 
            public string NameOfBorrower { get; set; } = string.Empty;
            public string MemberNumber { get; set; } = string.Empty;
            public string PositionHeld { get; set; } = string.Empty;
            public string LoanTypeName { get; set; } = string.Empty;
            public decimal AmountAppliedFor { get; set; }
            public decimal AmountGranted { get; set; }
            public DateTime DateApprovedOrRatified { get; set; }
            public decimal AmountOfBosaDeposits { get; set; }
            public string NatureOfSecurity { get; set; } = string.Empty;
            public DateTime RepaymentCommencementDate { get; set; }
            public string RepaymentPeriod { get; set; } = string.Empty;
            public string? OtherRemarks { get; set; }
            public decimal? OutstandingAmount { get; set; }
            public string? PerfomanceCategory { get; set; }
            public string RepaymentStatus { get; set; } = string.Empty;
        }

        public static InsiderLendingReportDTO ImportInsiderLendingReport(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("Processing Insider Lending Report: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                         $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                         "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                         "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    // Copy the file to a memory stream
                    logger.LogInformation("Copying file to memory stream");
                    file.CopyTo(stream);

                    // This will hold all insider loans (both new and outstanding)
                    var insiderLoans = new List<InsiderLoanDTO>();

                    // Open the Excel workbook
                    using (var workbook = new XLWorkbook(stream))
                    {
                        logger.LogInformation("Workbook opened");
                        // Get the first worksheet (Insider return)
                        var worksheet = workbook.Worksheets.First();

                        // Create the header DTO
                        var reportDTO = new InsiderLendingReportDTO
                        {
                            SaccoName = GetCellValueOrEmpty(worksheet.Cell("C3")),
                            SaccoSocietyCsNumber = GetCellValueOrEmpty(worksheet.Cell("C4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C5"))).Value,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))).Value,
                            SubmissionDate = DateTime.Now,
                            Year = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))).Value.Year.ToString(),
                            Quarter = $"Q{(DateTime.Now.Month - 1) / 3 + 1}"
                        };

                        // Find section markers
                        int newLoansHeaderRow = FindRowWithText(worksheet, "New loans Granted", "C");
                        int outstandingLoansHeaderRow = FindRowWithText(worksheet, "PERFORMANCE OF INSIDER OUTSTANDING LOAN", "C");
                        int totalNewLoansRow = FindRowWithText(worksheet, "TOTAL LOANS GRANTED FOR INSIDERS", "C");
                        int totalOutstandingLoansRow = FindRowWithText(worksheet, "TOTAL OUTSTANDING LOANS FOR INSIDERS AS AT END OF MONTH", "C");

                        // Process new loans section
                        if (newLoansHeaderRow > 0 && totalNewLoansRow > 0)
                        {
                            int firstNewLoanRow = newLoansHeaderRow + 3; // Skip the header and column titles rows
                            int lastNewLoanRow = totalNewLoansRow - 1;   // Stop before the total row

                            // Process new loans dynamically for all rows in section
                            for (int rowNum = firstNewLoanRow; rowNum <= lastNewLoanRow; rowNum++)
                            {
                                var row = worksheet.Row(rowNum);

                                // Skip if no borrower name
                                string nameOfBorrower = GetCellValueOrEmpty(row.Cell(3));
                                if (string.IsNullOrWhiteSpace(nameOfBorrower))
                                    continue;

                                var loanDTO = new InsiderLoanDTO
                                {
                                    LoanCategory = "New", // Mark as a new loan
                                    NameOfBorrower = nameOfBorrower,
                                    MemberNumber = GetCellValueOrEmpty(row.Cell(4)),
                                    PositionHeld = GetCellValueOrEmpty(row.Cell(5)),
                                    LoanTypeName = GetCellValueOrEmpty(row.Cell(6)),
                                    AmountAppliedFor = GetDecimalOrZero(row.Cell(7)),
                                    AmountGranted = GetDecimalOrZero(row.Cell(8)),
                                    DateApprovedOrRatified = ParseDateOrNull(GetCellValueOrEmpty(row.Cell(9))).Value,
                                    AmountOfBosaDeposits = GetDecimalOrZero(row.Cell(10)),
                                    NatureOfSecurity = GetCellValueOrEmpty(row.Cell(11)),
                                    RepaymentCommencementDate = ParseDateOrNull(GetCellValueOrEmpty(row.Cell(12))).Value,
                                    RepaymentPeriod = GetCellValueOrEmpty(row.Cell(13)),
                                    OtherRemarks = GetCellValueOrEmpty(row.Cell(14)),
                                    OutstandingAmount = GetDecimalOrZero(row.Cell(8)), // For new loans, initially outstanding = amount granted
                                    PerfomanceCategory = "Performing", // New loans are initially performing
                                    RepaymentStatus = "Current"
                                };

                                insiderLoans.Add(loanDTO);
                            }
                        }

                        // Process outstanding loans section
                        if (outstandingLoansHeaderRow > 0 && totalOutstandingLoansRow > 0)
                        {
                            int firstOutstandingLoanRow = outstandingLoansHeaderRow + 1; // Skip the header and column titles rows
                            int lastOutstandingLoanRow = totalOutstandingLoansRow - 1;   // Stop before the total row

                            // Process outstanding loans dynamically for all rows in section
                            for (int rowNum = firstOutstandingLoanRow; rowNum <= lastOutstandingLoanRow; rowNum++)
                            {
                                var row = worksheet.Row(rowNum);

                                // Skip if no borrower name
                                string nameOfBorrower = GetCellValueOrEmpty(row.Cell(3));
                                if (string.IsNullOrWhiteSpace(nameOfBorrower))
                                    continue;

                                var loanDTO = new InsiderLoanDTO
                                {
                                    LoanCategory = "Outstanding", // Mark as an outstanding loan
                                    NameOfBorrower = nameOfBorrower,
                                    MemberNumber = GetCellValueOrEmpty(row.Cell(4)),
                                    PositionHeld = GetCellValueOrEmpty(row.Cell(5)),
                                    LoanTypeName = GetCellValueOrEmpty(row.Cell(6)),
                                    AmountAppliedFor = GetDecimalOrZero(row.Cell(7)),
                                    AmountGranted = GetDecimalOrZero(row.Cell(8)),
                                    DateApprovedOrRatified = ParseDateOrNull(GetCellValueOrEmpty(row.Cell(9))).Value,
                                    AmountOfBosaDeposits = GetDecimalOrZero(row.Cell(10)),
                                    NatureOfSecurity = GetCellValueOrEmpty(row.Cell(11)),
                                    RepaymentCommencementDate = ParseDateOrNull(GetCellValueOrEmpty(row.Cell(12))).Value,
                                    RepaymentPeriod = GetCellValueOrEmpty(row.Cell(13)),
                                    OutstandingAmount = GetDecimalOrZero(row.Cell(14)),
                                    PerfomanceCategory = GetCellValueOrEmpty(row.Cell(15)) ?? "Not Specified",
                                    RepaymentStatus = "Existing",
                                    OtherRemarks = null
                                };

                                insiderLoans.Add(loanDTO);
                            }
                        }

                        // Calculate summary statistics
                        reportDTO.TotalNewLoansAmount = insiderLoans
                            .Where(l => l.LoanCategory == "New")
                            .Sum(l => l.AmountGranted);

                        reportDTO.TotalOutstandingLoansAmount = insiderLoans
                            .Where(l => l.LoanCategory == "Outstanding")
                            .Sum(l => l.OutstandingAmount ?? 0);

                        reportDTO.TotalLoansCount = insiderLoans.Count;

                        // Attach loans to report
                        reportDTO.Loans = insiderLoans;

                        return reportDTO;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;// new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;// new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException ex)
            {
                    throw new ValidationException(
                      $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                      "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                      "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportInsiderLendingReport");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }

        public static DailyLiquidityStatement ImportDailyLiquidityRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                         $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                         "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                         "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    // Copy the file to a memory stream
                    logger.LogInformation("Copying file to memory stream");
                    file.CopyTo(stream);

                    // This will hold our result
                    var dailyLiquidityStatement = new DailyLiquidityStatement();

                    // Open the Excel workbook
                    using (var workbook = new XLWorkbook(stream))
                    {
                        logger.LogInformation("Workbook opened");

                        // Get the first worksheet
                        var worksheet = workbook.Worksheets.First();

                        // Extract SACCO details and report date
                        dailyLiquidityStatement.SACCOName = GetCellValueOrEmpty(worksheet.Cell("C5"));
                        dailyLiquidityStatement.CSNO = GetCellValueOrEmpty(worksheet.Cell("E5"));
                        dailyLiquidityStatement.ReportDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("E6"))) ?? DateTime.Now;

                        // Opening Balances
                        dailyLiquidityStatement.BankBalancesOpening = GetDecimalOrZero(worksheet.Cell("F8"));
                        dailyLiquidityStatement.ConsolidatedTreasuryCashBalancesOpening = GetDecimalOrZero(worksheet.Cell("F9"));
                        dailyLiquidityStatement.TellersBalancesOpening = GetDecimalOrZero(worksheet.Cell("F10"));
                        dailyLiquidityStatement.MobileMoneyChannelsOpening = GetDecimalOrZero(worksheet.Cell("F11"));
                        dailyLiquidityStatement.PlacementWithBanksOpening = GetDecimalOrZero(worksheet.Cell("F12"));
                        dailyLiquidityStatement.SubTotalOpening = GetDecimalOrZero(worksheet.Cell("F13"));

                        // Day Receipts
                        dailyLiquidityStatement.DepositsFromMembers = GetDecimalOrZero(worksheet.Cell("F15"));
                        dailyLiquidityStatement.CashLoanRepayments = GetDecimalOrZero(worksheet.Cell("F16"));
                        dailyLiquidityStatement.OtherCashReceipts = GetDecimalOrZero(worksheet.Cell("F17"));
                        dailyLiquidityStatement.SubTotalReceipts = GetDecimalOrZero(worksheet.Cell("F18"));
                        dailyLiquidityStatement.TotalOpeningAndReceipts = GetDecimalOrZero(worksheet.Cell("F19"));

                        // Day Payments
                        dailyLiquidityStatement.CashWithdrawalsByMembers = GetDecimalOrZero(worksheet.Cell("F21"));
                        dailyLiquidityStatement.CashPaymentsToMembers = GetDecimalOrZero(worksheet.Cell("F22"));
                        dailyLiquidityStatement.OtherCashPayments = GetDecimalOrZero(worksheet.Cell("F23"));
                        dailyLiquidityStatement.SubTotalPayments = GetDecimalOrZero(worksheet.Cell("F24"));

                        // Closing Balances
                        dailyLiquidityStatement.BankBalancesClosing = GetDecimalOrZero(worksheet.Cell("F26"));
                        dailyLiquidityStatement.ConsolidatedTreasuryCashBalancesClosing = GetDecimalOrZero(worksheet.Cell("F27"));
                        dailyLiquidityStatement.TellersBalancesClosing = GetDecimalOrZero(worksheet.Cell("F28"));
                        dailyLiquidityStatement.MobileMoneyChannelsClosing = GetDecimalOrZero(worksheet.Cell("F29"));
                        dailyLiquidityStatement.PlacementWithBanksClosing = GetDecimalOrZero(worksheet.Cell("F30"));
                        dailyLiquidityStatement.TotalClosingBalance = GetDecimalOrZero(worksheet.Cell("F31"));

                        // Deposit Liabilities
                        dailyLiquidityStatement.BOSADeposits = GetDecimalOrZero(worksheet.Cell("F33"));
                        dailyLiquidityStatement.FOSADeposits = GetDecimalOrZero(worksheet.Cell("F34"));
                        dailyLiquidityStatement.TotalDeposits = GetDecimalOrZero(worksheet.Cell("F35"));

                        // Liquidity Ratios
                        dailyLiquidityStatement.TotalClosingBalanceToTotalDepositsRatio = GetDecimalOrZero(worksheet.Cell("F37"));
                        dailyLiquidityStatement.TotalClosingBalanceToFOSADepositsRatio = GetDecimalOrZero(worksheet.Cell("F38"));

                        return dailyLiquidityStatement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw; //new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw; //new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw;
                    /*new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");*/
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportDailyLiquidityRows");
                throw; 
                   /* new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }

        private static decimal GetDecimalOrZero(IXLCell cell)
        {
            if (cell == null)
                return 0;

            string value = cell.GetString().Trim();

            if (string.IsNullOrWhiteSpace(value) || value == "-")
                return 0;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return 0;
        }
        private static int FindRowWithText(IXLWorksheet worksheet, string text, string column)
        {
            int lastRowUsed = worksheet.LastRowUsed().RowNumber();
            for (int row = 1; row <= lastRowUsed; row++)
            {
                string cellValue = GetCellValueOrEmpty(worksheet.Cell($"{column}{row}"));
                if (cellValue.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }
            return -1; // Not found
        }

        public static SectoralLendingReportDto ImportSectoralLendingReport(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("Importing Sectoral Lending data from file: {FileName}", file.FileName);

                if (file == null)
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                if (file.Length == 0)
                    throw new ArgumentException("The uploaded file is empty", nameof(file));

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx" )
                    throw new ValidationException($"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " + "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " + "Please save the sheet in .xlsx format and upload again.");

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        // 1. Find the correct worksheet (adjust if needed)
                        var worksheet = workbook.Worksheets.FirstOrDefault(ws =>
                            ws.Name.Equals("Sheet1", StringComparison.OrdinalIgnoreCase));
                        if (worksheet == null)
                            throw new Exception("Worksheet 'Sheet1' not found.");

                        // 2. Extract Metadata (adjust cell references for your file):
                        string saccoId = GetCellValueOrEmpty(worksheet.Cell("C3"));
                        string saccoName = GetCellValueOrEmpty(worksheet.Cell("C3"));
                        string financialYear = GetCellValueOrEmpty(worksheet.Cell("C4"));
                        DateTime? startDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C5")));
                        DateTime? endDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6")));

                        if (string.IsNullOrWhiteSpace(saccoId))
                            throw new Exception("SaccoId not found in cell C3");
                        if (string.IsNullOrWhiteSpace(saccoName))
                            throw new Exception("SaccoName not found in cell C4");
                        if (string.IsNullOrWhiteSpace(financialYear))
                            throw new Exception("Financial Year not found in cell C5");
                        if (!startDate.HasValue)
                            throw new Exception("Start Date not found or invalid in cell C6");

                        string month = endDate.Value.ToString("MMMM", CultureInfo.InvariantCulture);

                        // 3. Locate Header Row (where column 2 says "CODE")
                        int headerRow = -1;
                        int searchLimit = 20;
                        for (int r = 1; r <= searchLimit; r++)
                        {
                            string cellVal = GetCellValueOrEmpty(worksheet.Cell(r, 2)).ToUpper();
                            if (cellVal.Contains("CODE"))
                            {
                                headerRow = r;
                                break;
                            }
                        }
                        if (headerRow == -1)
                            throw new Exception("Could not locate the data header row (containing 'CODE').");

                        // Initialize the main DTO
                        var reportDto = new SectoralLendingReportDto
                        {
                            SaccoId = saccoId,
                            SaccoName = saccoName,
                            Year = financialYear,
                            Month = month,
                            StartDate = startDate.Value,
                            EndDate = endDate.Value,
                            Categories = new List<CategoryDto>()
                        };

                        // We'll keep track of the "current" category and subcategory as we parse.
                        CategoryDto currentCategory = null!;
                        SubCategoryDto currentSubCategory = null!;

                        int dataStartRow = headerRow + 1;
                        int lastRow = worksheet.LastRowUsed().RowNumber();

                        // 4. Process each data row
                        for (int rowNum = dataStartRow; rowNum <= lastRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);

                            // Column 2 => if non-empty => Category code
                            string catCode = GetCellValueOrEmpty(row.Cell(2)).Trim();

                            // Column 3 => either sub-cat or economic sector code + name combined
                            string combinedCellValue = GetCellValueOrEmpty(row.Cell(3)).Trim();

                            // Column 4 => numeric amount (only relevant for economic sectors)
                            decimal amount = GetDecimalOrNull(row.Cell(4)) ?? 0;

                            // CASE A: If catCode is non-empty => treat this row as a Category row
                            // e.g. "1000 => AGRICULTURE"
                            if (!string.IsNullOrWhiteSpace(catCode))
                            {
                                // By assumption, category codes end with "000"
                                currentCategory = new CategoryDto
                                {
                                    CategoryCode = catCode,
                                    // For the category name, let's read the "ECONOMIC SECTORS" text from column 3
                                    CategoryName = combinedCellValue,
                                    SubCategories = new List<SubCategoryDto>()
                                };
                                reportDto.Categories.Add(currentCategory);
                                currentSubCategory = null!;
                            }
                            else
                            {
                                // CASE B: catCode is empty => this row is either a SubCategory or an EconomicSector
                                if (string.IsNullOrWhiteSpace(combinedCellValue))
                                {
                                    // no data => break or continue
                                    break;
                                }

                                // We parse out the code and name from combinedCellValue
                                (string code, string name) = ParseCodeAndName(combinedCellValue);

                                if (code.EndsWith("100"))
                                {
                                    // => SUB-CATEGORY
                                    currentSubCategory = new SubCategoryDto
                                    {
                                        SubCategoryCode = code,
                                        SubCategoryName = name,
                                        EconomicSectors = new List<EconomicSectorDto>()
                                    };
                                    if (currentCategory == null)
                                        throw new Exception("SubCategory found without an active Category row above it.");
                                    currentCategory.SubCategories.Add(currentSubCategory);
                                }
                                else
                                {
                                    // => ECONOMIC SECTOR
                                    if (currentSubCategory == null)
                                        throw new Exception("Economic Sector row found but no current SubCategory is set.");

                                    // This row's code & name are for an economic sector
                                    var econSector = new EconomicSectorDto
                                    {
                                        EconomicSectorCode = code,
                                        EconomicSectorName = name,
                                        Amount = amount
                                    };
                                    currentSubCategory.EconomicSectors.Add(econSector);
                                }
                            }
                        } 

                        return reportDto;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing Sectoral Lending data");
                throw;
            }
        }

        private static (string code, string name) ParseCodeAndName(string combined)
        {
            Regex CodeNameRegex = new Regex(@"^(?<code>\d+)\s+(?<name>.+)$", RegexOptions.Compiled);

            // "1100   Crop Farming" => code="1100", name="Crop Farming"
            if (string.IsNullOrWhiteSpace(combined))
                return (string.Empty, string.Empty);

            var match = CodeNameRegex.Match(combined);
            if (!match.Success)
            {
                // If it doesn't match, fallback to entire text as name
                return (string.Empty, combined.Trim());
            }
            string code = match.Groups["code"].Value.Trim();
            string name = match.Groups["name"].Value.Trim();
            return (code, name);
        }
        private static string ExtractEconomicSectorName(string combinedText)
        {
            if (string.IsNullOrWhiteSpace(combinedText))
                return string.Empty;

            // This regular expression matches a series of digits at the beginning,
            // followed by one or more whitespace characters, then captures the rest of the text.
            Regex regex = new Regex(@"^\d+\s+(?<name>.+)$");
            Match match = regex.Match(combinedText.Trim());
            if (match.Success)
            {
                return match.Groups["name"].Value.Trim();
            }
            return combinedText.Trim();
        }


        //NWDT 

        public static Form2AStatement ImportForm2ARows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx" )
                {
                    throw new ValidationException(
                                        $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                        "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                        "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        if (worksheet == null)
                        {
                            worksheet = workbook.Worksheets.First(); // Fallback
                            logger.LogWarning("Worksheet 'Capital Adequacy' not found, using first sheet");
                        }

                        var form2AStatement = new Form2AStatement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("D5")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D6"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D7"))) ?? DateTime.Now,
                        };

                        var rowsToUse = new List<CapitalAdequacyRow>();

                        int firstDataRow = 11;
                        int lastDataRow = 53;

                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            string index = row.Cell(2).GetString().Trim();  // Column B for index
                            string description = row.Cell(3).GetString().Trim(); // Column C for description
                            decimal? amount = GetDecimalOrNull(row.Cell(4)); // Column D for amount
                            string cellRef = row.Cell(4)?.Address.ToString() ?? string.Empty;

                            // Skip empty rows or section headers without indices
                            if (string.IsNullOrWhiteSpace(index) && string.IsNullOrWhiteSpace(description))
                                continue;

                            // Skip section headers like "LESS DEDUCTIONS"
                            if (string.IsNullOrWhiteSpace(index) &&
                                (description.StartsWith("LESS") || description == "OFF-BALANCE SHEET ASSETS" ||
                                 description == "CAPITAL RATIO CALCULATIONS" || description == "CAPITAL COMPONENTS" ||
                                 description == "ON - BALANCE SHEET ASSETS"))
                                continue;

                            // Add the row to our list
                            rowsToUse.Add(new CapitalAdequacyRow
                            {
                                Index = index,
                                Description = description,
                                Amount = amount,
                                CellNumberWithFigures = cellRef
                            });
                        }

                        form2AStatement.Rows = rowsToUse;

                        return form2AStatement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                     $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                     "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                     "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportForm2ARows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }

     

        private static List<SectoralLendingDataRow> ExtractSectoralData(IXLWorksheet worksheet, int headerRow)
        {
            var sectoralData = new List<SectoralLendingDataRow>();
            int firstDataRow = headerRow + 1;
            int lastRow = worksheet.LastRowUsed().RowNumber();

            string currentMainSectorCode = string.Empty;
            string currentSubSectorCode = string.Empty;

            for (int rowNum = firstDataRow; rowNum <= lastRow; rowNum++)
            {
                var row = worksheet.Row(rowNum);

                // Get cell values
                string code = GetCellValueOrEmpty(row.Cell(1)).Trim();
                string name = GetCellValueOrEmpty(row.Cell(2)).Trim();

                // For main sectors, the code is in column 1 and name is in column 2
                // For subsectors and details, both code and name are in column 2
                string codeCell = GetCellValueOrEmpty(row.Cell(1)).Trim();
                string nameOrCodeAndName = GetCellValueOrEmpty(row.Cell(2)).Trim();
                //decimal amount = (row.Cell(3));
                // Skip empty rows
                if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                // Determine the level and parent based on the code format
                var level = DetermineSectorLevel(code);
                string parentCode = null;

                if (level == SectorLevel.MainSector)
                {
                    currentMainSectorCode = code;
                    // Main sectors don't have parents
                }
                else if (level == SectorLevel.SubSector)
                {
                    currentSubSectorCode = code;
                    parentCode = currentMainSectorCode;
                }
                else if (level == SectorLevel.DetailItem)
                {
                    parentCode = currentSubSectorCode;
                }

                // Create and add the data dto
                var dataDto = new SectoralLendingDataRow
                {
                    SectorCode = code,
                    SectorName = name,
                    //Amount = (decimal)amount,
                    Level = level,
                    ParentCode = parentCode
                };

                sectoralData.Add(dataDto);
            }

            return sectoralData;
        }

        private static SectorLevel DetermineSectorLevel(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return SectorLevel.DetailItem; // Default to detail if code is empty
            }

            // Example: "1000" is a main sector, "1100" is a subsector, "1110" is a detail item
            if (code.Length == 4 && code.EndsWith("00"))
            {
                if (code.Substring(1, 3) == "000")
                {
                    return SectorLevel.MainSector; // e.g., 1000
                }
                else
                {
                    return SectorLevel.SubSector; // e.g., 1100, 1200
                }
            }
            else
            {
                return SectorLevel.DetailItem; // e.g., 1110, 1120
            }
        }

        private static int FindHeaderRow(IXLWorksheet worksheet)
        {
            for (int row = 1; row <= Math.Min(20, worksheet.LastRowUsed().RowNumber()); row++)
            {
                string cellA = GetCellValueOrEmpty(worksheet.Cell(row, 1)).ToUpper();
                string cellB = GetCellValueOrEmpty(worksheet.Cell(row, 2)).ToUpper();

                if (cellA.Contains("CODE") && (cellB.Contains("ECONOMIC") || cellB.Contains("SECTOR")))
                {
                    return row;
                }
            }
            return -1; // Not found
        }

        public static Form2FStatement ImportForm2FRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();

                        var form2FStatement = new Form2FStatement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("C5")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C7"))) ?? DateTime.Now,
                        };

                        var rowsToUse = new List<StatementOfComprehensiveIncomeRow>();

                        // Form 2F data starts at row 11 and goes to row 58
                        for (int rowNum = 11; rowNum <= 58; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            string refNum = row.Cell(1).GetString().Trim();
                            string description = row.Cell(2).GetString().Trim();
                            decimal? amount = GetDecimalOrNull(row.Cell(3));
                            string cellRef = row.Cell(3)?.Address.ToString() ?? string.Empty;

                            // Skip empty rows
                            if (string.IsNullOrWhiteSpace(refNum) && string.IsNullOrWhiteSpace(description))
                                continue;

                            // Add the row to our list
                            rowsToUse.Add(new StatementOfComprehensiveIncomeRow
                            {
                                RefNumber = refNum,
                                Description = description,
                                Amount = amount,
                                CellNumberWithFigures = cellRef,
                            });
                        }

                        form2FStatement.Rows = rowsToUse;

                        return form2FStatement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                 $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                 "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                 "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportForm2FRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }

        public static Form2BStatement ImportForm2BStatement(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("Importing Form 2B from file: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var rows = new List<LiquidityStatementRow>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        if (worksheet == null)
                        {
                            worksheet = workbook.Worksheets.First(); // Fallback to first sheet if named sheet not found
                        }
                        var form2B = new Form2BStatement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("D5")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D6"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D7"))) ?? DateTime.Now,
                        };
                        // Extract metadata
                        form2B.SaccoName = GetCellValueOrEmpty(worksheet.Cell("D3"));
                        form2B.CsNumber = GetCellValueOrEmpty(worksheet.Cell("D4"));

                        int firstDataRow = 9; // Data starts from row 9 (where amounts begin)
                        int lastDataRow = 43; // Adjust based on the form

                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);

                            // For Liquidity Statement: Column B is Index, C is Description, D is Amount
                            string indexOrCode = row.Cell(2).GetString().Trim();    // Column B
                            string description = row.Cell(3).GetString().Trim();    // Column C
                            decimal? amount = GetDecimalOrNull(row.Cell(4));        // Column D
                            string cellNumberWithFigures = row.Cell(4)?.Address.ToString() ?? string.Empty;

                            // Skip empty rows but include rows that might only have descriptions
                            if (!string.IsNullOrWhiteSpace(indexOrCode) || !string.IsNullOrWhiteSpace(description))
                            {
                                var item = new LiquidityStatementRow
                                {
                                    Index = indexOrCode,
                                    Description = description,
                                    Amount = amount,
                                    CellNumberWithFigures = cellNumberWithFigures
                                };
                                rows.Add(item);
                            }
                        }
                        form2B.Rows = rows;
                        return form2B;

                    }


                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                   $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                   "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                   "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Form 2B Liquidity Statement");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }

        public static Form1Statement ImportCapitalAdequacyRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                // var path = @"C:\Projects\SASRA\Code\src\SASRAXRBSS.Application.Shared\ExcelUpload\CapitalAdequency.xlsx";
                using (var stream = new MemoryStream())
                {
                    // Copy the file to a memory stream
                    logger.LogInformation("Copying file to memory stream");
                    file.CopyTo(stream);
                    // This list will hold all the rows we read
                    var rowDataList = new List<CapitalAdequacyRow>();

                    // Open the Excel workbook
                    using (var workbook = new XLWorkbook(stream))
                    {

                        logger.LogInformation("Workbook opened");
                        // Get the first worksheet by name (or use .Worksheets.First() if you prefer)
                        var worksheet = workbook.Worksheets.First();
                        var form1Statement = new Form1Statement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("D4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D6"))) ?? DateTime.Now,
                        };

                        int firstDataRow = 10;
                        int lastDataRow = 53;

                        var lastRow = worksheet.LastRowUsed();
                        int lastRowNumber = lastRow.RowNumber();

                        // Iterate from the row after the header to the last row
                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            // Read the row
                            var row = worksheet.Row(rowNum);

                            // Extract cells
                            string indexOrCode = row.Cell(2).GetString().Trim(); // Column B
                            string description = row.Cell(3).GetString().Trim(); // Column C
                            decimal? amount = GetDecimalOrNull(row.Cell(4));   // Column D
                            string cellNumberWithFigures = row.Cell(4)?.Address.ToString() ?? string.Empty;

                            var rowData = new CapitalAdequacyRow();

                            // skip blank can go to hell
                            if (!string.IsNullOrWhiteSpace(indexOrCode) || !string.IsNullOrWhiteSpace(description))
                            {
                                var item = new CapitalAdequacyRow
                                {
                                    Index = indexOrCode,
                                    Description = description,
                                    Amount = amount,
                                    CellNumberWithFigures = cellNumberWithFigures
                                };

                                rowDataList.Add(item);

                            }
                        }
                        form1Statement.Rows = rowDataList;

                        return form1Statement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
                //new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
                //new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException ex)
            {
                logger.LogError(ex, "Invalid file format");
                throw new ValidationException(
                    $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                    "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                    "Please save the sheet in .xlsx format and upload again.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportCapitalAdequacyRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }


        }

        public static Form3Statement ImportDepositRangeDataRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException($"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " + "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " + "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var depositRangeDataList = new List<DepositRangeData>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();

                        // So data might start at row 9
                        int firstDataRow = 9;

                        // Figure out the last row with data
                        int lastDataRow = 26; //worksheet.LastRowUsed().RowNumber();

                        Form3Statement form3Statement = new Form3Statement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("E4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("E5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("E6"))) ?? DateTime.Now,
                        };

                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);

                            // Column A -> RangeName
                            string rangeName = row.Cell(2).GetString().Trim();

                            // Column C -> DepositType (we skip B, which is Cell(2))
                            string depositType = row.Cell(3).GetString().Trim();

                            // Column D -> NumberOfAccounts
                            int numberOfAccounts = GetIntOrNull(row.Cell(4)) ?? 0;

                            // Column E -> AmountInKshs000
                            decimal amountInKshs000 = GetDecimalOrNull(row.Cell(5)) ?? 0;

                            string cellNumberWithFigures = row.Cell(5)?.Address.ToString() ?? string.Empty;


                            // Skip rows that are completely empty (no range & no deposit type)
                            if (string.IsNullOrEmpty(rangeName) && string.IsNullOrEmpty(depositType))
                            {
                                continue;

                            }

                            depositRangeDataList.Add(new DepositRangeData
                            {
                                RangeName = rangeName,
                                DepositType = depositType,
                                NumberOfAccounts = numberOfAccounts,
                                AmountInKshs000 = amountInKshs000,
                                CellNumber = cellNumberWithFigures
                            });
                        }

                        form3Statement.Rows = depositRangeDataList;
                        return form3Statement;

                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                logger.LogError(ex, "Invalid file format");
                throw new ValidationException(
                    $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                    "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                    "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Deposit Range Data import");
                throw; // new Exception($"Error processing Excel file '{file.FileName}': {ex.Message}", ex);
            }
        }


        public static Form2CStatement ImportForm2CDataRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException($"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " + "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " + "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var depositRangeDataList = new List<DepositRangeData>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var rows = new List<DepositRangeRow>();

                        var worksheet = workbook.Worksheets.First();
                        var form2C = new Form2CStatement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("D5")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D6"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D7"))) ?? DateTime.Now,
                        };


                        // So data might start at row 9
                        int firstDataRow = 10;

                        // Figure out the last row with data
                        int lastDataRow = 19; // Last data row before totals

                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);

                            if (IsEmptyRow(row, 1, 5))
                            {
                                continue;
                            }
                            string number = row.Cell(1).GetString().Trim();
                            string range = row.Cell(2).GetString().Trim();
                            string depositType = row.Cell(3).GetString().Trim();

                            // Only process rows that have either a range or deposit type
                            if (!string.IsNullOrWhiteSpace(range) || !string.IsNullOrWhiteSpace(depositType))
                            {
                                // Skip the totals row
                                if (depositType.Equals("TOTAL", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                var accountsCell = row.Cell(4);
                                var amountCell = row.Cell(5);

                                int numberOfAccounts = GetIntOrNull(accountsCell).Value;
                                decimal amount = GetIntOrNull(amountCell).Value;

                                var item = new DepositRangeRow
                                {
                                    Number = number,
                                    Range = range,
                                    DepositType = depositType,
                                    NumberOfAccounts = numberOfAccounts,
                                    Amount = amount,
                                    CellNumberWithAccounts = accountsCell.Address.ToString(),
                                    CellNumberWithAmount = amountCell.Address.ToString()
                                };

                                rows.Add(item);
                            }
                        }
                        form2C.Rows = rows;

                        return form2C;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                       $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                       "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                       "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Deposit Range Data import");
                throw;
                //new Exception($"Error processing Excel file '{file.FileName}': {ex.Message}", ex);
            }
        }




        public static Form7Statement ImportStatementOfComprehensiveIncomeRows(
            IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx" )
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var rowDataList = new List<StatementOfComprehensiveIncomeRow>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        int firstDataRow = 10;  // Data starts from row 10
                        int lastDataRow = 56;   // Last data row is at row 56

                        Form7Statement form5Statement = new Form7Statement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("C4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))) ?? DateTime.Now,
                        };

                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            string refNumber = row.Cell("A").GetString().Trim();
                            string description = row.Cell("B").GetString().Trim();
                            decimal? amount = GetDecimalOrNull(row.Cell("C"));
                            string cellNumberWithFigures = row.Cell("C")?.Address.ToString() ?? string.Empty;


                            // Skip empty rows and section headers
                            if (!string.IsNullOrWhiteSpace(refNumber) || !string.IsNullOrWhiteSpace(description))
                            {
                                var item = new StatementOfComprehensiveIncomeRow
                                {
                                    RefNumber = refNumber,
                                    Description = description,
                                    Amount = amount,
                                    CellNumberWithFigures = cellNumberWithFigures,
                                };
                                rowDataList.Add(item);
                            }
                        }

                        form5Statement.Rows = rowDataList;
                        return form5Statement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                logger.LogError(ex, "Invalid file format");
                throw new ValidationException(
                    $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                    "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                    "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportStatementOfComprehensiveIncomeRows");
                throw; /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }





        public static Form2Statement ImportLiquidityStatementRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var rowDataList = new List<LiquidityStatementRow>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();

                        // Parse the metadata
                        var form2Statement = new Form2Statement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("D4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("D6"))) ?? DateTime.Now,
                        };

                        // Liquidity statement typically starts from row 7 and goes to row 52
                        int firstDataRow = 7;
                        int lastDataRow = 52;

                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);

                            // For Liquidity Statement: Column B is Index, C is Description, D is Amount
                            string indexOrCode = row.Cell(2).GetString().Trim();    // Column B
                            string description = row.Cell(3).GetString().Trim();    // Column C
                            decimal? amount = GetDecimalOrNull(row.Cell(4));        // Column D
                            string cellNumberWithFigures = row.Cell(4)?.Address.ToString() ?? string.Empty;

                            // Skip empty rows but include rows that might only have descriptions
                            if (!string.IsNullOrWhiteSpace(indexOrCode) || !string.IsNullOrWhiteSpace(description))
                            {
                                var item = new LiquidityStatementRow
                                {
                                    Index = indexOrCode,
                                    Description = description,
                                    Amount = amount,
                                    CellNumberWithFigures = cellNumberWithFigures
                                };
                                rowDataList.Add(item);
                            }

                        }
                        form2Statement.Rows = rowDataList;
                        return form2Statement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                    throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                      $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                      "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                      "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportLiquidityStatementRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }


        }

        public static Form4Statement ImportRiskClassificationRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var rowDataList = new List<RiskClassificationRow>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();

                        // Parse the metadata   
                        var form4Statement = new Form4Statement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("F4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("F5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("F6"))) ?? DateTime.Now,
                        };

                        // Parse regular loans (rows 10-14)
                        for (int rowNum = 10; rowNum <= 14; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            // Capture the outstanding loan portfolio cell (Column D)
                            var outstandingCell = row.Cell(4);

                            var asset = new RiskClassificationRow
                            {
                                LoanType = "Regular",
                                Classification = row.Cell(2).GetString(), // Column B
                                NumberOfAccounts = GetIntOrNull(row.Cell(3)), // Column C
                                OutstandingLoanPortfolio = GetDecimalOrNull(outstandingCell), // Column D
                                OutstandingLoanPortfolioCellAddress = outstandingCell.Address.ToString() ?? string.Empty,
                                RequiredProvision = GetDecimalOrNull(row.Cell(5)), // Column E
                                RequiredProvisionAmount = GetDecimalOrNull(row.Cell(6)) // Column F
                            };
                            rowDataList.Add(asset);
                        }

                        // Parse rescheduled/renegotiated loans (rows 17-21)
                        for (int rowNum = 17; rowNum <= 21; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            var outstandingCell = row.Cell(4);

                            var asset = new RiskClassificationRow
                            {
                                LoanType = "Rescheduled/Renegotiated",
                                Classification = row.Cell(2).GetString(), // Column B
                                NumberOfAccounts = GetIntOrNull(row.Cell(3)), // Column C
                                OutstandingLoanPortfolio = GetDecimalOrNull(outstandingCell), // Column D
                                OutstandingLoanPortfolioCellAddress = outstandingCell.Address.ToString() ?? string.Empty,
                                RequiredProvision = GetDecimalOrNull(row.Cell(5)), // Column E
                                RequiredProvisionAmount = GetDecimalOrNull(row.Cell(6)) // Column F
                            };
                            rowDataList.Add(asset);
                        }

                        // Get the Totals. They Start at row 23. If they chage this we are doomed.
                        {
                            var row = worksheet.Row(23);
                            var outstandingCell = row.Cell(4);

                            var totalsRow = new RiskClassificationRow
                            {
                                LoanType = "Total",
                                Classification = row.Cell(2).GetString(), // e.g., "Total"
                                NumberOfAccounts = GetIntOrNull(row.Cell(3)),
                                OutstandingLoanPortfolio = GetDecimalOrNull(outstandingCell),
                                OutstandingLoanPortfolioCellAddress = outstandingCell.Address.ToString() ?? string.Empty,
                                RequiredProvision = GetDecimalOrNull(row.Cell(5)),
                                RequiredProvisionAmount = GetDecimalOrNull(row.Cell(6))
                            };
                            rowDataList.Add(totalsRow);
                        }

                        form4Statement.Rows = rowDataList;
                        return form4Statement;

                    }

                }

            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportRiskClassificationRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }

        }


        public static Form2DStatement ImportForm2DRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("Importing Form 2D from file: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var rowDataList = new List<RiskClassificationRow>();


                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        if (worksheet == null)
                        {
                            worksheet = workbook.Worksheets.First(); // Fallback to first sheet if named sheet not found
                            logger.LogWarning("Named worksheet 'Sheet1' not found, using first sheet instead");
                        }
                        var form2D = new Form2DStatement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("F5")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("F6"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("F7"))) ?? DateTime.Now,
                        };

                        // Extract metadata
                        form2D.SaccoName = GetCellValueOrEmpty(worksheet.Cell("C3"));
                        form2D.CsNumber = GetCellValueOrEmpty(worksheet.Cell("C4"));
                        form2D.FinancialYear = GetCellValueOrEmpty(worksheet.Cell("C5"));


                        for (int rowNum = 11; rowNum <= 15; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            var outstandingCell = row.Cell(4);

                            var asset = new RiskClassificationRow
                            {
                                LoanType = "Regular",
                                Classification = row.Cell(2).GetString(), // Column B
                                NumberOfAccounts = GetIntOrNull(row.Cell(3)), // Column C
                                OutstandingLoanPortfolio = GetDecimalOrNull(outstandingCell), // Column D
                                OutstandingLoanPortfolioCellAddress = outstandingCell.Address.ToString() ?? string.Empty,
                                RequiredProvision = GetDecimalOrNull(row.Cell(5)), // Column E
                                RequiredProvisionAmount = GetDecimalOrNull(row.Cell(6)) // Column F
                            };
                            rowDataList.Add(asset);
                        }

                        // Parse rescheduled/renegotiated loans (rows 18-22)
                        for (int rowNum = 18; rowNum <= 22; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            var outstandingCell = row.Cell(4);

                            var asset = new RiskClassificationRow
                            {
                                LoanType = "Rescheduled/Renegotiated",
                                Classification = row.Cell(2).GetString(), // Column B
                                NumberOfAccounts = GetIntOrNull(row.Cell(3)), // Column C
                                OutstandingLoanPortfolio = GetDecimalOrNull(outstandingCell), // Column D
                                OutstandingLoanPortfolioCellAddress = outstandingCell.Address.ToString() ?? string.Empty,
                                RequiredProvision = GetDecimalOrNull(row.Cell(5)), // Column E
                                RequiredProvisionAmount = GetDecimalOrNull(row.Cell(6)) // Column F
                            };
                            rowDataList.Add(asset);
                        }

                        {
                            var row = worksheet.Row(24);
                            var outstandingCell = row.Cell(4);
                            var totalsRow = new RiskClassificationRow
                            {
                                LoanType = "Total",
                                Classification = row.Cell(2).GetString(), // e.g., "GRAND TOTAL"
                                NumberOfAccounts = GetIntOrNull(row.Cell(3)),
                                OutstandingLoanPortfolio = GetDecimalOrNull(outstandingCell),
                                OutstandingLoanPortfolioCellAddress = outstandingCell.Address.ToString() ?? string.Empty,
                                RequiredProvision = GetDecimalOrNull(row.Cell(5)),
                                RequiredProvisionAmount = GetDecimalOrNull(row.Cell(6))
                            };
                            rowDataList.Add(totalsRow);
                        }

                        form2D.Rows = rowDataList;
                        return form2D;

                    }


                }

            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                    throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                                        $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                        "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                        "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportRiskClassificationRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }

        }

        public static Form5Statement ImportInvestmentRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var rowDataList = new List<InvestmentRow>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        int firstDataRow = 7;
                        int lastDataRow = 27;

                        Form5Statement form5Statement = new Form5Statement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("C4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))) ?? DateTime.Now,
                        };

                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            string index = row.Cell(1).GetString().Trim();
                            string description = row.Cell(2).GetString().Trim();
                            decimal? amount = GetDecimalOrNull(row.Cell(3));
                            string cellNumberWithFigures = row.Cell(3)?.Address.ToString() ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(index) || !string.IsNullOrWhiteSpace(description))
                            {
                                rowDataList.Add(new InvestmentRow
                                {
                                    Index = index,
                                    Description = description,
                                    Amount = amount,
                                    cellNumberWithFigures = cellNumberWithFigures
                                });
                            }
                        }

                        form5Statement.Rows = rowDataList;
                        return form5Statement;
                    }
                }
            }

            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                     $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                     "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                     "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportInvestmentRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }

        public static Form2EStatement ImportForm2ERows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx" )
                {
                    throw new ValidationException(
                                                            $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                                                            "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                                                            "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);


                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        int firstDataRow = 8;
                        int lastDataRow = 41;
                        var statement = new Form2EStatement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("C4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))) ?? DateTime.Now,
                        };
                        var rowDataList = new List<Form2ERow>();
                        for (int rowNum = firstDataRow; rowNum <= lastDataRow; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            string index = row.Cell(1).GetString().Trim();
                            string description = row.Cell(2).GetString().Trim();
                            decimal? amount = GetDecimalOrNull(row.Cell(3));
                            string cellNumberWithFigures = row.Cell(3)?.Address.ToString() ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(index) || !string.IsNullOrWhiteSpace(description))
                            {
                                rowDataList.Add(new Form2ERow
                                {
                                    Index = index,
                                    Description = description,
                                    Amount = amount,
                                    CellReference = cellNumberWithFigures
                                });
                            }
                        }

                        statement.Rows = rowDataList;
                        return statement;
                    }

                }
            }

            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                     $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                     "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                     "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportInvestmentRows");
                throw; /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }


        }

        public static Form6Statement ImportFinancialPositionRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx" )
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var RowsToUse = new List<StatementOfFinancialPositionRow>();

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        int firstDataRow = 10;  // First data row starts at row 10
                        int lastDataRow = 72;   // Last data row is at row 72

                        Form6Statement form6Statement = new Form6Statement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("C4")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C5"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))) ?? DateTime.Now,
                        };

                        var ExtractedRows = worksheet.Rows(firstDataRow, lastDataRow)
                            .Select(row => new
                            {
                                Index = row.Cell(1).GetString().Trim(),
                                Description = row.Cell(2).GetString().Trim(),
                                Amount = GetDecimalOrNull(row.Cell(3)),
                                CellNumberWithFigures = row.Cell(3)?.Address.ToString() ?? string.Empty
                            });

                        var validRows = ExtractedRows
                            .Where(x =>
                            {
                                // A row is considered valid if it has a non-empty index
                                bool hasValidIndex = !string.IsNullOrWhiteSpace(x.Index);

                                // Or if it has a description that isn't empty and isn't a section header.
                                bool hasValidDescription = !string.IsNullOrWhiteSpace(x.Description) &&
                                                           x.Description != "ASSETS" &&
                                                           x.Description != "LIABILITIES" &&
                                                           x.Description != "EQUITY";

                                // Return true if either condition is met.
                                return hasValidIndex || hasValidDescription;
                            })
                            .ToList();

                        RowsToUse = validRows
                        .Select(x => new StatementOfFinancialPositionRow
                        {
                            RefNumber = x.Index,
                            Description = x.Description,
                            Amount = x.Amount,
                            CellNumberWithFigures = x.CellNumberWithFigures
                        })
                        .ToList();

                        form6Statement.Rows = RowsToUse;
                        return form6Statement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {
                logger.LogError(ex, "Invalid file format");
                throw new ValidationException(
                     $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                     "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                     "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportFinancialPositionRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}", ex);*/
            }
        }

        public static Form2GStatement ImportForm2GRows(IFormFile file, ILogger logger)
        {
            try
            {
                logger.LogInformation("File Name: " + file.FileName);
                if (file == null)
                {
                    logger.LogError("File is null");
                    throw new ArgumentNullException(nameof(file), "No file was provided for processing");
                }

                if (file.Length == 0)
                {
                    throw new ArgumentException("The uploaded file is empty", nameof(file));
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                   throw new ValidationException(
                     $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                     "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                     "Please save the sheet in .xlsx format and upload again.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);


                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();

                        var Form2GStatement = new Form2GStatement
                        {
                            Period = GetCellValueOrEmpty(worksheet.Cell("C5")),
                            StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C6"))) ?? DateTime.Now,
                            EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell("C7"))) ?? DateTime.Now,
                        };

                        var RowsToUse = new List<StatementOfFinancialPositionRow>();

                        for (int rowNum = 10; rowNum <= 75; rowNum++)
                        {
                            var row = worksheet.Row(rowNum);
                            string refNum = row.Cell(1).GetString().Trim();
                            string description = row.Cell(2).GetString().Trim();
                            decimal? amount = GetDecimalOrNull(row.Cell(3));
                            string cellRef = row.Cell(3)?.Address.ToString() ?? string.Empty;

                            // Skip empty rows or section headers
                            if (string.IsNullOrWhiteSpace(refNum) && string.IsNullOrWhiteSpace(description))
                                continue;

                            if ((string.IsNullOrWhiteSpace(refNum) && description == "ASSETS") ||
                                (string.IsNullOrWhiteSpace(refNum) && description == "LIABILITIES") ||
                                (string.IsNullOrWhiteSpace(refNum) && description == "EQUITY"))
                                continue;

                            // Add the row to our list
                            RowsToUse.Add(new StatementOfFinancialPositionRow
                            {
                                RefNumber = refNum,
                                Description = description,
                                Amount = amount,
                                CellNumberWithFigures = cellRef,
                            });
                        }

                        Form2GStatement.Rows = RowsToUse;

                        return Form2GStatement;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No file was provided for processing");
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw;
            }
            catch (FileFormatException ex)
            {

                logger.LogError(ex, "Invalid file format");
                throw new ValidationException(
                    $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                    "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                    "Please save the sheet in .xlsx format and upload again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportFinancialPositionRows");
                throw;
                /*new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");*/
            }
        }



        private static decimal? GetDecimalOrNull(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return 0;

            // Attempt to parse cell value as decimal
            if (decimal.TryParse(cell.GetString().Trim(), out decimal result))
                return result;

            return 0;
        }

        private static int? GetIntOrNull(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return 0;

            if (int.TryParse(cell.GetString().Trim(), out int result))
                return result;

            return 0;
        }

        private static string GetCellValueOrEmpty(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return string.Empty;

            return cell.GetString().Trim();
        }

        private static DateTime? ParseDateOrNull(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParse(dateString, out DateTime result))
                return result;

            return null;
        }

        private static bool IsEmptyRow(IXLRow row, int startColumn, int endColumn)
        {
            for (int col = startColumn; col <= endColumn; col++)
            {
                if (!string.IsNullOrWhiteSpace(row.Cell(col).GetString()))
                    return false;
            }
            return true;
        }

    }
}

