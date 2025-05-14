using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Returns.DTOs.Forms;
using Returns.Helpers;
using Returns.Models;
using Returns.Models.Data;
using static Returns.Helpers.TokenHelper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class FormsController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        string baseUrl = "";
        public FormsController(ReturnsDbContext context)
        {
            _context = context;
        }

        // create form
        [HttpPost("CreateForm")]
        public async Task<ActionResult<FormDTO>> CreateFormAsync([FromForm] CreateFormDTO createFormDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            // Check if form of this type already exists
            await FormsHelper.ValidateFormTypeUniqueness(createFormDTO);
            var templatePath = "";
            if (!createFormDTO.IsOtherForm)
            {
                // we need it to be an excel file
                if (!FormsHelper.IsValidExcelFile(createFormDTO.Template))
                {
                    return StatusCode(500, "Only Excel files with .xlsx extension are allowed. Please ensure you're uploading a modern Excel file, not a legacy format.");
                }
                templatePath = await FormsHelper.SaveFileAsync(createFormDTO.Template, "Templates", createFormDTO.DisplayName);
                if (templatePath == null)
                {
                    return StatusCode(500, "Failed to save template file.");
                }
            }
            // for other, no need to attach a file
            else
            {
                createFormDTO.Template = null;
                templatePath = "";
            }

            var form = new ReturnForm
            {
                FormName = createFormDTO.Name,
                PeriodId = createFormDTO.PeriodId,
                SaccoTypeId = createFormDTO.SaccoTypeId,
                IsCapitalAdequencyForm = createFormDTO.IsCapitalAdequencyForm,
                Code = createFormDTO.DisplayName,
                TemplateUrl = templatePath,
                IsLiquidityStatement = createFormDTO.IsLiquidityStatement,
                IsRiskClassification = createFormDTO.IsRiskClassification,
                IsInvestmentReturn = createFormDTO.IsInvestmentReturn,
                IsFinancialPosition = createFormDTO.IsFinancialPosition,
                IsInsiderLending = createFormDTO.IsInsiderLending,
                IsDailyLiquidity = createFormDTO.IsDailyLiquidity,
                IsSectoralLending = createFormDTO.IsSectoralLending,
                IsStatementOfComprehensiveIncome = createFormDTO.IsStatementOfComprehensiveIncome,
                IsDepositReturnForm = createFormDTO.IsDepositReturnForm,
                IsOtherForm = createFormDTO.IsOtherForm
            };

            _context.ReturnForms.Add(form);
            await _context.SaveChangesAsync();
            var m = new FormDTO
            {
                Id = form.Id,
                Name = form.FormName,
                DisplayName = form.Code,
                SaccoTypeId = form.SaccoTypeId,
                IsCapitalAdequencyForm = form.IsCapitalAdequencyForm,
                IsRiskClassification = createFormDTO.IsRiskClassification,
                IsInvestmentReturn = createFormDTO.IsInvestmentReturn,
                IsFinancialPosition = createFormDTO.IsFinancialPosition,
                IsDailyLiquidity = createFormDTO.IsDailyLiquidity,
                IsSectoralLending = createFormDTO.IsSectoralLending,
                IsInsiderLending = createFormDTO.IsInsiderLending,
                IsStatementOfComprehensiveIncome = createFormDTO.IsStatementOfComprehensiveIncome,
                IsDepositReturnForm = createFormDTO.IsDepositReturnForm,
                IsLiquidityStatement = createFormDTO.IsLiquidityStatement,
                IsOtherForm = createFormDTO.IsOtherForm,
                TemplateUrl = form.IsOtherForm ? "" : $"{baseUrl}{form.TemplateUrl}"
            };

            return StatusCode(201, m);

        }

        // delete form
        [HttpDelete("DeleteForm/{formId}")]
        public async Task<ActionResult> DeleteFormAsync(string formId)
        {
            try
            {
                var form = await _context.ReturnForms.FindAsync(formId);
                if (form == null)
                {
                    return NotFound();
                }
                // Delete the file from the server if it exists
                if (!string.IsNullOrEmpty(form.TemplateUrl))
                {
                    FormsHelper.DeleteFile(form.TemplateUrl);
                }
                _context.ReturnForms.Remove(form);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("FormsToSubmit")]
        public async Task<ActionResult<List<FormsToSubmitDTO>>> GetFormsToSubmit()
            {
            try
            {
                ReturnsHelper returnsHelper = new ReturnsHelper(_context);
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401);
                }
                DateTime requestDate = DateTime.Now;

                var allForms = await _context.ReturnForms
                  .Where(x => x.SaccoTypeId == loggedInSacco.SaccoType)
                  .Include(f => f.Period)
                  .ToListAsync();

                var relevantForms = new List<FormsToSubmitDTO>();

                foreach (var form in allForms)
                {
                    if (returnsHelper.IsFormDueForSubmission(form, requestDate))
                    {
                        (DateTime reportingStartDate, DateTime reportingEndDate) = returnsHelper.GetReportingPeriod(form, requestDate);

                        DateTime dueDate = returnsHelper.GetDueDate(form, reportingEndDate);

                        var formDto = new FormsToSubmitDTO
                        {
                            Id = form.Id,
                            Name = form.FormName,
                            Code = form.Code,
                            // PeriodName = form.Period.Name,
                            ReportingPeriodStart = reportingStartDate,
                            ReportingPeriodEnd = reportingEndDate,
                            SubmissionDeadLine = dueDate,
                            IsLate = requestDate > dueDate,
                            TemplateUrl = $"{baseUrl}{form.TemplateUrl}"
                        };

                        formDto.IsSubmitted = false; //await IsFormSubmittedAsync(form, requestDate);

                        relevantForms.Add(formDto);
                    }
                }

                return Ok(relevantForms);

            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        private DateTime GetDueDate(ReturnForm form, DateTime reportingPeriodEndDate)
        {
            if (reportingPeriodEndDate == DateTime.MinValue)
            {
                return DateTime.MinValue;
            }

            var periodName = form.Period.Name;

            switch (periodName)
            {
                case "Daily":
                    // Daily returns are due at the end of the next day
                    return reportingPeriodEndDate.AddDays(1);

                case "Monthly":
                    // Monthly returns are due by the 15th of the following month
                    return new DateTime(
                        reportingPeriodEndDate.AddMonths(1).Year,
                        reportingPeriodEndDate.AddMonths(1).Month,
                        15
                    );

                case "Quarterly":
                    // Quarterly returns are due by the 15th of the first month of the next quarter
                    return new DateTime(
                        reportingPeriodEndDate.AddDays(1).Year,
                        reportingPeriodEndDate.AddDays(1).Month,
                        15
                    );

                case "Annual":
                    // Annual returns are due by January 15th of the following year
                    return new DateTime(reportingPeriodEndDate.Year + 1, 1, 15);

                case "Semi-Annual":
                    // Semi-annual returns are due by the 15th of the month following the half-year
                    if (reportingPeriodEndDate.Month == 6)
                    {
                        // First half due by July 15th
                        return new DateTime(reportingPeriodEndDate.Year, 7, 15);
                    }
                    else
                    {
                        // Second half due by January 15th of next year
                        return new DateTime(reportingPeriodEndDate.Year + 1, 1, 15);
                    }

                case "Bi-Monthly":
                    // Bi-monthly returns are due on the 1st or 15th
                    if (reportingPeriodEndDate.Day == 14)
                    {
                        // First half of month due on the 15th
                        return new DateTime(reportingPeriodEndDate.Year, reportingPeriodEndDate.Month, 15);
                    }
                    else
                    {
                        // Second half of month due on the 1st of the next month
                        return new DateTime(
                            reportingPeriodEndDate.AddMonths(1).Year,
                            reportingPeriodEndDate.AddMonths(1).Month,
                            1
                        );
                    }

                default:
                    return DateTime.MaxValue;
            }
        }


        private (DateTime start, DateTime end) GetReportingPeriod(ReturnForm form, DateTime currentDate)
        {
            var periodName = form.Period.Name;

            switch (periodName)
            {
                case "Daily":
                    // For daily, the reporting period is the previous day
                    DateTime previousDay = currentDate.AddDays(-1);
                    return (previousDay, previousDay);

                case "Monthly":
                    // For monthly, we're reporting on the previous month
                    DateTime previousMonth = currentDate.AddMonths(-1);
                    return (
                        new DateTime(previousMonth.Year, previousMonth.Month, 1),
                        new DateTime(
                            previousMonth.Year,
                            previousMonth.Month,
                            DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month)
                        )
                    );

                case "Quarterly":
                    // For quarterly, we're reporting on the previous quarter
                    int currentQuarter = (currentDate.Month - 1) / 3 + 1;
                    int previousQuarter = currentQuarter == 1 ? 4 : currentQuarter - 1;
                    int previousQuarterYear = currentQuarter == 1 ? currentDate.Year - 1 : currentDate.Year;

                    int startMonth = ((previousQuarter - 1) * 3) + 1;
                    int endMonth = previousQuarter * 3;

                    return (
                        new DateTime(previousQuarterYear, startMonth, 1),
                        new DateTime(
                            previousQuarterYear,
                            endMonth,
                            DateTime.DaysInMonth(previousQuarterYear, endMonth)
                        )
                    );

                case "Annual":
                    // For annual, we're reporting on the previous year
                    return (
                        new DateTime(currentDate.Year - 1, 1, 1),
                        new DateTime(currentDate.Year - 1, 12, 31)
                    );

                case "Semi-Annual":
                    // For semi-annual, we're reporting on the previous half-year
                    if (currentDate.Month == 1)
                    {
                        // January: reporting on July-December of previous year
                        return (
                            new DateTime(currentDate.Year - 1, 7, 1),
                            new DateTime(currentDate.Year - 1, 12, 31)
                        );
                    }
                    else if (currentDate.Month == 7)
                    {
                        // July: reporting on January-June of current year
                        return (
                            new DateTime(currentDate.Year, 1, 1),
                            new DateTime(currentDate.Year, 6, 30)
                        );
                    }
                    else
                    {
                        // Not a due month for semi-annual returns
                        return (DateTime.MinValue, DateTime.MinValue);
                    }

                case "Bi-Monthly":
                    // For bi-monthly, determine which half of the month we're in
                    if (currentDate.Day == 1)
                    {
                        // On the 1st, we're reporting on the second half of the previous month
                        DateTime prevMonth = currentDate.AddMonths(-1);
                        int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

                        return (
                            new DateTime(prevMonth.Year, prevMonth.Month, 16),
                            new DateTime(prevMonth.Year, prevMonth.Month, daysInPrevMonth)
                        );
                    }
                    else if (currentDate.Day == 15)
                    {
                        // On the 15th, we're reporting on the first half of the current month
                        return (
                            new DateTime(currentDate.Year, currentDate.Month, 1),
                            new DateTime(currentDate.Year, currentDate.Month, 14)
                        );
                    }
                    else
                    {
                        // Not a due day for bi-monthly returns
                        return (DateTime.MinValue, DateTime.MinValue);
                    }

                default:
                    return (DateTime.MinValue, DateTime.MinValue);
            }
        }

      

        private async Task<bool> IsFormSubmittedAsync(ReturnForm form, DateTime requestDate)
        {
            var submit = await _context.Returns
            .Include(f => f.CapitalAdequencies)
            .Include(f => f.DepositReturns)
            .Include(f => f.DepositReturns)
            .Include(f => f.RiskClassifications)
            .Include(f => f.InvestmentReturns)
            .Include(f => f.StatementOfFinancialPositionReturns)
            .Include(f => f.StatementOfComprehensiveIncomeReturns)
            .Include(f => f.LiquidityReturns)
            .Where(f => f.SubmittedAt.Date == requestDate.Date)
            .FirstOrDefaultAsync();

            if (submit == null)
            {
                return false;
            }

            if (form.IsCapitalAdequencyForm)
            {
                return submit.CapitalAdequencies != null;
            }
            else if (form.IsDepositReturnForm)
            {
                return submit.DepositReturns != null;
            }
            else if (form.IsRiskClassification)
            {
                return submit.RiskClassifications != null;
            }
            else if (form.IsInvestmentReturn)
            {
                return submit.InvestmentReturns != null;
            }
            else if (form.IsFinancialPosition)
            {
                return submit.StatementOfFinancialPositionReturns != null;
            }
            else if (form.IsStatementOfComprehensiveIncome)
            {
                return submit.StatementOfComprehensiveIncomeReturns != null;
            }
            else if (form.IsLiquidityStatement)
            {
                return submit.LiquidityReturns != null;
            }

            return false;
        }

        // Get Forms Given Period
        [HttpGet("GetFormsByPeriod/{periodId}")]
        public async Task<ActionResult<List<FormDTO>>> GetFormsByPeriodAsync([FromQuery] string periodId)
        {
            try
            {
                baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                var forms = await _context.ReturnForms
                    .Where(f => f.PeriodId == periodId)
                    .ToListAsync();

                var formDTOs = new List<FormDTO>();

                foreach (var form in forms)
                {
                    formDTOs.Add(new FormDTO
                    {
                        Id = form.Id,
                        Name = form.FormName,
                        DisplayName = form.Code,
                        SaccoTypeId = form.SaccoTypeId,
                        IsCapitalAdequencyForm = form.IsCapitalAdequencyForm,
                        IsRiskClassification = form.IsRiskClassification,
                        IsInvestmentReturn = form.IsInvestmentReturn,
                        IsSectoralLending = form.IsSectoralLending,
                        IsFinancialPosition = form.IsFinancialPosition,
                        IsStatementOfComprehensiveIncome = form.IsStatementOfComprehensiveIncome,
                        IsDepositReturnForm = form.IsDepositReturnForm,
                        IsLiquidityStatement = form.IsLiquidityStatement,
                        TemplateUrl = $"{baseUrl}{form.TemplateUrl}"
                    });
                }

                return Ok(formDTOs);
            }
            catch (System.Exception Ex)
            {

                CustomErrorHandler.LogException(Ex);
                return StatusCode(500, CustomErrorHandler.HandleException(Ex));
            }
        }


    }
}
