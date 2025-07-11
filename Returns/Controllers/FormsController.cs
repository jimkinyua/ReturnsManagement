using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Returns.DTOs.Forms;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.Helpers;
using Returns.Models;
using Returns.Models.Data;
using static Returns.Helpers.TokenHelper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;
using Returns.Helpers.Interfaces;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class FormsController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IReturnSubmissionService _returnSubmissionService;

        public FormsController(ReturnsDbContext context, IReturnSubmissionService returnSubmissionService)
        {
            _context = context;
            _returnSubmissionService = returnSubmissionService;
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
                await FormsHelper.ValidateFormTypeUniqueness(createFormDTO, _context);
                var templatePath = "";
                if (createFormDTO.Category != Helpers.Enums.FormCategory.Other)
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
                    Code = createFormDTO.DisplayName,
                    SaccoTypeId = createFormDTO.SaccoTypeId,
                    Category = createFormDTO.Category,
                    TemplateUrl = templatePath,
                    IsActive = true
                };

                _context.ReturnForms.Add(form);
                await _context.SaveChangesAsync();
                var m = new FormDTO
                {
                    Id = form.Id,
                    Name = form.FormName,
                    DisplayName = form.Code,
                    SaccoTypeId = form.SaccoTypeId,
                    Category = form.Category,
                    IsActive = form.IsActive,
                    TemplateUrl = form.Category == Helpers.Enums.FormCategory.Other ? "" : $"{baseUrl}{form.TemplateUrl}"
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

                // Check if form has any expected returns
                var hasExpectedReturns = await _context.ExpectedReturns
                    .AnyAsync(er => er.ReturnFormId == formId && er.IsActive);

                if (hasExpectedReturns)
                {
                    return BadRequest(new
                    {
                        error = "Cannot delete form that has expected returns. Please remove all expected returns first or disable the form instead."
                    });
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

        // update form
        [HttpPut("UpdateForm/{formId}")]
        public async Task<ActionResult<FormDTO>> UpdateFormAsync(string formId, [FromForm] UpdateFormDTO updateFormDTO)
        {
            try
            {
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var form = await _context.ReturnForms.FindAsync(formId);
                if (form == null)
                {
                    return NotFound();
                }

                // Update basic properties
                form.FormName = updateFormDTO.Name;
                form.Code = updateFormDTO.DisplayName;
                form.SaccoTypeId = updateFormDTO.SaccoTypeId;
                form.Category = updateFormDTO.Category;

                // Handle template update if requested
                if (updateFormDTO.UpdateTemplate && updateFormDTO.Category != Helpers.Enums.FormCategory.Other)
                {
                    if (updateFormDTO.Template == null)
                    {
                        return BadRequest("Template file is required when UpdateTemplate is true");
                    }

                    if (!FormsHelper.IsValidExcelFile(updateFormDTO.Template))
                    {
                        return StatusCode(500, "Only Excel files with .xlsx extension are allowed.");
                    }

                    // Delete old template file if exists
                    if (!string.IsNullOrEmpty(form.TemplateUrl))
                    {
                        FormsHelper.DeleteFile(form.TemplateUrl);
                    }

                    // Save new template
                    var templatePath = await FormsHelper.SaveFileAsync(updateFormDTO.Template, "Templates", updateFormDTO.DisplayName);
                    if (templatePath == null)
                    {
                        return StatusCode(500, "Failed to save template file.");
                    }

                    form.TemplateUrl = templatePath;
                }
                else if (updateFormDTO.Category == Helpers.Enums.FormCategory.Other)
                {
                    // If changing to "other form", remove template
                    if (!string.IsNullOrEmpty(form.TemplateUrl))
                    {
                        FormsHelper.DeleteFile(form.TemplateUrl);
                        form.TemplateUrl = "";
                    }
                }

                await _context.SaveChangesAsync();

                var responseDto = new FormDTO
                {
                    Id = form.Id,
                    Name = form.FormName,
                    DisplayName = form.Code,
                    SaccoTypeId = form.SaccoTypeId,
                    Category = form.Category,
                    IsActive = form.IsActive,
                    TemplateUrl = form.Category == Helpers.Enums.FormCategory.Other ? "" : $"{baseUrl}{form.TemplateUrl}"
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }


        /// <summary>
        /// Get forms due for a specific year and month
        /// </summary>
        /// <param name="year">The year (e.g., 2024)</param>
        /// <param name="month">The month (1-12)</param>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpGet("GetFormsDueByMonth")]
        public async Task<ActionResult<List<FormsDueByMonthDTO>>> GetFormsDueByMonth(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                // Validate input
                if (month < 1 || month > 12)
                {
                    return BadRequest("Month must be between 1 and 12");
                }

                // Get the first and last day of the specified month
                var firstDayOfMonth = new DateTime(year, month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Find all expected returns where the filing deadline falls within this month
                var expectedReturnsQuery = _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.ReportingYear)
                    .Where(er => er.FilingDeadline.Date.Month >= firstDayOfMonth.Date.Month &&
                                er.FilingDeadline.Date.Month <= lastDayOfMonth.Date.Month &&
                                er.IsActive);

                // Filter by sacco type if provided
                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    expectedReturnsQuery = expectedReturnsQuery.Where(er => er.ReturnForm.SaccoTypeId == saccoTypeId);
                }

                var expectedReturns = await expectedReturnsQuery.ToListAsync();

                // Map to DTOs
                var formsDue = expectedReturns.Select(er => new FormsDueByMonthDTO
                {
                    FormId = er.ReturnFormId,
                    ExpectedReturnId = er.Id,
                    FormName = er.ReturnForm.FormName,
                    FormCode = er.ReturnForm.Code,
                    PeriodId = er.PeriodId,
                    PeriodName = er.Period.Name,
                    PeriodStartDate = er.Period.StartDate,
                    PeriodEndDate = er.Period.EndDate,
                    FilingDeadline = er.FilingDeadline,
                    Status = er.Status,
                    TemplateUrl = er.ReturnForm.Category == Helpers.Enums.FormCategory.Other ? null : $"{baseUrl}{er.ReturnForm.TemplateUrl}",
                    SaccoTypeId = er.ReturnForm.SaccoTypeId
                })
                .OrderBy(f => f.FilingDeadline)
                .ThenBy(f => f.FormName)
                .ToList();

                return Ok(formsDue);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Enable or disable a form
        /// </summary>
        [HttpPut("ToggleFormStatus/{formId}")]
        public async Task<ActionResult> ToggleFormStatus(string formId, [FromQuery] bool isActive)
        {
            try
            {
                var form = await _context.ReturnForms.FindAsync(formId);
                if (form == null)
                {
                    return NotFound();
                }

                form.IsActive = isActive;
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Form {(isActive ? "enabled" : "disabled")} successfully" });
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Get all forms (with optional filtering)
        /// </summary>
        [HttpGet("GetAllForms")]
        public async Task<ActionResult<List<FormDTO>>> GetAllForms(
            [FromQuery] string? saccoTypeId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                var formsQuery = _context.ReturnForms.AsQueryable();

                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    formsQuery = formsQuery.Where(f => f.SaccoTypeId == saccoTypeId);
                }

                if (isActive.HasValue)
                {
                    formsQuery = formsQuery.Where(f => f.IsActive == isActive.Value);
                }

                var forms = await formsQuery
                    .Select(f => new FormDTO
                    {
                        Id = f.Id,
                        Name = f.FormName,
                        DisplayName = f.Code,
                        SaccoTypeId = f.SaccoTypeId,
                        Category = f.Category,
                        IsActive = f.IsActive,
                        TemplateUrl = f.Category == Helpers.Enums.FormCategory.Other ? "" : $"{baseUrl}{f.TemplateUrl}"
                    })
                    .OrderBy(f => f.Name)
                    .ToListAsync();

                return Ok(forms);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Get a yearly summary of forms due
        /// </summary>
        /// <param name="year">The year (e.g., 2024)</param>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpGet("GetFormsDueByYear")]
        public async Task<ActionResult<List<FormsDueByMonthDTO>>> GetFormsDueByYear(
            [FromQuery] int year,
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                // Get the first and last day of the year
                var firstDayOfYear = new DateTime(year, 1, 1);
                var lastDayOfYear = new DateTime(year, 12, 31);

                // Find all expected returns where the filing deadline falls within this year
                var expectedReturnsQuery = _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.ReportingYear)
                    .Where(er => er.FilingDeadline >= firstDayOfYear.Date &&
                                er.FilingDeadline <= lastDayOfYear.Date &&
                                er.IsActive);

                // Filter by sacco type if provided
                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    expectedReturnsQuery = expectedReturnsQuery.Where(er => er.ReturnForm.SaccoTypeId == saccoTypeId);
                }

                var expectedReturns = await expectedReturnsQuery.ToListAsync();

                // Map to DTOs
                var formsDue = expectedReturns.Select(er => new FormsDueByMonthDTO
                {
                    FormId = er.ReturnFormId,
                    FormName = er.ReturnForm.FormName,
                    FormCode = er.ReturnForm.Code,
                    PeriodId = er.PeriodId,
                    PeriodName = er.Period.Name,
                    PeriodStartDate = er.Period.StartDate,
                    PeriodEndDate = er.Period.EndDate,
                    FilingDeadline = er.FilingDeadline,
                    Status = er.Status,
                    TemplateUrl = er.ReturnForm.Category == Helpers.Enums.FormCategory.Other ? null : $"{baseUrl}{er.ReturnForm.TemplateUrl}",
                    SaccoTypeId = er.ReturnForm.SaccoTypeId
                })
                .OrderBy(f => f.FilingDeadline)
                .ThenBy(f => f.FormName)
                .ToList();

                return Ok(formsDue);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Get a summary of forms due with counts by status
        /// </summary>
        /// <param name="year">Optional: Filter by year</param>
        /// <param name="month">Optional: Filter by month (requires year)</param>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpGet("GetFormsDueSummary")]
        public async Task<ActionResult<FormsDueSummaryDTO>> GetFormsDueSummary(
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                // Build the date range
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (year.HasValue)
                {
                    if (month.HasValue)
                    {
                        if (month.Value < 1 || month.Value > 12)
                        {
                            return BadRequest("Month must be between 1 and 12");
                        }
                        startDate = new DateTime(year.Value, month.Value, 1);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                    }
                    else
                    {
                        startDate = new DateTime(year.Value, 1, 1);
                        endDate = new DateTime(year.Value, 12, 31);
                    }
                }

                // Build the query
                var expectedReturnsQuery = _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.Period)
                    .Where(er => er.IsActive);

                // Apply date filter if provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    expectedReturnsQuery = expectedReturnsQuery.Where(er =>
                        er.FilingDeadline >= startDate.Value &&
                        er.FilingDeadline <= endDate.Value);
                }

                // Apply sacco type filter if provided
                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    expectedReturnsQuery = expectedReturnsQuery.Where(er =>
                        er.ReturnForm.SaccoTypeId == saccoTypeId);
                }

                var expectedReturns = await expectedReturnsQuery.ToListAsync();

                var currentDate = DateTime.Now;

                // Calculate overall summary
                var summary = new FormsDueSummaryDTO
                {
                    TotalExpectedReturns = expectedReturns.Count,
                    DueCount = expectedReturns.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Due && er.FilingDeadline >= currentDate),
                    LateCount = expectedReturns.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Late ||
                                                           (er.Status == Helpers.Enums.ExpectedStatus.Due && er.FilingDeadline < currentDate)),
                    FiledCount = expectedReturns.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Filed),
                    WaivedCount = expectedReturns.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Waived)
                };

                // Group by form for detailed breakdown
                var byForm = expectedReturns
                    .GroupBy(er => new { er.ReturnForm.FormName, er.ReturnForm.Code })
                    .Select(g => new FormsDueSummaryByFormDTO
                    {
                        FormName = g.Key.FormName,
                        FormCode = g.Key.Code,
                        TotalExpected = g.Count(),
                        DueCount = g.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Due && er.FilingDeadline >= currentDate),
                        LateCount = g.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Late ||
                                                 (er.Status == Helpers.Enums.ExpectedStatus.Due && er.FilingDeadline < currentDate)),
                        FiledCount = g.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Filed),
                        WaivedCount = g.Count(er => er.Status == Helpers.Enums.ExpectedStatus.Waived)
                    })
                    .OrderBy(f => f.FormName)
                    .ToList();

                summary.ByForm = byForm;

                return Ok(summary);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Get forms by category
        /// </summary>
        /// <param name="category">The form category</param>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpGet("GetFormsByCategory")]
        public async Task<ActionResult<List<FormDTO>>> GetFormsByCategory(
            [FromQuery] Helpers.Enums.FormCategory category,
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                var formsQuery = _context.ReturnForms
                    .Where(f => f.Category == category && f.IsActive);

                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    formsQuery = formsQuery.Where(f => f.SaccoTypeId == saccoTypeId);
                }

                var forms = await formsQuery
                    .Select(f => new FormDTO
                    {
                        Id = f.Id,
                        Name = f.FormName,
                        DisplayName = f.Code,
                        SaccoTypeId = f.SaccoTypeId,
                        Category = f.Category,
                        IsActive = f.IsActive,
                        TemplateUrl = f.Category == Helpers.Enums.FormCategory.Other ? "" : $"{baseUrl}{f.TemplateUrl}"
                    })
                    .OrderBy(f => f.Name)
                    .ToListAsync();

                return Ok(forms);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Get a summary of forms grouped by category
        /// </summary>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpGet("GetFormCategorySummary")]
        public async Task<ActionResult<List<FormCategorySummaryDTO>>> GetFormCategorySummary(
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                var formsQuery = _context.ReturnForms.AsQueryable();

                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    formsQuery = formsQuery.Where(f => f.SaccoTypeId == saccoTypeId);
                }

                var groupedForms = await formsQuery
                    .GroupBy(f => f.Category)
                    .Select(g => new FormCategorySummaryDTO
                    {
                        Category = g.Key,
                        FormCount = g.Count(),
                        ActiveFormCount = g.Count(f => f.IsActive),
                        InactiveFormCount = g.Count(f => !f.IsActive)
                    })
                    .OrderBy(g => g.Category)
                    .ToListAsync();

                return Ok(groupedForms);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Get forms due for the current month, grouped by period, with clear submission statuses
        /// Optimized for performance with simplified queries and clear status tracking
        /// </summary>
        /// <param name="year">The year (e.g., 2024)</param>
        /// <param name="month">The month (1-12)</param>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpGet("GetFormsDueByMonthGrouped")]
        public async Task<ActionResult<FormsDueGroupedResponseDTO>> GetFormsDueByMonthGrouped(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                // Validate input
                if (month < 1 || month > 12)
                {
                    return BadRequest("Month must be between 1 and 12");
                }

                // Get the first and last day of the specified month
                var firstDayOfMonth = new DateTime(year, month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var currentDate = DateTime.Now;

                // Optimized query: Get expected returns with submission data in a single query
                var expectedReturnsQuery = _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.ReportingYear)
                    .Include(er => er.ReturnSubmissions.Where(rs => rs.IsActive))
                    .Where(er => er.FilingDeadline.Date.Month >= firstDayOfMonth.Date.Month &&
                                er.FilingDeadline.Date.Month <= lastDayOfMonth.Date.Month &&
                                er.IsActive);

                // Filter by sacco type if provided
                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    expectedReturnsQuery = expectedReturnsQuery.Where(er => er.ReturnForm.SaccoTypeId == saccoTypeId);
                }

                var expectedReturns = await expectedReturnsQuery.ToListAsync();

                // Get submission statuses efficiently using the service
                var expectedReturnIds = expectedReturns.Select(er => er.Id).ToList();
                var submissionData = await _returnSubmissionService.GetSubmissionStatusesAsync(expectedReturnIds);

                // Group by frequency with optimized submission status determination
                var groupedData = expectedReturns
                    .GroupBy(er => new { er.Period.FrequencyCatalog.Code, er.Period.FrequencyCatalog.Name })
                    .Select(g => new GroupedFormsDueDTO
                    {
                        FrequencyCode = g.Key.Code,
                        FrequencyName = g.Key.Name,
                        TotalFormsInGroup = g.Count(),
                        FiledCount = g.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.Submitted),
                        DueCount = g.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline >= currentDate),
                        LateCount = g.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline < currentDate),
                        Forms = g.Select(er => CreateFormDueDTO(er, baseUrl, currentDate, submissionData.GetValueOrDefault(er.Id)))
                            .OrderBy(f => f.FilingDeadline)
                            .ThenBy(f => f.FormName)
                            .ToList()
                    })
                    .OrderBy(g => GetFrequencyOrder(g.FrequencyCode))
                    .ToList();

                var response = new FormsDueGroupedResponseDTO
                {
                    TotalExpectedReturns = expectedReturns.Count,
                    TotalFiled = expectedReturns.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.Submitted),
                    TotalDue = expectedReturns.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline >= currentDate),
                    TotalLate = expectedReturns.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline < currentDate),
                    TotalWaived = 0, // No waivers allowed
                    GroupedByFrequency = groupedData
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Get a yearly summary of forms due, grouped by frequency
        /// Enhanced with submission status and group-level submission capabilities
        /// </summary>
        /// <param name="year">The year (e.g., 2024)</param>
        /// <param name="saccoTypeId">Optional: Filter by sacco type</param>
        [HttpGet("GetFormsDueByYearGrouped")]
        public async Task<ActionResult<FormsDueGroupedResponseDTO>> GetFormsDueByYearGrouped(
            [FromQuery] int year,
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                var baseUrl = _configuration.GetSection("GateWayConfigs:GatewayURL").Value;

                // Get the first and last day of the year
                var firstDayOfYear = new DateTime(year, 1, 1);
                var lastDayOfYear = new DateTime(year, 12, 31);

                // Find all expected returns where the filing deadline falls within this year
                var expectedReturnsQuery = _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                    .Include(er => er.Period)
                        .ThenInclude(p => p.ReportingYear)
                    .Where(er => er.FilingDeadline >= firstDayOfYear.Date &&
                                er.FilingDeadline <= lastDayOfYear.Date &&
                                er.IsActive);

                // Filter by sacco type if provided
                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    expectedReturnsQuery = expectedReturnsQuery.Where(er => er.ReturnForm.SaccoTypeId == saccoTypeId);
                }

                var expectedReturns = await expectedReturnsQuery.ToListAsync();
                var currentDate = DateTime.Now;

                // Get submission statuses efficiently using the service
                var expectedReturnIds = expectedReturns.Select(er => er.Id).ToList();
                var submissionData = await _returnSubmissionService.GetSubmissionStatusesAsync(expectedReturnIds);

                // Group by frequency with optimized submission status determination
                var groupedData = expectedReturns
                    .GroupBy(er => new { er.Period.FrequencyCatalog.Code, er.Period.FrequencyCatalog.Name })
                    .Select(g => new GroupedFormsDueDTO
                    {
                        FrequencyCode = g.Key.Code,
                        FrequencyName = g.Key.Name,
                        TotalFormsInGroup = g.Count(),
                        FiledCount = g.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.Submitted),
                        DueCount = g.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline >= currentDate),
                        LateCount = g.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline < currentDate),
                        Forms = g.Select(er => CreateFormDueDTO(er, baseUrl, currentDate, submissionData.GetValueOrDefault(er.Id)))
                            .OrderBy(f => f.FilingDeadline)
                            .ThenBy(f => f.FormName)
                            .ToList()
                    })
                    .OrderBy(g => GetFrequencyOrder(g.FrequencyCode))
                    .ToList();

                var response = new FormsDueGroupedResponseDTO
                {
                    TotalExpectedReturns = expectedReturns.Count,
                    TotalFiled = expectedReturns.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.Submitted),
                    TotalDue = expectedReturns.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline >= currentDate),
                    TotalLate = expectedReturns.Count(er => submissionData.GetValueOrDefault(er.Id).Status == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted && er.FilingDeadline < currentDate),
                    TotalWaived = 0, // No waivers allowed
                    GroupedByFrequency = groupedData
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                CustomErrorHandler.LogException(ex);
                return StatusCode(500, CustomErrorHandler.HandleException(ex));
            }
        }

        /// <summary>
        /// Helper method to order frequencies logically
        /// </summary>
        private int GetFrequencyOrder(string frequencyCode)
        {
            return frequencyCode?.ToUpper() switch
            {
                "DAY" => 1,
                "WK" => 2,
                "BWK" => 3,
                "MTH" => 4,
                "QTR" => 5,
                "FY" => 6,
                _ => 99
            };
        }

        /// <summary>
        /// Create FormDueDTO with optimized status determination
        /// </summary>
        private FormsDueByMonthDTO CreateFormDueDTO(ExpectedReturn er, string baseUrl, DateTime currentDate, (Returns.DTOs.Returns.Returns_Submission.SubmissionStatus Status, string? SubmissionId, DateTime? SubmittedAt) submissionData)
        {
            var isSubmitted = submissionData.Status != Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted;
            var isLate = _returnSubmissionService.IsFormLate(er.FilingDeadline, submissionData.Status);

            return new FormsDueByMonthDTO
            {
                FormId = er.ReturnFormId,
                FormName = er.ReturnForm.FormName,
                ExpectedReturnId = er.Id,
                FormCode = er.ReturnForm.Code,
                PeriodId = er.PeriodId,
                PeriodName = er.Period.Name,
                PeriodStartDate = er.Period.StartDate,
                PeriodEndDate = er.Period.EndDate,
                FilingDeadline = er.FilingDeadline,
                Status = er.Status,
                TemplateUrl = er.ReturnForm.Category == Helpers.Enums.FormCategory.Other ? null : $"{baseUrl}{er.ReturnForm.TemplateUrl}",
                SaccoTypeId = er.ReturnForm.SaccoTypeId,
                IsSubmitted = isSubmitted,
                SubmissionId = submissionData.SubmissionId,
                SubmissionStatus = submissionData.Status,
                SubmittedAt = submissionData.SubmittedAt
            };
        }

    }
}
