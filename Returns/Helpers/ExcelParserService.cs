using Returns.Helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Returns.Models;
using static Returns.Helpers.ExcelService;
using System.ComponentModel.DataAnnotations;
using Returns.Models.Data;
using Returns.Helpers.Enums;
using static Returns.Helpers.Constants;

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

        public async Task<ExcelParseResult> ParseAsync(IFormFile file, FormCategory formCategory, string SaccoType)
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

                switch (SaccoType)
                {
                    case Constants.SaccoType.DepositTaking:
                        switch (formCategory)
                        {
                            case FormCategory.CapitalAdequacy:
                                return await ParseCapitalAdequacy(file);
                            case FormCategory.LiquidityStatement:
                                return await ParseLiquidity(file);
                            case FormCategory.DepositReturn:
                                return await ParseDepositReturn(file);
                            case FormCategory.RiskClassification:
                                return await ParseRiskClassification(file);
                            case FormCategory.InvestmentReturn:
                                return await ParseInvestment(file);
                            case FormCategory.FinancialPosition:
                                return await ParseFinancialPosition(file);
                            case FormCategory.StatementOfComprehensiveIncome:
                                return await ParseComprehensiveIncome(file);
                       /*     case FormCategory.Management:
                                return await ParseManagementReturn(file);
                            case FormCategory.SectoralLending:
                                return await ParseSectoralLending(file, SaccoType);
                            case FormCategory.DailyLiquidity:
                                return await ParseDailyLiquidity(file, SaccoType);
                            case FormCategory.InsiderLending:
                                return await ParseInsiderLending(file, SaccoType);*/
                            default:
                                result.Success = false;
                                result.Errors.Add($"Unknown form category for DepositTaking: {formCategory}");
                                return result;
                        }

                    case Constants.SaccoType.NWDT:
                        switch (formCategory)
                        {
                            case FormCategory.CapitalAdequacy:
                                return await ParseNWDTCapitalAdequacy(file);
                            case FormCategory.LiquidityStatement:
                                return await ParseNWDTLiquidity(file);
                            case FormCategory.DepositReturn:
                                return await ParseNWDTDeposit(file);
                            case FormCategory.RiskClassification:
                                return await ParseNWDTRiskClassification(file);
                            case FormCategory.InvestmentReturn:
                                return await ParseNWDTInvestment(file);
                            case FormCategory.FinancialPosition:
                                return await ParseNWDTFinancialPosition(file);
                            case FormCategory.StatementOfComprehensiveIncome:
                                return await ParseNWDTComprehensiveIncome(file);
                         /*   case FormCategory.Management:
                                return await ParseManagementReturn(file);
                            case FormCategory.SectoralLending:
                                return await ParseSectoralLending(file, SaccoType);
                            case FormCategory.DailyLiquidity:
                                return await ParseDailyLiquidity(file, SaccoType);
                            case FormCategory.InsiderLending:
                                return await ParseInsiderLending(file, SaccoType);*/
                            default:
                                result.Success = false;
                                result.Errors.Add($"Unknown form category for NonDepositTaking: {formCategory}");
                                return result;
                        }
                    default:
                        result.Success = false;
                        result.Errors.Add($"Unknown sacco type: {SaccoType}");
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
                _logger.LogError(ex, $"Error parsing form {formCategory}");
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
        private async Task<ExcelParseResult> ParseRiskClassification(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportRiskClassificationRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Risk Classification Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DTRiskClassification";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new RiskClassificationParsedRow
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

        private async Task<ExcelParseResult> ParseInvestment(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportInvestmentRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Investment Return Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DTInvestment";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new InvestmentParsedRow
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

        private async Task<ExcelParseResult> ParseFinancialPosition(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportFinancialPositionRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Financial Position Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DTFinancialPosition";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new FinancialPositionParsedRow
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

        private async Task<ExcelParseResult> ParseComprehensiveIncome(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportStatementOfComprehensiveIncomeRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Comprehensive Income Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DTComprehensiveIncome";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new ComprehensiveIncomeParsedRow
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

        private async Task<ExcelParseResult> ParseNWDTLiquidity(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportForm2BStatement(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in NWDT Liquidity Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "NWDTLiquidity";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new NWDTLiquidityParsedRow
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

        private async Task<ExcelParseResult> ParseNWDTDeposit(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportForm2CDataRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in NWDT Deposit Return Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "NWDTDepositReturn";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new NWDTDepositParsedRow
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

        private async Task<ExcelParseResult> ParseNWDTRiskClassification(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportForm2DRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in NWDT Risk Classification Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "NWDTRiskClassification";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.CsNumber;

                    var parsedRow = new NWDTRiskClassificationParsedRow
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

        private async Task<ExcelParseResult> ParseNWDTInvestment(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportForm2ERows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in NWDT Investment Return Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "NWDTInvestment";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new NWDTInvestmentParsedRow
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

        private async Task<ExcelParseResult> ParseNWDTComprehensiveIncome(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportForm2FRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in NWDT Comprehensive Income Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "NWDTComprehensiveIncome";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new NWDTComprehensiveIncomeParsedRow
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

        private async Task<ExcelParseResult> ParseNWDTFinancialPosition(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportForm2GRows(file, _logger);
                    if (data == null || !data.Rows.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in NWDT Financial Position Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "NWDTFinancialPosition";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Period"] = data.Period;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new NWDTFinancialPositionParsedRow
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

       /* private async Task<ExcelParseResult> ParseManagementReturn(IFormFile file)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportManagementRows(file, _logger);
                    if (data == null || !data.ManagementReports.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Management Return Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "ManagementReturn";
                    // Management returns might not have date metadata
                    result.Metadata["MRating"] = data.MRating;

                    var parsedRow = new ManagementReturnParsedRow
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
        }*/

      /*  private async Task<ExcelParseResult> ParseSectoralLending(IFormFile file, string saccoType)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportSectoralLendingReport(file, _logger, saccoType);
                    if (data == null || !data.Categories.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Sectoral Lending Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "SectoralLending";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["Year"] = data.Year;
                    result.Metadata["Month"] = data.Month;
                    result.Metadata["SaccoName"] = data.SaccoName;
                    result.Metadata["SaccoCsNumber"] = data.SaccoCsNumber;

                    var parsedRow = new SectoralLendingParsedRow
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
        }*/

        /*private async Task<ExcelParseResult> ParseDailyLiquidity(IFormFile file, string saccoType)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportDailyLiquidityRows(file, _logger);
                    if (data == null)
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Daily Liquidity Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "DailyLiquidity";
                    result.Metadata["ReportDate"] = data.ReportDate;
                    result.Metadata["SACCOName"] = data.SACCOName;
                    result.Metadata["CSNO"] = data.CSNO;

                    var parsedRow = new DailyLiquidityParsedRow
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
        }*/

       /* private async Task<ExcelParseResult> ParseInsiderLending(IFormFile file, string saccoType)
        {
            return await Task.Run(() =>
            {
                var result = new ExcelParseResult();
                try
                {
                    var data = ExcelService.ImportInsiderLendingReport(file, _logger);
                    if (data == null || !data.Loans.Any())
                    {
                        result.Success = false;
                        result.Errors.Add("No data found in Insider Lending Form");
                        return result;
                    }

                    result.Success = true;
                    result.FormType = "InsiderLending";
                    result.Metadata["StartDate"] = data.StartDate;
                    result.Metadata["EndDate"] = data.EndDate;
                    result.Metadata["SaccoName"] = data.SaccoName;
                    result.Metadata["SaccoSocietyCsNumber"] = data.SaccoSocietyCsNumber;

                    var parsedRow = new InsiderLendingParsedRow
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
        }*/

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