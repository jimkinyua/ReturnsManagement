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
using Returns.Helpers.Enums;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Hangfire;
using DocumentFormat.OpenXml.Drawing.Charts;
using Returns.DTOs.Compliance;

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
        private readonly IAmendmentService _amendmentService;
        private readonly IReturnAmendmentPolicy _returnAmendmentPolicy;


        public ReturnsController(ReturnsDbContext context, ILogger<ReturnsController> logger, IEmailService emailService, IReturnAssignmentService returnAssignmentService, IWorkflowEngineService workflowService, ICamelsAnalysisService camelsAnalysisService, IReturnChild returnChild, IReturnSubmissionService returnSubmissionService, IConsistencyCheckService consistencyCheckService, IAdminReturnService adminReturnService, IAmendmentService amendmentService, IReturnAmendmentPolicy returnAmendmentPolicy, IComplianceService complianceService)
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
            this.complianceService = complianceService;
            _returnChild = returnChild;
            //_resubmissionService = new FormResubmissionService(context, emailService, logger, _formProcessor);
            _returnSubmissionService = returnSubmissionService;
            _consistencyCheckService = consistencyCheckService;
            _adminReturnService = adminReturnService;
            _amendmentService = amendmentService;
            _returnAmendmentPolicy = returnAmendmentPolicy;
            this.complianceService = complianceService;
        }
        [HttpPost("CheckConsistency")]
        public async Task<IActionResult> CheckConsistency([FromBody] CheckConsistencyDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto?.PeriodId))
                {
                    return BadRequest("PeriodId is required.");
                }

                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return Unauthorized("Unauthorized access. Invalid Sacco details.");
                }

                var saccoDetails = await complianceService.GetSaccoByIdAsync(loggedInSacco.SaccoId);
                if (saccoDetails == null)
                {
                    return NotFound("Sacco not found.");
                }

                // Fetch the period details (assume Periods table exists with Id and PeriodName)
                var period = await _context.ReturnPeriods.FirstOrDefaultAsync(p => p.Id == dto.PeriodId);
                if (period == null)
                {
                    return NotFound("Period not found.");
                }
                string commonPeriod = period.Name ?? dto.PeriodId; // Use PeriodName if available, else Id

                var ratingToUse = await _context.RatingDefinations
                    .Where(r => r.RatingName == "Consistency Check Forms DT" && r.SaccoType == loggedInSacco.SaccoType)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();
                if (ratingToUse == null)
                {
                    return NotFound("No CAMEL rating definition found for this SACCO type.");
                }

                // Fetch all ExpectedReturns for this period
                var expectedReturns = await _context.ExpectedReturns
                    .Where(er => er.PeriodId == dto.PeriodId)
                    .Include(er => er.ReturnForm)
                    .ToListAsync();

                if (!expectedReturns.Any())
                {
                    return BadRequest("No expected returns found for this period.");
                }

                // Build FormUploads by fetching latest draft files
                var formUploads = new List<ReturnFormUploadDTO>();
                foreach (var expected in expectedReturns)
                {
                    // Get the latest submission (IsLatest=true, order by Version desc for safety)
                    var latestSubmission = await _context.ReturnSubmissions
                        .Where(s => s.ExpectedReturnId == expected.Id
                                    && s.SaccoId == loggedInSacco.SaccoId
                                    && s.IsLatest
                                    && s.Status == SubmissionStatus.Draft.ToString()) // Ensure it's a draft
                        .OrderByDescending(s => s.Version)
                        .FirstOrDefaultAsync();

                    if (latestSubmission == null || string.IsNullOrEmpty(latestSubmission.FileUrl))
                    {
                        continue; // Skip if no draft submission or no file
                    }

                    var file = await FormsHelper.GetFileFromUrlAsync(latestSubmission.FileUrl);
                    if (file == null)
                    {
                        // Log but continue; service will handle missing
                        _logger.LogWarning($"Failed to fetch file for ExpectedReturnId {expected.Id}: {latestSubmission.FileUrl}");
                        continue;
                    }

                    formUploads.Add(new ReturnFormUploadDTO
                    {
                        formFile = file,
                        ExpectedReturnId = expected.Id,
                        FormId = expected.ReturnForm?.Id // Assuming ReturnForm.Id is string; adjust if Guid
                    });
                }

                if (!formUploads.Any())
                {
                    return BadRequest("No draft submissions with files found for this period.");
                }

                // Build DTO for service
                var createFormDTO = new NewReturnDTO { FormUploads = formUploads };

                // Call service
                var (isValid, processingSummary, consistencyErrors, hasChecked, formData, _) =await _consistencyCheckService.CheckConsistencyAsync(createFormDTO, ratingToUse.RatingName, loggedInSacco);

                if (!isValid)
                {
                    BackgroundJob.Enqueue<IConsistencyCheckService>(
                        s => s.SendConsistencyReportAsync(
                            loggedInSacco.SaccoId,
                            consistencyErrors,
                            commonPeriod // Use the fetched commonPeriod
                        ));

                    return BadRequest(consistencyErrors);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                var errors = CustomErrorHandler.HandleException(ex);
                var errorsAsString = string.Join(", ", errors);
                return StatusCode(500, errorsAsString);
            }
        }

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
        [HttpPost("admin/AdminAmendmentRequest")]
        public async Task<IActionResult> CreateAdminAmendmentRequestAsync([FromBody] AdminAmendmentRequestDTO dto)
        {
            try
            {
                LoggedInEntity admin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (admin == null)
                {
                    return Unauthorized("Only admins can create amendment requests.");
                }
                var amendmentRequest = await _amendmentService.CreateAdminAmendmentRequestAsync(dto, admin);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }
        [HttpGet("sacco/AmendmentRequestsWaitngResponse")]
        public async Task<IActionResult> GetPendingAmendmentRequestsAsync()
        {
            try
            {
                LoggedInEntity loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                var requests = await _amendmentService.GetAmendmentRequestsPendingSaccoResponseAsync();
                
                 requests = requests.Where(r => r.SaccoId == loggedInEntity.SaccoId).ToList();

               /* if (loggedInEntity.IsAdmin && !string.IsNullOrEmpty(saccoId))
                {
                    requests = requests.Where(r => r.SaccoId == saccoId).ToList();
                }*/
                /*else if (!loggedInEntity.IsAdmin)
                {
                    requests = requests.Where(r => r.SaccoId == loggedInEntity.SaccoId).ToList();
                }*/
                return Ok(requests);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to get pending amendment requests.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during retrieval of pending amendment requests.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("admin/AmendmentRequestsPendingReview")]
        public async Task<IActionResult> AmendmentRequestsPendingReview([FromQuery] string? saccoId = null)
        {
            try
            {
                LoggedInEntity loggedInEntity = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                var requests = await _amendmentService.GetAmendmentRequestsPendingAdminApprovalAsync();
                if (!string.IsNullOrEmpty(saccoId))
                {
                    requests = requests.Where(r => r.SaccoId == saccoId).ToList();
                }
                return Ok(requests);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to get pending amendment requests.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during retrieval of pending amendment requests.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }



        [HttpGet("admin/AmendmentRequestDetails/{requestId}")]
        public async Task<IActionResult> GetAmendmentRequestDetailsAsync(string requestId)
        {
            try
            {
                LoggedInEntity admin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                var details = await _amendmentService.GetAmendmentRequestDetailsAsync(requestId, admin);
                return Ok(details);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to get amendment request details {RequestId}.", requestId);
                return Unauthorized(ex.Message );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for amendment request details {RequestId}: {Message}", requestId, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving amendment request details {RequestId}.", requestId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex) );
            }
        }

        [HttpPost("admin/ReviewAmendmentRequest")]
        public async Task<IActionResult> ReviewAmendmentRequestAsync([FromBody] ReviewAmendmentRequestDTO dto)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    _logger.LogWarning("Unauthorized attempt to review amendment request {RequestId}.", dto.RequestId);
                    return Unauthorized("Invalid credentials.");
                }

                await _amendmentService.ReviewAmendmentRequest(dto.RequestId, dto.Approve, loggedInSacco.UserId);
                return Ok(new { message = $"Amendment request {dto.RequestId} {(dto.Approve ? "approved" : "rejected")} successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for review amendment request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to review amendment request: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for review amendment request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during review amendment request.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }


        [HttpPost("sacco/SaccoRespondToAdminAmendment")]
        public async Task<IActionResult> RespondToAdminAmendmentAsync([FromForm] SaccoAmendmentResponseDTO dto)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return Unauthorized();
                }
                var amendmentRequest = await _amendmentService.RespondToAdminAmendmentRequestAsync(dto, loggedInSacco);
                return Ok(amendmentRequest);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized( ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest( ex.Message );
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }


        [HttpPost("Draft")]
        public async Task<IActionResult> FileDraftReturnsAsync([FromForm] NewReturnDTO dto)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return Unauthorized("Unauthorized access. Invalid Sacco details.");
                }

                var today = DateTime.UtcNow.Date;
                var checkErrors = new List<string>();

                // Check all forms for existing submissions first
                foreach (var item in dto.FormUploads)
                {
                    var existing = await _amendmentService.GetSubmissionUsingExpectedIdAsync(item.ExpectedReturnId, loggedInSacco.SaccoId, true);
                    if (existing != null && !_returnAmendmentPolicy.CanAutoAmend(today))
                    {
                        checkErrors.Add($"A submission already exists for ExpectedReturnId {item.ExpectedReturnId}. Please use the amendment request option.");
                    }
                }

                if (checkErrors.Any())
                {
                    return BadRequest(string.Join("; ", checkErrors));
                }

                var results = await _returnSubmissionService.UploadDraftAsync(dto, loggedInSacco);

                var failedResults = results.Where(r => r.Status == SubmissionStatus.Failed).ToList();
                if (failedResults.Any())
                {
                    var errorMessages = failedResults.SelectMany(r => r.Messages)
                                                     .Distinct() 
                                                     .ToList();
                    return BadRequest(string.Join("; ", errorMessages));
                }

                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }


        [HttpPost("sacco/SaccoRequestAmendment")]
        public async Task<IActionResult> RequestAmendmentAsync([FromForm] AmendmentRequestDTO dto)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return Unauthorized("Invalid SACCO credentials.");
                }

                var amendmentRequest = await _amendmentService.CreateAmendRequestForSacco(dto, loggedInSacco);
                return Ok(new { message = "Amendment request submitted successfully.", requestId = amendmentRequest.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during amendment request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during amendment request.");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

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
                      .FirstOrDefaultAsync(ca => ca.ResubmissionRequestId == returnId);

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
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTCapitalAdequacyReturn>(capEntity.ResubmissionRequestId, hdr.SaccoType)
                      };
                  }

                  // 2-b  Deposit-Range Return (multi-row)
                  var depositEntities = await _context.DepositReturns
                      .AsNoTracking()
                      .Where(dr => dr.ResubmissionRequestId == returnId)
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
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DepositReturn>(depositEntities.FirstOrDefault()?.ResubmissionRequestId ?? string.Empty, hdr.SaccoType),
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
                      .FirstOrDefaultAsync(ci => ci.ResubmissionRequestId == returnId);

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
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTComprehensiveIncomeReturn>(incomeEntity.ResubmissionRequestId, hdr.SaccoType)
                      };
                  }

                  // 2-d  Financial-Position (single row)
                  var balanceEntity = await _context.DTFinancialPositionReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(fp => fp.ResubmissionRequestId == returnId);

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
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTFinancialPositionReturn>(balanceEntity.ResubmissionRequestId, hdr.SaccoType)
                      };
                  }

                  // 2-e  Liquidity (single row)
                  var liquidityEntity = await _context.DTLiquidityReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(liq => liq.ResubmissionRequestId == returnId);

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
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTLiquidityReturn>(liquidityEntity.ResubmissionRequestId, hdr.SaccoType)
                      };
                  }

                  // 2-f  Risk classification (multi-row)
                  var riskEntities = await _context.DTRiskClassificationReturns
                      .AsNoTracking()
                      .Where(rc => rc.ResubmissionRequestId == returnId)
                      .ToListAsync();

                  int riskDaysLate = riskEntities.FirstOrDefault()?.DaysLateBy ?? 0;
                  var riskClassifications = new RiskClassificationDTO();

                  if (riskEntities.Any())
                  {
                      var firstEntity = riskEntities.First();
                      riskClassifications.FormId = firstEntity.FormId ?? string.Empty;
                      riskClassifications.RequiresResubmission = firstEntity.RequiresResubmission;
                      //riskClassifications.FilePath = firstEntity.FilePath;
                      riskClassifications.PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTRiskClassificationReturn>(firstEntity.ResubmissionRequestId, hdr.SaccoType);

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
                      .Where(o => o.ResubmissionRequestId == returnId)
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
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<OtherReturn>(o.ResubmissionRequestId, hdr.SaccoType)
                      });
                  }

                  // 2-h  Investment (single row)
                  var investmentEntity = await _context.DTInvestmentReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(inv => inv.ResubmissionRequestId == returnId);

                  var ApprovalComments = await _context.ApprovalActions
                                            .AsNoTracking()
                                            .Where(o => o.ResubmissionRequestId == returnId)
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
                          PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<DTInvestmentReturn>(investmentEntity.ResubmissionRequestId, hdr.SaccoType)
                      };
                  }

                  var managementEntity = await _context.ManagementReturns
                      .AsNoTracking()
                      .FirstOrDefaultAsync(m => m.ResubmissionRequestId == returnId);

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
                          //PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<ManagementReturn>(managementEntity.ResubmissionRequestId, hdr.SaccoType)
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
                      .FirstOrDefaultAsync(x => x.ResubmissionRequestId == returnId);

                  if (sectoral != null)
                  {
                      var sectoralData = await _context.SectoralLendingData
                          .AsNoTracking()
                          .Where(x => x.ResubmissionRequestId == returnId)
                          .ToListAsync();

                      dto.SectoralLending = new SectoralLendingDTO
                      {
                          StartDate = sectoral.StartDate,
                          EndDate = sectoral.EndDate,
                          //PreviousVersionIds = await helper.GetPreviousVersionChoicesAsync<SectoralLendingReport>(sectoral.ResubmissionRequestId, hdr.SaccoType),
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
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401, "Unauthorized");
                }

                // Use the actual saccoId from parameter and saccoType from token
                var GroupToUse = _context.RatingDefinations
                    .Where(r => r.SaccoType == loggedInSacco.SaccoType && r.RatingName == "CAELS")
                    .FirstOrDefault();

                if (GroupToUse == null)
                {
                    return BadRequest("Rating definition not found for the specified Sacco type");
                }

                var result = await camelsAnalysisService.CalculateAnalysisAsync(GroupToUse.Id, periodId, saccoId, loggedInSacco.SaccoType);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating CAMELS analysis");
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        [HttpGet("dt/GetPerfomanceReportPdf/{periodId}/{saccoId}")]
        public async Task<ActionResult<SaccoPerformanceReportDTO>> GetDTPerfomanceReportPdf(string periodId, string saccoId, [FromQuery] string? ratingName)
        {
            try
            {
                // Initialize report
                var report = new SaccoPerformanceReportDTO();

                // Get sacco details from compliance service
                var saccoDetails = await complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    return BadRequest("Sacco not found");
                }

                report.ReportDate = DateTime.Now;
                report.SaccoName = saccoDetails.SaccoName;

                // Get current period
                var currentPeriod = await _context.ReturnPeriods.FindAsync(periodId);
                if (currentPeriod == null)
                {
                    return BadRequest("Period not found");
                }

                // Get rating definition for DT saccos
                // Use provided ratingName or default to CAMELS
                var requestedRating = string.IsNullOrWhiteSpace(ratingName) ? "CAELS" : ratingName.ToUpperInvariant();
                var ratingDef = await _context.RatingDefinations
                    .Include(rd => rd.RatingForms)
                    .FirstOrDefaultAsync(rd => rd.SaccoType == "0" && rd.RatingName == requestedRating);

                if (ratingDef == null)
                {
                    return BadRequest($"Rating definition '{requestedRating}' not found for DT SACCO type");
                }

                // Extract selector from rating name (CAMEL, CAELS, CAMELS, CAEL)
                string selector = ratingDef.RatingName.ToUpperInvariant();

                // Add prudential standards
                report.PrudentialStandards.Add("CoreCapital", "≥10M");
                report.PrudentialStandards.Add("CoreCapitalToTotalAssets", "≥10%");
                report.PrudentialStandards.Add("InstitutionalCapitalToTotalAssets", "≥8%");
                report.PrudentialStandards.Add("NPL", "<5%");
                report.PrudentialStandards.Add("NonEarningAssets", "<10%");

                // Get historical periods (current + 2 previous)
                var periods = await _context.ReturnPeriods
                    .Where(p => p.FrequencyId == currentPeriod.FrequencyId &&
                                p.StartDate <= currentPeriod.StartDate)
                    .Include(x => x.FrequencyCatalog)
                    .OrderByDescending(p => p.StartDate)
                    .Take(3)
                    .ToListAsync();

                var requiredFormCodes = ratingDef.RatingForms
                    .Select(rf => rf.FormCode)
                    .ToList();

                foreach (var period in periods)
                {
                    // Get filed submissions for this period
                    var submissions = await GetFiledSubmissionsForPeriod(period.Id, saccoId, requiredFormCodes);

                    // Find submissions by form category
                    var finPosSub = FindSubmissionByCategory(submissions, FormCategory.FinancialPosition);
                    var incStmtSub = FindSubmissionByCategory(submissions, FormCategory.StatementOfComprehensiveIncome);
                    var capAdeSub = FindSubmissionByCategory(submissions, FormCategory.CapitalAdequacy);
                    var liqSub = FindSubmissionByCategory(submissions, FormCategory.LiquidityStatement);
                    var depSub = FindSubmissionByCategory(submissions, FormCategory.DepositReturn);
                    var riskSub = FindSubmissionByCategory(submissions, FormCategory.RiskClassification);
                    var invSub = FindSubmissionByCategory(submissions, FormCategory.InvestmentReturn);
                    var mgtSub = FindSubmissionByCategory(submissions, FormCategory.Management);

                    // Fetch data from submissions
                    var balanceSheet = finPosSub == null
                        ? new DTFinancialPositionReturn()
                        : await FromSubmissionAsync<DTFinancialPositionReturn>(finPosSub)
                          ?? new DTFinancialPositionReturn();

                    var incomeStatement = incStmtSub == null
                        ? new DTComprehensiveIncomeReturn()
                        : await FromSubmissionAsync<DTComprehensiveIncomeReturn>(incStmtSub)
                          ?? new DTComprehensiveIncomeReturn();

                    var capitalReturn = capAdeSub == null
                        ? new DTCapitalAdequacyReturn()
                        : await FromSubmissionAsync<DTCapitalAdequacyReturn>(capAdeSub)
                          ?? new DTCapitalAdequacyReturn();

                    var liquidityReturn = liqSub == null
                        ? new DTLiquidityReturn()
                        : await FromSubmissionAsync<DTLiquidityReturn>(liqSub)
                          ?? new DTLiquidityReturn();

                    var depositReturns = depSub == null
                        ? new List<DepositReturn>()
                        : await FromSubmissionListAsync<DepositReturn>(depSub)
                          ?? new List<DepositReturn>();

                    var riskClassificationReturn = riskSub == null
                        ? new List<DTRiskClassificationReturn>()
                        : await FromSubmissionListAsync<DTRiskClassificationReturn>(riskSub)
                          ?? new List<DTRiskClassificationReturn>();

                    var investmentReturn = invSub == null
                        ? new DTInvestmentReturn()
                        : await FromSubmissionAsync<DTInvestmentReturn>(invSub)
                          ?? new DTInvestmentReturn();

                    var managementReturns = mgtSub == null
                        ? null
                        : await FromSubmissionAsync<ManagementReturn>(mgtSub);

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
                        PeriodLabel = period.Name,
                        PeriodType = period.FrequencyCatalog?.Name ?? "Returns",
                        PeriodStartDate = period.StartDate.ToString("yyyy-MM-dd"),

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
                        PeriodStartDate = DateTime.Now.ToString("yyyy-MM-dd"),

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

                // Get approval actions for the current period submissions
                var currentPeriodSubmissions = await GetFiledSubmissionsForPeriod(currentPeriod.Id, saccoId, requiredFormCodes);
                if (currentPeriodSubmissions.Any())
                {
                    /*         var submissionIds = currentPeriodSubmissions.Select(s => s.Id).ToList();
                    var approvals = await _context.ApprovalActions
                        .Where(a => submissionIds.Contains(a.ResubmissionRequestId))
                        .Include(a => a.WorkFlowStep)
                        .ToListAsync();
                    report.approvalActions = approvals;*/
                }

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
        }



        [HttpGet("nwdt/CalculateAnalysis/{periodId}/{saccoId}")]
        public async Task<ActionResult<CamelsRatingsDTO>> CalculateNwdtAnalysis(string periodId, string saccoId)
        {
            try
            {
                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401, "Unauthorized");
                }

                // Verify this is an NWDT SACCO
                if (loggedInSacco.SaccoType != Constants.SaccoType.NWDT.ToString())
                {
                    return BadRequest("This endpoint is only for NWDT SACCOs");
                }

                // Get the rating definition for NWDT
                var GroupToUse = _context.RatingDefinations
                    .Where(r => r.SaccoType == Constants.SaccoType.NWDT.ToString() && r.RatingName == "CAELS")
                    .FirstOrDefault();

                if (GroupToUse == null)
                {
                    return BadRequest("Rating definition not found for NWDT SACCOs");
                }

                var result = await camelsAnalysisService.CalculateAnalysisAsync(GroupToUse.Id, periodId, saccoId, Constants.SaccoType.NWDT.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }



        [HttpGet("nwdt/GetPerfomanceReportPdf/{periodId}/{saccoId}")]
        public async Task<ActionResult<NWDTPerformanceReportDTO>> GetNwdtPerfomanceReportPdf(string periodId, string saccoId, [FromQuery] string? ratingName)
        {
            try
            {
                var report = new NWDTPerformanceReportDTO();

                // Get sacco details from compliance service
                var saccoDetails = await complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    return BadRequest("Sacco not found");
                }

                report.ReportDate = DateTime.Now;
                report.SaccoName = saccoDetails.SaccoName;

                // Get current period
                var currentPeriod = await _context.ReturnPeriods.FindAsync(periodId);
                if (currentPeriod == null)
                {
                    return BadRequest("Period not found");
                }

                // Get rating definition for NWDT saccos
                // Use provided ratingName or default to CAMELS
                var requestedRating = string.IsNullOrWhiteSpace(ratingName) ? "CAMELS" : ratingName.ToUpperInvariant();
                var ratingDef = await _context.RatingDefinations
                    .Include(rd => rd.RatingForms)
                    .FirstOrDefaultAsync(rd => rd.SaccoType == "1" && rd.RatingName == requestedRating);

                if (ratingDef == null)
                {
                    return BadRequest($"Rating definition '{requestedRating}' not found for NWDT SACCO type");
                }

                // Extract selector from rating name (CAMEL, CAELS, CAMELS, CAEL)
                string selector = ratingDef.RatingName.ToUpperInvariant();
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

                // Get historical periods (current + 2 previous)
                var periods = await _context.ReturnPeriods
                    .Where(p => p.FrequencyId == currentPeriod.FrequencyId &&
                                p.StartDate <= currentPeriod.StartDate)
                    .Include(x => x.FrequencyCatalog)
                    .OrderByDescending(p => p.StartDate)
                    .Take(3)
                    .ToListAsync();

                var requiredFormCodes = ratingDef.RatingForms
                    .Select(rf => rf.FormCode)
                    .ToList();

                foreach (var period in periods)
                {
                    // Get filed submissions for this period
                    var submissions = await GetFiledSubmissionsForPeriod(period.Id, saccoId, requiredFormCodes);

                    // Find submissions by form category
                    var finPosSub = FindSubmissionByCategory(submissions, FormCategory.FinancialPosition);
                    var incStmtSub = FindSubmissionByCategory(submissions, FormCategory.StatementOfComprehensiveIncome);
                    var capAdeSub = FindSubmissionByCategory(submissions, FormCategory.CapitalAdequacy);
                    var liqSub = FindSubmissionByCategory(submissions, FormCategory.LiquidityStatement);
                    var depSub = FindSubmissionByCategory(submissions, FormCategory.DepositReturn);
                    var riskSub = FindSubmissionByCategory(submissions, FormCategory.RiskClassification);
                    var invSub = FindSubmissionByCategory(submissions, FormCategory.InvestmentReturn);
                    var mgtSub = FindSubmissionByCategory(submissions, FormCategory.Management);

                    // Fetch data from submissions - NWDT forms
                    var balanceSheet = finPosSub == null
                        ? new NWDTFinancialPositionReturn()
                        : await FromNWDTSubmissionAsync<NWDTFinancialPositionReturn>(finPosSub)
                          ?? new NWDTFinancialPositionReturn();

                    var incomeStatement = incStmtSub == null
                        ? new NWDTComprehensiveIncomeReturn()
                        : await FromNWDTSubmissionAsync<NWDTComprehensiveIncomeReturn>(incStmtSub)
                          ?? new NWDTComprehensiveIncomeReturn();

                    var capitalReturn = capAdeSub == null
                        ? new NWDTCapitalAdequacyReturn()
                        : await FromNWDTSubmissionAsync<NWDTCapitalAdequacyReturn>(capAdeSub)
                          ?? new NWDTCapitalAdequacyReturn();

                    var liquidityReturn = liqSub == null
                        ? new NWDTLiquidityReturn()
                        : await FromNWDTSubmissionAsync<NWDTLiquidityReturn>(liqSub)
                          ?? new NWDTLiquidityReturn();

                    var depositReturns = depSub == null
                        ? new List<NWDTDepositReturn>()
                        : await FromNWDTSubmissionListAsync<NWDTDepositReturn>(depSub)
                          ?? new List<NWDTDepositReturn>();

                    var riskClassificationReturn = riskSub == null
                        ? new List<NWDTRiskClassificationReturn>()
                        : await FromNWDTSubmissionListAsync<NWDTRiskClassificationReturn>(riskSub)
                          ?? new List<NWDTRiskClassificationReturn>();

                    var investmentReturn = invSub == null
                        ? new NWDTInvestmentReturn()
                        : await FromNWDTSubmissionAsync<NWDTInvestmentReturn>(invSub)
                          ?? new NWDTInvestmentReturn();

                    var managementReturn = mgtSub == null
                        ? null
                        : await FromNWDTSubmissionAsync<ManagementReturn>(mgtSub);
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
                        PeriodLabel = period.Name,
                        PeriodType = period.FrequencyCatalog?.Name ?? "Returns",
                        PeriodStartDate = period.StartDate.ToString("yyyy-MM-dd"),

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
                        periodData.CoreCapitalToTotalDepositsRatio = 0 / SavedCapitalAdequacy.TotalDepositsLiabilitiesPerBalanceSheet / SavedCapitalAdequacy.TotalAssets;
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
                        PeriodStartDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    };

                    report.Periods.Add(blank);
                }

                // Get approval actions for the current period submissions
                var currentPeriodSubmissions = await GetFiledSubmissionsForPeriod(currentPeriod.Id, saccoId, requiredFormCodes);
                if (currentPeriodSubmissions.Any())
                {
                    /* var submissionIds = currentPeriodSubmissions.Select(s => s.Id).ToList();
                     var approvals = await _context.ApprovalActions
                         .Where(a => submissionIds.Contains(a.ResubmissionRequestId))
                         .Include(a => a.WorkFlowStep)
                         .ToListAsync();*/
                    //report.approvalActions = approvals;
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

        }


        [HttpPost("BulkSubmitByPeriod")]
        public async Task<ActionResult> BulkSubmitByPeriod([FromBody] BulkSubmissionRequestDTO request)
        {
            try
            {
                var logged = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (logged == null || string.IsNullOrWhiteSpace(logged.SaccoId) || string.IsNullOrWhiteSpace(logged.SaccoType))
                    return StatusCode(401, "Unauthorized");

                var rating = await _context.RatingDefinations
                            .Where(r => r.RatingName == "CAELS" && r.SaccoType == logged.SaccoType)
                            .OrderByDescending(r => r.CreatedAt)
                            .FirstOrDefaultAsync();

                var result = await _returnSubmissionService.BulkSubmitByPeriodAsync(
              request.PeriodId,
              logged.SaccoId,
              logged.SaccoType);

                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                var period = await _context.ReturnPeriods.FindAsync(request.PeriodId);

                if (period?.FrequencyId == 5)// quarterly returns detected
                {
                    // Always start workflow for quarterly, but handle analysis only if complete
                    if (result.IsPeriodComplete)
                    {
                        BackgroundJob.Enqueue<IReturnSubmissionService>(s =>
                            s.SendSubmissionConfirmationEmailAsync(logged.SaccoId, request.PeriodId));

                        if (rating != null)
                        {
                            var analysisJobId = BackgroundJob.Enqueue<ICamelsAnalysisService>(s =>
                                s.CalculateCurrentAnalysisAsync(
                                    rating.Id, request.PeriodId, logged.SaccoId, logged.SaccoType));

                            // Chain workflow to run after analysis succeeds
                            BackgroundJob.ContinueJobWith<IWorkflowEngineService>(analysisJobId, s =>
                                s.StartWorkflowAsync(request.PeriodId, logged.SaccoId, null));
                        }
                        else
                        {
                            _logger.LogWarning("No CAELS rating definition for saccoType {SaccoType}", logged.SaccoType);

                            // Still start workflow even without rating
                            BackgroundJob.Enqueue<IWorkflowEngineService>(s =>
                                s.StartWorkflowAsync(request.PeriodId, logged.SaccoId, null));
                        }
                    }
                    else
                    {
                        // Incomplete: Start workflow directly, without analysis
                        BackgroundJob.Enqueue<IWorkflowEngineService>(s =>
                            s.StartWorkflowAsync(request.PeriodId, logged.SaccoId, null));
                    }
                }
                else
                {
                    // Non-quarterly: Workflow per successful detail, regardless of completeness
                    foreach (var d in result.Details.Where(x => x.Success))
                    {
                        BackgroundJob.Enqueue<IWorkflowEngineService>(s =>
                            s.StartWorkflowAsync(request.PeriodId, logged.SaccoId, d.SubmissionId));
                    }

                    if (result.IsPeriodComplete)
                    {
                        BackgroundJob.Enqueue<IReturnSubmissionService>(s =>
                            s.SendSubmissionConfirmationEmailAsync(logged.SaccoId, request.PeriodId));
                    }
                }

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk submission for period {PeriodId}", request.PeriodId);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }




        // Admin Grouped Returns Methods
        [HttpGet("admin/grouped-returns")]
        public async Task<ActionResult<List<AdminGroupedReturnDTO>>> GetAdminGroupedReturns([FromQuery] AdminReturnFilterDTO filter)
        {
            try
            {
                LoggedInEntity loggedInAdmin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInAdmin == null || string.IsNullOrEmpty(loggedInAdmin.UserId))
                {
                    return Unauthorized();
                }

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
                LoggedInEntity loggedInAdmin = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInAdmin == null || string.IsNullOrEmpty(loggedInAdmin.UserId))
                {
                    return StatusCode(401, "Unauthorized");
                }

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

        [HttpGet("dt/GetPerfomanceReport/{periodId}/{saccoId}")]
        public async Task<ActionResult<SaccoPerformanceReportDTO>> GetDTPerfomanceReport(string periodId, string saccoId)
        {
            try
            {
                var report = new SaccoPerformanceReportDTO();

                // Get sacco details from compliance service
                var saccoDetails = await complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    return BadRequest("Sacco not found");
                }

                report.ReportDate = DateTime.Now;
                report.SaccoName = saccoDetails.SaccoName;

                // Add prudential standards for DT
                report.PrudentialStandards.Add("CoreCapital", "≥10M");
                report.PrudentialStandards.Add("CoreCapita/Total Assets", "≥10%");
                report.PrudentialStandards.Add("InstitutionalCapitalToTotalAssets", ">8%");
                report.PrudentialStandards.Add("Retained Earnings/Core Capital", ">5%");
                report.PrudentialStandards.Add("Non-Performing Loans", "≤5%");
                report.PrudentialStandards.Add("Non-Earning Assets", "≤5%");
                report.PrudentialStandards.Add("Total Finacial Investements to Core Capital", "<40%");
                report.PrudentialStandards.Add("Subsidary Investments to Total Assets", "<50%");
                report.PrudentialStandards.Add("Equity Investments to Core Capital Ratio", "<20%");
                report.PrudentialStandards.Add("Other Financial investments to Core Capital Ratio ", "<30%");
                report.PrudentialStandards.Add("Liquid Assets/Short-term Liabilities", ">15%");
                report.PrudentialStandards.Add("External Borrowing to Total Assets", "<25%");
                report.PrudentialStandards.Add("Gross loans /Total Assets", "70 - 80%");
                report.PrudentialStandards.Add("Gross Loans to Deposits", ">100%");

                // Get current period
                var currentPeriod = await _context.ReturnPeriods.FindAsync(periodId);
                if (currentPeriod == null)
                {
                    return BadRequest("Period not found");
                }

                // Get historical periods (current + 2 previous)
                var periods = await _context.ReturnPeriods
                    .Where(p => p.FrequencyId == currentPeriod.FrequencyId &&
                                p.StartDate <= currentPeriod.StartDate)
                    .Include(x => x.FrequencyCatalog)
                    .OrderByDescending(p => p.StartDate)
                    .Take(3)
                    .ToListAsync();

                // Get rating definition for DT saccos (using CAMEL which includes Management forms)
                var ratingDef = await _context.RatingDefinations
                    .Include(rd => rd.RatingForms)
                    .FirstOrDefaultAsync(rd => rd.SaccoType == "0" && rd.RatingName == "CAELS");

                if (ratingDef == null)
                {
                    return BadRequest("Rating definition not found for DT SACCO type");
                }

                var requiredFormCodes = ratingDef.RatingForms
                    .Select(rf => rf.FormCode)
                    .ToList();

                foreach (var period in periods)
                {
                    // Get filed submissions for this period
                    var submissions = await GetFiledSubmissionsForPeriod(period.Id, saccoId, requiredFormCodes);

                    // Find submissions by form category
                    var finPosSub = FindSubmissionByCategory(submissions, FormCategory.FinancialPosition);
                    var incStmtSub = FindSubmissionByCategory(submissions, FormCategory.StatementOfComprehensiveIncome);
                    var capAdeSub = FindSubmissionByCategory(submissions, FormCategory.CapitalAdequacy);
                    var liqSub = FindSubmissionByCategory(submissions, FormCategory.LiquidityStatement);
                    var depSub = FindSubmissionByCategory(submissions, FormCategory.DepositReturn);
                    var riskSub = FindSubmissionByCategory(submissions, FormCategory.RiskClassification);
                    var invSub = FindSubmissionByCategory(submissions, FormCategory.InvestmentReturn);
                    var mgtSub = FindSubmissionByCategory(submissions, FormCategory.Management);

                    // Fetch data from submissions
                    var balanceSheet = finPosSub == null
                        ? new DTFinancialPositionReturn()
                        : await FromNWDTSubmissionAsync<DTFinancialPositionReturn>(finPosSub)
                          ?? new DTFinancialPositionReturn();

                    var incomeStatement = incStmtSub == null
                        ? new DTComprehensiveIncomeReturn()
                        : await FromNWDTSubmissionAsync<DTComprehensiveIncomeReturn>(incStmtSub)
                          ?? new DTComprehensiveIncomeReturn();

                    var capitalReturn = capAdeSub == null
                        ? new DTCapitalAdequacyReturn()
                        : await FromNWDTSubmissionAsync<DTCapitalAdequacyReturn>(capAdeSub)
                          ?? new DTCapitalAdequacyReturn();

                    var liquidityReturn = liqSub == null
                        ? new DTLiquidityReturn()
                        : await FromNWDTSubmissionAsync<DTLiquidityReturn>(liqSub)
                          ?? new DTLiquidityReturn();

                    var depositReturns = depSub == null
                        ? new List<DepositReturn>()
                        : await _context.DepositReturns
                              .Where(d => d.ReturnSubmissionId == depSub.Id)
                              .ToListAsync();

                    var riskClassificationReturn = riskSub == null
                        ? new List<DTRiskClassificationReturn>()
                        : await _context.DTRiskClassificationReturns
                              .Where(r => r.ReturnSubmissionId == riskSub.Id)
                              .ToListAsync();

                    var investmentReturn = invSub == null
                        ? new DTInvestmentReturn()
                        : await FromNWDTSubmissionAsync<DTInvestmentReturn>(invSub)
                          ?? new DTInvestmentReturn();

                    // Build period data similar to NWDT but with DT specific calculations
                    var periodData = new SaccoPerformanceReportDTO.PeriodData
                    {
                        PeriodLabel = period.Name,
                        PeriodType = period.FrequencyCatalog.Name,
                        PeriodStartDate = period.StartDate.ToShortDateString(),
                        PeriodEndDate = period.EndDate.ToShortDateString(),
                        CoreCapital = capitalReturn.CoreCapital,
                        CoreCapitalToTotalAssets = capitalReturn.CoreCapitalToAssetsRatio,
                        CoreCapitalToTotalDepositsRatio = capitalReturn.CoreCapitalToDepositsRatio,
                        NonPerformingLoans = ReturnAnalysisHelper.CalculateNonPerformingLoans(riskClassificationReturn),
                        TotalAssets = capitalReturn.TotalAssets,
                        TotalDeposits = balanceSheet.TotalDepositLiabilities,
                        GrossLoansForm4 = balanceSheet.GrossLoanPortfolio,
                        GrossLoansForm6 = capitalReturn.LoansAndAdvances,
                        LiquidAssets = liquidityReturn.NetLiquidAssets,
                        ExternalBorrowing = balanceSheet.ExternalBorrowings,
                        NetIncome = incomeStatement.NetIncomeAfterTaxesAndDonations,
                        TotalIncome = incomeStatement.TotalFinancialIncome,
                        OperatingExpenses = incomeStatement.TotalOperatingExpenses
                    };

                    report.Periods.Add(periodData);
                }

                if (report.Periods.Count < 2 && report.Periods.Count > 0)
                {
                    var blank = new SaccoPerformanceReportDTO.PeriodData
                    {
                        PeriodLabel = "N/A",
                        PeriodType = "N/A",
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

        [HttpGet("nwdt/GetPerfomanceReport/{periodId}/{saccoId}")]
        public async Task<ActionResult<NWDTPerformanceReportDTO>> GetNwdtPerfomanceReport(string periodId, string saccoId)
        {
            try
            {
                var report = new NWDTPerformanceReportDTO();

                // Get sacco details from compliance service
                var saccoDetails = await complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    return BadRequest("Sacco not found");
                }

                report.ReportDate = DateTime.UtcNow;
                report.SaccoName = saccoDetails.SaccoName;

                // Add prudential standards
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

                // Get current period
                var currentPeriod = await _context.ReturnPeriods.FindAsync(periodId);
                if (currentPeriod == null)
                {
                    return BadRequest("Period not found");
                }

                // Get historical periods (current + 2 previous)
                var periods = await _context.ReturnPeriods
                    .Where(p => p.FrequencyId == currentPeriod.FrequencyId &&
                                p.StartDate <= currentPeriod.StartDate)
                    .OrderByDescending(p => p.StartDate)
                    .Take(3)
                    .ToListAsync();

                // Get rating definition for NWDT saccos (using CAMEL which includes Management forms)
                var ratingDef = await _context.RatingDefinations
                    .Include(rd => rd.RatingForms)
                    .FirstOrDefaultAsync(rd => rd.SaccoType == "1" && rd.RatingName == "CAMEL Forms For NWDT");

                if (ratingDef == null)
                {
                    return BadRequest("Rating definition not found for NWDT SACCO type");
                }

                var requiredFormCodes = ratingDef.RatingForms
                    .Select(rf => rf.FormCode)
                    .ToList();

                foreach (var period in periods)
                {
                    // Get filed submissions for this period
                    var submissions = await GetFiledSubmissionsForPeriod(period.Id, saccoId, requiredFormCodes);

                    // Find submissions by form category
                    var finPosSub = FindSubmissionByCategory(submissions, FormCategory.FinancialPosition);
                    var incStmtSub = FindSubmissionByCategory(submissions, FormCategory.StatementOfComprehensiveIncome);
                    var capAdeSub = FindSubmissionByCategory(submissions, FormCategory.CapitalAdequacy);
                    var liqSub = FindSubmissionByCategory(submissions, FormCategory.LiquidityStatement);
                    var depSub = FindSubmissionByCategory(submissions, FormCategory.DepositReturn);
                    var riskSub = FindSubmissionByCategory(submissions, FormCategory.RiskClassification);
                    var invSub = FindSubmissionByCategory(submissions, FormCategory.InvestmentReturn);
                    var mgtSub = FindSubmissionByCategory(submissions, FormCategory.Management);

                    // Fetch data from submissions
                    var balanceSheet = finPosSub == null
                        ? new NWDTFinancialPositionReturn()
                        : await FromNWDTSubmissionAsync<NWDTFinancialPositionReturn>(finPosSub)
                          ?? new NWDTFinancialPositionReturn();

                    var incomeStatement = incStmtSub == null
                        ? new NWDTComprehensiveIncomeReturn()
                        : await FromNWDTSubmissionAsync<NWDTComprehensiveIncomeReturn>(incStmtSub)
                          ?? new NWDTComprehensiveIncomeReturn();

                    var capitalReturn = capAdeSub == null
                        ? new NWDTCapitalAdequacyReturn()
                        : await FromNWDTSubmissionAsync<NWDTCapitalAdequacyReturn>(capAdeSub)
                          ?? new NWDTCapitalAdequacyReturn();

                    var liquidityReturn = liqSub == null
                        ? new NWDTLiquidityReturn()
                        : await FromNWDTSubmissionAsync<NWDTLiquidityReturn>(liqSub)
                          ?? new NWDTLiquidityReturn();

                    var depositReturns = depSub == null
                        ? new List<NWDTDepositReturn>()
                        : await _context.NWDTDepositReturns
                              .Where(d => d.ReturnSubmissionId == depSub.Id)
                              .ToListAsync();

                    var riskClassificationReturn = riskSub == null
                        ? new List<NWDTRiskClassificationReturn>()
                        : await _context.NWDTRiskClassificationReturns
                              .Where(r => r.ReturnSubmissionId == riskSub.Id)
                              .ToListAsync();

                    var investmentReturn = invSub == null
                        ? new NWDTInvestmentReturn()
                        : await FromNWDTSubmissionAsync<NWDTInvestmentReturn>(invSub)
                          ?? new NWDTInvestmentReturn();

                    var managementReturn = mgtSub == null
                        ? new ManagementReturn()
                        : await FromNWDTSubmissionAsync<ManagementReturn>(mgtSub)
                          ?? new ManagementReturn();

                    // Use fetched data
                    var SavedCapitalAdequacy = capitalReturn;
                    var SavedLiquidityStatement = liquidityReturn;
                    var SavedDepositReturn = depositReturns;
                    var SavedRiskClassification = riskClassificationReturn;
                    var SavedInvestmentReturn = investmentReturn;
                    var SavedFinancialPositionStatement = balanceSheet;
                    var SavedComprehensiveStatement = incomeStatement;

                    var periodData = new NWDTPerformanceReportDTO.NWDTPeriodData
                    {
                        PeriodLabel = period.StartDate.ToString("yyyy-MM-dd"),
                        PeriodType = "Returns",
                        PeriodStartDate = period.StartDate.ToShortDateString(),

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
                        periodData.CoreCapitalToTotalDepositsRatio = 0 / SavedCapitalAdequacy.TotalDepositsLiabilitiesPerBalanceSheet / SavedCapitalAdequacy.TotalAssets;
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
                        PeriodStartDate = DateTime.Now.ToString("yyyy-MM-dd"),
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

        // Helper method to get filed submissions for a period
        private async Task<Dictionary<string, ReturnSubmission>> GetFiledSubmissionsForPeriod(string periodId, string saccoId, IReadOnlyCollection<string> requiredFormCodes)
        {
            return await _context.ReturnSubmissions
                .Where(rs => rs.SaccoId == saccoId && rs.IsActive)
                .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                .Where(rs =>
                    rs.ExpectedReturn.PeriodId == periodId &&
                    requiredFormCodes.Contains(rs.ExpectedReturn.ReturnForm.Code))
                .GroupBy(rs => rs.ExpectedReturnId)
                .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())
                .ToDictionaryAsync(rs => rs.ExpectedReturnId);
        }

        // Helper method to find submission by form category
        private ReturnSubmission? FindSubmissionByCategory(Dictionary<string, ReturnSubmission> submissions, FormCategory category)
        {
            foreach (var submission in submissions.Values)
            {
                var form = submission.ExpectedReturn?.ReturnForm;
                if (form != null && (FormCategory)form.Category == category)
                {
                    return submission;
                }
            }
            return null;
        }

        // Helper method to fetch NWDT data from submission
        private async Task<T?> FromNWDTSubmissionAsync<T>(ReturnSubmission sub) where T : class
        {
            return await _context.Set<T>()
                .FirstOrDefaultAsync(e => EF.Property<string>(e, "ReturnSubmissionId") == sub.Id);
        }

        // For DT single entity
        private async Task<T?> FromSubmissionAsync<T>(ReturnSubmission sub) where T : class
        {
            return await _context.Set<T>()
                .FirstOrDefaultAsync(e => EF.Property<string>(e, "ReturnSubmissionId") == sub.Id);
        }

        // For DT list
        private async Task<List<T>> FromSubmissionListAsync<T>(ReturnSubmission sub) where T : class
        {
            return await _context.Set<T>()
                .Where(e => EF.Property<string>(e, "ReturnSubmissionId") == sub.Id)
                .ToListAsync();
        }

        // For NWDT list
        private async Task<List<T>> FromNWDTSubmissionListAsync<T>(ReturnSubmission sub) where T : class
        {
            return await _context.Set<T>()
                .Where(e => EF.Property<string>(e, "ReturnSubmissionId") == sub.Id)
                .ToListAsync();
        }

    }
}
