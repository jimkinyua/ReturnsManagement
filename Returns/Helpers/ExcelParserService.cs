using Returns.Helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Returns.Models;
using static Returns.Helpers.ExcelService;
using System.ComponentModel.DataAnnotations;

namespace Returns.Helpers
{
    public class ExcelParserService : IExcelParser
    {
        private readonly ILogger<ExcelParserService> _logger;
        private readonly ReturnsDbContext _context;

        public ExcelParserService(ILogger<ExcelParserService> logger, ReturnsDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<ExcelParseResult> ParseAsync(IFormFile file, string formCode)
        {
            var result = new ExcelParseResult();
            
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    result.Success = false;
                    result.Errors.Add("File is required and cannot be empty.");
                    return result;
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    result.Success = false;
                    result.Errors.Add($"'{file.FileName}' is an *.xls* (Excel 97-2003) file. The system only accepts *.xlsx* workbooks (Excel 2007 or later).");
                    return result;
                }

                // Parse based on form code
                switch (formCode.ToUpper())
                {
                    case "FORM1":
                    case "CAPITALADEQUACY":
                    case "DT_CAPITAL_ADEQUACY":
                        return await ParseCapitalAdequacy(file);

                    case "FORM2":
                    case "LIQUIDITY":
                    case "DT_LIQUIDITY":
                        return await ParseLiquidity(file);

                    case "FORM3":
                    case "DEPOSITRETURN":
                    case "DT_DEPOSIT":
                        return await ParseDepositReturn(file);

                    case "FORM4":
                    case "RISKCLASSIFICATION":
                    case "DT_RISK":
                        return await ParseRiskClassification(file);

                    case "FORM5":
                    case "INVESTMENT":
                    case "DT_INVESTMENT":
                        return await ParseInvestment(file);

                    case "FORM6":
                    case "FINANCIALPOSITION":
                    case "DT_FINANCIAL_POSITION":
                        return await ParseFinancialPosition(file);

                    case "FORM7":
                    case "COMPREHENSIVEINCOME":
                    case "DT_COMPREHENSIVE_INCOME":
                        return await ParseComprehensiveIncome(file);

                    // NWDT Forms
                    case "FORM2A":
                    case "NWDT_CAPITAL_ADEQUACY":
                        return await ParseNWDTCapitalAdequacy(file);

                    case "FORM2B":
                    case "NWDT_LIQUIDITY":
                        return await ParseNWDTLiquidity(file);

                    case "FORM2C":
                    case "NWDT_DEPOSIT":
                        return await ParseNWDTDeposit(file);

                    case "FORM2D":
                    case "NWDT_RISK":
                        return await ParseNWDTRiskClassification(file);

                    case "FORM2E":
                    case "NWDT_INVESTMENT":
                        return await ParseNWDTInvestment(file);

                    case "FORM2F":
                    case "NWDT_COMPREHENSIVE_INCOME":
                        return await ParseNWDTComprehensiveIncome(file);

                    case "FORM2G":
                    case "NWDT_FINANCIAL_POSITION":
                        return await ParseNWDTFinancialPosition(file);

                    case "MANAGEMENT":
                        return await ParseManagementReturn(file);

                    case "SECTORAL_LENDING":
                        return await ParseSectoralLending(file);

                    case "DAILY_LIQUIDITY":
                        return await ParseDailyLiquidity(file);

                    case "INSIDER_LENDING":
                        return await ParseInsiderLending(file);

                    default:
                        result.Success = false;
                        result.Errors.Add($"Unknown form code: {formCode}");
                        return result;
                }
            }
            catch (ValidationException ex)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing form {formCode}");
                result.Success = false;
                result.Errors.Add($"Error processing file: {ex.Message}");
                return result;
            }
        }

        private async Task<ExcelParseResult> ParseCapitalAdequacy(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportCapitalAdequacyRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Capital Adequacy Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DTCapitalAdequacy";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    // Convert to parsed row
                    var parsedRow = new CapitalAdequacyParsedRow
                    {
                        Data = data
                    };
                    result.Rows.Add(parsedRow);

                    return result;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add(ex.Message);
                    return result;
                }
            });
        }

        private async Task<ExcelParseResult> ParseLiquidity(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportLiquidityStatementRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Liquidity Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DTLiquidity";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new LiquidityParsedRow
                    {
                        Data = data
                    };
                    result.Rows.Add(parsedRow);

                    return result;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add(ex.Message);
                    return result;
                }
            });
        }

        // Similar methods for other form types...
        // I'll implement a few more as examples

        private async Task<ExcelParseResult> ParseDepositReturn(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportDepositRangeDataRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Deposit Return Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DTDepositReturn";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new DepositReturnParsedRow
                    {
                        Data = data
                    };
                    result.Rows.Add(parsedRow);

                    return result;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add(ex.Message);
                    return result;
                }
            });
        }

        // Add other parsing methods as needed...

        private async Task<ExcelParseResult> ParseNWDTCapitalAdequacy(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportForm2ARows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in NWDT Capital Adequacy Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "NWDTCapitalAdequacy";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new NWDTCapitalAdequacyParsedRow
                    {
                        Data = data
                    };
                    result.Rows.Add(parsedRow);

                    return result;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add(ex.Message);
                    return result;
                }
            });
        }

        // Placeholder methods for remaining form types
        private async Task<ExcelParseResult> ParseRiskClassification(IFormFile file) => await ParseNotImplemented("Risk Classification");
        private async Task<ExcelParseResult> ParseInvestment(IFormFile file) => await ParseNotImplemented("Investment");
        private async Task<ExcelParseResult> ParseFinancialPosition(IFormFile file) => await ParseNotImplemented("Financial Position");
        private async Task<ExcelParseResult> ParseComprehensiveIncome(IFormFile file) => await ParseNotImplemented("Comprehensive Income");
        private async Task<ExcelParseResult> ParseNWDTLiquidity(IFormFile file) => await ParseNotImplemented("NWDT Liquidity");
        private async Task<ExcelParseResult> ParseNWDTDeposit(IFormFile file) => await ParseNotImplemented("NWDT Deposit");
        private async Task<ExcelParseResult> ParseNWDTRiskClassification(IFormFile file) => await ParseNotImplemented("NWDT Risk Classification");
        private async Task<ExcelParseResult> ParseNWDTInvestment(IFormFile file) => await ParseNotImplemented("NWDT Investment");
        private async Task<ExcelParseResult> ParseNWDTComprehensiveIncome(IFormFile file) => await ParseNotImplemented("NWDT Comprehensive Income");
        private async Task<ExcelParseResult> ParseNWDTFinancialPosition(IFormFile file) => await ParseNotImplemented("NWDT Financial Position");
        private async Task<ExcelParseResult> ParseManagementReturn(IFormFile file) => await ParseNotImplemented("Management");
        private async Task<ExcelParseResult> ParseSectoralLending(IFormFile file) => await ParseNotImplemented("Sectoral Lending");
        private async Task<ExcelParseResult> ParseDailyLiquidity(IFormFile file) => await ParseNotImplemented("Daily Liquidity");
        private async Task<ExcelParseResult> ParseInsiderLending(IFormFile file) => await ParseNotImplemented("Insider Lending");

        private async Task<ExcelParseResult> ParseNotImplemented(string formType)
        {
            return await Task.FromResult(new ExcelParseResult
            {
                Success = false,
                Errors = new List<string> { $"{formType} parsing not yet implemented" }
            });
        }
    }
}