using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Perfomance_Report;
using Returns.DTOs.Returns_Analysis;
using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.NWDT;
using Returns.DTOs.Returns_Submission.Returns_Submission.DT;
using Returns.Helpers;
using Returns.Models;
using Returns.Models.Data;
using SASRAXRBSS.Dto.Returns_Analysis;
using static Returns.Helpers.ExcelService;
using static Returns.Helpers.ReturnAnalysisHelper;
using static Returns.Helpers.TokenHelper;
using static Returns.Helpers.ExcelService.Form2CStatement;
using Microsoft.AspNetCore.Routing.Template;
using Returns.DTOs.Returns.Returns_Submission.DT;
using System.Linq;
using Returns.DTOs.Perfomance_Report.NWDT;
using Microsoft.AspNetCore.Http;
using static Returns.Helpers.Constants;
using Returns.Helpers.Interfaces;
using Returns.DTOs.Returns.Return_Assignement;
using Returns.DTOs.Returns.Returns_Submission.NWDT;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.Returns.Returns_Analysis;
using System.Text;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class ReturnsController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<ReturnsController> _logger;
        private readonly FormProcessingService _formProcessor;
        private readonly IEmailService _emailService;
        private readonly IReturnAssignmentService _returnAssignmentService;
        private readonly IWorkflowEngineService _workflowService;
        private readonly ICamelsAnalysisService camelsAnalysisService;
        private readonly IComplianceService complianceService;
        //private readonly FormResubmissionService _resubmissionService;
        private readonly IReturnSubmissionService _returnSubmissionService;
        private readonly IReturnChild _returnChild;
        private readonly IConfiguration _configuration;
        private readonly IConsistencyCheckService _consistencyCheckService;
        private readonly IAdminReturnService _adminReturnService;

        public ReturnsController(ReturnsDbContext context, ILogger<ReturnsController> logger, IEmailService emailService, IReturnAssignmentService returnAssignmentService, IWorkflowEngineService workflowService, ICamelsAnalysisService camelsAnalysisService, IComplianceService compliance, IReturnChild returnChild, IReturnSubmissionService returnSubmissionService, IConsistencyCheckService consistencyCheckService, IAdminReturnService adminReturnService)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _context = context;
            _logger = logger;
            _formProcessor = new FormProcessingService(context, logger);
            _emailService = emailService;
            //_resubmissionService = returnAssignmentService;
            _returnAssignmentService = returnAssignmentService;
            _workflowService = workflowService;
            this.camelsAnalysisService = camelsAnalysisService;
            this.complianceService = compliance;
            _returnChild = returnChild;
            //_resubmissionService = new FormResubmissionService(context, emailService, logger, _formProcessor);
            _returnSubmissionService = returnSubmissionService;
            _consistencyCheckService = consistencyCheckService;
            _adminReturnService = adminReturnService;
        }

        [HttpPost("CheckConsistency")]
        public async Task<IActionResult> CheckConsistency([FromForm] NewReturnDTO createFormDTO)
        {

            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);

                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401);
                }

                var SaccoDetails = await complianceService.GetSaccoByIdAsync(loggedInSacco.SaccoId);
                if (SaccoDetails == null)
                {
                    return NotFound("Sacco not found");
                }

                var ratingToUse = await _context.RatingDefinations
                    .Where(r => r.RatingName == "Consistency Check Forms DT" && r.SaccoType == loggedInSacco.SaccoType)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();
                if (ratingToUse == null)
                {
                    return NotFound("No CAMEL rating definition found for this SACCO type.");
                }

                var result = await _consistencyCheckService.CheckConsistencyAsync(createFormDTO, ratingToUse.RatingName, loggedInSacco.SaccoType);

                /*
                                if (loggedInSacco.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                                {
                                    var (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, _, _, _, _, _, _, _, CommonPeriod) = await CheckConsistencyForDT(createFormDTO);
                                    if (!HasConsistencyBeenChecked)
                                    {
                                        return StatusCode(409, string.Join(", ", processingSummary));
                                    }
                                    if (!isValid)
                                    {
                                        string htmlReport = ReportsHelper.GenerateHtmlReport(ConsistencyErrors, CommonPeriod);
                                        byte[] ConsistencyReport = ReportsHelper.GenerateConsistencyPdfReport(ConsistencyErrors, CommonPeriod);

                                        _ = _emailService
                                        .SendEmailAsync(
                                            SaccoDetails.OfficialSaccoEmail,
                                            "Validation Report - Consistency Errors",
                                            htmlReport)
                                        .ContinueWith(t =>
                                        {
                                            if (t.IsFaulted)
                                            {
                                                _logger.LogError(t.Exception, "Failed to send validation-report email.");
                                            }
                                            else
                                            {
                                                _logger.LogInformation("Validation-report email sent successfully.");
                                            }
                                        }, TaskContinuationOptions.OnlyOnRanToCompletion);


                                        _ = Task.Run(async () =>
                                       {
                                           try
                                           {
                                               await _emailService.SendEmailWithAttachmentAsync(
                                                    SaccoDetails.OfficialSaccoEmail,
                                                    "Validation Report - Consistency Errors",
                                                    "PFA",
                                                    ConsistencyReport,
                                                    $"ValidationReport_{CommonPeriod}.pdf"
                                                );
                                               _logger.LogInformation("Validation-report (PDF) email sent successfully.");
                                           }
                                           catch (Exception ex)
                                           {
                                               _logger.LogError(ex, "Failed to send validation-report (PDF) email.");
                                           }
                                       });

                                        return BadRequest(ConsistencyErrors);
                                    }
                                }
                                else
                                {
                                    var (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, _, _, _, _, _, _, _, CommonPeriod) = await CheckConsistencyForNWDT(createFormDTO);
                                    if (!HasConsistencyBeenChecked)
                                    {
                                        return StatusCode(409, string.Join(", ", processingSummary));
                                    }

                                    if (!isValid)
                                    {
                                        if (!isValid)
                                        {
                                            string htmlReport = ReportsHelper.GenerateHtmlReport(ConsistencyErrors, CommonPeriod);
                                            byte[] ConsistencyReport = ReportsHelper.GenerateConsistencyPdfReport(ConsistencyErrors, CommonPeriod);

                                            _ = _emailService
                                      .SendEmailAsync(
                                          SaccoDetails.OfficialSaccoEmail,
                                          "Validation Report - Consistency Errors",
                                          htmlReport)
                                      .ContinueWith(t =>
                                      {
                                          if (t.IsFaulted)
                                          {
                                              _logger.LogError(t.Exception, "Failed to send validation-report email.");
                                          }
                                          else
                                          {
                                              _logger.LogInformation("Validation-report email sent successfully.");
                                          }
                                      }, TaskContinuationOptions.OnlyOnRanToCompletion);


                                            _ = Task.Run(async () =>
                                            {
                                                try
                                                {
                                                    await _emailService.SendEmailWithAttachmentAsync(
                                                        SaccoDetails.OfficialSaccoEmail,
                                                        "Validation Report - Consistency Errors",
                                                        $"<p>Please find attached the validation report for your SACCO's financial returns for the period <strong>{CommonPeriod}</strong>.</p>",
                                                        ConsistencyReport,
                                                        $"ValidationReport_{CommonPeriod}.pdf"
                                                    );
                                                    _logger.LogInformation("Validation-report (PDF) email sent successfully.");
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError(ex, "Failed to send validation-report (PDF) email.");
                                                }
                                            });

                                            return BadRequest(ConsistencyErrors);
                                        }

                                        return BadRequest(ConsistencyErrors);
                                    }
                                }*/

                return Ok(result.ConsistencyErrors);
            }
            catch (Exception ex)
            {
                var errors = CustomErrorHandler.HandleException(ex);
                var errorsasString = string.Join(", ", errors);
                return StatusCode(500, errorsasString);

            }

        }

        /* [HttpPost("RequestFormResubmission")]
         public async Task<IActionResult> RequestFormResubmission([FromBody] ResubmissionRequestDto request)
         {
             try
             {
                 LoggedInEntity loggedInAdmin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                 if (loggedInAdmin == null || string.IsNullOrEmpty(loggedInAdmin.UserId))
                 {
                     return StatusCode(401);
                 }

                 var returnToResubmit = await _context.Returns.FindAsync(request.ReturnId);
                 if (returnToResubmit == null)
                 {
                     return NotFound("Return not found");
                 }

                 var formToResubmit = await _context.ReturnForms.FindAsync(request.FormId);
                 if (formToResubmit == null)
                 {
                     return NotFound("Form not found");
                 }

                 var ComplianceOfficer = await complianceService.GetUserById(loggedInAdmin.UserId);
                 var SaccoDetails = await complianceService.GetSaccoByIdAsync(returnToResubmit.SaccoId);

                 var result = await _resubmissionService.RequestFormResubmissionAsync(
                  request.ReturnId,
                  formToResubmit,
                  loggedInAdmin.UserId,
                  ComplianceOfficer?.FullName ?? "Unknown Compliance Officer",
                  loggedInAdmin.EmailAddress,
                  request.Reason ?? "No reason provided",
                  returnToResubmit.SaccoId,
                  returnToResubmit.SaccoType,
                  SaccoDetails.OfficialSaccoEmail
                 );

                 if (result.Success)
                 {
                     return Ok();
                 }
                 else
                 {
                     return BadRequest(result.Message);
                 }

             }
             catch (Exception ex)
             {
                 return StatusCode(500, CustomErrorHandler.HandleException(ex));

             }
         }*/

        /* [HttpPost("ResubmitForm")]
         public async Task<IActionResult> ResubmitForm([FromForm] SaccoResubmissionDto request)
         {
             try
             {
                 var loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                 if (loggedInSacco == null)
                 {
                     return Unauthorized();
                 }

                 if (request.FormFile == null || request.FormFile.Length == 0)
                 {
                     return BadRequest("Form file is required.");
                 }
                 if (string.IsNullOrEmpty(request.ReturnId) || string.IsNullOrEmpty(request.FormId))
                 {
                     return BadRequest("ReturnId and FormId are required.");
                 }

                 var form = await _context.ReturnForms
                             .Include(x => x.Period)
                             .FirstOrDefaultAsync(x => x.Id == request.FormId);

                 if (form == null)
                 {
                     return BadRequest("Form not found");
                 }

                 var returnRecord = await _context.Returns.FindAsync(request.ReturnId);
                 if (returnRecord == null)
                 {
                     return BadRequest("Return not found");
                 }

                 if (returnRecord.SaccoId != loggedInSacco.SaccoId)
                 {
                     return StatusCode(401, "You can only resubmit your own forms");
                 }

                 var pendingRequest = await _context.FormResubmissionRequests
                     .FirstOrDefaultAsync(r => r.ReturnId == request.ReturnId &&
                      r.FormId == request.FormId &&
                      r.Status == "Pending");

                 if (pendingRequest == null)
                 {
                     return BadRequest("No pending resubmission request found for this form");
                 }

                 var result = await _resubmissionService.HandleSaccoFormResubmissionAsync(
                    request.ReturnId,
                    form,
                    request.FormFile,
                    loggedInSacco.SaccoId,
                    loggedInSacco.SaccoType,
                    request.ResubmissionNotes?.Trim() ?? string.Empty
                );

                 if (result.Success)
                 {
                     return Ok();
                 }
                 else
                 {
                     return BadRequest(result.Message);
                 }
             }
             catch (Exception ex)
             {
                 return StatusCode(500, CustomErrorHandler.HandleException(ex));
             }
         }*/

        /*private async Task<(
            bool IsValid,
            List<string> ProcessingSummary,
            List<ValidationError> ConsistencyErrors,
            bool HasConsistencyBeenChecked,
            List<CapitalAdequacyRow> CapitalAdequacy,
            List<LiquidityStatementRow> LiquidityStatement,
            List<DepositRangeRow> DepositReturn,
            List<RiskClassificationRow> RiskClassification,
            List<Form2ERow> Investment,
            List<StatementOfFinancialPositionRow> FinancialPosition,
            List<StatementOfComprehensiveIncomeRow> ComprehensiveStatement, string CommonPeriod)> CheckConsistencyForNWDT(NewReturnDTO createFormNWDTDTO)
        {
            var processingSummary = new List<string>();
            List<ValidationError> ConsistencyErrors = new List<ValidationError>();
            ReturnsHelper returnsHelper = new ReturnsHelper(_context);

            // Check for attachments
            if (createFormNWDTDTO.FormUploads == null ||
                createFormNWDTDTO.FormUploads.Count == 0 ||
                !createFormNWDTDTO.FormUploads.Any(f => f.formFile != null))
            {
                throw new Exception("No attachments found. Please attach at least one form");
            }

            // await returnsHelper.NWDTValidateUploadedForms(createFormNWDTDTO);

            // Initialize form data holders
            List<CapitalAdequacyRow> capital_adequacy_form1 = null;
            List<LiquidityStatementRow> liquidityStatement_form_2 = null;
            List<DepositRangeRow> depositreturn_form_3 = null;
            List<RiskClassificationRow> riskClassification_form_4 = null;
            List<Form2ERow> inverstment_return_form_5 = null;
            List<StatementOfFinancialPositionRow> financialPositionStatement_form_6 = null;
            List<StatementOfComprehensiveIncomeRow> comprehensiveStatement_form7 = null;
            var HasConsistencyBeenChecked = true;
            Form2AStatement form1Statement = null;
            Form2BStatement Form2 = null;
            Form2CStatement Form3 = null;
            Form2DStatement form4 = null;
            Form2EStatement Form5 = null;
            Form2GStatement form6 = null;
            Form2FStatement form7 = null;


            // Process each form
            foreach (var form in createFormNWDTDTO.FormUploads)
            {
                if (form.formFile == null) continue;

                try
                {

                    ReturnForm? fm = await _context.ReturnForms.FirstOrDefaultAsync(f => f.Id == form.FormId);
                    if (fm == null)
                    {
                        processingSummary.Add($"Form with ID {form.FormId} was not found in the system");
                        continue;
                    }

                    // Form 1: Capital Adequacy Form
                    if (fm.IsCapitalAdequencyForm)
                    {
                        form1Statement = ExcelService.ImportForm2ARows(form.formFile, _logger);
                        if (form1Statement == null || !form1Statement.Rows.Any())
                        {
                            throw new Exception("No data found in Capital Adequacy Form");
                        }

                        capital_adequacy_form1 = form1Statement.Rows;

                        if (capital_adequacy_form1 == null || !capital_adequacy_form1.Any())
                        {
                            throw new Exception("No data found in Capital Adequacy Form");
                        }
                    }
                    // Form 2: Liquidity Statement Form
                    else if (fm.IsLiquidityStatement)
                    {
                        Form2 = ExcelService.ImportForm2BStatement(form.formFile, _logger);
                        if (Form2 == null || !Form2.Rows.Any())
                        {
                            throw new Exception("No data found in Liquidity Statement Form");
                        }
                        liquidityStatement_form_2 = Form2.Rows;
                        if (liquidityStatement_form_2 == null || !liquidityStatement_form_2.Any())
                        {
                            throw new Exception("No data found in Liquidity Statement Form");
                        }
                    }
                    // Form 3: Deposit Return Form
                    else if (fm.IsDepositReturnForm)
                    {
                        Form3 = ExcelService.ImportForm2CDataRows(form.formFile, _logger);
                        if (Form3 == null || !Form3.Rows.Any())
                        {
                            throw new Exception("No data found in Deposit Return Form");
                        }
                        depositreturn_form_3 = Form3.Rows;
                        if (depositreturn_form_3 == null || !depositreturn_form_3.Any())
                        {
                            throw new Exception("No data found in Deposit Return Form");
                        }
                    }
                    // Form 5: Investment Return Form
                    else if (fm.IsInvestmentReturn)
                    {
                        Form5 = ImportForm2ERows(form.formFile, _logger);
                        if (Form5 == null || !Form5.Rows.Any())
                        {
                            throw new Exception("No data found in Investment Return Form");
                        }
                        inverstment_return_form_5 = Form5.Rows;
                        if (inverstment_return_form_5 == null || !inverstment_return_form_5.Any())
                        {
                            throw new Exception("No data found in Investment Return Form");
                        }
                    }
                    // Form 4: Risk Classification Form
                    else if (fm.IsRiskClassification)
                    {
                        form4 = ExcelService.ImportForm2DRows(form.formFile, _logger);
                        if (form4 == null || !form4.Rows.Any())
                        {
                            throw new Exception("No data found in Risk Classification Form");
                        }
                        riskClassification_form_4 = form4.Rows;
                        if (riskClassification_form_4 == null || !riskClassification_form_4.Any())
                        {
                            throw new Exception("No data found in Risk Classification Form");
                        }
                    }

                    // Form 6: Statement of Financial Position
                    else if (fm.IsFinancialPosition)
                    {
                        form6 = ImportForm2GRows(form.formFile, _logger);
                        if (form6 == null || !form6.Rows.Any())
                        {
                            throw new Exception("No data found in Statement of Financial Position");
                        }
                        financialPositionStatement_form_6 = form6.Rows;
                        if (financialPositionStatement_form_6 == null || !financialPositionStatement_form_6.Any())
                        {
                            throw new Exception("No data found in Statement of Financial Position");
                        }
                    }
                    // Form 7: Statement of Comprehensive Income
                    else if (fm.IsStatementOfComprehensiveIncome)
                    {
                        form7 = ImportForm2FRows(form.formFile, _logger);
                        if (form7 == null || !form7.Rows.Any())
                        {
                            throw new Exception("No data found in Statement of Comprehensive Income");
                        }
                        comprehensiveStatement_form7 = form7.Rows;
                        if (comprehensiveStatement_form7 == null || !comprehensiveStatement_form7.Any())
                        {
                            throw new Exception("No data found in Statement of Comprehensive Income");
                        }
                    }
                }
                catch (Exception ex)
                {
                    processingSummary.Add($"Error processing '{form.formFile.FileName}': {ex.Message}");
                    throw new FileProcessingException(form.formFile.FileName, form.FormId, ex);
                }
            }

            var red = returnsHelper.AreAllFormsInSamePeriodNWDT(form1Statement, Form2, Form3, form4, Form5, form6, form7);

            if (!red.IsValid)
            {
                HasConsistencyBeenChecked = false;
                processingSummary.Add(red.Message);
                return (false, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, null, null, null, null, null, null, null, red.CommonPeriod);
            }


            bool isValid = true;
            if (returnsHelper.AreAllFormsPresent(capital_adequacy_form1, liquidityStatement_form_2, depositreturn_form_3,
                riskClassification_form_4, inverstment_return_form_5, financialPositionStatement_form_6,
                comprehensiveStatement_form7))
            {
                var validationResult = ValidateNWDTReturns(
                    capital_adequacy_form1, liquidityStatement_form_2, depositreturn_form_3,
                    riskClassification_form_4, inverstment_return_form_5, financialPositionStatement_form_6,
                    comprehensiveStatement_form7
                );

                isValid = validationResult.IsValid;
                if (!isValid)
                {
                    ConsistencyErrors.AddRange(validationResult.ValidationErrors);
                    IDocument report = new ConsistencyReport(validationResult, "Test", "System");
                    var pdfBytes = report.GeneratePdf();
                    await FormsHelper.SaveReportAsync(pdfBytes, "ConsistencyReport", "System", "Test");

                }
            }

            return (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, capital_adequacy_form1, liquidityStatement_form_2,
                depositreturn_form_3, riskClassification_form_4, inverstment_return_form_5,
                financialPositionStatement_form_6, comprehensiveStatement_form7, red.CommonPeriod);
        }
*/
        public sealed class FileProcessingException : Exception
        {
            public string FileName { get; }
            public string FormId { get; }

            public FileProcessingException(string fileName, string formId, Exception inner)
                : base($"Error processing file '{fileName}' (FormId: {formId}).", inner)
            {
                FileName = fileName;
                FormId = formId;
            }
        }

        /* [HttpGet("GetChildDetails")]
         public async Task<IActionResult> GetChildDetails([FromQuery] string ChildId, [FromQuery] string FormId, [FromQuery] string ReturnId)
         {
             try
             {
                 var FormDetails = await _context.ReturnForms
                       .Include(f => f.Period)
                       .FirstOrDefaultAsync(f => f.Id == FormId);
                 if (FormDetails == null)
                 {
                     return NotFound("Form not found");
                 }
                 var ReturnDetails = await _context.Returns.FindAsync(ReturnId);
                 if (ReturnDetails == null)
                 {
                     return NotFound("Return not found");
                 }
                 var FormType = _formProcessor.GetFormTypeFromForm(FormDetails);
                 var SaccoType = ReturnDetails.SaccoType;

                 var childDetails = await _returnChild.GetChildDetailsByFormType(ChildId, FormType, SaccoType);

                 return Ok(childDetails);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error fetching child details");
                 return StatusCode(500, "Internal server error");
             }
         }*/

        [HttpPost("Draft")]
        public async Task<IActionResult> FileDraftReturnsAsync([FromForm] NewReturnDTO dto)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401);
                }

                var result = await _returnSubmissionService.UploadDraftAsync(dto, loggedInSacco);

                return Ok(result);
            }
            catch (Exception)
            {

                throw;
            }


        }

        // File Return
        /* [HttpPost("Draft")]
        public async Task<IActionResult> FileReturnsAsync([FromForm] NewReturnDTO createFormDTO)
        {

            ReturnsHelper returnsHelper = new ReturnsHelper(_context);
            try
            {
                var processingSummary = new List<string>();
                var ConError = new List<string>();
                Boolean IsConsistent = true;
                string PeriodToUse = string.Empty;
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401);
                }


                if (!returnsHelper.HasValidUploads(createFormDTO))
                {
                    return BadRequest("No attachments found. Please attach at least one form.");
                }
                // Validate that all forms have the same reporting period
                foreach (var upload in createFormDTO.FormUploads.Where(u => u.formFile != null && !string.IsNullOrEmpty(u.FormId)))
                {
                    var form = await _context.ReturnForms.FirstOrDefaultAsync(f => f.Id == upload.FormId);
                    if (form != null)
                    {
                        // Skip validation for management forms since they don't have periods
                        if (!form.IsManagement)
                        {
                            var (endDate, year) = await _formProcessor.ExtractReportingEndDate(upload.formFile, form, loggedInSacco.SaccoType);

                            if (endDate != DateTime.MinValue && !string.IsNullOrEmpty(year))
                            {
                                if (PeriodToUse == string.Empty)
                                {
                                    // First valid form sets the period
                                    PeriodToUse = year;
                                }
                                else if (PeriodToUse != year)
                                {
                                    // If we find a different period, flag inconsistency
                                    ConError.Add($"Form {form.Code} has period {year} which differs from {PeriodToUse}. Are you using the Correct template? ");
                                    IsConsistent = false;
                                }
                            }
                        }
                    }
                }

                if (PeriodToUse == null)
                {
                    return BadRequest("No valid reporting period found in the uploaded forms.");
                }


                if (!IsConsistent)
                {
                    return BadRequest(new
                    {
                        Message = "Inconsistent reporting periods detected across forms",
                        Errors = ConError
                    });
                }


                var HasAssignedUser = await _returnAssignmentService.CheckSaccoAssignedUserAsync(loggedInSacco.SaccoId);

                if (!HasAssignedUser.Success)
                {
                    return BadRequest(HasAssignedUser.ErrorMessage);
                }

                if (loggedInSacco.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                {
                    if (_formProcessor.ShouldConsistencyChecksBeDone(_context, createFormDTO).Result)
                    {
                        var (isValid, _, ConsistencyErrors, _, _, _, _, _, _, _, _, CommonPeriod) = await CheckConsistencyForDT(createFormDTO);
                        if (!isValid)
                        {
                            ConError = ConsistencyErrors.Select(error => $"{error.Category}: {error.Description} - {string.Join(", ", error.Details.Select(d => $"{d.Key}: {d.Value}"))}").ToList();
                        }
                    }

                    //PeriodToUse = CommonPeriod;
                }
                else
                {

                    if (_formProcessor.ShouldConsistencyChecksBeDone(_context, createFormDTO).Result)
                    {
                        var (isValid, _, ConsistencyError, _, _, _, _, _, _, _, _, CommonPeriod) = await CheckConsistencyForNWDT(createFormDTO);
                        if (!isValid)
                        {

                            ConError = ConsistencyError.Select(error => $"{error.Category}: {error.Description} - {string.Join(", ", error.Details.Select(d => $"{d.Key}: {d.Value}"))}").ToList();
                        }
                    }

                    //PeriodToUse = CommonPeriod;
                }

                var (success, EffectiveReturnId, processingMessages) = await _formProcessor.ProcessFormBatchAsync(createFormDTO, loggedInSacco, IsConsistent, ConError, PeriodToUse);

                if (!success)
                {
                    return StatusCode(409, string.Join(", ", processingMessages));
                }
                var PreviousReturnDetails = _context.Returns.Find(EffectiveReturnId);
                var IsAssigned = await _returnAssignmentService.AssignReturnAsync(PreviousReturnDetails, loggedInSacco.SaccoId);
                if (!IsAssigned.Success)
                {
                    _logger.LogError("Error assigning return: {ErrorMessage}", IsAssigned.ErrorMessage);

                    var returnToDelete = await _context.Returns.FindAsync(EffectiveReturnId);
                    if (returnToDelete != null)
                    {
                        _context.Returns.Remove(returnToDelete);
                        await _context.SaveChangesAsync();
                    }

                    return StatusCode(500, IsAssigned.ErrorMessage);
                }
                var ratingResult = await camelsAnalysisService.CalculateAnalysisAsync(PreviousReturnDetails.Id, PreviousReturnDetails.SaccoType);
                var WorkFlowResult = await _workflowService.StartWorkflowAsync(PreviousReturnDetails, ratingResult.OverallRating);

                await _emailService.SendEmailAsync(loggedInSacco.EmailAddress, "Return Submission Confirmation", "Your returns have been successfully submitted.");

                var lateForms = new List<(string FormName, DateTime DueDate, DateTime SubmissionDate)>();
                var saccoDetails = await complianceService.GetSaccoByIdAsync(loggedInSacco.SaccoId);
                foreach (var form in createFormDTO.FormUploads)
                {
                    if (form.formFile == null) continue;

                    var formDetails = await _context.ReturnForms.FirstOrDefaultAsync(f => f.Id == form.FormId);
                    if (formDetails != null)
                    {
                        (DateTime reportingStartDate, DateTime reportingEndDate) = returnsHelper.GetReportingPeriod(formDetails, DateTime.Now);
                        DateTime dueDate = returnsHelper.GetDueDate(formDetails, reportingEndDate);

                        // Check if submission is late
                        if (DateTime.Now > dueDate)
                        {
                            lateForms.Add((formDetails.FormName, dueDate, DateTime.Now));
                        }
                    }
                }
                if (lateForms.Any())
                {
                    var subject = lateForms.Count == 1
                        ? $"Late Submission – {lateForms.First().FormName} Return"
                        : $"Late Submissions – {lateForms.Count} Returns";

                    var body = GenerateLateFormsEmailBody(saccoDetails.SaccoName, lateForms);

                    await _emailService
                        .SendEmailAsync(saccoDetails.OfficialSaccoEmail, subject, body)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, "Failed to send late forms notification email.");
                            }
                            else
                            {
                                _logger.LogInformation($"Late forms notification email sent successfully for {lateForms.Count} form(s).");
                            }
                        });
                }

                return Ok(processingMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing files");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }

        }*/

        private string GenerateLateFormsEmailBody(string saccoName, List<(string FormName, DateTime DueDate, DateTime SubmissionDate)> lateForms)
        {
            var body = new StringBuilder();
            body.AppendLine($"Dear {saccoName} Team,");
            body.AppendLine();

            if (lateForms.Count == 1)
            {
                var form = lateForms.First();
                body.AppendLine($"Thank you for submitting your **{form.FormName}** return.");
                body.AppendLine($"Please note that it was received **after the statutory deadline** of {form.DueDate:dd MMM yyyy}.");
            }
            else
            {
                body.AppendLine($"Thank you for submitting your returns. However, we note that the following {lateForms.Count} returns were received **after their statutory deadlines**:");
                body.AppendLine();

                foreach (var form in lateForms)
                {
                    body.AppendLine($"• **{form.FormName}** - Due: {form.DueDate:dd MMM yyyy}, Submitted: {form.SubmissionDate:dd MMM yyyy}");
                }
            }

            body.AppendLine();
            body.AppendLine("Under the Regulations, late submissions may attract penalties or additional supervisory follow-up.");
            body.AppendLine("Kindly ensure future returns are lodged on or before their due dates to remain in full compliance.");
            body.AppendLine();
            body.AppendLine("Regards,");
            body.AppendLine("Compliance Desk");

            return body.ToString();
        }


        // Re assign Return 
        [HttpPost("ReassignReturn")]
        public async Task<IActionResult> ReassignReturn([FromBody] ReAssignReturnDTO reAssignReturnDTO)
        {
            LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
            {
                return StatusCode(401);
            }
            var returnToReassign = await _context.Returns.FindAsync(reAssignReturnDTO.ReturnId);
            if (returnToReassign == null)
            {
                return NotFound("Return not found");
            }
            var result = await _returnAssignmentService.ReassignReturnAsync(reAssignReturnDTO.ReturnId, reAssignReturnDTO.AssignedUserId);
            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok("Return reassigned successfully");
        }

        [HttpGet("GetSubmittedReturns")]
        public async Task<ActionResult<List<SubmittedReturnDTO>>> GetSubmittedReturns()
        {
            ReturnsHelper returnsHelper = new ReturnsHelper(_context);
            // 1. Load active assignments + their Returns
            var activeAssignments = await _context.ReturnsAssigments
                .Include(a => a.Return)
                .Where(a =>
                    a.Return.SaccoType == Constants.SaccoType.DepositTaking.ToString() &&
                    a.Return.IsActiveVersion
                )
                .ToListAsync();

            if (!activeAssignments.Any())
                return Ok(new List<SubmittedReturnDTO>());

            // 2. Get distinct SaccoCsNumber+Year pairs as separate lists
            var saccoIds = activeAssignments
                .Select(a => a.Return.SaccoId)
                .Distinct()
                .ToList();

            var years = activeAssignments
                .Select(a => a.Return.ReturnFor.Year)
                .Distinct()
                .ToList();

            // 3. Get all relevant returns in one query
            var allReturns = await _context.Returns
                .Where(r => r.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                .Where(r => saccoIds.Contains(r.SaccoId))
                .Where(r => years.Contains(r.ReturnFor.Year))
                .Select(r => new
                {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Build a lookup for (SaccoCsNumber, Year) combinations
            var lookup = activeAssignments
                .Select(a => new { a.Return.SaccoId, a.Return.ReturnFor.Year })
                .Distinct()
                .ToList();

            // 5. Filter the returns to only those we actually need
            var filteredReturns = allReturns
                .Where(r => lookup.Any(x => x.SaccoId == r.SaccoId && x.Year == r.Year))
                .ToList();

            // 6. Group into a map: (SaccoCsNumber,Year) → { Id → PreviousVersionId }
            var linkMap = filteredReturns
                .GroupBy(x => new { x.SaccoId, x.Year })
                .ToDictionary(
                    g => (g.Key.SaccoId, g.Key.Year),
                    g => g.ToDictionary(x => x.Id, x => x.PreviousVersionId ?? "")
                );

            // 7. Build DTOs in memory
            var results = new List<SubmittedReturnDTO>();
            foreach (var assignment in activeAssignments)
            {
                var r = assignment.Return;
                var key = (r.SaccoId, r.ReturnFor.Year);

                if (!linkMap.TryGetValue(key, out var prevMap))
                    continue;

                // walk the chain
                var chain = new List<string>();
                var currentId = r.Id;
                while (prevMap.TryGetValue(currentId, out var prevId) && !string.IsNullOrEmpty(prevId))
                {
                    chain.Add(prevId);
                    currentId = prevId;
                }

                results.Add(new SubmittedReturnDTO
                {
                    Id = r.Id,
                    ReturnsFor = r.ReturnFor.Year.ToString(),
                    SaccoId = r.SaccoId,
                    IsConsistent = !r.IsNotConsistent,
                    ConsistentErrorMessage = r.ConsistentErrorMessage,
                    SaccoName = r.SaccoName,
                    SubmittedAt = r.SubmittedAt,
                    LateNessStatus = returnsHelper.CheckLateReturns(r) ? "Late" : "On Time",
                    TotalReturns = returnsHelper.CountPopulatedReturns(r),
                    TotalLateReturns = returnsHelper.CountLateReturns(r),
                    VersionNumber = r.VersionNumber,
                    PreviousVersionIds = chain
                });
            }

            return Ok(results);
        }

        [HttpGet("GetSubmittedReturnsForSacco")]
        public async Task<ActionResult<List<SubmittedReturnDTO>>> GetSubmittedReturnsForSacco()
        {
            var loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
            {
                return StatusCode(401);
            }
            ReturnsHelper returnsHelper = new ReturnsHelper(_context);

            // 1. Load active assignments + their Returns
            var activeAssignments = await _context.Returns
                .Where(a =>
                    a.SaccoType == Constants.SaccoType.DepositTaking.ToString() &&
                    a.SaccoId == loggedInSacco.SaccoId
                )
                .ToListAsync();

            if (!activeAssignments.Any())
                return Ok(new List<SubmittedReturnDTO>());

            // 2. Get distinct SaccoCsNumber+Year pairs as separate lists
            var saccoIds = activeAssignments
                .Select(a => a.SaccoId)
                .Distinct()
                .ToList();

            var years = activeAssignments
                .Select(a => a.ReturnFor.Year)
                .Distinct()
                .ToList();

            // 3. Get all relevant returns in one query
            var allReturns = await _context.Returns
                .Where(r => r.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                .Where(r => saccoIds.Contains(r.SaccoId))
                .Where(r => years.Contains(r.ReturnFor.Year))
                .Select(r => new
                {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Build a lookup for (SaccoCsNumber, Year) combinations
            var lookup = activeAssignments
                .Select(a => new { a.SaccoId, a.ReturnFor.Year })
                .Distinct()
                .ToList();

            // 5. Filter the returns to only those we actually need
            var filteredReturns = allReturns
                .Where(r => lookup.Any(x => x.SaccoId == r.SaccoId && x.Year == r.Year))
                .ToList();

            // 6. Group into a map: (SaccoCsNumber,Year) → { Id → PreviousVersionId }
            var linkMap = filteredReturns
                .GroupBy(x => new { x.SaccoId, x.Year })
                .ToDictionary(
                    g => (g.Key.SaccoId, g.Key.Year),
                    g => g.ToDictionary(x => x.Id, x => x.PreviousVersionId ?? "")
                );

            // 7. Build DTOs in memory
            var results = new List<SubmittedReturnDTO>();
            foreach (var assignment in activeAssignments)
            {
                var r = assignment;
                var key = (r.SaccoId, r.ReturnFor.Year);

                if (!linkMap.TryGetValue(key, out var prevMap))
                    continue;

                // walk the chain
                var chain = new List<string>();
                var currentId = r.Id;
                while (prevMap.TryGetValue(currentId, out var prevId) && !string.IsNullOrEmpty(prevId))
                {
                    chain.Add(prevId);
                    currentId = prevId;
                }

                results.Add(new SubmittedReturnDTO
                {
                    Id = r.Id,
                    ReturnsFor = r.ReturnFor.Year.ToString(),
                    SaccoId = r.SaccoId,
                    IsConsistent = !r.IsNotConsistent,
                    ConsistentErrorMessage = r.ConsistentErrorMessage,
                    SaccoName = r.SaccoName,
                    SubmittedAt = r.SubmittedAt,
                    LateNessStatus = returnsHelper.CheckLateReturns(r) ? "Late" : "On Time",
                    TotalReturns = returnsHelper.CountPopulatedReturns(r),
                    TotalLateReturns = returnsHelper.CountLateReturns(r),
                    VersionNumber = r.VersionNumber,
                    PreviousVersionIds = chain
                });
            }

            return Ok(results);
        }

        [HttpGet("GetSubmittedNWDTReturnsForSacco")]
        public async Task<ActionResult<List<SubmittedReturnDTO>>> GetSubmittedNWDTReturnsForSacco()
        {

            var loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
            {
                return StatusCode(401);
            }
            ReturnsHelper returnsHelper = new ReturnsHelper(_context);

            // 1. Load all active NWDT assignments (one DB call)
            var activeAssignments = await _context.Returns
                .Where(a =>
                    a.SaccoType == Constants.SaccoType.NWDT.ToString() &&
                    a.IsActiveVersion
                    && a.SaccoId == loggedInSacco.SaccoId
                )
                .ToListAsync();

            if (!activeAssignments.Any()) return Ok(new List<SubmittedReturnDTO>());

            // 2. Figure out which Sacco+Year combos we actually need
            var requiredSaccoIds = activeAssignments
                .Select(a => a.SaccoId)
                .Distinct()
                .ToList();

            var requiredYears = activeAssignments
                .Select(a => a.ReturnFor.Year)
                .Distinct()
                .ToList();

            // 3. One DB call: get all relevant returns
            var allLinks = await _context.Returns
                .Where(r => r.SaccoType == Constants.SaccoType.NWDT.ToString())
                .Where(r => requiredSaccoIds.Contains(r.SaccoId))
                .Where(r => requiredYears.Contains(r.ReturnFor.Year))
                .Select(r => new
                {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    Year = r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Filter to only the exact (SaccoCsNumber, Year) pairs we need
            var requiredPairs = activeAssignments
                .Select(a => (a.SaccoId, a.ReturnFor.Year))
                .Distinct()
                .ToList();

            var filteredLinks = allLinks
                .Where(x => requiredPairs.Contains((x.SaccoId, x.Year)))
                .ToList();

            // 5. Group into a map: (SaccoCsNumber,Year) → Dictionary<Id,PreviousVersionId>
            var linkMap = filteredLinks
                .GroupBy(x => (x.SaccoId, x.Year))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.Id, x => x.PreviousVersionId ?? "")
                );

            // 6. Build DTOs in memory
            var results = new List<SubmittedReturnDTO>(activeAssignments.Count);
            foreach (var assignment in activeAssignments)
            {
                var r = assignment;
                var key = (r.SaccoId, r.ReturnFor.Year);

                if (!linkMap.TryGetValue(key, out var prevMap))
                    continue;

                // walk the version chain
                var chain = new List<string>();
                var currentId = r.Id;
                while (prevMap.TryGetValue(currentId, out var prevId)
                       && !string.IsNullOrEmpty(prevId))
                {
                    chain.Add(prevId);
                    currentId = prevId;
                }

                var isAnyLate = returnsHelper.CheckLateReturnsNWDT(r);

                results.Add(new SubmittedReturnDTO
                {
                    Id = r.Id,
                    ReturnsFor = r.ReturnFor.Year.ToString(),
                    SaccoId = r.SaccoId,
                    IsConsistent = !r.IsNotConsistent,
                    ConsistentErrorMessage = r.ConsistentErrorMessage,
                    SaccoName = r.SaccoName,
                    SubmittedAt = r.SubmittedAt,
                    LateNessStatus = isAnyLate ? "Late" : "On Time",
                    TotalReturns = returnsHelper.CountPopulatedReturnsNWDT(r),
                    TotalLateReturns = returnsHelper.CountLateReturnsNWDT(r),
                    VersionNumber = r.VersionNumber,
                    PreviousVersionIds = chain
                });
            }

            return Ok(results);
        }


        [HttpGet("GetSubmittedNWDTReturns")]
        public async Task<ActionResult<List<SubmittedReturnDTO>>> GetSubmittedNWDTReturns()
        {
            ReturnsHelper returnsHelper = new ReturnsHelper(_context);

            // 1. Load all active NWDT assignments (one DB call)
            var activeAssignments = await _context.ReturnsAssigments
                .Include(a => a.Return)
                .Where(a =>
                    a.Return.SaccoType == Constants.SaccoType.NWDT.ToString() &&
                    a.Return.IsActiveVersion
                )
                .ToListAsync();

            if (!activeAssignments.Any())
                return Ok(new List<SubmittedReturnDTO>());

            // 2. Figure out which Sacco+Year combos we actually need
            var requiredSaccoIds = activeAssignments
                .Select(a => a.Return.SaccoId)
                .Distinct()
                .ToList();

            var requiredYears = activeAssignments
                .Select(a => a.Return.ReturnFor.Year)
                .Distinct()
                .ToList();

            // 3. One DB call: get all relevant returns
            var allLinks = await _context.Returns
                .Where(r => r.SaccoType == Constants.SaccoType.NWDT.ToString())
                .Where(r => requiredSaccoIds.Contains(r.SaccoId))
                .Where(r => requiredYears.Contains(r.ReturnFor.Year))
                .Select(r => new
                {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    Year = r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Filter to only the exact (SaccoCsNumber, Year) pairs we need
            var requiredPairs = activeAssignments
                .Select(a => (a.Return.SaccoId, a.Return.ReturnFor.Year))
                .Distinct()
                .ToList();

            var filteredLinks = allLinks
                .Where(x => requiredPairs.Contains((x.SaccoId, x.Year)))
                .ToList();

            // 5. Group into a map: (SaccoCsNumber,Year) → Dictionary<Id,PreviousVersionId>
            var linkMap = filteredLinks
                .GroupBy(x => (x.SaccoId, x.Year))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.Id, x => x.PreviousVersionId ?? "")
                );

            // 6. Build DTOs in memory
            var results = new List<SubmittedReturnDTO>(activeAssignments.Count);
            foreach (var assignment in activeAssignments)
            {
                var r = assignment.Return;
                var key = (r.SaccoId, r.ReturnFor.Year);

                if (!linkMap.TryGetValue(key, out var prevMap))
                    continue;

                // walk the version chain
                var chain = new List<string>();
                var currentId = r.Id;
                while (prevMap.TryGetValue(currentId, out var prevId)
                       && !string.IsNullOrEmpty(prevId))
                {
                    chain.Add(prevId);
                    currentId = prevId;
                }

                var isAnyLate = returnsHelper.CheckLateReturnsNWDT(r);

                results.Add(new SubmittedReturnDTO
                {
                    Id = r.Id,
                    ReturnsFor = r.ReturnFor.Year.ToString(),
                    SaccoId = r.SaccoId,
                    IsConsistent = !r.IsNotConsistent,
                    ConsistentErrorMessage = r.ConsistentErrorMessage,
                    SaccoName = r.SaccoName,
                    SubmittedAt = r.SubmittedAt,
                    LateNessStatus = isAnyLate ? "Late" : "On Time",
                    TotalReturns = returnsHelper.CountPopulatedReturnsNWDT(r),
                    TotalLateReturns = returnsHelper.CountLateReturnsNWDT(r),
                    VersionNumber = r.VersionNumber,
                    PreviousVersionIds = chain
                });
            }

            return Ok(results);
        }


        /*  [HttpGet("GetReturnDetails/{returnId}")]
          public async Task<ActionResult<ReturnDetailsDTO>> GetReturnDetails(string returnId)
          {
              try
              {
                  var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                  var helper = new ReturnsHelper(_context);

                  Return? hdr = await _context.Returns
                      .AsNoTracking()
                      .Where(r => r.Id == returnId)
                      .FirstOrDefaultAsync();
                  if (hdr == null)
                  {
                      return new ReturnDetailsDTO();
                  }

                  var ApprovalStatus = await _workflowService.GetReturnStatus(hdr.Id);


                  var capEntity = await _context.DTCapitalAdequacyReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(ca => ca.ReturnId == returnId);

                  CapitalAdequacyDTO? capitalAdequacy = null;
                  int capitalDaysLate = 0;
                  if (capEntity != null)
                  {
                      capitalDaysLate = capEntity.DaysLateBy;
                      capitalAdequacy = new CapitalAdequacyDTO
                      {
                          FormId = capEntity.FormId ?? string.Empty,
                          RequiresResubmission = capEntity.RequiresResubmission,
                          ShareCapital = capEntity.ShareCapital,
                          StatutoryReserves = capEntity.StatutoryReserves,
                          RetainedEarningsAccumulatedLosses = capEntity.RetainedEarningsAccumulatedLosses,
                          NetSurplusAfterTaxCurrentYearToDate = capEntity.NetSurplusAfterTaxCurrentYearToDate,
                          CapitalGrantsEquityInNature = capEntity.CapitalGrantsEquityInNature,
                          GeneralReserves = capEntity.GeneralReserves,
                          OtherReserves = capEntity.OtherReserves,
                          SubTotalCoreCapital = capEntity.SubTotalCoreCapital,
                          InvestmentsInSubsidiaryAndEquityInstruments = capEntity.InvestmentsInSubsidiaryAndEquityInstruments,
                          OtherDeductions = capEntity.OtherDeductions,
                          TotalDeductions = capEntity.TotalDeductions,
                          CoreCapital = capEntity.CoreCapital,
                          InstitutionalCapital = capEntity.InstitutionalCapital,
                          CashLocalAndForeignCurrency = capEntity.CashLocalAndForeignCurrency,
                          GovernmentSecurities = capEntity.GovernmentSecurities,
                          DepositsAndBalancesAtOtherInstitutions = capEntity.DepositsAndBalancesAtOtherInstitutions,
                          LoansAndAdvances = capEntity.LoansAndAdvances,
                          Investments = capEntity.Investments,
                          PropertyAndEquipment = capEntity.PropertyAndEquipment,
                          OtherAssets = capEntity.OtherAssets,
                          TotalOnBalanceSheetAssets = capEntity.TotalOnBalanceSheetAssets,
                          TotalAssetsPerBalanceSheet = capEntity.TotalAssetsPerBalanceSheet,
                          Difference = capEntity.Difference,
                          CoreCapitalToAssetsRatio = capEntity.CoreCapitalToAssetsRatio,
                          CoreCapitalToAssetsRatioExcessDeficiency = capEntity.CoreCapitalToAssetsRatioExcessDeficiency,
                          InstitutionalCapitalToAssetsRatio = capEntity.InstitutionalCapitalToAssetsRatio,
                          InstitutionalCapitalToAssetsRatioExcessDeficiency = capEntity.InstitutionalCapitalToAssetsRatioExcessDeficiency,
                          CoreCapitalToDepositsRatio = capEntity.CoreCapitalToDepositsRatio,
                          CoreCapitalToDepositsRatioExcessDeficiency = capEntity.CoreCapitalToDepositsRatioExcessDeficiency,
                          FilePath = $"{baseUrl}{capEntity.FilePath}",
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTCapitalAdequacyReturn>(capEntity.ReturnId, hdr.SaccoType)
                      };
                  }

                  // 2-b  Deposit-Range Return (multi-row)
                  var depositEntities = await _context.DepositReturns
                      .AsNoTracking()
                      .Where(dr => dr.ReturnId == returnId)
                      .ToListAsync();

                  int depositDaysLate = depositEntities.FirstOrDefault()?.DaysLateBy ?? 0;
                  DepositReturnDto? depositReturn = null;
                  if (depositEntities.Any())
                  {
                      depositReturn = new DepositReturnDto
                      {
                          FormId = depositEntities.FirstOrDefault()?.FormId ?? string.Empty,
                          RequiresResubmission = depositEntities.FirstOrDefault()?.RequiresResubmission ?? false,
                          //FilePath = depositEntities.FirstOrDefault()?.FilePath ?? string.Empty,
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DepositReturn>(depositEntities.FirstOrDefault()?.ReturnId ?? string.Empty, hdr.SaccoType),
                          //NWDTDepositReturnData = new List<NWDTDepositReturnData>()
                      };
                      foreach (var dr in depositEntities)
                      {
                          depositReturn.DepositReturnData.Add(new DTOs.Returns_Submission.Returns_Submission.DT.DepositReturnData
                          {
                              RangeName = dr.RangeName,
                              DepositType = dr.DepositType,
                              NumberOfAccounts = dr.NumberOfAccounts,
                              Amount = dr.AmountInKshs000
                          });
                      }
                  }

                  // 2-c  Comprehensive-Income (single row)
                  var incomeEntity = await _context.DTComprehensiveIncomeReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(ci => ci.ReturnId == returnId);

                  ComprehesiveIncomeStatementDTO? incomeStatement = null;
                  int incomeDaysLate = 0;
                  if (incomeEntity != null)
                  {
                      incomeDaysLate = incomeEntity.DaysLateBy;
                      incomeStatement = new ComprehesiveIncomeStatementDTO
                      {
                          FormId = incomeEntity.FormId ?? string.Empty,
                          RequiresResubmission = incomeEntity.RequiresResubmission,
                          InterestOnLoanPortfolio = incomeEntity.InterestOnLoanPortfolio,
                          FeesAndCommissionOnLoanPortfolio = incomeEntity.FeesAndCommissionOnLoanPortfolio,
                          GovernmentSecurities = incomeEntity.GovernmentSecurities,
                          DepositsWithBanks = incomeEntity.DepositsWithBanks,
                          OtherInvestments = incomeEntity.OtherInvestments,
                          OtherOperatingIncome = incomeEntity.OtherOperatingIncome,
                          InterestExpenseOnDeposits = incomeEntity.InterestExpenseOnDeposits,
                          CostOfExternalBorrowings = incomeEntity.CostOfExternalBorrowings,
                          DividendExpenses = incomeEntity.DividendExpenses,
                          OtherFinancialExpense = incomeEntity.OtherFinancialExpense,
                          FeesAndCommissionExpense = incomeEntity.FeesAndCommissionExpense,
                          OtherExpense = incomeEntity.OtherExpense,
                          ProvisionForLoanLosses = incomeEntity.ProvisionForLoanLosses,
                          ValueOfLoansRecovered = incomeEntity.ValueOfLoansRecovered,
                          PersonnelExpenses = incomeEntity.PersonnelExpenses,
                          GovernanceExpenses = incomeEntity.GovernanceExpenses,
                          MarketingExpenses = incomeEntity.MarketingExpenses,
                          DepreciationAndAmortization = incomeEntity.DepreciationAndAmortization,
                          AdministrativeExpenses = incomeEntity.AdministrativeExpenses,
                          NonOperatingIncome = incomeEntity.NonOperatingIncome,
                          NonOperatingExpense = incomeEntity.NonOperatingExpense,
                          Taxes = incomeEntity.Taxes,
                          Donations = incomeEntity.Donations,
                          FilePath = $"{baseUrl}{incomeEntity.FilePath}",
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTComprehensiveIncomeReturn>(incomeEntity.ReturnId, hdr.SaccoType)
                      };
                  }

                  // 2-d  Financial-Position (single row)
                  var balanceEntity = await _context.DTFinancialPositionReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(fp => fp.ReturnId == returnId);

                  FinancialPositionDTO? financialPosition = null;
                  int balanceDaysLate = 0;
                  if (balanceEntity != null)
                  {
                      balanceDaysLate = balanceEntity.DaysLateBy;
                      financialPosition = new FinancialPositionDTO
                      {
                          FormId = balanceEntity.FormId ?? string.Empty,
                          RequiresResubmission = balanceEntity.RequiresResubmission,
                          CashInHand = balanceEntity.CashInHand,
                          CashAtBank = balanceEntity.CashAtBank,
                          PrepaymentsAndSundryReceivables = balanceEntity.PrepaymentsAndSundryReceivables,
                          GovernmentSecurities = balanceEntity.GovernmentSecurities,
                          OtherSecurities = balanceEntity.OtherSecurities,
                          BalancesWithOtherSaccos = balanceEntity.BalancesWithOtherSaccos,
                          InvestmentsInCompanies = balanceEntity.InvestmentsInCompanies,
                          GrossLoanPortfolio = balanceEntity.GrossLoanPortfolio,
                          AllowanceForLoanLoss = balanceEntity.AllowanceForLoanLoss,
                          TaxRecoverable = balanceEntity.TaxRecoverable,
                          DeferredTaxAssets = balanceEntity.DeferredTaxAssets,
                          RetirementBenefitAssets = balanceEntity.RetirementBenefitAssets,
                          InvestmentProperties = balanceEntity.InvestmentProperties,
                          PropertyAndEquipment = balanceEntity.PropertyAndEquipment,
                          PrepaidLeaseRentals = balanceEntity.PrepaidLeaseRentals,
                          IntangibleAssets = balanceEntity.IntangibleAssets,
                          OtherAssets = balanceEntity.OtherAssets,
                          SavingsDeposits = balanceEntity.SavingsDeposits,
                          ShortTermDeposits = balanceEntity.ShortTermDeposits,
                          NonWithdrawableDeposits = balanceEntity.NonWithdrawableDeposits,
                          TaxPayable = balanceEntity.TaxPayable,
                          DividendsPayable = balanceEntity.DividendsPayable,
                          DeferredTaxLiability = balanceEntity.DeferredTaxLiability,
                          RetirementBenefitsLiability = balanceEntity.RetirementBenefitsLiability,
                          OtherLiabilities = balanceEntity.OtherLiabilities,
                          ExternalBorrowings = balanceEntity.ExternalBorrowings,
                          ShareCapital = balanceEntity.ShareCapital,
                          CapitalGrants = balanceEntity.CapitalGrants,
                          PriorYearsRetainedEarnings = balanceEntity.PriorYearsRetainedEarnings,
                          CurrentYearSurplus = balanceEntity.CurrentYearSurplus,
                          StatutoryReserve = balanceEntity.StatutoryReserve,
                          FilePath = $"{baseUrl}{balanceEntity.FilePath}",
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTFinancialPositionReturn>(balanceEntity.ReturnId, hdr.SaccoType)
                      };
                  }

                  // 2-e  Liquidity (single row)
                  var liquidityEntity = await _context.DTLiquidityReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(liq => liq.ReturnId == returnId);

                  LiquidityStatementDTO? liquidityStatement = null;
                  int liquidityDaysLate = 0;
                  if (liquidityEntity != null)
                  {
                      liquidityDaysLate = liquidityEntity.DaysLateBy;
                      liquidityStatement = new LiquidityStatementDTO
                      {
                          FormId = capEntity.FormId ?? string.Empty,
                          RequiresResubmission = liquidityEntity.RequiresResubmission,
                          LocalNotesAndCoins = liquidityEntity.LocalNotesAndCoins,
                          ForeignNotesAndCoins = liquidityEntity.ForeignNotesAndCoins,
                          BalancesWithCommercialBanks = liquidityEntity.BalancesWithCommercialBanks,
                          TimeDepositsWithBanksMoreThan90Days = liquidityEntity.TimeDepositsWithBanksMoreThan90Days,
                          OverdraftsAndMaturedLoans = liquidityEntity.OverdraftsAndMaturedLoans,
                          BalancesWithOtherSaccoSocieties = liquidityEntity.BalancesWithOtherSaccoSocieties,
                          BalancesWithOtherFinancialInstitutions =
                              liquidityEntity.BalancesWithOtherFinancialInstitutions,
                          BalancesDueToOtherSaccoSocieties = liquidityEntity.BalancesDueToOtherSaccoSocieties,
                          BalancesDueToFinancialInstitutions =
                              liquidityEntity.BalancesDueToFinancialInstitutions,
                          TreasuryBills = liquidityEntity.TreasuryBills,
                          TreasuryBonds = liquidityEntity.TreasuryBonds,
                          DepositsFromMembers = liquidityEntity.DepositsFromMembers,
                          DepositsFromOtherSources = liquidityEntity.DepositsFromOtherSources,
                          MaturedLiabilities = liquidityEntity.MaturedLiabilities,
                          LiabilitiesMaturing91Days = liquidityEntity.LiabilitiesMaturing91Days,
                          TotalNotesAndCoins = liquidityEntity.TotalNotesAndCoins,
                          TotalGovernmentSecurities = liquidityEntity.TotalGovernmentSecurities,
                          NetLiquidAssets = liquidityEntity.NetLiquidAssets,
                          TotalDeposits = liquidityEntity.TotalDeposits,
                          TotalOtherLiabilities = liquidityEntity.TotalOtherLiabilities,
                          LiquidityRatio = liquidityEntity.LiquidityRatio,
                          LiquidityRatioExcessDeficit = liquidityEntity.LiquidityRatioExcessDeficit,
                          NetFinancialInstitutionBalances = liquidityEntity.NetFinancialInstitutionBalances,
                          NetBankBalances = liquidityEntity.NetBankBalances,
                          FilePath = $"{baseUrl}{liquidityEntity.FilePath}",
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTLiquidityReturn>(liquidityEntity.ReturnId, hdr.SaccoType)
                      };
                  }

                  // 2-f  Risk classification (multi-row)
                  var riskEntities = await _context.DTRiskClassificationReturns
                      .AsNoTracking()
                      .Where(rc => rc.ReturnId == returnId)
                      .ToListAsync();

                  int riskDaysLate = riskEntities.FirstOrDefault()?.DaysLateBy ?? 0;
                  var riskClassifications = new RiskClassificationDTO();

                  if (riskEntities.Any())
                  {
                      var firstEntity = riskEntities.First();
                      riskClassifications.FormId = firstEntity.FormId ?? string.Empty;
                      riskClassifications.RequiresResubmission = firstEntity.RequiresResubmission;
                      //riskClassifications.FilePath = firstEntity.FilePath;
                      riskClassifications.PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTRiskClassificationReturn>(firstEntity.ReturnId, hdr.SaccoType);

                      foreach (var rc in riskEntities)
                      {
                          riskClassifications.RiskClassificationData.Add(new RiskClassificationData
                          {
                              LoanType = rc.LoanType,
                              Classification = rc.Classification,
                              NumberOfAccounts = rc.NumberOfAccounts,
                              OutstandingLoanPortfolio = rc.OutstandingLoanPortfolio,
                              RequiredProvision = rc.RequiredProvision,
                              RequiredProvisionAmount = rc.RequiredProvisionAmount,
                              FilePath = $"{baseUrl}{rc.FilePath}"
                          });
                      }
                  }

                  // 2-g  Other returns (multi-row, simple)
                  var otherReturnsEntities = await _context.OtherReturns
                      .AsNoTracking()
                      .Where(o => o.ReturnId == returnId)
                      .ToListAsync();

                  var otherReturns = new List<OtherReturnDTO>();
                  foreach (var o in otherReturnsEntities)
                  {
                      otherReturns.Add(new OtherReturnDTO
                      {
                          FormName = o.FormName,
                          FileUrl = o.FileUrl,
                          RequiresResubmission = o.RequiresResubmission,
                          FormId = o.FormId ?? string.Empty,
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<OtherReturn>(o.ReturnId, hdr.SaccoType)
                      });
                  }

                  // 2-h  Investment (single row)
                  var investmentEntity = await _context.DTInvestmentReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(inv => inv.ReturnId == returnId);

                  var ApprovalComments = await _context.ApprovalActions
                                            .AsNoTracking()
                                            .Where(o => o.ReturnId == returnId)
                                            .ToListAsync();

                  InvestmentReturnDTO? investment = null;
                  int investmentDaysLate = 0;
                  if (investmentEntity != null)
                  {
                      investmentDaysLate = investmentEntity.DaysLateBy;
                      investment = new InvestmentReturnDTO
                      {
                          FormId = investmentEntity.FormId ?? string.Empty,
                          RequiresResubmission = investmentEntity.RequiresResubmission,
                          CoreCapital = investmentEntity.CoreCapital,
                          TotalAssets = investmentEntity.TotalAssets,
                          TotalDeposits = investmentEntity.TotalDeposits,
                          NonEarningAssets = investmentEntity.NonEarningAssets,
                          FinancialInvestments = investmentEntity.FinancialInvestments,
                          LandAndBuildings = investmentEntity.LandAndBuildings,
                          LandBuildingsToTotalAssetsRatio = investmentEntity.LandBuildingsToTotalAssetsRatio,
                          LandBuildingsRatioExcessDeficiency = investmentEntity.LandBuildingsRatioExcessDeficiency,
                          NonEarningAssetsToTotalAssetsRatio = investmentEntity.NonEarningAssetsToTotalAssetsRatio,
                          NonEarningAssetsRatioExcessDeficiency = investmentEntity.NonEarningAssetsRatioExcessDeficiency,
                          FinancialInvestmentsToCoreCapitalRatio = investmentEntity.FinancialInvestmentsToCoreCapitalRatio,
                          FinancialInvestmentsToCoreCapitalExcessDeficiency =
                              investmentEntity.FinancialInvestmentsToCoreCapitalExcessDeficiency,
                          FinancialInvestmentsToDepositsRatio = investmentEntity.FinancialInvestmentsToDepositsRatio,
                          FinancialInvestmentsToDepositsExcessDeficiency =
                              investmentEntity.FinancialInvestmentsToDepositsExcessDeficiency,
                          FilePath = $"{baseUrl}{investmentEntity.FilePath}",
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTInvestmentReturn>(investmentEntity.ReturnId, hdr.SaccoType)
                      };
                  }

                  var managementEntity = await _context.ManagementReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(m => m.ReturnId == returnId);

                  ManagementReturnDTO? managementReturn = null;
                  if (managementEntity != null)
                  {
                      managementReturn = new ManagementReturnDTO
                      {
                          SaccoCsNumber = managementEntity.SaccoCsNumber,


                          GovernanceStructureScore = managementEntity.GorvenanceStructureScore,
                          GovernanceStructureWeight = managementEntity.GorvenanceStructureWeight,
                          GovernanceStructureWeightedScore = managementEntity.GorvenanceStructureWeightedScore,

                          InternalControlsScore = managementEntity.InternalControlsScore,
                          InternalControlsWeight = managementEntity.InternalControlsWeight,
                          InternalControlsWeightedScore = managementEntity.InternalControlsWeightedScore,

                          ComplianceWithLawsScore = managementEntity.ComplianceWithLawsAndRegulationsScore,
                          ComplianceWithLawsWeight = managementEntity.ComplianceWithLawsAndRegulationsWeight,
                          ComplianceWithLawsWeightedScore =
                              managementEntity.ComplianceWithLawsAndRegulationsWeightedScore,

                          MemberProtectionScore = managementEntity.MemberProtectionScore,
                          MemberProtectionWeight = managementEntity.MemberProtectionWeight,
                          MemberProtectionWeightedScore = managementEntity.MemberProtectionWeightedScore,

                          AdequacyOfMISScore = managementEntity.AdequacyOfMISScore,
                          AdequacyOfMISWeight = managementEntity.AdequacyOfMISWeight,
                          AdequacyOfMISWeightedScore = managementEntity.AdequacyOfMISWeightedScore,

                          OverallRiskProfileScore = managementEntity.OverallRiskProfileScore,
                          OverallRiskProfileWeight = managementEntity.OverallRiskProfileWeight,
                          OverallRiskProfileWeightedScore =
                              managementEntity.OverallRiskProfileWeightedScore,

                          MRating = managementEntity.MRating,
                          //PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<ManagementReturn>(managementEntity.ReturnId, hdr.SaccoType)
                      };
                  }


                  var dto = new ReturnDetailsDTO
                  {
                      // header
                      Id = hdr.Id,
                      SaccoName = hdr.SaccoName,
                      IsConsistent = hdr.IsNotConsistent,
                      ConsistentErrorMessage = hdr.ConsistentErrorMessage,
                      SubmittedAt = hdr.SubmittedAt,

                      // days-late fields
                      CapitalAdequacyDaysLate = capitalDaysLate,
                      DepositReturnDaysLate = depositDaysLate,
                      StatementOfComprehensiveIncomeDaysLate = incomeDaysLate,
                      StatementOfFinancialPositionDaysLate = balanceDaysLate,
                      LiquidityReturnDaysLate = liquidityDaysLate,
                      RiskClassificationDaysLate = riskDaysLate,
                      InvestmentReturnDaysLate = investmentDaysLate,

                      // full sections
                      CapitalAdequacy = capitalAdequacy,
                      DepositReturn = depositReturn,
                      IncomeStatement = incomeStatement,
                      FinancialPosition = financialPosition,
                      LiquidityStatement = liquidityStatement,
                      RiskClassifications = riskClassifications,
                      OtherReturns = otherReturns,
                      Investment = investment,
                      ManagementReturn = managementReturn,
                      ReturnStatus = ApprovalStatus,

                      Year = hdr.Year.ToString(),
                      VersionNumber = hdr.VersionNumber,
                      IsActiveVersion = hdr.IsActiveVersion,
                      AmendmentDate = hdr.AmendmentDate,
                      CanReportBeViewed = hdr.CanReportBeViewed
                  };


                  List<CommentDetails> commentDetails = new List<CommentDetails>();

                  foreach (var comment in ApprovalComments)
                  {

                      var UserDetails = await complianceService.GetUserById(comment.UserId);
                      var Name = string.Empty;
                      if (UserDetails == null)
                      {
                      }
                      else
                      {
                          Name = UserDetails.FullName ?? string.Empty;
                      }
                      commentDetails.Add(new CommentDetails
                      {
                          Comment = comment.Comment,
                          UserId = comment.UserId,
                          ApproverName = Name,
                          Status = comment.Status,
                          CreatedAt = comment.CreatedAt
                      });
                  }

                  dto.ApprovalComments = commentDetails;


                  // ───────────────────────────────────────────────────────────────
                  //  5.  Sectoral lending – separate table set
                  // ───────────────────────────────────────────────────────────────
                  var sectoral = await _context.SectoralLendingReports
                      .AsNoTracking()
                      .FirstOrDefaultAsync(x => x.ReturnId == returnId);

                  if (sectoral != null)
                  {
                      var sectoralData = await _context.SectoralLendingData
                          .AsNoTracking()
                          .Where(x => x.ReturnId == returnId)
                          .ToListAsync();

                      dto.SectoralLending = new SectoralLendingDTO
                      {
                          StartDate = sectoral.StartDate,
                          EndDate = sectoral.EndDate,
                          //PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<SectoralLendingReport>(sectoral.ReturnId, hdr.SaccoType),
                          SubSectorData = sectoralData.Select(sd => new SectoralLendingDataDTO
                          {
                              Amount = sd.Amount,
                              Category = sd.Category,
                              SubCategory = sd.SubCategory,
                              EconomicSectorName = sd.EconomicSectorName
                          }).ToList()
                      };
                  }

                  return dto;
              }
              catch (Exception ex)
              {
                  return StatusCode(500, CustomErrorHandler.HandleException(ex));
              }
          }*/

        [HttpGet("CalculateAnalysis/{periodId}/{saccoId}")]
        public async Task<ActionResult<CamelsRatingsDTO>> CalculateAnalysis(string periodId, string saccoId)
        {
            try
            {
               /* LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401, "Unauthorized");
                }*/

                var GroupToUse =  _context.RatingDefinations
                    .Where(r => r.SaccoType == "0" && r.RatingName =="CAELS")
                    .FirstOrDefault();

                if (GroupToUse == null)
                {
                    return BadRequest("Rating definition not found for the specified Sacco type");
                }

                var result = await camelsAnalysisService.CalculateAnalysisAsync(GroupToUse.Id,periodId, "7", "0" );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating CAMELS analysis");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }


        /* [HttpGet("GetPerfomanceReport/{returnId}")]
         public async Task<ActionResult<SaccoPerformanceReportDTO>> GetPerfomanceReport(string returnId)
         {
             try
             {
                 // Initialize report
                 var report = new SaccoPerformanceReportDTO();

                 // Find current return
                 var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId);
                 if (currentReturn == null)
                 {
                     return BadRequest("Return not found");
                 }

                 report.ReportDate = currentReturn.CreatedAt;

                 // Add prudential standards
                 report.PrudentialStandards.Add("CoreCapital", "≥10M");
                 report.PrudentialStandards.Add("CoreCapitalToTotalAssets", "≥10%");
                 report.PrudentialStandards.Add("InstitutionalCapitalToTotalAssets", "≥8%");
                 report.PrudentialStandards.Add("NPL", "<5%");
                 report.PrudentialStandards.Add("NonEarningAssets", "<10%");

                 // Get historical returns (2 most recent before current)
                 var historicalReturns = await _context.Returns
                     .Where(r => r.CreatedAt < currentReturn.CreatedAt && r.Id != returnId)
                     .ToListAsync();

                 historicalReturns = historicalReturns
                     .OrderByDescending(r => r.CreatedAt)
                     .Take(2)
                     .ToList();

                 // Combine current return with historical returns
                 var allReturns = new[] { currentReturn }.Concat(historicalReturns);

                 // Process each return period
                 foreach (var returnPeriod in allReturns)
                 {
                     // Fetch all required data for this return period
                     var balanceSheet = await _context.DTFinancialPositionReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var incomeStatement = await _context.DTComprehensiveIncomeReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var capitalReturn = await _context.DTCapitalAdequacyReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var liquidityReturn = await _context.DTLiquidityReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var depositReturns = await _context.DepositReturns
                         .Where(x => x.ReturnId == returnPeriod.Id)
                         .ToListAsync();

                     var riskClassificationReturn = await _context.DTRiskClassificationReturns
                         .Where(x => x.ReturnId == returnPeriod.Id)
                         .ToListAsync();

                     var investmentReturn = await _context.DTInvestmentReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     // Use default objects if data is missing
                     var SavedCapitalAdequacy = capitalReturn ?? new DTCapitalAdequacyReturn();
                     var SavedLiquidityStatement = liquidityReturn ?? new DTLiquidityReturn();
                     var SavedDepositReturn = depositReturns ?? new List<DepositReturn>();
                     var SavedRiskClassification = riskClassificationReturn ?? new List<DTRiskClassificationReturn>();
                     var SavedInvestmentReturn = investmentReturn ?? new DTInvestmentReturn();
                     var SavedFinancialPositionStatement = balanceSheet ?? new DTFinancialPositionReturn();
                     var SavedComprehensiveStatement = incomeStatement ?? new DTComprehensiveIncomeReturn();

                     // Calculate ratios with null safety
                     decimal coreCapitalToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         coreCapitalToTotalAssets = SavedCapitalAdequacy.CoreCapital / SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal institutionalCapitalToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         institutionalCapitalToTotalAssets = SavedCapitalAdequacy.InstitutionalCapital / SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal equityInvestmentsToDeposits = 0;
                     if (SavedInvestmentReturn.TotalDeposits != 0)
                     {
                         equityInvestmentsToDeposits = SavedInvestmentReturn.FinancialInvestments / SavedInvestmentReturn.TotalDeposits;
                     }

                     decimal equityInvestmentsToCoreCapital = 0;
                     if (SavedInvestmentReturn.CoreCapital != 0)
                     {
                         equityInvestmentsToCoreCapital = SavedInvestmentReturn.FinancialInvestments / SavedInvestmentReturn.CoreCapital;
                     }

                     decimal yieldOnGrossLoans = 0;
                     if (SavedFinancialPositionStatement.GrossLoanPortfolio != 0)
                     {
                         yieldOnGrossLoans = (SavedComprehensiveStatement.InterestOnLoanPortfolio +
                             SavedComprehensiveStatement.FeesAndCommissionOnLoanPortfolio) /
                             SavedFinancialPositionStatement.GrossLoanPortfolio;
                     }

                     decimal totalExpenseToTotalIncome = 0;
                     if (SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations != 0)
                     {
                         totalExpenseToTotalIncome = (SavedComprehensiveStatement.InterestExpenseOnDeposits +
                                        SavedComprehensiveStatement.CostOfExternalBorrowings +
                                        SavedComprehensiveStatement.DividendExpenses +
                                        SavedComprehensiveStatement.OtherFinancialExpense +
                                        SavedComprehensiveStatement.FeesAndCommissionExpense +
                                        SavedComprehensiveStatement.OtherExpense) / SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations;
                     }
                     decimal netIncomeToAverageAssetsRatio = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         netIncomeToAverageAssetsRatio = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations / SavedFinancialPositionStatement.TotalAssets;
                     }
                     else
                     {
                         netIncomeToAverageAssetsRatio = 0;
                     }

                     decimal roa = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         roa = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations /
                             SavedFinancialPositionStatement.TotalAssets;
                     }

                     decimal OpertatingExpenseToFinancialOpex = 0;
                     if (SavedComprehensiveStatement.NetFinancialIncome != 0)
                     {
                         OpertatingExpenseToFinancialOpex = SavedComprehensiveStatement.TotalOperatingExpenses /
                             SavedComprehensiveStatement.NetFinancialIncome;
                     }

                     decimal liquidAssetsToShortTermLiabilities = 0;
                     decimal ShortTermLiabilities = SavedFinancialPositionStatement.SavingsDeposits
                            + SavedFinancialPositionStatement.ShortTermDeposits
                            + SavedFinancialPositionStatement.TaxPayable
                            + SavedFinancialPositionStatement.DividendsPayable
                            + SavedFinancialPositionStatement.DeferredTaxLiability
                            + SavedFinancialPositionStatement.RetirementBenefitsLiability
                            + SavedFinancialPositionStatement.OtherLiabilities;
                     decimal LiquidAssets = SavedFinancialPositionStatement.TotalCashAndCashEquivalent
                             + SavedFinancialPositionStatement.GovernmentSecurities
                             + SavedFinancialPositionStatement.OtherSecurities;
                     if (ShortTermLiabilities != 0)
                     {
                         liquidAssetsToShortTermLiabilities = LiquidAssets / ShortTermLiabilities;
                     }

                     decimal externalBorrowingToTotalAssets = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         externalBorrowingToTotalAssets = SavedFinancialPositionStatement.ExternalBorrowings /
                             SavedFinancialPositionStatement.TotalAssets;
                     }

                     decimal liquidAssetsToTotalAssets = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         liquidAssetsToTotalAssets = LiquidAssets /
                             SavedFinancialPositionStatement.TotalAssets;
                     }

                     decimal grossLoansToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         grossLoansToTotalAssets = SavedFinancialPositionStatement.GrossLoanPortfolio /
                             SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal grossLoansToDeposits = 0;
                     if (SavedFinancialPositionStatement.TotalDepositLiabilities != 0)
                     {
                         grossLoansToDeposits = SavedFinancialPositionStatement.GrossLoanPortfolio /
                             SavedFinancialPositionStatement.TotalDepositLiabilities;
                     }

                     decimal financialInvestmentsToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         financialInvestmentsToTotalAssets = SavedInvestmentReturn.FinancialInvestments /
                             SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal dividendsAndInterestOnDepositsToTotalIncome = 0;
                     if (SavedComprehensiveStatement.TotalFinancialIncome != 0)
                     {
                         dividendsAndInterestOnDepositsToTotalIncome = (SavedComprehensiveStatement.DividendExpenses +
                             SavedComprehensiveStatement.InterestExpenseOnDeposits) /
                             SavedComprehensiveStatement.TotalFinancialIncome;
                     }
                     decimal OperatingExpenseToFinancialIncomeRatio = 0;
                     if (SavedComprehensiveStatement.NetFinancialIncome != 0)
                     {
                         OperatingExpenseToFinancialIncomeRatio = SavedComprehensiveStatement.TotalOperatingExpenses / SavedComprehensiveStatement.NetFinancialIncome;
                     }
                     else
                     {
                         OperatingExpenseToFinancialIncomeRatio = 0;
                     }

                     var ShorttERM = SavedFinancialPositionStatement.NonWithdrawableDeposits + SavedFinancialPositionStatement.TaxPayable + SavedFinancialPositionStatement.DividendsPayable + SavedFinancialPositionStatement.DeferredTaxLiability + SavedFinancialPositionStatement.RetirementBenefitsLiability + SavedFinancialPositionStatement.OtherLiabilities;
                     var LIqAeet = SavedLiquidityStatement.NetLiquidAssets;
                     decimal LiquidAssetsToShortTermLiabilitiesRatio = 0;
                     // LiquidAssetsToShortTermLiabilitiesRatio
                     if (ShorttERM != 0)
                     {
                         LiquidAssetsToShortTermLiabilitiesRatio = LIqAeet / ShorttERM;
                     }
                     else
                     {
                         LiquidAssetsToShortTermLiabilitiesRatio = 0;
                     }
                     decimal EXB = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         EXB = SavedFinancialPositionStatement.ExternalBorrowings / SavedCapitalAdequacy.TotalAssets;
                     }
                     else
                     {
                         EXB = 0;
                     }


                     var periodData = new SaccoPerformanceReportDTO.PeriodData
                     {
                         PeriodLabel = returnPeriod.CreatedAt.ToString("dd MMMM, yyyy"),
                         PeriodType = "Returns",
                         PeriodDate = returnPeriod.CreatedAt,

                         CoreCapital = SavedCapitalAdequacy.CoreCapital,
                         CoreCapitalToTotalAssets = coreCapitalToTotalAssets,
                         InstitutionalCapitalToTotalAssets = institutionalCapitalToTotalAssets,

                         NonPerformingLoans = CalculateNonPerformingLoans(SavedRiskClassification),

                         NonEarningAssets = SavedFinancialPositionStatement.TotalAssets != 0
                             ? (SavedFinancialPositionStatement.PrepaymentsAndSundryReceivables
                              + SavedFinancialPositionStatement.TotalAccountsReceivables
                              + SavedFinancialPositionStatement.PropertyAndEquipment
                              + SavedFinancialPositionStatement.PrepaidLeaseRentals
                              + SavedFinancialPositionStatement.IntangibleAssets
                              + SavedFinancialPositionStatement.OtherAssets)
                                / SavedFinancialPositionStatement.TotalAssets
                             : 0,

                         EquityInvestmentsToDeposits = SavedFinancialPositionStatement.TotalDepositLiabilities != 0
                             ? SavedFinancialPositionStatement.InvestmentsInCompanies
                               / SavedFinancialPositionStatement.TotalDepositLiabilities
                             : 0,

                         EquityInvestmentsToCoreCapital = equityInvestmentsToCoreCapital,
                         NetIncomeToAverageAssets = netIncomeToAverageAssetsRatio,
                         YieldOnGrossLoans = yieldOnGrossLoans,
                         TotalExpenseToTotalIncome = totalExpenseToTotalIncome,
                         ROA = roa,
                         OPEX = OpertatingExpenseToFinancialOpex,
                         OpertatingExpenseToFinancialOpex = OperatingExpenseToFinancialIncomeRatio,
                         LiquidAssetsToShortTermLiabilities = liquidAssetsToShortTermLiabilities,
                         ExternalBorrowingToTotalAssets = EXB,
                         LiquidAssetsToTotalAssets = LiquidAssetsToShortTermLiabilitiesRatio,

                         GrossLoansToTotalAssets = grossLoansToTotalAssets,
                         GrossLoansToDeposits = grossLoansToDeposits,
                         FinancialInvestmentsToTotalAssets = financialInvestmentsToTotalAssets,
                         DividendsAndInterestOnDepositsToTotalIncome = dividendsAndInterestOnDepositsToTotalIncome,

                         TotalAssets = SavedCapitalAdequacy.TotalAssets,
                         TotalDeposits = SavedFinancialPositionStatement.TotalDepositLiabilities,
                         GrossLoansForm4 = CalculateGrossLoans(SavedRiskClassification),
                         GrossLoansForm6 = SavedFinancialPositionStatement.GrossLoanPortfolio,
                         InstitutionalCapital = SavedCapitalAdequacy.InstitutionalCapital,
                         PropertyAndEquipment = SavedCapitalAdequacy.PropertyAndEquipment,
                         EquityInvestments = SavedInvestmentReturn.FinancialInvestments,
                         FinancialInvestments = SavedCapitalAdequacy.Investments,
                         LiquidAssets = LIqAeet,
                         ShortTermLiabilities = SavedFinancialPositionStatement.SavingsDeposits
                         + SavedFinancialPositionStatement.ShortTermDeposits
                         + SavedFinancialPositionStatement.TaxPayable
                         + SavedFinancialPositionStatement.DividendsPayable
                         + SavedFinancialPositionStatement.DeferredTaxLiability
                         + SavedFinancialPositionStatement.RetirementBenefitsLiability
                         + SavedFinancialPositionStatement.OtherLiabilities,
                         ExternalBorrowing = SavedFinancialPositionStatement.ExternalBorrowings,
                         AverageGrossLoans = (SavedFinancialPositionStatement.GrossLoanPortfolio + SavedCapitalAdequacy.LoansAndAdvances) / 2,
                         TotalIncome = SavedComprehensiveStatement.TotalFinancialIncome,
                         NetFinancialIncome = SavedComprehensiveStatement.NetFinancialIncome,
                         DividendsAndInterestOnDeposits = SavedComprehensiveStatement.DividendExpenses + SavedComprehensiveStatement.InterestExpenseOnDeposits,
                         OperatingExpenses = SavedComprehensiveStatement.TotalOperatingExpenses,
                         InterestOnLoanPortfolioAndFeesCommission = SavedComprehensiveStatement.InterestOnLoanPortfolio + SavedComprehensiveStatement.FeesAndCommissionOnLoanPortfolio,
                         TotalExpenses = SavedComprehensiveStatement.TotalFinancialExpense + SavedComprehensiveStatement.TotalOperatingExpenses + SavedComprehensiveStatement.NonOperatingExpense,
                         NetIncome = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations,
                     };

                     // Add period data to report
                     report.Periods.Add(periodData);
                 }

                 // Add placeholder data if we don't have enough periods
                 if (report.Periods.Count < 2 && report.Periods.Count > 0)
                 {
                     var blank = new SaccoPerformanceReportDTO.PeriodData
                     {
                         PeriodLabel = "N/A",
                         PeriodType = "N/A",
                         PeriodDate = DateTime.Now,

                         CoreCapital = 0,
                         CoreCapitalToTotalAssets = 0,
                         InstitutionalCapitalToTotalAssets = 0,

                         NonPerformingLoans = 0,
                         NonEarningAssets = 0,
                         EquityInvestmentsToDeposits = 0,
                         EquityInvestmentsToCoreCapital = 0,

                         YieldOnGrossLoans = 0,
                         TotalExpenseToTotalIncome = 0,
                         ROA = 0,
                         OPEX = 0,

                         LiquidAssetsToShortTermLiabilities = 0,
                         ExternalBorrowingToTotalAssets = 0,
                         LiquidAssetsToTotalAssets = 0,

                         GrossLoansToTotalAssets = 0,
                         GrossLoansToDeposits = 0,
                         FinancialInvestmentsToTotalAssets = 0,
                         DividendsAndInterestOnDepositsToTotalIncome = 0,

                         TotalAssets = 0,
                         TotalDeposits = 0,
                         GrossLoansForm4 = 0,
                         GrossLoansForm6 = 0,
                         InstitutionalCapital = 0,
                         PropertyAndEquipment = 0,
                         EquityInvestments = 0,
                         FinancialInvestments = 0,
                         LiquidAssets = 0,
                         ShortTermLiabilities = 0,
                         ExternalBorrowing = 0,
                         AverageGrossLoans = 0,
                         TotalIncome = 0,
                         NetFinancialIncome = 0,
                         DividendsAndInterestOnDeposits = 0,
                         OperatingExpenses = 0,
                         InterestOnLoanPortfolioAndFeesCommission = 0,
                         TotalExpenses = 0,
                         NetIncome = 0,
                     };

                     report.Periods.Add(blank);
                 }

                 return report;
             }
             catch (System.Exception Ex)
             {
                 CustomErrorHandler.LogException(Ex);
                 return StatusCode(500, CustomErrorHandler.HandleException(Ex));
             }
         }*/

        /* [HttpGet("GetPerfomanceReportPdf/{returnId}")]
         public async Task<ActionResult<SaccoPerformanceReportDTO>> GetPerfomanceReportPdf(string returnId, [FromQuery] string? components)
         {
             try
             {
                 string selector = string.IsNullOrWhiteSpace(components)
                 ? "CAMELS"
                 : new string(components.ToUpperInvariant()
                                        .Where(c => "CAMELS".Contains(c))
                                        .Distinct()
                                        .ToArray());

                 // Initialize report
                 var report = new SaccoPerformanceReportDTO();

                 // Find current return
                 var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId);
                 if (currentReturn == null)
                 {
                     return BadRequest("Return not found");
                 }

                 report.ReportDate = currentReturn.CreatedAt;

                 // Add prudential standards
                 report.PrudentialStandards.Add("CoreCapital", "≥10M");
                 report.PrudentialStandards.Add("CoreCapitalToTotalAssets", "≥10%");
                 report.PrudentialStandards.Add("InstitutionalCapitalToTotalAssets", "≥8%");
                 report.PrudentialStandards.Add("NPL", "<5%");
                 report.PrudentialStandards.Add("NonEarningAssets", "<10%");

                 // Get historical returns (2 most recent before current)
                 var historicalReturns = await _context.Returns
                     .Where(r => r.CreatedAt < currentReturn.CreatedAt && r.Id != returnId)
                     .ToListAsync();

                 historicalReturns = historicalReturns
                     .OrderByDescending(r => r.CreatedAt)
                     .Take(2)
                     .ToList();

                 // Combine current return with historical returns
                 var allReturns = new[] { currentReturn }.Concat(historicalReturns);

                 // Process each return period
                 foreach (var returnPeriod in allReturns)
                 {
                     // Fetch all required data for this return period
                     var balanceSheet = await _context.DTFinancialPositionReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var incomeStatement = await _context.DTComprehensiveIncomeReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var capitalReturn = await _context.DTCapitalAdequacyReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var liquidityReturn = await _context.DTLiquidityReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var depositReturns = await _context.DepositReturns
                         .Where(x => x.ReturnId == returnPeriod.Id)
                         .ToListAsync();

                     var riskClassificationReturn = await _context.DTRiskClassificationReturns
                         .Where(x => x.ReturnId == returnPeriod.Id)
                         .ToListAsync();

                     var investmentReturn = await _context.DTInvestmentReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     var managementReturns = await _context.ManagementReturns
                         .FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                     // Use default objects if data is missing
                     var SavedCapitalAdequacy = capitalReturn ?? new DTCapitalAdequacyReturn();
                     var SavedLiquidityStatement = liquidityReturn ?? new DTLiquidityReturn();
                     var SavedDepositReturn = depositReturns ?? new List<DepositReturn>();
                     var SavedRiskClassification = riskClassificationReturn ?? new List<DTRiskClassificationReturn>();
                     var SavedInvestmentReturn = investmentReturn ?? new DTInvestmentReturn();
                     var SavedFinancialPositionStatement = balanceSheet ?? new DTFinancialPositionReturn();
                     var SavedComprehensiveStatement = incomeStatement ?? new DTComprehensiveIncomeReturn();

                     // Calculate ratios with null safety
                     decimal coreCapitalToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         coreCapitalToTotalAssets = SavedCapitalAdequacy.CoreCapital / SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal institutionalCapitalToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         institutionalCapitalToTotalAssets = SavedCapitalAdequacy.InstitutionalCapital / SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal equityInvestmentsToDeposits = 0;
                     if (SavedInvestmentReturn.TotalDeposits != 0)
                     {
                         equityInvestmentsToDeposits = SavedInvestmentReturn.FinancialInvestments / SavedInvestmentReturn.TotalDeposits;
                     }

                     decimal equityInvestmentsToCoreCapital = 0;
                     if (SavedInvestmentReturn.CoreCapital != 0)
                     {
                         equityInvestmentsToCoreCapital = SavedInvestmentReturn.FinancialInvestments / SavedInvestmentReturn.CoreCapital;
                     }

                     decimal yieldOnGrossLoans = 0;
                     if (SavedFinancialPositionStatement.GrossLoanPortfolio != 0)
                     {
                         yieldOnGrossLoans = (SavedComprehensiveStatement.InterestOnLoanPortfolio +
                             SavedComprehensiveStatement.FeesAndCommissionOnLoanPortfolio) /
                             SavedFinancialPositionStatement.GrossLoanPortfolio;
                     }

                     decimal totalExpenseToTotalIncome = 0;
                     if (SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations != 0)
                     {
                         totalExpenseToTotalIncome = (SavedComprehensiveStatement.InterestExpenseOnDeposits +
                                        SavedComprehensiveStatement.CostOfExternalBorrowings +
                                        SavedComprehensiveStatement.DividendExpenses +
                                        SavedComprehensiveStatement.OtherFinancialExpense +
                                        SavedComprehensiveStatement.FeesAndCommissionExpense +
                                        SavedComprehensiveStatement.OtherExpense) / SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations;
                     }
                     decimal netIncomeToAverageAssetsRatio = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         netIncomeToAverageAssetsRatio = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations / SavedFinancialPositionStatement.TotalAssets;
                     }
                     else
                     {
                         netIncomeToAverageAssetsRatio = 0;
                     }

                     decimal roa = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         roa = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations /
                             SavedFinancialPositionStatement.TotalAssets;
                     }

                     decimal OpertatingExpenseToFinancialOpex = 0;
                     if (SavedComprehensiveStatement.NetFinancialIncome != 0)
                     {
                         OpertatingExpenseToFinancialOpex = SavedComprehensiveStatement.TotalOperatingExpenses /
                             SavedComprehensiveStatement.NetFinancialIncome;
                     }

                     decimal liquidAssetsToShortTermLiabilities = 0;
                     decimal ShortTermLiabilities = SavedFinancialPositionStatement.SavingsDeposits
                            + SavedFinancialPositionStatement.ShortTermDeposits
                            + SavedFinancialPositionStatement.TaxPayable
                            + SavedFinancialPositionStatement.DividendsPayable
                            + SavedFinancialPositionStatement.DeferredTaxLiability
                            + SavedFinancialPositionStatement.RetirementBenefitsLiability
                            + SavedFinancialPositionStatement.OtherLiabilities;
                     decimal LiquidAssets = SavedFinancialPositionStatement.TotalCashAndCashEquivalent
                             + SavedFinancialPositionStatement.GovernmentSecurities
                             + SavedFinancialPositionStatement.OtherSecurities;
                     if (ShortTermLiabilities != 0)
                     {
                         liquidAssetsToShortTermLiabilities = LiquidAssets / ShortTermLiabilities;
                     }

                     decimal externalBorrowingToTotalAssets = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         externalBorrowingToTotalAssets = SavedFinancialPositionStatement.ExternalBorrowings /
                             SavedFinancialPositionStatement.TotalAssets;
                     }

                     decimal liquidAssetsToTotalAssets = 0;
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         liquidAssetsToTotalAssets = LiquidAssets /
                             SavedFinancialPositionStatement.TotalAssets;
                     }

                     decimal grossLoansToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         grossLoansToTotalAssets = SavedFinancialPositionStatement.GrossLoanPortfolio /
                             SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal grossLoansToDeposits = 0;
                     if (SavedFinancialPositionStatement.TotalDepositLiabilities != 0)
                     {
                         grossLoansToDeposits = SavedFinancialPositionStatement.GrossLoanPortfolio /
                             SavedFinancialPositionStatement.TotalDepositLiabilities;
                     }

                     decimal financialInvestmentsToTotalAssets = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         financialInvestmentsToTotalAssets = SavedInvestmentReturn.FinancialInvestments /
                             SavedCapitalAdequacy.TotalAssets;
                     }

                     decimal dividendsAndInterestOnDepositsToTotalIncome = 0;
                     if (SavedComprehensiveStatement.TotalFinancialIncome != 0)
                     {
                         dividendsAndInterestOnDepositsToTotalIncome = (SavedComprehensiveStatement.DividendExpenses +
                             SavedComprehensiveStatement.InterestExpenseOnDeposits) /
                             SavedComprehensiveStatement.TotalFinancialIncome;
                     }
                     decimal OperatingExpenseToFinancialIncomeRatio = 0;
                     if (SavedComprehensiveStatement.NetFinancialIncome != 0)
                     {
                         OperatingExpenseToFinancialIncomeRatio = SavedComprehensiveStatement.TotalOperatingExpenses / SavedComprehensiveStatement.NetFinancialIncome;
                     }
                     else
                     {
                         OperatingExpenseToFinancialIncomeRatio = 0;
                     }

                     var ShorttERM = SavedFinancialPositionStatement.NonWithdrawableDeposits + SavedFinancialPositionStatement.TaxPayable + SavedFinancialPositionStatement.DividendsPayable + SavedFinancialPositionStatement.DeferredTaxLiability + SavedFinancialPositionStatement.RetirementBenefitsLiability + SavedFinancialPositionStatement.OtherLiabilities;
                     var LIqAeet = SavedLiquidityStatement.NetLiquidAssets;
                     decimal LiquidAssetsToShortTermLiabilitiesRatio = 0;
                     // LiquidAssetsToShortTermLiabilitiesRatio
                     if (ShorttERM != 0)
                     {
                         LiquidAssetsToShortTermLiabilitiesRatio = LIqAeet / ShorttERM;
                     }
                     else
                     {
                         LiquidAssetsToShortTermLiabilitiesRatio = 0;
                     }
                     decimal EXB = 0;
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         EXB = SavedFinancialPositionStatement.ExternalBorrowings / SavedCapitalAdequacy.TotalAssets;
                     }
                     else
                     {
                         EXB = 0;
                     }


                     var periodData = new SaccoPerformanceReportDTO.PeriodData
                     {
                         PeriodLabel = returnPeriod.CreatedAt.ToString("dd MMMM, yyyy"),
                         PeriodType = "Returns",
                         PeriodDate = returnPeriod.CreatedAt,

                         CoreCapital = SavedCapitalAdequacy.CoreCapital,
                         CoreCapitalToTotalAssets = coreCapitalToTotalAssets,
                         InstitutionalCapitalToTotalAssets = institutionalCapitalToTotalAssets,

                         NonPerformingLoans = CalculateNonPerformingLoans(SavedRiskClassification),

                         NonEarningAssets = SavedFinancialPositionStatement.TotalAssets != 0
                             ? (SavedFinancialPositionStatement.PrepaymentsAndSundryReceivables
                              + SavedFinancialPositionStatement.TotalAccountsReceivables
                              + SavedFinancialPositionStatement.PropertyAndEquipment
                              + SavedFinancialPositionStatement.PrepaidLeaseRentals
                              + SavedFinancialPositionStatement.IntangibleAssets
                              + SavedFinancialPositionStatement.OtherAssets)
                                / SavedFinancialPositionStatement.TotalAssets
                             : 0,

                         EquityInvestmentsToDeposits = SavedFinancialPositionStatement.TotalDepositLiabilities != 0
                             ? SavedFinancialPositionStatement.InvestmentsInCompanies
                               / SavedFinancialPositionStatement.TotalDepositLiabilities
                             : 0,

                         EquityInvestmentsToCoreCapital = equityInvestmentsToCoreCapital,
                         NetIncomeToAverageAssets = netIncomeToAverageAssetsRatio,
                         YieldOnGrossLoans = yieldOnGrossLoans,
                         TotalExpenseToTotalIncome = totalExpenseToTotalIncome,
                         ROA = roa,
                         OPEX = OpertatingExpenseToFinancialOpex,
                         OpertatingExpenseToFinancialOpex = OperatingExpenseToFinancialIncomeRatio,
                         LiquidAssetsToShortTermLiabilities = liquidAssetsToShortTermLiabilities,
                         ExternalBorrowingToTotalAssets = EXB,
                         LiquidAssetsToTotalAssets = LiquidAssetsToShortTermLiabilitiesRatio,

                         GrossLoansToTotalAssets = grossLoansToTotalAssets,
                         GrossLoansToDeposits = grossLoansToDeposits,
                         FinancialInvestmentsToTotalAssets = financialInvestmentsToTotalAssets,
                         DividendsAndInterestOnDepositsToTotalIncome = dividendsAndInterestOnDepositsToTotalIncome,

                         TotalAssets = SavedCapitalAdequacy.TotalAssets,
                         TotalDeposits = SavedFinancialPositionStatement.TotalDepositLiabilities,
                         GrossLoansForm4 = CalculateGrossLoans(SavedRiskClassification),
                         GrossLoansForm6 = SavedFinancialPositionStatement.GrossLoanPortfolio,
                         InstitutionalCapital = SavedCapitalAdequacy.InstitutionalCapital,
                         PropertyAndEquipment = SavedCapitalAdequacy.PropertyAndEquipment,
                         EquityInvestments = SavedInvestmentReturn.FinancialInvestments,
                         FinancialInvestments = SavedCapitalAdequacy.Investments,
                         LiquidAssets = LIqAeet,
                         ShortTermLiabilities = SavedFinancialPositionStatement.SavingsDeposits
                         + SavedFinancialPositionStatement.ShortTermDeposits
                         + SavedFinancialPositionStatement.TaxPayable
                         + SavedFinancialPositionStatement.DividendsPayable
                         + SavedFinancialPositionStatement.DeferredTaxLiability
                         + SavedFinancialPositionStatement.RetirementBenefitsLiability
                         + SavedFinancialPositionStatement.OtherLiabilities,
                         ExternalBorrowing = SavedFinancialPositionStatement.ExternalBorrowings,
                         AverageGrossLoans = (SavedFinancialPositionStatement.GrossLoanPortfolio + SavedCapitalAdequacy.LoansAndAdvances) / 2,
                         TotalIncome = SavedComprehensiveStatement.TotalFinancialIncome,
                         NetFinancialIncome = SavedComprehensiveStatement.NetFinancialIncome,
                         DividendsAndInterestOnDeposits = SavedComprehensiveStatement.DividendExpenses + SavedComprehensiveStatement.InterestExpenseOnDeposits,
                         OperatingExpenses = SavedComprehensiveStatement.TotalOperatingExpenses,
                         InterestOnLoanPortfolioAndFeesCommission = SavedComprehensiveStatement.InterestOnLoanPortfolio + SavedComprehensiveStatement.FeesAndCommissionOnLoanPortfolio,
                         TotalExpenses = SavedComprehensiveStatement.TotalFinancialExpense + SavedComprehensiveStatement.TotalOperatingExpenses + SavedComprehensiveStatement.NonOperatingExpense,
                         NetIncome = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations,
                         //ManagementScore = managementReturns.MRating
                     };


                     periodData.MemberProtectionScore = managementReturns?.MemberProtectionScore ?? 0;
                     periodData.GovernanceStructureScore = managementReturns?.GorvenanceStructureScore ?? 0;
                     periodData.InternalControlsScore = managementReturns?.InternalControlsScore ?? 0;
                     periodData.ComplianceWithLawsScore = managementReturns?.ComplianceWithLawsAndRegulationsScore ?? 0;

                     // Add period data to report
                     report.Periods.Add(periodData);
                 }

                 // Add placeholder data if we don't have enough periods
                 if (report.Periods.Count < 2 && report.Periods.Count > 0)
                 {
                     var blank = new SaccoPerformanceReportDTO.PeriodData
                     {
                         PeriodLabel = "N/A",
                         PeriodType = "N/A",
                         PeriodDate = DateTime.Now,

                         CoreCapital = 0,
                         CoreCapitalToTotalAssets = 0,
                         InstitutionalCapitalToTotalAssets = 0,

                         NonPerformingLoans = 0,
                         NonEarningAssets = 0,
                         EquityInvestmentsToDeposits = 0,
                         EquityInvestmentsToCoreCapital = 0,

                         YieldOnGrossLoans = 0,
                         TotalExpenseToTotalIncome = 0,
                         ROA = 0,
                         OPEX = 0,

                         LiquidAssetsToShortTermLiabilities = 0,
                         ExternalBorrowingToTotalAssets = 0,
                         LiquidAssetsToTotalAssets = 0,

                         GrossLoansToTotalAssets = 0,
                         GrossLoansToDeposits = 0,
                         FinancialInvestmentsToTotalAssets = 0,
                         DividendsAndInterestOnDepositsToTotalIncome = 0,

                         TotalAssets = 0,
                         TotalDeposits = 0,
                         GrossLoansForm4 = 0,
                         GrossLoansForm6 = 0,
                         InstitutionalCapital = 0,
                         PropertyAndEquipment = 0,
                         EquityInvestments = 0,
                         FinancialInvestments = 0,
                         LiquidAssets = 0,
                         ShortTermLiabilities = 0,
                         ExternalBorrowing = 0,
                         AverageGrossLoans = 0,
                         TotalIncome = 0,
                         NetFinancialIncome = 0,
                         DividendsAndInterestOnDeposits = 0,
                         OperatingExpenses = 0,
                         InterestOnLoanPortfolioAndFeesCommission = 0,
                         TotalExpenses = 0,
                         NetIncome = 0,
                     };

                     report.Periods.Add(blank);
                 }

                 var approvals = await _context.ApprovalActions
                     .Where(r => r.ReturnId == returnId)
                     .Include(r => r.WorkFlowStep)
                     .ToListAsync();
                 report.approvalActions = approvals;

                 var reportBytes = ReportsHelper.GenerateSaccoPerformancePdfReport(report, selector);
                 var base64String = Convert.ToBase64String(reportBytes);

                 return Ok(new
                 {
                     pdfData = base64String,
                     fileName = $"SACCO_Performance_Report_{DateTime.Now:yyyyMMdd}.pdf"
                 });

                 //return File(reportBytes, "application/pdf", $"SACCO_Performance_Report_{DateTime.Now:yyyyMMdd}.pdf");

             }
             catch (System.Exception Ex)
             {
                 CustomErrorHandler.LogException(Ex);
                 return StatusCode(500, CustomErrorHandler.HandleException(Ex));
             }
         }*/


        /*[HttpGet("nwdt/GetReturnDetails/{returnId}")]
        public async Task<ActionResult<NWDTReturnDetailsDTO>> GetNwdtReturnDetails(string returnId)
        {
            try
            {

                var hdr = await _context.Returns
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == returnId);

                if (hdr == null) return new NWDTReturnDetailsDTO();

                var helper = new ReturnsHelper(_context);


                var ca = await _context.NWDTCapitalAdequacyReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(ca => ca.ReturnId == returnId);

                var deposits = await _context.NWDTDepositReturns
                                             .AsNoTracking()
                                             .Where(dr => dr.ReturnId == returnId)
                                             .ToListAsync();

                var fp = await _context.NWDTFinancialPositionReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(fp => fp.ReturnId == returnId);

                var inc = await _context.NWDTComprehensiveIncomeReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(ci => ci.ReturnId == returnId);

                var inv = await _context.NWDTInvestmentReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(inv => inv.ReturnId == returnId);

                var liq = await _context.NDWTLiquidityReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(liq => liq.ReturnId == returnId);

                var risks = await _context.NWDTRiskClassificationReturns
                                             .AsNoTracking()
                                             .Where(rc => rc.ReturnId == returnId)
                                             .ToListAsync();

                var others = await _context.OtherReturns
                                             .AsNoTracking()
                                             .Where(o => o.ReturnId == returnId)
                                             .ToListAsync();

                var mgr = await _context.ManagementReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(m => m.ReturnId == returnId);

                var ApprovalComments = await _context.ApprovalActions
                                             .AsNoTracking()
                                             .Where(o => o.ReturnId == returnId)
                                             .ToListAsync();

                var ApprovalStatus = await _workflowService.GetReturnStatus(hdr.Id);

                var dto = new NWDTReturnDetailsDTO
                {
                    // header
                    Id = hdr.Id,
                    SaccoName = hdr.SaccoName,
                    SubmittedAt = hdr.SubmittedAt,
                    IsConsistent = hdr.IsNotConsistent,
                    ConsistentErrorMessage = hdr.ConsistentErrorMessage,
                    ReturnStatus = ApprovalStatus,



                    // late counters
                    CapitalAdequacyDaysLate = ca?.DaysLateBy ?? 0,
                    DepositReturnDaysLate = deposits.FirstOrDefault()?.DaysLateBy ?? 0,
                    StatementOfComprehensiveIncomeDaysLate = inc?.DaysLateBy ?? 0,
                    StatementOfFinancialPositionDaysLate = fp?.DaysLateBy ?? 0,
                    LiquidityReturnDaysLate = liq?.DaysLateBy ?? 0,
                    RiskClassificationDaysLate = risks.FirstOrDefault()?.DaysLateBy ?? 0,
                    InvestmentReturnDaysLate = inv?.DaysLateBy ?? 0,

                    // capital adequacy
                    NWDTCapitalAdequacy = ca == null ? null : new NWDTCapitalAdequacyDTO
                    {
                        FormId = ca.FormId,
                        RequiresResubmission = ca.RequiresResubmission,
                        ShareCapital = ca.ShareCapital,
                        StatutoryReserves = ca.StatutoryReserves,
                        RetainedEarningsAccumulatedLosses = ca.RetainedEarnings,
                        NetSurplusAfterTaxCurrentYearToDate = ca.NetSurplusAfterTax,
                        CapitalGrantsEquityInNature = ca.CapitalGrants,
                        OtherReserves = ca.OtherReserves,
                        SubTotalCoreCapital = ca.SubTotalCoreCapital,
                        InvestmentsInSubsidiaryAndEquityInstruments = ca.InvestmentsInSubsidiary,
                        OtherDeductions = ca.OtherDeductions,
                        TotalDeductions = ca.TotalDeductions,
                        CoreCapital = ca.CoreCapital,
                        CashLocalAndForeignCurrency = ca.CashLocalForeign,
                        GovernmentSecurities = ca.GovernmentSecurities,
                        DepositsAndBalancesAtOtherInstitutions = ca.DepositsBalancesAtOtherInstitutions,
                        LoansAndAdvances = ca.LoansAndAdvances,
                        Investments = ca.Investments,
                        PropertyAndEquipment = ca.PropertyAndEquipment,
                        OtherAssets = ca.OtherAssets,
                        TotalOnBalanceSheetAssets = ca.TotalOnBalanceSheetAssets,
                        TotalAssetsPerBalanceSheet = ca.TotalAssetsPerBalanceSheet,
                        Difference = ca.DifferenceInAssets,
                        CoreCapitalToAssetsRatio = ca.CoreCapitalToAssetsRatio,
                        CoreCapitalToAssetsRatioExcessDeficiency =
                            ca.CoreCapitalToAssetsExcessDeficiency,
                        CoreCapitalToDepositsRatio = ca.CoreCapitalToDepositsRatio,
                        CoreCapitalToDepositsRatioExcessDeficiency =
                            ca.CoreCapitalToDepositsExcessDeficiency,
                        FilePath = ca.FilePath,
                        PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<NWDTCapitalAdequacyReturn>(ca.ReturnId, hdr.SaccoType)
                    },

                    // deposit list
                    NWDTDepositReturn = deposits.Count == 0
                        ? null
                        : new NWDTDepositReturnDto
                        {
                            FormId = deposits[0].FormId ?? string.Empty,
                            RequiresResubmission = deposits[0].RequiresResubmission,
                            PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<NWDTDepositReturn>(
                                                       deposits[0].ReturnId, hdr.SaccoType),

                            DepositReturnData = deposits.Select(dr => new NWDTDepositReturnData   // <-- fixed name
                            {
                                RangeName = dr.RangeName ?? string.Empty,
                                DepositType = dr.DepositType ?? string.Empty,
                                NumberOfAccounts = dr.NumberOfAccounts,
                                Amount = dr.AmountInKshs000,
                                FilePath = dr.FilePath ?? string.Empty
                            }).ToList()
                        },


                    // income statement
                    NWDTIncomeStatement = inc == null ? null : new NWDTComprehesiveIncomeStatementDTO
                    {
                        FormId = inc.FormId,
                        RequiresResubmission = inc.RequiresResubmission,
                        InterestOnLoanPortfolio = inc.InterestOnLoanPortfolio,
                        FeesAndCommissionOnLoanPortfolio = inc.FeesCommissionOnLoanPortfolio,
                        TotalFinancialIncomeFromLoans = inc.FinancialIncomeFromLoansPortfolio,
                        GovernmentSecurities = inc.GovernmentSecuritiesIncome,
                        DepositsWithBanks = inc.PlacementInBanksIncome,
                        OtherInvestments =
                            inc.CommercialPapersIncome +
                            inc.CollectiveInvestmentSchemesIncome +
                            inc.DerivativesIncome +
                            inc.EquityInvestmentsIncome +
                            inc.InvestmentInCompaniesIncome,
                        TotalFinancialIncomeFromInvestments = inc.FinancialIncomeFromInvestments,
                        TotalFinancialIncome = inc.FinancialIncome,
                        InterestExpenseOnDeposits = inc.InterestExpenseOnDeposits,
                        CostOfExternalBorrowings = inc.CostOfExternalBorrowings,
                        DividendExpenses = inc.DividendExpenses,
                        OtherFinancialExpense = inc.OtherFinancialExpense,
                        FeesAndCommissionExpense = inc.FeesCommissionExpense,
                        OtherExpense = inc.OtherExpense,
                        TotalFinancialExpense = inc.FinancialExpense,
                        NetFinancialIncome = inc.NetFinancialIncome,
                        ProvisionForLoanLosses = inc.ProvisionForLoanLosses,
                        ValueOfLoansRecovered = inc.ValueOfLoansRecovered,
                        NetAllowanceForLoanLoss = inc.AllowanceForLoanLoss,
                        PersonnelExpenses = inc.PersonnelExpenses,
                        GovernanceExpenses = inc.GovernanceExpenses,
                        MarketingExpenses = inc.MarketingExpenses,
                        DepreciationAndAmortization = inc.DepreciationAmortizationCharges,
                        AdministrativeExpenses = inc.AdministrativeExpenses,
                        TotalOperatingExpenses = inc.OperatingExpenses,
                        NetOperatingIncome = inc.NetOperatingIncome,
                        NonOperatingIncome = inc.NonOperatingIncome,
                        NonOperatingExpense = inc.NonOperatingExpense,
                        NetNonOperatingIncome = inc.NetNonOperatingIncome,
                        Taxes = inc.Taxes,
                        NetIncomeBeforeTaxes = inc.NetIncomeBeforeTaxes,
                        NetIncomeAfterTaxes = inc.NetIncomeAfterTaxesBeforeDonations,
                        Donations = inc.Donations,
                        NetIncomeAfterTaxesAndDonations = inc.NetIncomeAfterTaxesAndDonations,
                        FilePath = inc.FilePath,
                        PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<NWDTComprehensiveIncomeReturn>(inc.ReturnId, hdr.SaccoType)
                    },

                    // financial position
                    NWDTFinancialPosition = fp == null ? null : new NWDTFinancialPositionDTO
                    {
                        FormId = fp.FormId,
                        RequiresResubmission = fp.RequiresResubmission,
                        CashInHand = fp.CashInHand,
                        CashAtBank = fp.CashAtBank,
                        TotalCashAndCashEquivalent = fp.CashAndCashEquivalent,
                        PrepaymentsAndSundryReceivables = fp.PrepaymentsAndSundryReceivables,
                        GovernmentSecurities = fp.GovernmentSecurities,
                        OtherSecurities =
                            fp.PlacementInFinancialInstitutions +
                            fp.CommercialPapers +
                            fp.CollectiveInvestmentSchemes +
                            fp.Derivatives +
                            fp.EquityInvestments +
                            fp.InvestmentInCompanies,
                        BalancesWithOtherSaccos = 0M,
                        InvestmentsInCompanies = fp.InvestmentInCompanies,
                        TotalFinancialInvestments = fp.FinancialInvestments,
                        GrossLoanPortfolio = fp.GrossLoanPortfolio,
                        AllowanceForLoanLoss = fp.AllowanceForLoanLoss,
                        NetLoanPortfolio = fp.NetLoanPortfolio,
                        TaxRecoverable = fp.TaxRecoverable,
                        DeferredTaxAssets = fp.DeferredTaxAssets,
                        RetirementBenefitAssets = fp.RetirementBenefitAssets,
                        TotalAccountsReceivables = fp.AccountsReceivables,
                        InvestmentProperties = fp.InvestmentProperties,
                        PropertyAndEquipment = fp.PropertyAndEquipment,
                        PrepaidLeaseRentals = fp.PrepaidLeaseRentals,
                        IntangibleAssets = fp.IntangibleAssets,
                        OtherAssets = fp.OtherAssets,
                        TotalPropertyAndEquipment = fp.PropertyEquipmentOtherAssets,
                        TotalAssets = fp.TotalAssets,
                        SavingsDeposits = 0M,
                        ShortTermDeposits = 0M,
                        NonWithdrawableDeposits = fp.NonWithdrawableDeposits,
                        TotalDepositLiabilities = fp.TotalDepositLiabilities,
                        TaxPayable = fp.TaxPayable,
                        DividendsPayable = fp.DividendsPayable,
                        DeferredTaxLiability = fp.DeferredTaxLiability,
                        RetirementBenefitsLiability = fp.RetirementBenefitsLiability,
                        OtherLiabilities = fp.OtherLiabilities,
                        ExternalBorrowings = fp.ExternalBorrowings,
                        TotalAccountsPayable = fp.AccountsPayableOtherLiabilities,
                        TotalLiabilities = fp.TotalLiabilities,
                        ShareCapital = fp.ShareCapital,
                        CapitalGrants = fp.CapitalGrants,
                        PriorYearsRetainedEarnings = fp.PriorYearsRetainedEarnings,
                        CurrentYearSurplus = fp.CurrentYearSurplus,
                        TotalRetainedEarnings = fp.RetainedEarnings,
                        StatutoryReserve = fp.StatutoryReserve,
                        OtherReserves = fp.OtherReserves,
                        RevaluationReserves = fp.RevaluationReserves,
                        ProposedDividends = fp.ProposedDividends,
                        AdjustmentToEquity = fp.AdjustmentToEquity,
                        TotalOtherEquityAccounts = fp.OtherEquityAccounts,
                        TotalEquity = fp.TotalEquity,
                        FilePath = fp.FilePath,
                        PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<NWDTFinancialPositionReturn>(fp.ReturnId, hdr.SaccoType)
                    },

                    // liquidity
                    NWDTLiquidityStatement = liq == null ? null : new NWDTLiquidityStatementDTO
                    {
                        FormId = liq.FormId,
                        RequiresResubmission = liq.RequiresResubmission,
                        LocalNotesAndCoins = liq.LocalNotesAndCoins,
                        ForeignNotesAndCoins = liq.ForeignNotesAndCoins,
                        BalancesWithCommercialBanks = liq.BalancesWithCommercialBanks,
                        TimeDepositsWithBanksMoreThan90Days = liq.TimeDepositsWithBanksMoreThan90Days,
                        OverdraftsAndMaturedLoans = liq.OverdraftsAndMaturedLoans,
                        BalancesWithOtherSaccoSocieties = liq.BalancesWithOtherSaccoSocieties,
                        BalancesWithOtherFinancialInstitutions =
                            liq.BalancesWithOtherFinancialInstitutions,
                        BalancesDueToOtherSaccoSocieties = liq.BalancesDueToOtherSaccoSocieties,
                        BalancesDueToFinancialInstitutions = liq.BalancesDueToFinancialInstitutions,
                        TreasuryBills = liq.TreasuryBills,
                        TreasuryBonds = liq.TreasuryBondsBearerBonds,
                        MaturedLiabilities = liq.MaturedLiabilities,
                        LiabilitiesMaturing91Days = liq.LiabilitiesMaturing91Days,
                        TotalNotesAndCoins = liq.TotalNotesAndCoins,
                        TotalGovernmentSecurities = liq.TotalGovernmentSecurities,
                        NetLiquidAssets = liq.NetLiquidAssets,
                        TotalOtherLiabilities = liq.TotalOtherLiabilities,
                        LiquidityRatio = liq.LiquidityRatio,
                        LiquidityRatioExcessDeficit = liq.LiquidityRatioExcessDeficit,
                        NetFinancialInstitutionBalances = liq.NetFinancialInstitutionBalances,
                        NetBankBalances = liq.NetBankBalances,
                        PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<NWDTLiquidityReturn>(liq.ReturnId, hdr.SaccoType)
                    },

                    // risks
                    NWDTRiskClassifications = risks.Count == 0 ? null : new NWDTRiskClassificationDTO
                    {
                        FormId = risks[0].FormId,
                        RequiresResubmission = risks[0].RequiresResubmission,
                        PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<NWDTRiskClassificationReturn>(risks[0].ReturnId, hdr.SaccoType),
                        NWDTRiskClassificationData = risks.Select(rc => new NWDTRiskClassificationData
                        {
                            LoanType = rc.LoanType,
                            Classification = rc.Classification,
                            NumberOfAccounts = rc.NumberOfAccounts,
                            OutstandingLoanPortfolio = rc.OutstandingLoanPortfolio,
                            RequiredProvision = rc.RequiredProvision,
                            RequiredProvisionAmount = rc.RequiredProvisionAmount,
                            FilePath = rc.FilePath
                        }).ToList()
                    },

                    // other returns
                    OtherReturns = others.Select(o => new OtherReturnDTO
                    {
                        FormId = o.FormId,
                        RequiresResubmission = o.RequiresResubmission,
                        FormName = o.FormName,
                        FileUrl = o.FileUrl
                    }).ToList(),

                    // investment
                    NWDTInvestment = inv == null ? null : new NWDTInvestmentReturnDTO
                    {
                        FormId = inv.FormId,
                        RequiresResubmission = inv.RequiresResubmission,
                        CoreCapital = inv.CoreCapital,
                        TotalAssets = inv.TotalAssets,
                        TotalDeposits = inv.TotalDeposits,
                        NonEarningAssets = inv.NonEarningAssets,
                        FinancialInvestments = inv.FinancialAssets,
                        LandAndBuildings = inv.LandAndBuilding,

                        LandBuildingsToTotalAssetsRatio = inv.TotalAssets > 0
                            ? inv.LandAndBuilding / inv.TotalAssets * 100 : 0,
                        LandBuildingsRatioExcessDeficiency = inv.TotalAssets > 0
                            ? inv.LandAndBuilding / inv.TotalAssets * 100 -
                              inv.MaxLandBuildingToTotalAssetRequirement * 100 : 0,

                        NonEarningAssetsToTotalAssetsRatio = inv.TotalAssets > 0
                            ? inv.NonEarningAssets / inv.TotalAssets * 100 : 0,
                        NonEarningAssetsRatioExcessDeficiency = inv.TotalAssets > 0
                            ? inv.NonEarningAssets / inv.TotalAssets * 100 : 0,

                        FinancialInvestmentsToCoreCapitalRatio = inv.CoreCapital > 0
                            ? inv.FinancialAssets / inv.CoreCapital * 100 : 0,
                        FinancialInvestmentsToCoreCapitalExcessDeficiency = inv.CoreCapital > 0
                            ? inv.FinancialAssets / inv.CoreCapital * 100 -
                              inv.MaxFinancialInvestmentsToCoreCapital * 100 : 0,

                        FinancialInvestmentsToDepositsRatio = inv.TotalDeposits > 0
                            ? inv.FinancialAssets / inv.TotalDeposits * 100 : 0,
                        FinancialInvestmentsToDepositsExcessDeficiency = inv.TotalDeposits > 0
                            ? inv.FinancialAssets / inv.TotalDeposits * 100 -
                              inv.MaxEquityInvestmentsToTotalDeposits * 100 : 0,
                        FilePath = inv.FilePath,
                        PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<NWDTInvestmentReturn>(inv.ReturnId, hdr.SaccoType)
                    },

                    // management
                    ManagementReturn = mgr == null ? null : new ManagementReturnDTO
                    {
                        SaccoCsNumber = mgr.SaccoCsNumber,
                        GovernanceStructureScore = mgr.GorvenanceStructureScore,
                        GovernanceStructureWeight = mgr.GorvenanceStructureWeight,
                        GovernanceStructureWeightedScore = mgr.GorvenanceStructureWeightedScore,
                        InternalControlsScore = mgr.InternalControlsScore,
                        InternalControlsWeight = mgr.InternalControlsWeight,
                        InternalControlsWeightedScore = mgr.InternalControlsWeightedScore,
                        ComplianceWithLawsScore = mgr.ComplianceWithLawsAndRegulationsScore,
                        ComplianceWithLawsWeight = mgr.ComplianceWithLawsAndRegulationsWeight,
                        ComplianceWithLawsWeightedScore = mgr.ComplianceWithLawsAndRegulationsWeightedScore,
                        MemberProtectionScore = mgr.MemberProtectionScore,
                        MemberProtectionWeight = mgr.MemberProtectionWeight,
                        MemberProtectionWeightedScore = mgr.MemberProtectionWeightedScore,
                        AdequacyOfMISScore = mgr.AdequacyOfMISScore,
                        AdequacyOfMISWeight = mgr.AdequacyOfMISWeight,
                        AdequacyOfMISWeightedScore = mgr.AdequacyOfMISWeightedScore,
                        OverallRiskProfileScore = mgr.OverallRiskProfileScore,
                        OverallRiskProfileWeight = mgr.OverallRiskProfileWeight,
                        OverallRiskProfileWeightedScore = mgr.OverallRiskProfileWeightedScore,
                        MRating = mgr.MRating
                    },

                    Year = hdr.Year.ToString(),
                    VersionNumber = hdr.VersionNumber,
                    IsActiveVersion = hdr.IsActiveVersion,
                    AmendmentDate = hdr.AmendmentDate,
                    PreviousVersionId = hdr.PreviousVersionId,
                    CanReportBeViewed = hdr.CanReportBeViewed
                };

                // previous-version IDs
                dto.PreviousVersionIds =
                    await helper.GetPreviousVersionChoicesAsync(hdr);
                List<CommentDetails> commentDetails = new List<CommentDetails>();

                foreach (var comment in ApprovalComments)
                {

                    var UserDetails = await complianceService.GetUserById(comment.UserId);
                    var Name = string.Empty;
                    if (UserDetails == null)
                    {
                    }
                    else
                    {
                        Name = UserDetails.FullName;
                    }
                    commentDetails.Add(new CommentDetails
                    {
                        Comment = comment.Comment,
                        UserId = comment.UserId,
                        ApproverName = Name,
                        Status = comment.Status,
                        CreatedAt = comment.CreatedAt
                    });
                }

                dto.ApprovalComments = commentDetails;

                // sectoral lending
                var sectoral = await _context.SectoralLendingReports
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ReturnId == returnId);

                if (sectoral != null)
                {
                    var data = await _context.SectoralLendingData
                        .AsNoTracking()
                        .Where(x => x.ReturnId == returnId)
                        .ToListAsync();

                    dto.SectoralLending = new SectoralLendingDTO
                    {
                        StartDate = sectoral.StartDate,
                        EndDate = sectoral.EndDate,
                        SubSectorData = data.Select(sd => new SectoralLendingDataDTO
                        {
                            Amount = sd.Amount,
                            Category = sd.Category,
                            SubCategory = sd.SubCategory,
                            EconomicSectorName = sd.EconomicSectorName
                        }).ToList()
                    };
                }

                return dto;
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }*/

        [HttpGet("nwdt/CalculateAnalysis/{returnId}")]

        public async Task<ActionResult<CamelsRatingsDTO>> CalculateNwdtAnalysis(string returnId)
        {
            // Retrieve current return and historical returns
            var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId);

            if (currentReturn == null)
            {
                return BadRequest("Return not found");
            }

            try
            {
                //var result = await camelsAnalysisService.CalculateAnalysisAsync(currentReturn.Id, currentReturn.SaccoType);

                return Ok();
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }

        }


        /*[HttpGet("nwdt/GetPerfomanceReport/{returnId}")]
        public async Task<ActionResult<NWDTPerformanceReportDTO>> GetNwdtPerfomanceReport(string returnId)
        {
            try
            {
                var report = new NWDTPerformanceReportDTO();

                var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId);
                if (currentReturn == null)
                {
                    return BadRequest("Return not found");
                }
                report.ReportDate = currentReturn.CreatedAt;
                report.SaccoName = currentReturn.SaccoName;
                report.PrudentialStandards.Add("CoreCapital", "≥5M");
                report.PrudentialStandards.Add("CoreCapita/Total Assets", "≥8%");
                report.PrudentialStandards.Add("InstitutionalCapitalToTotalAssets", ">5%");
                report.PrudentialStandards.Add("Retained Earnings/Core Capital", ">5%");
                report.PrudentialStandards.Add("Non-Performing Loans", "≤5%");
                report.PrudentialStandards.Add("Non-Earning Assets", "≤5%");
                report.PrudentialStandards.Add("Total Finacial Investements to Core Capital", "<40%");
                report.PrudentialStandards.Add("Subsidary Investments to Total Assets", "<50%");
                report.PrudentialStandards.Add("Equity Investments to Core Capital Ratio", "<20%");
                report.PrudentialStandards.Add("Other Financial investments to Core Capital Ratio ", "<30%");
                report.PrudentialStandards.Add("Liquid Assets/Short-term Liabilities", ">10%");
                report.PrudentialStandards.Add("External Borrowing to Total Assets", "<25%");
                report.PrudentialStandards.Add("Gross loans /Total Assets", "70 - 80%");
                report.PrudentialStandards.Add("Gross Loans to Deposits", ">100%");

                var historicalReturns = await _context.Returns.Where(r => r.CreatedAt < currentReturn.CreatedAt && r.Id != returnId).ToListAsync();
                historicalReturns = historicalReturns.OrderByDescending(r => r.CreatedAt).Take(2).ToList();

                var allReturns = new[] { currentReturn }.Concat(historicalReturns);
                var result = new CamelsRatingsDTO { ReturnId = returnId };

                foreach (var returnPeriod in allReturns)
                {
                    var balanceSheet = await _context.NWDTFinancialPositionReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                    var incomeStatement = await _context.NWDTComprehensiveIncomeReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                    var capitalReturn = await _context.NWDTCapitalAdequacyReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                    var liquidityReturn = await _context.NDWTLiquidityReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                    var depositReturns = await _context.NWDTDepositReturns.Where(x => x.ReturnId == returnPeriod.Id).ToListAsync();
                    var riskClassificationReturn = await _context.NWDTRiskClassificationReturns.Where(x => x.ReturnId == returnPeriod.Id).ToListAsync();
                    var investmentReturn = await _context.NWDTInvestmentReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);

                    // Use default objects if data is missing

                    var SavedCapitalAdequacy = capitalReturn ?? new NWDTCapitalAdequacyReturn();
                    var SavedLiquidityStatement = liquidityReturn ?? new NWDTLiquidityReturn();
                    var SavedDepositReturn = depositReturns ?? new List<NWDTDepositReturn>();
                    var SavedRiskClassification = riskClassificationReturn ?? new List<NWDTRiskClassificationReturn>();
                    var SavedInvestmentReturn = investmentReturn ?? new NWDTInvestmentReturn();
                    var SavedFinancialPositionStatement = balanceSheet ?? new NWDTFinancialPositionReturn();
                    var SavedComprehensiveStatement = incomeStatement ?? new NWDTComprehensiveIncomeReturn();

                    var periodData = new NWDTPerformanceReportDTO.NWDTPeriodData
                    {
                        PeriodLabel = returnPeriod.CreatedAt.ToString("dd MMMM, yyyy"),
                        PeriodType = "Returns",
                        PeriodDate = returnPeriod.CreatedAt,

                        CoreCapital = SavedCapitalAdequacy.CoreCapital,
                    };

                    // CoreCapitalToTotalAssetsRatio
                    if (SavedCapitalAdequacy.TotalAssets != 0)
                    {
                        periodData.CoreCapitalToTotalAssetsRatio = (SavedCapitalAdequacy.CoreCapital / SavedCapitalAdequacy.TotalAssets) * 100;
                    }
                    else
                    {
                        periodData.CoreCapitalToTotalAssetsRatio = 0;
                    }

                    // CoreCapitalToTotalDepositsRatio
                    if (SavedCapitalAdequacy.TotalAssets != 0)
                    {
                        periodData.CoreCapitalToTotalDepositsRatio = 0 / SavedCapitalAdequacy.TotalDepositsLiabilities / SavedCapitalAdequacy.TotalAssets;
                    }
                    else
                    {
                        periodData.CoreCapitalToTotalDepositsRatio = 0;
                    }

                    periodData.NonPerformingLoans = ReturnAnalysisHelper.CalculateNwdtNonPerformingLoans(SavedRiskClassification);
                    periodData.NPL = periodData.NonPerformingLoans;
                    // Non Earnig Assets
                    if (SavedFinancialPositionStatement.TotalAssets != 0)
                    {
                        periodData.NonEarningAssets = (SavedFinancialPositionStatement.PrepaymentsAndSundryReceivables
                        + SavedFinancialPositionStatement.AccountsReceivables
                        + SavedFinancialPositionStatement.PropertyEquipmentOtherAssets
                        + SavedFinancialPositionStatement.PrepaidLeaseRentals
                        + SavedFinancialPositionStatement.IntangibleAssets
                        + SavedFinancialPositionStatement.OtherAssets) / SavedFinancialPositionStatement.TotalAssets;
                    }
                    else
                    {
                        periodData.NonEarningAssets = 0;
                    }

                    // EquityInvestmentsToDeposits
                    if (SavedInvestmentReturn.CoreCapital != 0)
                    {
                        periodData.EquityInvestmentsToDeposits = SavedInvestmentReturn.FinancialAssets / SavedCapitalAdequacy.CoreCapital;
                    }
                    else
                    {
                        periodData.EquityInvestmentsToDeposits = 0;
                    }

                    // SubsidiaryAndRelatedInvestmentToCoreCapitalRatio
                    if (SavedInvestmentReturn.CoreCapital != 0)
                    {
                        periodData.SubsidiaryAndRelatedInvestmentToCoreCapitalRatio = SavedInvestmentReturn.SubsidiaryRelatedEntityInvestments / SavedCapitalAdequacy.CoreCapital;
                    }
                    else
                    {
                        periodData.SubsidiaryAndRelatedInvestmentToCoreCapitalRatio = 0;
                    }

                    //EquityInvestmentsToCoreCapitalRatio\
                    if (SavedCapitalAdequacy.CoreCapital != 0)
                    {
                        periodData.EquityInvestmentsToCoreCapitalRatio = SavedFinancialPositionStatement.InvestmentInCompanies / SavedCapitalAdequacy.CoreCapital;
                    }
                    else
                    {
                        periodData.EquityInvestmentsToCoreCapitalRatio = 0;
                    }

                    // NetIncomeToAverageAssetsRatio
                    if (SavedFinancialPositionStatement.GrossLoanPortfolio != 0)
                    {
                        periodData.NetIncomeToAverageAssetsRatio = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations / SavedFinancialPositionStatement.TotalAssets;
                    }
                    else
                    {
                        periodData.NetIncomeToAverageAssetsRatio = 0;
                    }

                    // TotalExpenseToTotalIncomeRatio
                    if (SavedComprehensiveStatement.StoredNetFinancialIncome != 0) //Todo: TotalFinancialIncome not defined
                    {
                        periodData.TotalExpenseToTotalIncomeRatio = (SavedComprehensiveStatement.InterestExpenseOnDeposits +
                                      SavedComprehensiveStatement.CostOfExternalBorrowings +
                                      SavedComprehensiveStatement.DividendExpenses +
                                      SavedComprehensiveStatement.OtherFinancialExpense +
                                      SavedComprehensiveStatement.FeesCommissionOnLoanPortfolio +
                                      SavedComprehensiveStatement.OtherExpense) / SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations;
                    }
                    else
                    {
                        periodData.TotalExpenseToTotalIncomeRatio = 0;
                    }

                    //OperatingExpenseToFinancialIncomeRatio
                    if (SavedComprehensiveStatement.FinancialIncome != 0)
                    {
                        periodData.OperatingExpenseToFinancialIncomeRatio = SavedComprehensiveStatement.OperatingExpenses / SavedComprehensiveStatement.NetFinancialIncome;
                    }
                    else
                    {
                        periodData.OperatingExpenseToFinancialIncomeRatio = 0;
                    }

                    // OtherFinancialInvestmentsToCoreCapitalRatio
                    if (SavedCapitalAdequacy.CoreCapital != 0)
                    {
                        periodData.OtherFinancialInvestmentsToCoreCapitalRatio = SavedInvestmentReturn.OtherInvestments / SavedCapitalAdequacy.CoreCapital;
                    }
                    else
                    {
                        periodData.OtherFinancialInvestmentsToCoreCapitalRatio = 0;
                    }

                    // Finacial Invetsment to Total Assets
                    if (SavedCapitalAdequacy.TotalAssets != 0)
                    {
                        periodData.FinancialInvestmentsToTotalAssetsRatio = SavedFinancialPositionStatement.FinancialInvestments / SavedFinancialPositionStatement.TotalAssets;
                    }
                    else
                    {
                        periodData.FinancialInvestmentsToTotalAssetsRatio = 0;
                    }


                    var ShorttERM = SavedFinancialPositionStatement.NonWithdrawableDeposits + SavedFinancialPositionStatement.TaxPayable + SavedFinancialPositionStatement.DividendsPayable + SavedFinancialPositionStatement.DeferredTaxLiability + SavedFinancialPositionStatement.RetirementBenefitsLiability + SavedFinancialPositionStatement.OtherLiabilities;
                    var LiquidAssets = SavedLiquidityStatement.NetLiquidAssets;
                    // LiquidAssetsToShortTermLiabilitiesRatio
                    if (ShorttERM != 0)
                    {
                        periodData.LiquidAssetsToShortTermLiabilitiesRatio = (LiquidAssets / ShorttERM);
                    }
                    else
                    {
                        periodData.LiquidAssetsToShortTermLiabilitiesRatio = 0;
                    }




                    // ExternalBorrowingToTotalAssetsRatio
                    if (SavedCapitalAdequacy.TotalAssets != 0)
                    {
                        periodData.ExternalBorrowingToTotalAssetsRatio = (SavedFinancialPositionStatement.ExternalBorrowings / SavedCapitalAdequacy.TotalAssets);
                    }
                    else
                    {
                        periodData.ExternalBorrowingToTotalAssetsRatio = 0;
                    }

                    // LiquidAssetsToTotalAssets
                    if (SavedCapitalAdequacy.TotalAssets != 0)
                    {
                        periodData.LiquidAssetsToTotalAssetsRatio = (SavedLiquidityStatement.NetLiquidAssets / SavedCapitalAdequacy.TotalAssets);
                    }
                    else
                    {
                        periodData.LiquidAssetsToTotalAssetsRatio = 0;
                    }


                    periodData.TotalAssets = SavedCapitalAdequacy.TotalAssets;
                    periodData.TotalExpenses = SavedComprehensiveStatement.FinancialExpense + SavedComprehensiveStatement.OperatingExpenses;
                    periodData.TotalDeposits = SavedFinancialPositionStatement.TotalDepositLiabilities;
                    periodData.GrossLoansForm4 = SavedFinancialPositionStatement.GrossLoanPortfolio;
                    periodData.GrossLoansForm6 = SavedCapitalAdequacy.LoansAndAdvances;
                    periodData.PropertyAndEquipment = SavedCapitalAdequacy.PropertyAndEquipment;
                    periodData.EquityInvestments = SavedInvestmentReturn.EquityInvestments;
                    periodData.FinancialInvestments = SavedCapitalAdequacy.Investments;
                    periodData.LiquidAssets = SavedLiquidityStatement.NetLiquidAssets;
                    periodData.ShortTermLiabilities = SavedFinancialPositionStatement.TaxPayable + SavedFinancialPositionStatement.DividendsPayable + SavedFinancialPositionStatement.DeferredTaxLiability + SavedFinancialPositionStatement.RetirementBenefitsLiability + SavedFinancialPositionStatement.OtherLiabilities;
                    periodData.ExternalBorrowing = SavedFinancialPositionStatement.ExternalBorrowings;
                    periodData.AverageGrossLoans = (SavedFinancialPositionStatement.GrossLoanPortfolio + SavedCapitalAdequacy.LoansAndAdvances) / 2;
                    periodData.TotalIncome = SavedComprehensiveStatement.FinancialIncome;
                    periodData.NetFinancialIncome = SavedComprehensiveStatement.NetFinancialIncome;
                    periodData.DividendsAndInterestOnDeposits = SavedComprehensiveStatement.DividendExpenses + SavedComprehensiveStatement.InterestExpenseOnDeposits;
                    periodData.OperatingExpenses = SavedComprehensiveStatement.OperatingExpenses;
                    periodData.InterestOnLoanPortfolioAndFeesCommission = SavedComprehensiveStatement.InterestOnLoanPortfolio + 0; //Todo: FeesAndCommissionOnLoanPortfolio Not defined
                                                                                                                                   // periodData.TotalExpenses = SavedComprehensiveStatement.TotalFinancialExpense;
                    periodData.NetIncome = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations;
                    report.Periods.Add(periodData);
                }

                if (report.Periods.Count < 2 && report.Periods.Count > 0)
                {
                    var blank = new NWDTPerformanceReportDTO.NWDTPeriodData
                    {
                        PeriodLabel = "N/A",
                        PeriodType = "N/A",
                        PeriodDate = DateTime.Now,
                    };

                    report.Periods.Add(blank);
                }

                return report;
            }
            catch (System.Exception Ex)
            {
                CustomErrorHandler.LogException(Ex);
                return StatusCode(500, CustomErrorHandler.HandleException(Ex));
            }

        }*/


        /* [HttpGet("nwdt/GetPerfomanceReportPdf/{returnId}")]
         public async Task<ActionResult<NWDTPerformanceReportDTO>> GetNwdtPerfomanceReportPdf(string returnId, [FromQuery] string? components)
         {
             try
             {
                 var report = new NWDTPerformanceReportDTO();
                 string selector = string.IsNullOrWhiteSpace(components)
                 ? "CAMELS"
                 : new string(components.ToUpperInvariant()
                                        .Where(c => "CAMELS".Contains(c))
                                        .Distinct()
                                        .ToArray());
                 var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId);
                 if (currentReturn == null)
                 {
                     return BadRequest("Return not found");
                 }
                 report.ReportDate = currentReturn.CreatedAt;
                 report.SaccoName = currentReturn.SaccoName;
                 report.PrudentialStandards.Add("CoreCapital", "≥5M");
                 report.PrudentialStandards.Add("CoreCapita/Total Assets", "≥8%");
                 report.PrudentialStandards.Add("InstitutionalCapitalToTotalAssets", ">5%");
                 report.PrudentialStandards.Add("Retained Earnings/Core Capital", ">5%");
                 report.PrudentialStandards.Add("Non-Performing Loans", "≤5%");
                 report.PrudentialStandards.Add("Non-Earning Assets", "≤5%");
                 report.PrudentialStandards.Add("Total Finacial Investements to Core Capital", "<40%");
                 report.PrudentialStandards.Add("Subsidary Investments to Total Assets", "<50%");
                 report.PrudentialStandards.Add("Equity Investments to Core Capital Ratio", "<20%");
                 report.PrudentialStandards.Add("Other Financial investments to Core Capital Ratio ", "<30%");
                 report.PrudentialStandards.Add("Liquid Assets/Short-term Liabilities", ">10%");
                 report.PrudentialStandards.Add("External Borrowing to Total Assets", "<25%");
                 report.PrudentialStandards.Add("Gross loans /Total Assets", "70 - 80%");
                 report.PrudentialStandards.Add("Gross Loans to Deposits", ">100%");

                 var historicalReturns = await _context.Returns.Where(r => r.CreatedAt < currentReturn.CreatedAt && r.Id != returnId).ToListAsync();
                 historicalReturns = historicalReturns.OrderByDescending(r => r.CreatedAt).Take(2).ToList();

                 var allReturns = new[] { currentReturn }.Concat(historicalReturns);
                 var result = new CamelsRatingsDTO { ReturnId = returnId };

                 foreach (var returnPeriod in allReturns)
                 {
                     var balanceSheet = await _context.NWDTFinancialPositionReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                     var incomeStatement = await _context.NWDTComprehensiveIncomeReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                     var capitalReturn = await _context.NWDTCapitalAdequacyReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                     var liquidityReturn = await _context.NDWTLiquidityReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                     var depositReturns = await _context.NWDTDepositReturns.Where(x => x.ReturnId == returnPeriod.Id).ToListAsync();
                     var riskClassificationReturn = await _context.NWDTRiskClassificationReturns.Where(x => x.ReturnId == returnPeriod.Id).ToListAsync();
                     var investmentReturn = await _context.NWDTInvestmentReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                     var managementReturn = await _context.ManagementReturns.FirstOrDefaultAsync(x => x.ReturnId == returnPeriod.Id);
                     // Use default objects if data is missing

                     var SavedCapitalAdequacy = capitalReturn ?? new NWDTCapitalAdequacyReturn();
                     var SavedLiquidityStatement = liquidityReturn ?? new NWDTLiquidityReturn();
                     var SavedDepositReturn = depositReturns ?? new List<NWDTDepositReturn>();
                     var SavedRiskClassification = riskClassificationReturn ?? new List<NWDTRiskClassificationReturn>();
                     var SavedInvestmentReturn = investmentReturn ?? new NWDTInvestmentReturn();
                     var SavedFinancialPositionStatement = balanceSheet ?? new NWDTFinancialPositionReturn();
                     var SavedComprehensiveStatement = incomeStatement ?? new NWDTComprehensiveIncomeReturn();

                     var periodData = new NWDTPerformanceReportDTO.NWDTPeriodData
                     {
                         PeriodLabel = returnPeriod.CreatedAt.ToString("dd MMMM, yyyy"),
                         PeriodType = "Returns",
                         PeriodDate = returnPeriod.CreatedAt,

                         CoreCapital = SavedCapitalAdequacy.CoreCapital,
                     };

                     // CoreCapitalToTotalAssetsRatio
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         periodData.CoreCapitalToTotalAssetsRatio = (SavedCapitalAdequacy.CoreCapital / SavedCapitalAdequacy.TotalAssets) * 100;
                     }
                     else
                     {
                         periodData.CoreCapitalToTotalAssetsRatio = 0;
                     }

                     // CoreCapitalToTotalDepositsRatio
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         periodData.CoreCapitalToTotalDepositsRatio = 0 / SavedCapitalAdequacy.TotalDepositsLiabilities / SavedCapitalAdequacy.TotalAssets;
                     }
                     else
                     {
                         periodData.CoreCapitalToTotalDepositsRatio = 0;
                     }

                     periodData.NonPerformingLoans = ReturnAnalysisHelper.CalculateNwdtNonPerformingLoans(SavedRiskClassification);
                     periodData.NPL = periodData.NonPerformingLoans;
                     // Non Earnig Assets
                     if (SavedFinancialPositionStatement.TotalAssets != 0)
                     {
                         periodData.NonEarningAssets = (SavedFinancialPositionStatement.PrepaymentsAndSundryReceivables
                         + SavedFinancialPositionStatement.AccountsReceivables
                         + SavedFinancialPositionStatement.PropertyEquipmentOtherAssets
                         + SavedFinancialPositionStatement.PrepaidLeaseRentals
                         + SavedFinancialPositionStatement.IntangibleAssets
                         + SavedFinancialPositionStatement.OtherAssets) / SavedFinancialPositionStatement.TotalAssets;
                     }
                     else
                     {
                         periodData.NonEarningAssets = 0;
                     }

                     // EquityInvestmentsToDeposits
                     if (SavedInvestmentReturn.CoreCapital != 0)
                     {
                         periodData.EquityInvestmentsToDeposits = SavedInvestmentReturn.FinancialAssets / SavedCapitalAdequacy.CoreCapital;
                     }
                     else
                     {
                         periodData.EquityInvestmentsToDeposits = 0;
                     }

                     // SubsidiaryAndRelatedInvestmentToCoreCapitalRatio
                     if (SavedInvestmentReturn.CoreCapital != 0)
                     {
                         periodData.SubsidiaryAndRelatedInvestmentToCoreCapitalRatio = SavedInvestmentReturn.SubsidiaryRelatedEntityInvestments / SavedCapitalAdequacy.CoreCapital;
                     }
                     else
                     {
                         periodData.SubsidiaryAndRelatedInvestmentToCoreCapitalRatio = 0;
                     }

                     //EquityInvestmentsToCoreCapitalRatio\
                     if (SavedCapitalAdequacy.CoreCapital != 0)
                     {
                         periodData.EquityInvestmentsToCoreCapitalRatio = SavedFinancialPositionStatement.InvestmentInCompanies / SavedCapitalAdequacy.CoreCapital;
                     }
                     else
                     {
                         periodData.EquityInvestmentsToCoreCapitalRatio = 0;
                     }

                     // NetIncomeToAverageAssetsRatio
                     if (SavedFinancialPositionStatement.GrossLoanPortfolio != 0)
                     {
                         periodData.NetIncomeToAverageAssetsRatio = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations / SavedFinancialPositionStatement.TotalAssets;
                     }
                     else
                     {
                         periodData.NetIncomeToAverageAssetsRatio = 0;
                     }

                     // TotalExpenseToTotalIncomeRatio
                     if (SavedComprehensiveStatement.StoredNetFinancialIncome != 0) //Todo: TotalFinancialIncome not defined
                     {
                         periodData.TotalExpenseToTotalIncomeRatio = (SavedComprehensiveStatement.InterestExpenseOnDeposits +
                                       SavedComprehensiveStatement.CostOfExternalBorrowings +
                                       SavedComprehensiveStatement.DividendExpenses +
                                       SavedComprehensiveStatement.OtherFinancialExpense +
                                       SavedComprehensiveStatement.FeesCommissionOnLoanPortfolio +
                                       SavedComprehensiveStatement.OtherExpense) / SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations;
                     }
                     else
                     {
                         periodData.TotalExpenseToTotalIncomeRatio = 0;
                     }

                     //OperatingExpenseToFinancialIncomeRatio
                     if (SavedComprehensiveStatement.FinancialIncome != 0)
                     {
                         periodData.OperatingExpenseToFinancialIncomeRatio = SavedComprehensiveStatement.OperatingExpenses / SavedComprehensiveStatement.NetFinancialIncome;
                     }
                     else
                     {
                         periodData.OperatingExpenseToFinancialIncomeRatio = 0;
                     }

                     // OtherFinancialInvestmentsToCoreCapitalRatio
                     if (SavedCapitalAdequacy.CoreCapital != 0)
                     {
                         periodData.OtherFinancialInvestmentsToCoreCapitalRatio = SavedInvestmentReturn.OtherInvestments / SavedCapitalAdequacy.CoreCapital;
                     }
                     else
                     {
                         periodData.OtherFinancialInvestmentsToCoreCapitalRatio = 0;
                     }

                     // Finacial Invetsment to Total Assets
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         periodData.FinancialInvestmentsToTotalAssetsRatio = SavedFinancialPositionStatement.FinancialInvestments / SavedFinancialPositionStatement.TotalAssets;
                     }
                     else
                     {
                         periodData.FinancialInvestmentsToTotalAssetsRatio = 0;
                     }


                     var ShorttERM = SavedFinancialPositionStatement.NonWithdrawableDeposits + SavedFinancialPositionStatement.TaxPayable + SavedFinancialPositionStatement.DividendsPayable + SavedFinancialPositionStatement.DeferredTaxLiability + SavedFinancialPositionStatement.RetirementBenefitsLiability + SavedFinancialPositionStatement.OtherLiabilities;
                     var LiquidAssets = SavedLiquidityStatement.NetLiquidAssets;
                     // LiquidAssetsToShortTermLiabilitiesRatio
                     if (ShorttERM != 0)
                     {
                         periodData.LiquidAssetsToShortTermLiabilitiesRatio = (LiquidAssets / ShorttERM);
                     }
                     else
                     {
                         periodData.LiquidAssetsToShortTermLiabilitiesRatio = 0;
                     }




                     // ExternalBorrowingToTotalAssetsRatio
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         periodData.ExternalBorrowingToTotalAssetsRatio = (SavedFinancialPositionStatement.ExternalBorrowings / SavedCapitalAdequacy.TotalAssets);
                     }
                     else
                     {
                         periodData.ExternalBorrowingToTotalAssetsRatio = 0;
                     }

                     // LiquidAssetsToTotalAssets
                     if (SavedCapitalAdequacy.TotalAssets != 0)
                     {
                         periodData.LiquidAssetsToTotalAssetsRatio = (SavedLiquidityStatement.NetLiquidAssets / SavedCapitalAdequacy.TotalAssets);
                     }
                     else
                     {
                         periodData.LiquidAssetsToTotalAssetsRatio = 0;
                     }


                     periodData.TotalAssets = SavedCapitalAdequacy.TotalAssets;
                     periodData.TotalExpenses = SavedComprehensiveStatement.FinancialExpense + SavedComprehensiveStatement.OperatingExpenses;
                     periodData.TotalDeposits = SavedFinancialPositionStatement.TotalDepositLiabilities;
                     periodData.GrossLoansForm4 = SavedFinancialPositionStatement.GrossLoanPortfolio;
                     periodData.GrossLoansForm6 = SavedCapitalAdequacy.LoansAndAdvances;
                     periodData.PropertyAndEquipment = SavedCapitalAdequacy.PropertyAndEquipment;
                     periodData.EquityInvestments = SavedInvestmentReturn.EquityInvestments;
                     periodData.FinancialInvestments = SavedCapitalAdequacy.Investments;
                     periodData.LiquidAssets = SavedLiquidityStatement.NetLiquidAssets;
                     periodData.ShortTermLiabilities = SavedFinancialPositionStatement.TaxPayable + SavedFinancialPositionStatement.DividendsPayable + SavedFinancialPositionStatement.DeferredTaxLiability + SavedFinancialPositionStatement.RetirementBenefitsLiability + SavedFinancialPositionStatement.OtherLiabilities;
                     periodData.ExternalBorrowing = SavedFinancialPositionStatement.ExternalBorrowings;
                     periodData.AverageGrossLoans = (SavedFinancialPositionStatement.GrossLoanPortfolio + SavedCapitalAdequacy.LoansAndAdvances) / 2;
                     periodData.TotalIncome = SavedComprehensiveStatement.FinancialIncome;
                     periodData.NetFinancialIncome = SavedComprehensiveStatement.NetFinancialIncome;
                     periodData.DividendsAndInterestOnDeposits = SavedComprehensiveStatement.DividendExpenses + SavedComprehensiveStatement.InterestExpenseOnDeposits;
                     periodData.OperatingExpenses = SavedComprehensiveStatement.OperatingExpenses;
                     periodData.InterestOnLoanPortfolioAndFeesCommission = SavedComprehensiveStatement.InterestOnLoanPortfolio + 0; //Todo: FeesAndCommissionOnLoanPortfolio Not defined
                                                                                                                                    // periodData.TotalExpenses = SavedComprehensiveStatement.TotalFinancialExpense;
                     periodData.NetIncome = SavedComprehensiveStatement.NetIncomeAfterTaxesAndDonations;
                     periodData.MemberProtectionScore = managementReturn?.MemberProtectionScore ?? 0;
                     periodData.GovernanceStructureScore = managementReturn?.GorvenanceStructureScore ?? 0;
                     periodData.InternalControlsScore = managementReturn?.InternalControlsScore ?? 0;
                     periodData.ComplianceWithLawsScore = managementReturn?.ComplianceWithLawsAndRegulationsScore ?? 0;
                     report.Periods.Add(periodData);
                 }

                 if (report.Periods.Count < 2 && report.Periods.Count > 0)
                 {
                     var blank = new NWDTPerformanceReportDTO.NWDTPeriodData
                     {
                         PeriodLabel = "N/A",
                         PeriodType = "N/A",
                         PeriodDate = DateTime.Now,
                     };

                     report.Periods.Add(blank);
                 }

                 var reportBytes = NWDTReportHelper.GenerateNwdtSaccoPerformancePdfReport(report, selector);
                 var base64String = Convert.ToBase64String(reportBytes);
                 return Ok(new
                 {
                     pdfData = base64String,
                     fileName = $"NWDT_SACCO_Performance_Report_{DateTime.Now:yyyyMMdd}.pdf"
                 });

                 // return File(reportBytes, "application/pdf", $"NWDT_SACCO_Performance_Report_{DateTime.Now:yyyyMMdd}.pdf");

             }
             catch (System.Exception Ex)
             {
                 CustomErrorHandler.LogException(Ex);
                 return StatusCode(500, CustomErrorHandler.HandleException(Ex));
             }

         }*/


        /*
                private async Task<(
                 bool IsValid,
                 List<string> ProcessingSummary,
                List<ReturnAnalysisHelper.ValidationError> ConsistencyErrors,
                bool HasConsistencyBeenChecked,
                 List<CapitalAdequacyRow> CapitalAdequacy,
                 List<LiquidityStatementRow> LiquidityStatement,
                 List<DepositRangeData> DepositReturn,
                 List<RiskClassificationRow> RiskClassification,
                 List<InvestmentRow> Investment,
                 List<StatementOfFinancialPositionRow> FinancialPosition,
                 List<StatementOfComprehensiveIncomeRow> ComprehensiveStatement,
                 string CommonPeriod
                    )>
                    CheckConsistencyForDT(NewReturnDTO createFormDTO)
                {
                    var processingSummary = new List<string>();
                    List<ValidationError> ConsistencyErrors = new List<ValidationError>();
                    ReturnsHelper returnsHelper = new ReturnsHelper(_context);


                    // Check for attachments
                    if (createFormDTO.FormUploads == null ||
                        createFormDTO.FormUploads.Count == 0 ||
                        !createFormDTO.FormUploads.Any(f => f.formFile != null))
                    {
                        throw new Exception("No attachments found. Please attach at least one form");
                    }

                    // await returnsHelper.ValidateUploadedForms(Dto);
                    Form1Statement form1Statement = null;
                    Form2Statement Form2 = null;
                    Form3Statement Form3 = null;
                    Form4Statement form4 = null;
                    Form5Statement Form5 = null;
                    Form6Statement form6 = null;
                    Form7Statement form7 = null;


                    // Initialize form data holders
                    List<CapitalAdequacyRow> capital_adequacy_form1 = null;
                    List<LiquidityStatementRow> liquidityStatement_form_2 = null;
                    List<DepositRangeData> depositreturn_form_3 = null;
                    List<RiskClassificationRow> riskClassification_form_4 = null;
                    List<InvestmentRow> inverstment_return_form_5 = null;
                    List<StatementOfFinancialPositionRow> financialPositionStatement_form_6 = null;
                    List<StatementOfComprehensiveIncomeRow> comprehensiveStatement_form7 = null;
                    var HasConsistencyBeenChecked = true;
                    // Process each form
                    foreach (var form in createFormDTO.FormUploads)
                    {
                        try
                        {
                            if (form.formFile == null) continue;


                            ReturnForm? fm = await _context.ReturnForms.FirstOrDefaultAsync(f => f.Id == form.FormId);
                            if (fm == null)
                            {
                                processingSummary.Add($"Form with ID {form.FormId} was not found in the system");
                                continue;
                            }


                            // Form 1: Capital Adequacy Form
                            if (fm.IsCapitalAdequencyForm)
                            {
                                form1Statement = ExcelService.ImportCapitalAdequacyRows(form.formFile, _logger);
                                if (form1Statement == null || !form1Statement.Rows.Any())
                                {
                                    throw new Exception("No data found in Capital Adequacy Form");
                                }

                                capital_adequacy_form1 = form1Statement.Rows;

                                if (capital_adequacy_form1 == null || !capital_adequacy_form1.Any())
                                {
                                    throw new Exception("No data found in Capital Adequacy Form");
                                }
                            }
                            // Form 2: Liquidity Statement Form
                            else if (fm.IsLiquidityStatement)
                            {
                                Form2 = ExcelService.ImportLiquidityStatementRows(form.formFile, _logger);
                                if (Form2 == null || !Form2.Rows.Any())
                                {
                                    throw new Exception("No data found in Liquidity Statement Form");
                                }
                                liquidityStatement_form_2 = Form2.Rows;
                                if (liquidityStatement_form_2 == null || !liquidityStatement_form_2.Any())
                                {
                                    throw new Exception("No data found in Liquidity Statement Form");
                                }
                            }
                            // Form 3: Deposit Return Form
                            else if (fm.IsDepositReturnForm)
                            {
                                Form3 = ExcelService.ImportDepositRangeDataRows(form.formFile, _logger);
                                if (Form3 == null || !Form3.Rows.Any())
                                {
                                    throw new Exception("No data found in Deposit Return Form");
                                }
                                depositreturn_form_3 = Form3.Rows;
                                if (depositreturn_form_3 == null || !depositreturn_form_3.Any())
                                {
                                    throw new Exception("No data found in Deposit Return Form");
                                }
                            }
                            // Form 4: Risk Classification Form
                            else if (fm.IsRiskClassification)
                            {
                                form4 = ExcelService.ImportRiskClassificationRows(form.formFile, _logger);
                                if (form4 == null || !form4.Rows.Any())
                                {
                                    throw new Exception("No data found in Risk Classification Form");
                                }
                                riskClassification_form_4 = form4.Rows;
                                if (riskClassification_form_4 == null || !riskClassification_form_4.Any())
                                {
                                    throw new Exception("No data found in Risk Classification Form");
                                }
                            }
                            // Form 5: Investment Return Form
                            else if (fm.IsInvestmentReturn)
                            {
                                Form5 = ExcelService.ImportInvestmentRows(form.formFile, _logger);
                                if (Form5 == null || !Form5.Rows.Any())
                                {
                                    throw new Exception("No data found in Investment Return Form");
                                }
                                inverstment_return_form_5 = Form5.Rows;
                                if (inverstment_return_form_5 == null || !inverstment_return_form_5.Any())
                                {
                                    throw new Exception("No data found in Investment Return Form");
                                }
                            }
                            // Form 6: Statement of Financial Position
                            else if (fm.IsFinancialPosition)
                            {
                                form6 = ExcelService.ImportFinancialPositionRows(form.formFile, _logger);
                                if (form6 == null || !form6.Rows.Any())
                                {
                                    throw new Exception("No data found in Statement of Financial Position");
                                }
                                financialPositionStatement_form_6 = form6.Rows;
                                if (financialPositionStatement_form_6 == null || !financialPositionStatement_form_6.Any())
                                {
                                    throw new Exception("No data found in Statement of Financial Position");
                                }
                            }
                            // Form 7: Statement of Comprehensive Income
                            else if (fm.IsStatementOfComprehensiveIncome)
                            {
                                form7 = ExcelService.ImportStatementOfComprehensiveIncomeRows(form.formFile, _logger);
                                if (form7 == null || !form7.Rows.Any())
                                {
                                    throw new Exception("No data found in Statement of Comprehensive Income");
                                }
                                comprehensiveStatement_form7 = form7.Rows;
                                if (comprehensiveStatement_form7 == null || !comprehensiveStatement_form7.Any())
                                {
                                    throw new Exception("No data found in Statement of Comprehensive Income");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            processingSummary.Add($"Error processing '{form.formFile.FileName}': {ex.Message}");
                            throw new Exception($"Error processing {form.formFile.FileName}", ex);
                        }
                    }

                    var red = returnsHelper.AreAllFormsInSamePeriod(
                     form1Statement,
                     Form2,
                     Form3,
                     form4,
                     Form5,
                     form6,
                     form7);

                    if (!red.IsValid)
                    {
                        HasConsistencyBeenChecked = false;
                        processingSummary.Add(red.Message);
                        return (false, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, null, null, null, null, null, null, null, red.CommonPeriod);
                    }


                    bool isValid = true;
                    if (returnsHelper.AreAllFormsPresent(capital_adequacy_form1, liquidityStatement_form_2, depositreturn_form_3,
                        riskClassification_form_4, inverstment_return_form_5, financialPositionStatement_form_6,
                        comprehensiveStatement_form7))
                    {
                        var validationResult = ValidateReturns(
                            capital_adequacy_form1, liquidityStatement_form_2, depositreturn_form_3,
                            riskClassification_form_4, inverstment_return_form_5, financialPositionStatement_form_6,
                            comprehensiveStatement_form7
                        );

                        isValid = validationResult.IsValid;
                        if (!isValid)
                        {
                            ConsistencyErrors.AddRange(validationResult.ValidationErrors);
                            IDocument report = new ConsistencyReport(validationResult, "Test", "System");
                            var pdfBytes = report.GeneratePdf();
                            await FormsHelper.SaveReportAsync(pdfBytes, "ConsistencyReport", "System", "Test");

                        }
                    }

                    return (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, capital_adequacy_form1, liquidityStatement_form_2,
                        depositreturn_form_3, riskClassification_form_4, inverstment_return_form_5,
                        financialPositionStatement_form_6, comprehensiveStatement_form7, red.CommonPeriod);
                }*/


        /// <summary>
        /// Bulk submit all returns for a specific period
        /// </summary>
        /// <param name="periodId">The period ID to submit returns for</param>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpPost("BulkSubmitByPeriod")]
        public async Task<ActionResult<BulkSubmissionResultDTO>> BulkSubmitByPeriod(
            [FromBody] BulkSubmissionRequestDTO request)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401, "Unauthorized");
                }

                var result = await _returnSubmissionService.BulkSubmitByPeriodAsync(
                    request.PeriodId,
                    loggedInSacco.SaccoId,
                    loggedInSacco.SaccoType);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk submission for period {PeriodId}", request.PeriodId);
                return StatusCode(500, new { error = "An error occurred during bulk submission", details = ex.Message });
            }
        }

        // Admin Grouped Returns Methods
        [HttpGet("admin/grouped-returns")]
        public async Task<ActionResult<List<AdminGroupedReturnDTO>>> GetAdminGroupedReturns([FromQuery] AdminReturnFilterDTO filter)
        {
            try
            {
                /* LoggedInEntity loggedInAdmin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                 if (loggedInAdmin == null || string.IsNullOrEmpty(loggedInAdmin.UserId))
                 {
                     return StatusCode(401, "Unauthorized");
                 }*/

                var results = await _adminReturnService.GetGroupedReturnsAsync(filter);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin grouped returns");
                return StatusCode(500, new { error = "An error occurred while fetching grouped returns", details = ex.Message });
            }
        }

        [HttpGet("admin/grouped-returns/{groupId}/{periodId}/{saccoId}")]
        public async Task<ActionResult<AdminGroupedReturnDetailsDTO>> GetAdminGroupedReturnDetails(string groupId, string periodId, string saccoId)
        {
            try
            {
               /* LoggedInEntity loggedInAdmin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInAdmin == null || string.IsNullOrEmpty(loggedInAdmin.UserId))
                {
                    return StatusCode(401, "Unauthorized");
                }*/

                var result = await _adminReturnService.GetGroupedReturnDetailsAsync(groupId, periodId, saccoId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin grouped return details");
                return StatusCode(500, new { error = "An error occurred while fetching grouped return details", details = ex.Message });
            }
        }

        [HttpGet("admin/filter-options/years")]
        public async Task<ActionResult<List<string>>> GetAvailableYears()
        {
            try
            {
                var years = await _adminReturnService.GetAvailableYearsAsync();
                return Ok(years);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available years");
                return StatusCode(500, new { error = "An error occurred while fetching years", details = ex.Message });
            }
        }

        [HttpGet("admin/filter-options/sacco-types")]
        public async Task<ActionResult<List<string>>> GetAvailableSaccoTypes()
        {
            try
            {
                var saccoTypes = await _adminReturnService.GetAvailableSaccoTypesAsync();
                return Ok(saccoTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available SACCO types");
                return StatusCode(500, new { error = "An error occurred while fetching SACCO types", details = ex.Message });
            }
        }

        [HttpGet("admin/filter-options/frequencies")]
        public async Task<ActionResult<List<string>>> GetAvailableFrequencies()
        {
            try
            {
                var frequencies = await _adminReturnService.GetAvailableFrequenciesAsync();
                return Ok(frequencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available frequencies");
                return StatusCode(500, new { error = "An error occurred while fetching frequencies", details = ex.Message });
            }
        }

    }
}
