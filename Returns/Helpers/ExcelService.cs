using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using static Returns.Helpers.ExcelService.Form2CStatement;
using System;
using Microsoft.IdentityModel.Tokens;
using static Returns.Helpers.ExcelService;

namespace Returns.Helpers
{
    public class ExcelService
    {
        private readonly ILogger<ExcelService> _logger;
        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger;
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportForm2ARows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
            }
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportForm2FRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Form 2B Liquidity Statement");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
                throw new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportCapitalAdequacyRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
            catch (FileFormatException)
            {
                throw new FileFormatException("The Excel file appears to be corrupted or is not a valid Excel file.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Deposit Range Data import");
                throw new Exception($"Error processing Excel file '{file.FileName}': {ex.Message}", ex);
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
            catch (FileFormatException)
            {
                throw new FileFormatException("The Excel file appears to be corrupted or is not a valid Excel file.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Deposit Range Data import");
                throw new Exception($"Error processing Excel file '{file.FileName}': {ex.Message}", ex);
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
                throw new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportStatementOfComprehensiveIncomeRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
                throw new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportLiquidityStatementRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
                throw new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportRiskClassificationRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
                throw new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportRiskClassificationRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
                throw new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportInvestmentRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
                throw new ArgumentNullException("No file was provided for processing", ex);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid file type or empty file");
                throw new ArgumentException("Invalid file type or empty file", ex);
            }
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportInvestmentRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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
                if (extension != ".xlsx" && extension != ".xls")
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
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportFinancialPositionRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}", ex);
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
                if (extension != ".xlsx" && extension != ".xls")
                {
                    throw new ArgumentException($"Invalid file type. Expected .xlsx or .xls, got {extension}", nameof(file));
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
            catch (FileFormatException)
            {
                throw new FileFormatException(
                    $"The file '{file.FileName}' appears to be corrupted or is not a valid Excel file. " +
                    "Please ensure you're uploading a valid Excel workbook.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ImportFinancialPositionRows");
                throw new Exception(
                    $"Error processing Excel file '{file.FileName}': {ex.Message}");
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

