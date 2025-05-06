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
        private readonly ICamelsAnalysisService  camelsAnalysisService;



        public ReturnsController(ReturnsDbContext context, ILogger<ReturnsController> logger, IEmailService emailService, IReturnAssignmentService returnAssignmentService, IWorkflowEngineService workflowService, ICamelsAnalysisService camelsAnalysisService)
        {
            _context = context;
            _logger = logger;
            _formProcessor = new FormProcessingService(context, logger);
            _emailService = emailService;
            _returnAssignmentService = returnAssignmentService;
            _workflowService = workflowService;
            this.camelsAnalysisService = camelsAnalysisService;
        }

        [HttpPost("CheckConsistency")]
        public async Task<IActionResult> CheckConsistency([FromForm] NewReturnDTO createFormDTO)
        {


            LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
            if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
            {
                return StatusCode(401);
            }

            if (loggedInSacco.SaccoType == Constants.SaccoType.DepositTaking.ToString())
            {
                var (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, _, _, _, _, _, _, _, CommonPeriod) = await CheckConsistencyForDT(createFormDTO);
                if (!HasConsistencyBeenChecked)
                {
                    return StatusCode(409, processingSummary);
                }
                if (!isValid)
                {
                    return BadRequest(ConsistencyErrors);
                }
            }
            else
            {
                var (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, _, _, _, _, _, _, _, CommonPeriod) = await CheckConsistencyForNWDT(createFormDTO);
                if (!HasConsistencyBeenChecked)
                {
                    return StatusCode(409, processingSummary);
                }

                if (!isValid)
                {
                    return BadRequest(ConsistencyErrors);
                }
            }

            return Ok();

        }

        private async Task<(
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


            // Check for attachments
            if (createFormNWDTDTO.FormUploads == null ||
                createFormNWDTDTO.FormUploads.Count == 0 ||
                !createFormNWDTDTO.FormUploads.Any(f => f.formFile != null))
            {
                throw new Exception("No attachments found. Please attach at least one form");
            }

            // await ReturnsHelper.NWDTValidateUploadedForms(createFormNWDTDTO);

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
                    throw new Exception(
                        $"Error processing {form.formFile.FileName}': {ex.Message}"
                    );
                }
            }

            var red = ReturnsHelper.AreAllFormsInSamePeriodNWDT(form1Statement, Form2, Form3, form4, Form5, form6, form7);

            if (!red.IsValid)
            {
                HasConsistencyBeenChecked = false;
                processingSummary.Add(red.Message);
                return (false, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, null, null, null, null, null, null, null, red.CommonPeriod);
            }


            bool isValid = true;
            if (ReturnsHelper.AreAllFormsPresent(capital_adequacy_form1, liquidityStatement_form_2, depositreturn_form_3,
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
                }
            }

            return (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, capital_adequacy_form1, liquidityStatement_form_2,
                depositreturn_form_3, riskClassification_form_4, inverstment_return_form_5,
                financialPositionStatement_form_6, comprehensiveStatement_form7, red.CommonPeriod);
        }
        // File Return
        [HttpPost("FileReturns")]
        public async Task<IActionResult> FileReturnsAsync([FromForm] NewReturnDTO createFormDTO)
        {
         

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


                if (!ReturnsHelper.HasValidUploads(createFormDTO))
                {
                    return BadRequest("No attachments found. Please attach at least one form.");
                }
                // Validate that all forms have the same reporting period
                foreach (var upload in createFormDTO.FormUploads.Where(u => u.formFile != null && !string.IsNullOrEmpty(u.FormId)))
                {
                    var form = await _context.ReturnForms.FirstOrDefaultAsync(f => f.Id == upload.FormId);
                    if (form != null)
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
                                ConError.Add($"Form {form.FormName} has period {year} which differs from {PeriodToUse}");
                                IsConsistent = false;
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

                var (success, returnId, processingMessages) = await _formProcessor.ProcessFormBatchAsync(createFormDTO, loggedInSacco, IsConsistent, ConError, PeriodToUse);

                if (!success)
                {
                    _logger.LogError("Error processing form batch");
                    return StatusCode(500, "An error occurred while processing the forms.");
                }
                var ReturnDetails = _context.Returns.Find(returnId);
                var IsAssigned = await _returnAssignmentService.AssignReturnAsync(ReturnDetails, loggedInSacco.SaccoId);
                if (!IsAssigned.Success)
                {
                    _logger.LogError("Error assigning return: {ErrorMessage}", IsAssigned.ErrorMessage);

                    var returnToDelete = await _context.Returns.FindAsync(returnId);
                    if (returnToDelete != null)
                    {
                        _context.Returns.Remove(returnToDelete);
                        await _context.SaveChangesAsync();
                    }

                    return StatusCode(500, IsAssigned.ErrorMessage);
                }
                var ratingResult = await camelsAnalysisService.CalculateAnalysisAsync(ReturnDetails.Id, ReturnDetails.SaccoType);
                var WorkFlowResult = await _workflowService.StartWorkflowAsync(ReturnDetails, ratingResult.OverallRating);
                
                await _emailService.SendEmailAsync(loggedInSacco.EmailAddress, "Return Submission Confirmation", "Your returns have been successfully submitted.");
                return Ok(processingMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing files");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }

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

            // 2. Get distinct SaccoId+Year pairs as separate lists
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
                .Select(r => new {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Build a lookup for (SaccoId, Year) combinations
            var lookup = activeAssignments
                .Select(a => new { a.Return.SaccoId, a.Return.ReturnFor.Year })
                .Distinct()
                .ToList();

            // 5. Filter the returns to only those we actually need
            var filteredReturns = allReturns
                .Where(r => lookup.Any(x => x.SaccoId == r.SaccoId && x.Year == r.Year))
                .ToList();

            // 6. Group into a map: (SaccoId,Year) → { Id → PreviousVersionId }
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
                    LateNessStatus = ReturnsHelper.CheckLateReturns(r) ? "Late" : "On Time",
                    TotalReturns = ReturnsHelper.CountPopulatedReturns(r),
                    TotalLateReturns = ReturnsHelper.CountLateReturns(r),
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

            // 1. Load active assignments + their Returns
            var activeAssignments = await _context.Returns
                .Where(a =>
                    a.SaccoType == Constants.SaccoType.DepositTaking.ToString() &&
                    a.SaccoId == loggedInSacco.SaccoId 
                )
                .ToListAsync();

            if (!activeAssignments.Any())
                return Ok(new List<SubmittedReturnDTO>());

            // 2. Get distinct SaccoId+Year pairs as separate lists
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
                .Select(r => new {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Build a lookup for (SaccoId, Year) combinations
            var lookup = activeAssignments
                .Select(a => new { a.SaccoId, a.ReturnFor.Year })
                .Distinct()
                .ToList();

            // 5. Filter the returns to only those we actually need
            var filteredReturns = allReturns
                .Where(r => lookup.Any(x => x.SaccoId == r.SaccoId && x.Year == r.Year))
                .ToList();

            // 6. Group into a map: (SaccoId,Year) → { Id → PreviousVersionId }
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
                    LateNessStatus = ReturnsHelper.CheckLateReturns(r) ? "Late" : "On Time",
                    TotalReturns = ReturnsHelper.CountPopulatedReturns(r),
                    TotalLateReturns = ReturnsHelper.CountLateReturns(r),
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
                .Select(r => new {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    Year = r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Filter to only the exact (SaccoId, Year) pairs we need
            var requiredPairs = activeAssignments
                .Select(a => (a.SaccoId, a.ReturnFor.Year))
                .Distinct()
                .ToList();

            var filteredLinks = allLinks
                .Where(x => requiredPairs.Contains((x.SaccoId, x.Year)))
                .ToList();

            // 5. Group into a map: (SaccoId,Year) → Dictionary<Id,PreviousVersionId>
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

                var isAnyLate = ReturnsHelper.CheckLateReturnsNWDT(r);

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
                    TotalReturns = ReturnsHelper.CountPopulatedReturnsNWDT(r),
                    TotalLateReturns = ReturnsHelper.CountLateReturnsNWDT(r),
                    VersionNumber = r.VersionNumber,
                    PreviousVersionIds = chain
                });
            }

            return Ok(results);
        }


        [HttpGet("GetSubmittedNWDTReturns")]
        public async Task<ActionResult<List<SubmittedReturnDTO>>> GetSubmittedNWDTReturns()
        {
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
                .Select(r => new {
                    r.Id,
                    r.PreviousVersionId,
                    r.SaccoId,
                    Year = r.ReturnFor.Year
                })
                .ToListAsync();

            // 4. Filter to only the exact (SaccoId, Year) pairs we need
            var requiredPairs = activeAssignments
                .Select(a => (a.Return.SaccoId, a.Return.ReturnFor.Year))
                .Distinct()
                .ToList();

            var filteredLinks = allLinks
                .Where(x => requiredPairs.Contains((x.SaccoId, x.Year)))
                .ToList();

            // 5. Group into a map: (SaccoId,Year) → Dictionary<Id,PreviousVersionId>
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

                var isAnyLate = ReturnsHelper.CheckLateReturnsNWDT(r);

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
                    TotalReturns = ReturnsHelper.CountPopulatedReturnsNWDT(r),
                    TotalLateReturns = ReturnsHelper.CountLateReturnsNWDT(r),
                    VersionNumber = r.VersionNumber,
                    PreviousVersionIds = chain
                });
            }

            return Ok(results);
        }

        

        [HttpGet("GetReturnDetails/{returnId}")]
        public async Task<ActionResult<ReturnDetailsDTO>> GetReturnDetails(string returnId)
        {
            try
            {
                var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                var returnDTO = await _context.Returns
                    .Include(r => r.CapitalAdequencies)
                    .Include(r => r.DepositReturns)
                    .Include(r => r.StatementOfComprehensiveIncomeReturns)
                    .Include(r => r.StatementOfFinancialPositionReturns)
                    .Include(r => r.RiskClassifications)
                    .Include(r => r.InvestmentReturns)
                    .Include(r => r.SectoralLendingReports)
                    .Include(r => r.LiquidityReturns)
                    .Where(r => r.Id == returnId && r.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                    .AsNoTracking()
                    .Select(r => new ReturnDetailsDTO
                    {
                        Id = r.Id,
                        SaccoName = r.SaccoName,
                        IsConsistent = r.IsNotConsistent,
                        ConsistentErrorMessage = r.ConsistentErrorMessage,
                        SubmittedAt = r.SubmittedAt,

                        CapitalAdequacyDaysLate = r.CapitalAdequencies.Any() ? r.CapitalAdequencies.First().DaysLateBy : 0,
                        DepositReturnDaysLate = r.DepositReturns.Any() ? r.DepositReturns.First().DaysLateBy : 0,
                        StatementOfComprehensiveIncomeDaysLate = r.StatementOfComprehensiveIncomeReturns.Any() ? r.StatementOfComprehensiveIncomeReturns.First().DaysLateBy : 0,
                        StatementOfFinancialPositionDaysLate = r.StatementOfFinancialPositionReturns.Any() ? r.StatementOfFinancialPositionReturns.First().DaysLateBy : 0,
                        LiquidityReturnDaysLate = r.LiquidityReturns.Any() ? r.LiquidityReturns.First().DaysLateBy : 0,
                        RiskClassificationDaysLate = r.RiskClassifications.Any() ? r.RiskClassifications.First().DaysLateBy : 0,
                        InvestmentReturnDaysLate = r.InvestmentReturns.Any() ? r.InvestmentReturns.First().DaysLateBy : 0,

                        CapitalAdequacy = r.CapitalAdequencies.Select(ca => new CapitalAdequacyDTO
                        {
                            ShareCapital = ca.ShareCapital,
                            StatutoryReserves = ca.StatutoryReserves,
                            RetainedEarningsAccumulatedLosses = ca.RetainedEarningsAccumulatedLosses,
                            NetSurplusAfterTaxCurrentYearToDate = ca.NetSurplusAfterTaxCurrentYearToDate,
                            CapitalGrantsEquityInNature = ca.CapitalGrantsEquityInNature,
                            GeneralReserves = ca.GeneralReserves,
                            OtherReserves = ca.OtherReserves,
                            SubTotalCoreCapital = ca.SubTotalCoreCapital,
                            InvestmentsInSubsidiaryAndEquityInstruments = ca.InvestmentsInSubsidiaryAndEquityInstruments,
                            OtherDeductions = ca.OtherDeductions,
                            TotalDeductions = ca.TotalDeductions,
                            CoreCapital = ca.CoreCapital,
                            InstitutionalCapital = ca.InstitutionalCapital,
                            CashLocalAndForeignCurrency = ca.CashLocalAndForeignCurrency,
                            GovernmentSecurities = ca.GovernmentSecurities,
                            DepositsAndBalancesAtOtherInstitutions = ca.DepositsAndBalancesAtOtherInstitutions,
                            LoansAndAdvances = ca.LoansAndAdvances,
                            Investments = ca.Investments,
                            PropertyAndEquipment = ca.PropertyAndEquipment,
                            OtherAssets = ca.OtherAssets,
                            TotalOnBalanceSheetAssets = ca.TotalOnBalanceSheetAssets,
                            TotalAssetsPerBalanceSheet = ca.TotalAssetsPerBalanceSheet,
                            Difference = ca.Difference,
                            CoreCapitalToAssetsRatio = ca.CoreCapitalToAssetsRatio,
                            CoreCapitalToAssetsRatioExcessDeficiency = ca.CoreCapitalToAssetsRatioExcessDeficiency,
                            InstitutionalCapitalToAssetsRatio = ca.InstitutionalCapitalToAssetsRatio,
                            InstitutionalCapitalToAssetsRatioExcessDeficiency = ca.InstitutionalCapitalToAssetsRatioExcessDeficiency,
                            CoreCapitalToDepositsRatio = ca.CoreCapitalToDepositsRatio,
                            CoreCapitalToDepositsRatioExcessDeficiency = ca.CoreCapitalToDepositsRatioExcessDeficiency,
                            FilePath = ca.FilePath
                        }).FirstOrDefault(),
                        DepositReturn = r.DepositReturns.Select(dr => new DepositReturnDto
                        {
                            RangeName = dr.RangeName,
                            DepositType = dr.DepositType,
                            NumberOfAccounts = dr.NumberOfAccounts,
                            Amount = dr.AmountInKshs000,
                            FilePath = dr.FilePath
                        }).ToList(),
                        IncomeStatement = r.StatementOfComprehensiveIncomeReturns.Select(income => new ComprehesiveIncomeStatementDTO
                        {
                            InterestOnLoanPortfolio = income.InterestOnLoanPortfolio,
                            FeesAndCommissionOnLoanPortfolio = income.FeesAndCommissionOnLoanPortfolio,
                            GovernmentSecurities = income.GovernmentSecurities,
                            DepositsWithBanks = income.DepositsWithBanks,
                            OtherInvestments = income.OtherInvestments,
                            OtherOperatingIncome = income.OtherOperatingIncome,
                            InterestExpenseOnDeposits = income.InterestExpenseOnDeposits,
                            CostOfExternalBorrowings = income.CostOfExternalBorrowings,
                            DividendExpenses = income.DividendExpenses,
                            OtherFinancialExpense = income.OtherFinancialExpense,
                            FeesAndCommissionExpense = income.FeesAndCommissionExpense,
                            OtherExpense = income.OtherExpense,
                            ProvisionForLoanLosses = income.ProvisionForLoanLosses,
                            ValueOfLoansRecovered = income.ValueOfLoansRecovered,
                            PersonnelExpenses = income.PersonnelExpenses,
                            GovernanceExpenses = income.GovernanceExpenses,
                            MarketingExpenses = income.MarketingExpenses,
                            DepreciationAndAmortization = income.DepreciationAndAmortization,
                            AdministrativeExpenses = income.AdministrativeExpenses,
                            NonOperatingIncome = income.NonOperatingIncome,
                            NonOperatingExpense = income.NonOperatingExpense,
                            Taxes = income.Taxes,
                            Donations = income.Donations,
                            FilePath = income.FilePath
                        }).FirstOrDefault(),
                        FinancialPosition = r.StatementOfFinancialPositionReturns.Select(balance => new FinancialPositionDTO
                        {
                            CashInHand = balance.CashInHand,
                            CashAtBank = balance.CashAtBank,
                            PrepaymentsAndSundryReceivables = balance.PrepaymentsAndSundryReceivables,
                            GovernmentSecurities = balance.GovernmentSecurities,
                            OtherSecurities = balance.OtherSecurities,
                            BalancesWithOtherSaccos = balance.BalancesWithOtherSaccos,
                            InvestmentsInCompanies = balance.InvestmentsInCompanies,
                            GrossLoanPortfolio = balance.GrossLoanPortfolio,
                            AllowanceForLoanLoss = balance.AllowanceForLoanLoss,
                            TaxRecoverable = balance.TaxRecoverable,
                            DeferredTaxAssets = balance.DeferredTaxAssets,
                            RetirementBenefitAssets = balance.RetirementBenefitAssets,
                            InvestmentProperties = balance.InvestmentProperties,
                            PropertyAndEquipment = balance.PropertyAndEquipment,
                            PrepaidLeaseRentals = balance.PrepaidLeaseRentals,
                            IntangibleAssets = balance.IntangibleAssets,
                            OtherAssets = balance.OtherAssets,
                            SavingsDeposits = balance.SavingsDeposits,
                            ShortTermDeposits = balance.ShortTermDeposits,
                            NonWithdrawableDeposits = balance.NonWithdrawableDeposits,
                            TaxPayable = balance.TaxPayable,
                            DividendsPayable = balance.DividendsPayable,
                            DeferredTaxLiability = balance.DeferredTaxLiability,
                            RetirementBenefitsLiability = balance.RetirementBenefitsLiability,
                            OtherLiabilities = balance.OtherLiabilities,
                            ExternalBorrowings = balance.ExternalBorrowings,
                            ShareCapital = balance.ShareCapital,
                            CapitalGrants = balance.CapitalGrants,
                            PriorYearsRetainedEarnings = balance.PriorYearsRetainedEarnings,
                            CurrentYearSurplus = balance.CurrentYearSurplus,
                            StatutoryReserve = balance.StatutoryReserve,
                            FilePath = balance.FilePath
                        }).FirstOrDefault(),
                        LiquidityStatement = r.LiquidityReturns.Select(liq => new LiquidityStatementDTO
                        {
                            LocalNotesAndCoins = liq.LocalNotesAndCoins,
                            ForeignNotesAndCoins = liq.ForeignNotesAndCoins,
                            BalancesWithCommercialBanks = liq.BalancesWithCommercialBanks,
                            TimeDepositsWithBanksMoreThan90Days = liq.TimeDepositsWithBanksMoreThan90Days,
                            OverdraftsAndMaturedLoans = liq.OverdraftsAndMaturedLoans,
                            BalancesWithOtherSaccoSocieties = liq.BalancesWithOtherSaccoSocieties,
                            BalancesWithOtherFinancialInstitutions = liq.BalancesWithOtherFinancialInstitutions,
                            BalancesDueToOtherSaccoSocieties = liq.BalancesDueToOtherSaccoSocieties,
                            BalancesDueToFinancialInstitutions = liq.BalancesDueToFinancialInstitutions,
                            TreasuryBills = liq.TreasuryBills,
                            TreasuryBonds = liq.TreasuryBonds,
                            DepositsFromMembers = liq.DepositsFromMembers,
                            DepositsFromOtherSources = liq.DepositsFromOtherSources,
                            MaturedLiabilities = liq.MaturedLiabilities,
                            LiabilitiesMaturing91Days = liq.LiabilitiesMaturing91Days,
                            TotalNotesAndCoins = liq.TotalNotesAndCoins,
                            TotalGovernmentSecurities = liq.TotalGovernmentSecurities,
                            NetLiquidAssets = liq.NetLiquidAssets,
                            TotalDeposits = liq.TotalDeposits,
                            TotalOtherLiabilities = liq.TotalOtherLiabilities,
                            LiquidityRatio = liq.LiquidityRatio,
                            LiquidityRatioExcessDeficit = liq.LiquidityRatioExcessDeficit,
                            NetFinancialInstitutionBalances = liq.NetFinancialInstitutionBalances,
                            NetBankBalances = liq.NetBankBalances,
                            FilePath = liq.FilePath
                        }).FirstOrDefault(),
                        RiskClassifications = r.RiskClassifications.Select(rc => new RiskClassificationDTO
                        {
                            LoanType = rc.LoanType,
                            Classification = rc.Classification,
                            NumberOfAccounts = rc.NumberOfAccounts,
                            OutstandingLoanPortfolio = rc.OutstandingLoanPortfolio,
                            RequiredProvision = rc.RequiredProvision,
                            RequiredProvisionAmount = rc.RequiredProvisionAmount,
                            FilePath = rc.FilePath
                        }).ToList(),

                        OtherReturns = r.OtherReturns.Select(other => new OtherReturnDTO
                        {
                            FormName = other.FormName,
                            FileUrl = other.FileUrl

                        }).ToList(),

                        Investment = r.InvestmentReturns.Select(inv => new InvestmentReturnDTO
                        {
                            CoreCapital = inv.CoreCapital,
                            TotalAssets = inv.TotalAssets,
                            TotalDeposits = inv.TotalDeposits,
                            NonEarningAssets = inv.NonEarningAssets,
                            FinancialInvestments = inv.FinancialInvestments,
                            LandAndBuildings = inv.LandAndBuildings,
                            LandBuildingsToTotalAssetsRatio = inv.LandBuildingsToTotalAssetsRatio,
                            LandBuildingsRatioExcessDeficiency = inv.LandBuildingsRatioExcessDeficiency,
                            NonEarningAssetsToTotalAssetsRatio = inv.NonEarningAssetsToTotalAssetsRatio,
                            NonEarningAssetsRatioExcessDeficiency = inv.NonEarningAssetsRatioExcessDeficiency,
                            FinancialInvestmentsToCoreCapitalRatio = inv.FinancialInvestmentsToCoreCapitalRatio,
                            FinancialInvestmentsToCoreCapitalExcessDeficiency = inv.FinancialInvestmentsToCoreCapitalExcessDeficiency,
                            FinancialInvestmentsToDepositsRatio = inv.FinancialInvestmentsToDepositsRatio,
                            FinancialInvestmentsToDepositsExcessDeficiency = inv.FinancialInvestmentsToDepositsExcessDeficiency,
                            FilePath = inv.FilePath
                        }).FirstOrDefault(),
                        Year = r.Period.ToString(),
                        VersionNumber = r.VersionNumber,
                        IsActiveVersion = r.IsActiveVersion,
                        AmendmentDate = r.AmendmentDate,
                        CanReportBeViewed = r.CanReportBeViewed,
                        //PreviousVersionId = r.PreviousVersionId,
                        PreviousVersionIds = ReturnsHelper.GetPreviousVersionChoicesAsync(r).Result
                    })

                    .FirstOrDefaultAsync();


                if (returnDTO == null)
                {
                    return new ReturnDetailsDTO();
                }

                var sectoralLendingReports = await _context.SectoralLendingReports
                    .Where(x => x.ReturnId == returnId)
                    .FirstOrDefaultAsync();

                if (sectoralLendingReports != null)
                {
                    SectoralLendingDTO sectoralLendingDTO = new SectoralLendingDTO
                    {
                        StartDate = sectoralLendingReports.StartDate,
                        EndDate = sectoralLendingReports.EndDate,
                    };

                    var sectoralLendingDataList = await _context.SectoralLendingData
                       .Where(x => x.ReturnId == returnId)
                       .ToListAsync();

                    sectoralLendingDataList.ForEach(x =>
                    {
                        sectoralLendingDTO.SubSectorData.Add(new SectoralLendingDataDTO
                        {
                            Amount = x.Amount,
                            Category = x.Category,
                            SubCategory = x.SubCategory,
                            EconomicSectorName = x.EconomicSectorName,
                        });
                    });

                    returnDTO.SectoralLending = sectoralLendingDTO;

                }



                return returnDTO;
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("CalculateAnalysis/{returnId}")]
        public async Task<ActionResult<CamelsRatingsDTO>> CalculateAnalysis(string returnId)
        {
            try
            {
                var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId && r.SaccoType == Constants.SaccoType.DepositTaking.ToString());
                if (currentReturn == null)
                {
                    return BadRequest("Return not found");
                }

                var result = await camelsAnalysisService.CalculateAnalysisAsync(currentReturn.Id, currentReturn.SaccoType);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating CAMELS analysis");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }


        [HttpGet("GetPerfomanceReport/{returnId}")]
        public async Task<ActionResult<SaccoPerformanceReportDTO>> GetPerfomanceReport(string returnId)
        {
            try
            {
                // Initialize report
                var report = new SaccoPerformanceReportDTO();

                // Find current return
                var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId && r.SaccoType == Constants.SaccoType.DepositTaking.ToString());
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
        }




        [HttpGet("nwdt/GetReturnDetails/{returnId}")]
        public async Task<ActionResult<NWDTReturnDetailsDTO>> GetNwdtReturnDetails(string returnId)
        {
            try
            {
                var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                // Query the main return entity
                var returnEntity = await _context.Returns
                    .Include(r => r.NDWTCapitalAdequacyReturns)
                    .Include(r => r.NWDTDepositReturns)
                    .Include(r => r.NWDTFinancialPositionReturns)
                    .Include(r => r.NWDTComprehensiveIncomeReturns)
                    .Include(r => r.NWDTInvestmentReturns)
                    .Include(r => r.NWDTLiquidityReturns)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == returnId && r.SaccoType == Constants.SaccoType.NWDT.ToString());

                if (returnEntity == null)
                {
                    return new NWDTReturnDetailsDTO();
                }

                // Query related entities individually
                var capitalAdequacy = await _context.NWDTCapitalAdequacyReturns
                    .Where(ca => ca.ReturnId == returnId)
                                    .AsNoTracking()

                    .FirstOrDefaultAsync();

                var depositReturns = await _context.NWDTDepositReturns
                    .Where(dr => dr.ReturnId == returnId)
                                    .AsNoTracking()
                    .ToListAsync();

                var financialPositionReturns = await _context.NWDTFinancialPositionReturns
                    .Where(fp => fp.ReturnId == returnId)
                                    .AsNoTracking()

                    .FirstOrDefaultAsync();

                var comprehensiveIncomeReturns = await _context.NWDTComprehensiveIncomeReturns
                    .Where(ci => ci.ReturnId == returnId)
                                    .AsNoTracking()

                    .FirstOrDefaultAsync();

                var investmentReturns = await _context.NWDTInvestmentReturns
                    .Where(inv => inv.ReturnId == returnId)
                                    .AsNoTracking()

                    .FirstOrDefaultAsync();

                var liquidityReturns = await _context.NDWTLiquidityReturns
                    .Where(liq => liq.ReturnId == returnId)
                    .FirstOrDefaultAsync();

                var riskClassifications = await _context.DTRiskClassificationReturns
                    .Where(rc => rc.ReturnId == returnId)
                                    .AsNoTracking()
                    .ToListAsync();

                var otherReturns = await _context.OtherReturns
                    .Where(other => other.ReturnId == returnId)
                                    .AsNoTracking()

                    .ToListAsync();




                // Map the results to the DTO
                var returnDTO = new NWDTReturnDetailsDTO
                {
                    Id = returnEntity.Id,
                    SaccoName = returnEntity.SaccoName,
                    SubmittedAt = returnEntity.SubmittedAt,
                    IsConsistent = returnEntity.IsNotConsistent,
                    ConsistentErrorMessage = returnEntity.ConsistentErrorMessage,
                    CapitalAdequacyDaysLate = capitalAdequacy?.DaysLateBy ?? 0,
                    DepositReturnDaysLate = depositReturns.Any() ? depositReturns.First().DaysLateBy : 0,
                    StatementOfComprehensiveIncomeDaysLate = comprehensiveIncomeReturns?.DaysLateBy ?? 0,
                    StatementOfFinancialPositionDaysLate = financialPositionReturns?.DaysLateBy ?? 0,
                    LiquidityReturnDaysLate = liquidityReturns?.DaysLateBy ?? 0,
                    RiskClassificationDaysLate = riskClassifications.Any() ? riskClassifications.First().DaysLateBy : 0,
                    InvestmentReturnDaysLate = investmentReturns?.DaysLateBy ?? 0,

                    NWDTCapitalAdequacy = capitalAdequacy != null ? new NWDTCapitalAdequacyDTO
                    {
                        // Core Capital Components
                        ShareCapital = capitalAdequacy.ShareCapital,
                        StatutoryReserves = capitalAdequacy.StatutoryReserves,
                        RetainedEarningsAccumulatedLosses = capitalAdequacy.RetainedEarnings, // Adjusted to match entity
                        NetSurplusAfterTaxCurrentYearToDate = capitalAdequacy.NetSurplusAfterTax, // Adjusted to match entity
                        CapitalGrantsEquityInNature = capitalAdequacy.CapitalGrants, // Adjusted to match entity
                        OtherReserves = capitalAdequacy.OtherReserves,
                        SubTotalCoreCapital = capitalAdequacy.SubTotalCoreCapital,

                        // Deductions
                        InvestmentsInSubsidiaryAndEquityInstruments = capitalAdequacy.InvestmentsInSubsidiary, // Adjusted to match entity
                        OtherDeductions = capitalAdequacy.OtherDeductions,
                        TotalDeductions = capitalAdequacy.TotalDeductions,
                        CoreCapital = capitalAdequacy.CoreCapital,

                        // Balance Sheet Assets
                        CashLocalAndForeignCurrency = capitalAdequacy.CashLocalForeign, // Adjusted to match entity
                        GovernmentSecurities = capitalAdequacy.GovernmentSecurities,
                        DepositsAndBalancesAtOtherInstitutions = capitalAdequacy.DepositsBalancesAtOtherInstitutions, // Adjusted to match entity
                        LoansAndAdvances = capitalAdequacy.LoansAndAdvances,
                        Investments = capitalAdequacy.Investments,
                        PropertyAndEquipment = capitalAdequacy.PropertyAndEquipment,
                        OtherAssets = capitalAdequacy.OtherAssets,
                        TotalOnBalanceSheetAssets = capitalAdequacy.TotalOnBalanceSheetAssets,
                        TotalAssetsPerBalanceSheet = capitalAdequacy.TotalAssetsPerBalanceSheet,
                        Difference = capitalAdequacy.DifferenceInAssets, // Adjusted to match entity
                                                                         // Ratios
                        CoreCapitalToAssetsRatio = capitalAdequacy.CoreCapitalToAssetsRatio,
                        CoreCapitalToAssetsRatioExcessDeficiency = capitalAdequacy.CoreCapitalToAssetsExcessDeficiency, // Adjusted to match entity
                        CoreCapitalToDepositsRatio = capitalAdequacy.CoreCapitalToDepositsRatio,
                        CoreCapitalToDepositsRatioExcessDeficiency = capitalAdequacy.CoreCapitalToDepositsExcessDeficiency, // Adjusted to match entity,
                        FilePath = capitalAdequacy.FilePath
                    } : null,
                    NWDTDepositReturn = depositReturns.Select(dr => new NWDTDepositReturnDto
                    {
                        RangeName = dr.RangeName,
                        DepositType = dr.DepositType,
                        NumberOfAccounts = dr.NumberOfAccounts,
                        Amount = dr.AmountInKshs000,
                        FilePath = dr.FilePath
                    }).ToList(),
                    NWDTIncomeStatement = comprehensiveIncomeReturns != null ? new NWDTComprehesiveIncomeStatementDTO
                    {
                        // Financial Income
                        InterestOnLoanPortfolio = comprehensiveIncomeReturns.InterestOnLoanPortfolio,
                        FeesAndCommissionOnLoanPortfolio = comprehensiveIncomeReturns.FeesCommissionOnLoanPortfolio, // Adjusted to match entity
                        TotalFinancialIncomeFromLoans = comprehensiveIncomeReturns.FinancialIncomeFromLoansPortfolio, // Added
                        GovernmentSecurities = comprehensiveIncomeReturns.GovernmentSecuritiesIncome, // Adjusted to match entity
                        DepositsWithBanks = comprehensiveIncomeReturns.PlacementInBanksIncome, // Adjusted to match entity
                        OtherInvestments = comprehensiveIncomeReturns.CommercialPapersIncome + // Adjusted to match entity
                          comprehensiveIncomeReturns.CollectiveInvestmentSchemesIncome +
                          comprehensiveIncomeReturns.DerivativesIncome +
                          comprehensiveIncomeReturns.EquityInvestmentsIncome +
                          comprehensiveIncomeReturns.InvestmentInCompaniesIncome,
                        //OtherOperatingIncome = comprehensiveIncomeReturns.OtherOperatingIncome,
                        TotalFinancialIncomeFromInvestments = comprehensiveIncomeReturns.FinancialIncomeFromInvestments, // Added
                        TotalFinancialIncome = comprehensiveIncomeReturns.FinancialIncome, // Added

                        // Financial Expense
                        InterestExpenseOnDeposits = comprehensiveIncomeReturns.InterestExpenseOnDeposits,
                        CostOfExternalBorrowings = comprehensiveIncomeReturns.CostOfExternalBorrowings,
                        DividendExpenses = comprehensiveIncomeReturns.DividendExpenses,
                        OtherFinancialExpense = comprehensiveIncomeReturns.OtherFinancialExpense,
                        FeesAndCommissionExpense = comprehensiveIncomeReturns.FeesCommissionExpense, // Adjusted to match entity
                        OtherExpense = comprehensiveIncomeReturns.OtherExpense,
                        TotalFinancialExpense = comprehensiveIncomeReturns.FinancialExpense, // Added
                        NetFinancialIncome = comprehensiveIncomeReturns.NetFinancialIncome, // Added

                        // Loan Loss
                        ProvisionForLoanLosses = comprehensiveIncomeReturns.ProvisionForLoanLosses,
                        ValueOfLoansRecovered = comprehensiveIncomeReturns.ValueOfLoansRecovered,
                        NetAllowanceForLoanLoss = comprehensiveIncomeReturns.AllowanceForLoanLoss, // Added

                        // Operating Expenses
                        PersonnelExpenses = comprehensiveIncomeReturns.PersonnelExpenses,
                        GovernanceExpenses = comprehensiveIncomeReturns.GovernanceExpenses,
                        MarketingExpenses = comprehensiveIncomeReturns.MarketingExpenses,
                        DepreciationAndAmortization = comprehensiveIncomeReturns.DepreciationAmortizationCharges, // Adjusted to match entity
                        AdministrativeExpenses = comprehensiveIncomeReturns.AdministrativeExpenses,
                        TotalOperatingExpenses = comprehensiveIncomeReturns.OperatingExpenses, // Added
                        NetOperatingIncome = comprehensiveIncomeReturns.NetOperatingIncome, // Added

                        // Non-Operating Income/Expense
                        NonOperatingIncome = comprehensiveIncomeReturns.NonOperatingIncome,
                        NonOperatingExpense = comprehensiveIncomeReturns.NonOperatingExpense,
                        NetNonOperatingIncome = comprehensiveIncomeReturns.NetNonOperatingIncome, // Added

                        // Taxes and Donations
                        Taxes = comprehensiveIncomeReturns.Taxes,
                        NetIncomeBeforeTaxes = comprehensiveIncomeReturns.NetIncomeBeforeTaxes, // Added
                        NetIncomeAfterTaxes = comprehensiveIncomeReturns.NetIncomeAfterTaxesBeforeDonations, // Adjusted to match entity
                        Donations = comprehensiveIncomeReturns.Donations,
                        NetIncomeAfterTaxesAndDonations = comprehensiveIncomeReturns.NetIncomeAfterTaxesAndDonations, // Added,
                        FilePath = comprehensiveIncomeReturns.FilePath
                    } : null,
                    NWDTFinancialPosition = financialPositionReturns != null ? new NWDTFinancialPositionDTO
                    {
                        // Cash and Cash Equivalents
                        CashInHand = financialPositionReturns.CashInHand,
                        CashAtBank = financialPositionReturns.CashAtBank,
                        TotalCashAndCashEquivalent = financialPositionReturns.CashAndCashEquivalent, // Added

                        // Prepayments & Sundry Receivables
                        PrepaymentsAndSundryReceivables = financialPositionReturns.PrepaymentsAndSundryReceivables,

                        // Financial Investments
                        GovernmentSecurities = financialPositionReturns.GovernmentSecurities,
                        OtherSecurities = financialPositionReturns.PlacementInFinancialInstitutions + // Adjusted to match entity
                         financialPositionReturns.CommercialPapers +
                         financialPositionReturns.CollectiveInvestmentSchemes +
                         financialPositionReturns.Derivatives +
                         financialPositionReturns.EquityInvestments +
                         financialPositionReturns.InvestmentInCompanies,
                        BalancesWithOtherSaccos = 0, // Not present in the entity, defaulting to 0
                        InvestmentsInCompanies = financialPositionReturns.InvestmentInCompanies, // Adjusted to match entity
                        TotalFinancialInvestments = financialPositionReturns.FinancialInvestments, // Added

                        // Loan Portfolio
                        GrossLoanPortfolio = financialPositionReturns.GrossLoanPortfolio,
                        AllowanceForLoanLoss = financialPositionReturns.AllowanceForLoanLoss,
                        NetLoanPortfolio = financialPositionReturns.NetLoanPortfolio, // Added

                        // Accounts Receivables
                        TaxRecoverable = financialPositionReturns.TaxRecoverable,
                        DeferredTaxAssets = financialPositionReturns.DeferredTaxAssets,
                        RetirementBenefitAssets = financialPositionReturns.RetirementBenefitAssets,
                        TotalAccountsReceivables = financialPositionReturns.AccountsReceivables, // Added

                        // Property & Equipment
                        InvestmentProperties = financialPositionReturns.InvestmentProperties,
                        PropertyAndEquipment = financialPositionReturns.PropertyAndEquipment,
                        PrepaidLeaseRentals = financialPositionReturns.PrepaidLeaseRentals,
                        IntangibleAssets = financialPositionReturns.IntangibleAssets,
                        OtherAssets = financialPositionReturns.OtherAssets,
                        TotalPropertyAndEquipment = financialPositionReturns.PropertyEquipmentOtherAssets, // Added

                        // Total Assets
                        TotalAssets = financialPositionReturns.TotalAssets, // Added

                        // Liabilities
                        SavingsDeposits = 0, // Not present in the entity, defaulting to 0
                        ShortTermDeposits = 0, // Not present in the entity, defaulting to 0
                        NonWithdrawableDeposits = financialPositionReturns.NonWithdrawableDeposits,
                        TotalDepositLiabilities = financialPositionReturns.TotalDepositLiabilities, // Added

                        // Accounts Payable
                        TaxPayable = financialPositionReturns.TaxPayable,
                        DividendsPayable = financialPositionReturns.DividendsPayable,
                        DeferredTaxLiability = financialPositionReturns.DeferredTaxLiability,
                        RetirementBenefitsLiability = financialPositionReturns.RetirementBenefitsLiability,
                        OtherLiabilities = financialPositionReturns.OtherLiabilities,
                        ExternalBorrowings = financialPositionReturns.ExternalBorrowings,
                        TotalAccountsPayable = financialPositionReturns.AccountsPayableOtherLiabilities, // Added

                        // Total Liabilities
                        TotalLiabilities = financialPositionReturns.TotalLiabilities, // Added

                        // Equity
                        ShareCapital = financialPositionReturns.ShareCapital,
                        CapitalGrants = financialPositionReturns.CapitalGrants,
                        PriorYearsRetainedEarnings = financialPositionReturns.PriorYearsRetainedEarnings,
                        CurrentYearSurplus = financialPositionReturns.CurrentYearSurplus,
                        TotalRetainedEarnings = financialPositionReturns.RetainedEarnings, // Added
                        StatutoryReserve = financialPositionReturns.StatutoryReserve,
                        OtherReserves = financialPositionReturns.OtherReserves,
                        RevaluationReserves = financialPositionReturns.RevaluationReserves,
                        ProposedDividends = financialPositionReturns.ProposedDividends,
                        AdjustmentToEquity = financialPositionReturns.AdjustmentToEquity,
                        TotalOtherEquityAccounts = financialPositionReturns.OtherEquityAccounts, // Added
                        TotalEquity = financialPositionReturns.TotalEquity, // Added
                        FilePath = financialPositionReturns.FilePath
                    } : null,
                    NWDTLiquidityStatement = liquidityReturns != null ? new NWDTLiquidityStatementDTO
                    {
                        LocalNotesAndCoins = liquidityReturns.LocalNotesAndCoins,
                        ForeignNotesAndCoins = liquidityReturns.ForeignNotesAndCoins,
                        BalancesWithCommercialBanks = liquidityReturns.BalancesWithCommercialBanks,
                        TimeDepositsWithBanksMoreThan90Days = liquidityReturns.TimeDepositsWithBanksMoreThan90Days,
                        OverdraftsAndMaturedLoans = liquidityReturns.OverdraftsAndMaturedLoans,
                        BalancesWithOtherSaccoSocieties = liquidityReturns.BalancesWithOtherSaccoSocieties,
                        BalancesWithOtherFinancialInstitutions = liquidityReturns.BalancesWithOtherFinancialInstitutions,
                        BalancesDueToOtherSaccoSocieties = liquidityReturns.BalancesDueToOtherSaccoSocieties,
                        BalancesDueToFinancialInstitutions = liquidityReturns.BalancesDueToFinancialInstitutions,
                        TreasuryBills = liquidityReturns.TreasuryBills,
                        TreasuryBonds = liquidityReturns.TreasuryBondsBearerBonds,
                        //DepositsFromMembers = liquidityReturns.DepositsFromMembers,
                        //DepositsFromOtherSources = liquidityReturns.DepositsFromOtherSources,
                        MaturedLiabilities = liquidityReturns.MaturedLiabilities,
                        LiabilitiesMaturing91Days = liquidityReturns.LiabilitiesMaturing91Days,
                        TotalNotesAndCoins = liquidityReturns.TotalNotesAndCoins,
                        TotalGovernmentSecurities = liquidityReturns.TotalGovernmentSecurities,
                        NetLiquidAssets = liquidityReturns.NetLiquidAssets,
                        //TotalDeposits = liquidityReturns.TotalDeposits,
                        TotalOtherLiabilities = liquidityReturns.TotalOtherLiabilities,
                        LiquidityRatio = liquidityReturns.LiquidityRatio,
                        LiquidityRatioExcessDeficit = liquidityReturns.LiquidityRatioExcessDeficit,
                        NetFinancialInstitutionBalances = liquidityReturns.NetFinancialInstitutionBalances,
                        NetBankBalances = liquidityReturns.NetBankBalances,
                    } : null,
                    NWDTRiskClassifications = riskClassifications.Select(rc => new NWDTRiskClassificationDTO
                    {
                        LoanType = rc.LoanType,
                        Classification = rc.Classification,
                        NumberOfAccounts = rc.NumberOfAccounts,
                        OutstandingLoanPortfolio = rc.OutstandingLoanPortfolio,
                        RequiredProvision = rc.RequiredProvision,
                        RequiredProvisionAmount = rc.RequiredProvisionAmount,
                        FilePath = rc.FilePath
                    }).ToList(),
                    OtherReturns = otherReturns.Select(other => new OtherReturnDTO
                    {
                        FormName = other.FormName,
                        FileUrl = other.FileUrl
                    }).ToList(),
                    NWDTInvestment = investmentReturns != null ? new NWDTInvestmentReturnDTO
                    {
                        CoreCapital = investmentReturns.CoreCapital,
                        TotalAssets = investmentReturns.TotalAssets,
                        TotalDeposits = investmentReturns.TotalDeposits,
                        NonEarningAssets = investmentReturns.NonEarningAssets,

                        // FinancialInvestments is calculated as FinancialAssets (sum of subsidiary, equity, and other investments)
                        FinancialInvestments = investmentReturns.FinancialAssets,

                        // Map LandAndBuildings from the entity’s LandAndBuilding property
                        LandAndBuildings = investmentReturns.LandAndBuilding,

                        // Calculate Land Buildings Ratio and its excess/deficiency using the MaxLandBuildingToTotalAssetRequirement threshold
                        LandBuildingsToTotalAssetsRatio = investmentReturns.TotalAssets > 0
                        ? (investmentReturns.LandAndBuilding / investmentReturns.TotalAssets * 100)
                        : 0,
                        LandBuildingsRatioExcessDeficiency = investmentReturns.TotalAssets > 0
                        ? ((investmentReturns.LandAndBuilding / investmentReturns.TotalAssets * 100) - (investmentReturns.MaxLandBuildingToTotalAssetRequirement * 100))
                        : 0,

                        // Calculate Non-Earning Assets Ratio; using the same value for excess/deficiency since no threshold is defined
                        NonEarningAssetsToTotalAssetsRatio = investmentReturns.TotalAssets > 0
                        ? (investmentReturns.NonEarningAssets / investmentReturns.TotalAssets * 100)
                        : 0,
                        NonEarningAssetsRatioExcessDeficiency = investmentReturns.TotalAssets > 0
                        ? (investmentReturns.NonEarningAssets / investmentReturns.TotalAssets * 100)
                        : 0,

                        // Calculate Financial Investments to Core Capital Ratio and its excess/deficiency using MaxFinancialInvestmentsToCoreCapital
                        FinancialInvestmentsToCoreCapitalRatio = investmentReturns.CoreCapital > 0
                        ? (investmentReturns.FinancialAssets / investmentReturns.CoreCapital * 100)
                        : 0,
                        FinancialInvestmentsToCoreCapitalExcessDeficiency = investmentReturns.CoreCapital > 0
                        ? ((investmentReturns.FinancialAssets / investmentReturns.CoreCapital * 100) - (investmentReturns.MaxFinancialInvestmentsToCoreCapital * 100))
                        : 0,

                        // Calculate Financial Investments to Deposits Ratio and its excess/deficiency using MaxEquityInvestmentsToTotalDeposits
                        FinancialInvestmentsToDepositsRatio = investmentReturns.TotalDeposits > 0
                        ? (investmentReturns.FinancialAssets / investmentReturns.TotalDeposits * 100)
                        : 0,
                        FinancialInvestmentsToDepositsExcessDeficiency = investmentReturns.TotalDeposits > 0
                        ? ((investmentReturns.FinancialAssets / investmentReturns.TotalDeposits * 100) - (investmentReturns.MaxEquityInvestmentsToTotalDeposits * 100))
                        : 0,
                        FilePath = investmentReturns.FilePath
                    } : null,

                    Year = returnEntity.Year.ToString(),
                    VersionNumber = returnEntity.VersionNumber,
                    IsActiveVersion = returnEntity.IsActiveVersion,
                    AmendmentDate = returnEntity.AmendmentDate,
                    PreviousVersionId = returnEntity.PreviousVersionId,
                    CanReportBeViewed = returnEntity.CanReportBeViewed,
                    PreviousVersionIds = ReturnsHelper.GetPreviousVersionChoicesAsync(returnEntity).Result
                };

                var sectoralLendingReports = await _context.SectoralLendingReports
                  .Where(x => x.ReturnId == returnId)
                  .FirstOrDefaultAsync();

                if (sectoralLendingReports != null)
                {
                    SectoralLendingDTO sectoralLendingDTO = new SectoralLendingDTO
                    {
                        StartDate = sectoralLendingReports.StartDate,
                        EndDate = sectoralLendingReports.EndDate,
                    };

                    var sectoralLendingDataList = await _context.SectoralLendingData
                       .Where(x => x.ReturnId == returnId)
                       .ToListAsync();

                    sectoralLendingDataList.ForEach(x =>
                    {
                        sectoralLendingDTO.SubSectorData.Add(new SectoralLendingDataDTO
                        {
                            Amount = x.Amount,
                            Category = x.Category,
                            SubCategory = x.SubCategory,
                            EconomicSectorName = x.EconomicSectorName,
                        });
                    });

                    returnDTO.SectoralLending = sectoralLendingDTO;

                }

                return Ok(returnDTO);
            }
            catch (Exception ex)
            {

                return StatusCode(500, CustomErrorHandler.HandleException(ex));

            }
        }

        [HttpGet("nwdt/CalculateAnalysis/{returnId}")]

        public async Task<ActionResult<CamelsRatingsDTO>> CalculateNwdtAnalysis(string returnId)
        {
            // Retrieve current return and historical returns
            var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId && r.SaccoType == Constants.SaccoType.NWDT.ToString());

            if (currentReturn == null)
            {
                return BadRequest("Return not found");
            }

            try
            {
                var result = await camelsAnalysisService.CalculateAnalysisAsync(currentReturn.Id, currentReturn.SaccoType);
            
                return Ok(result);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        
        }


        [HttpGet("nwdt/GetPerfomanceReport/{returnId}")]
        public async Task<ActionResult<NWDTPerformanceReportDTO>> GetNwdtPerfomanceReport(string returnId)
        {
            try
            {
                var report = new NWDTPerformanceReportDTO();

                var currentReturn = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId && r.SaccoType == Constants.SaccoType.NWDT.ToString());
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

        }


        private async Task<(
         bool IsValid,
         List<string> ProcessingSummary,
        List<ValidationError> ConsistencyErrors,
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


            // Check for attachments
            if (createFormDTO.FormUploads == null ||
                createFormDTO.FormUploads.Count == 0 ||
                !createFormDTO.FormUploads.Any(f => f.formFile != null))
            {
                throw new Exception("No attachments found. Please attach at least one form");
            }

            // await ReturnsHelper.ValidateUploadedForms(createFormDTO);
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
                    throw new Exception(
                        $"Error processing {form.formFile.FileName}"
                    );
                }
            }

            var red = ReturnsHelper.AreAllFormsInSamePeriod(
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
            if (ReturnsHelper.AreAllFormsPresent(capital_adequacy_form1, liquidityStatement_form_2, depositreturn_form_3,
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
                }
            }

            return (isValid, processingSummary, ConsistencyErrors, HasConsistencyBeenChecked, capital_adequacy_form1, liquidityStatement_form_2,
                depositreturn_form_3, riskClassification_form_4, inverstment_return_form_5,
                financialPositionStatement_form_6, comprehensiveStatement_form7, red.CommonPeriod);
        }
      


    }
}
