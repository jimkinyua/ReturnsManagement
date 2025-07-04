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
using Microsoft.Extensions.Configuration;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class FormsController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        private readonly IConfiguration _configuration;

        public FormsController(ReturnsDbContext context)
        {
            _context = context;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        // create form
        [HttpPost("CreateForm")]
        public async Task<ActionResult<FormDTO>> CreateFormAsync([FromForm] CreateFormDTO createFormDTO)
        {
            var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

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
                    IsManagement = createFormDTO.IsManagement,
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
                    IsManagement = createFormDTO.IsManagement,
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
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

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
                            // PeriodName = form.ReturnPeriods.Name,
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


     

    }
}
