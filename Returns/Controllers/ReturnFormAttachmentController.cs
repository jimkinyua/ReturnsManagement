using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Forms;
using Returns.Helpers;
using Returns.Helpers.Interfaces;
using Returns.Models.Data;
using static Returns.Helpers.TokenHelper;

namespace Returns.Controllers
{
    //[Authorize]
    [Route("api/returns")]
    [ApiController]
    public class ReturnFormAttachmentController : ControllerBase
    {
        private readonly IReturnFormAttachmentService _attachmentService;
        private readonly ILogger<ReturnFormAttachmentController> _logger;
        private readonly ReturnsDbContext _context;

        public ReturnFormAttachmentController(
            IReturnFormAttachmentService attachmentService,
            ILogger<ReturnFormAttachmentController> logger,
            ReturnsDbContext context)
        {
            _attachmentService = attachmentService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Preview form attachments before confirming
        /// </summary>
        [HttpPost("preview")]
        public async Task<ActionResult<ReturnFormAttachmentPreviewDto>> PreviewAttachment([FromBody] AttachReturnFormsDto request)
        {
            try
            {
                var preview = await _attachmentService.PreviewAttachmentAsync(request);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating attachment preview");
                return StatusCode(500, new { error = "Failed to generate preview" });
            }
        }

        [HttpGet("GetObligations")]
        public async Task<IActionResult> GetObligations(DateTime? date = null)
        {
            try
            {
                var target = date ?? DateTime.Today;

                LoggedInEntity loggedInSacco = TokenHelper.GetLoggedInSaccoFromCurrentRequest(Request);
                if (loggedInSacco == null || string.IsNullOrEmpty(loggedInSacco.SaccoId) || string.IsNullOrEmpty(loggedInSacco.SaccoType))
                {
                    return StatusCode(401);
                }


                // 1. which periods cover that day?
                var periodIds = await _context.ReturnPeriods
                    .Where(p => target >= p.StartDate && target <= p.EndDate)
                    .Select(p => p.Id)
                    .ToListAsync();

                // 2. which forms were required?
                var rows = await _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.Period)
                    .Where(er => periodIds.Contains(er.PeriodId)
                              && er.ReturnForm.SaccoTypeId == loggedInSacco.SaccoType
                              && er.IsActive)
                    .Select(er => new {
                        Period = er.Period.Name,
                        Return = er.ReturnForm.FormName,
                        Deadline = er.FilingDeadline
                    })
                    .OrderBy(r => r.Period)
                    .ThenBy(r => r.Return)
                    .ToListAsync();

                return Ok(rows);
            }
            catch (Exception)
            {

                throw;
            }
        }



        /// <summary>
        /// Confirm and execute form attachment
        /// </summary>
        [HttpPost("confirm")]
        public async Task<ActionResult<ReturnFormAttachmentResultDto>> ConfirmAttachment(
            [FromBody] ConfirmReturnFormAttachmentDto request)
        {
            try
            {
                var result = await _attachmentService.ConfirmAttachmentAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming attachment");
                return StatusCode(500, new { error = "Failed to confirm attachment" });
            }
        }

        /// <summary>
        /// Get forms attached to a specific period
        /// </summary>
        [HttpGet("period/{periodId}")]
        public async Task<ActionResult<List<AttachedReturnFormDto>>> GetAttachedForms(string periodId)
        {
            try
            {
                var forms = await _attachmentService.GetAttachedFormsAsync(periodId);
                return Ok(forms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attached forms for period {PeriodId}", periodId);
                return StatusCode(500, new { error = "Failed to get attached forms" });
            }
        }

        /// <summary>
        /// Remove a form attachment (soft delete)
        /// </summary>
        [HttpDelete("{expectedReturnId}")]
        public async Task<ActionResult> RemoveAttachment(string expectedReturnId)
        {
            try
            {
                var success = await _attachmentService.RemoveAttachmentAsync(expectedReturnId);

                if (success)
                {
                    return Ok(new { message = "Attachment removed successfully" });
                }

                return NotFound(new { error = "Attachment not found or cannot be removed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing attachment {ExpectedReturnId}", expectedReturnId);
                return StatusCode(500, new { error = "Failed to remove attachment" });
            }
        }

        /// <summary>
        /// Update filing deadlines for multiple expected returns
        /// </summary>
        [HttpPut("filing-deadlines")]
        public async Task<ActionResult> UpdateFilingDeadlines(
            [FromBody] UpdateFilingDeadlinesDto request)
        {
            try
            {
                var success = await _attachmentService.UpdateFilingDeadlinesAsync(request);

                if (success)
                {
                    return Ok(new { message = "Filing deadlines updated successfully" });
                }

                return BadRequest(new { error = "No valid expected returns found to update" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating filing deadlines");
                return StatusCode(500, new { error = "Failed to update filing deadlines" });
            }
        }

        /// <summary>
        /// Batch attach forms to all periods in a year
        /// </summary>
       /* [HttpPost("batch-attach-year")]
        public async Task<ActionResult<ReturnFormAttachmentPreviewDto>> BatchAttachToYear(
            [FromBody] BatchAttachToYearDto request)
        {
            try
            {
                // Convert to standard attachment request
                var attachRequest = new AttachReturnFormsDto
                {
                    ReturnFormIds = request.ReturnFormIds,
                    FilingDeadlineDays = request.FilingDeadlineDays,
                    ApplyToAllPeriodsInYear = true,
                    YearId = request.YearId
                };

                var preview = await _attachmentService.PreviewAttachmentAsync(attachRequest);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch attach to year");
                return StatusCode(500, new { error = "Failed to generate batch attachment preview" });
            }
        }*/

        /// <summary>
        /// Get available forms for attachment by sacco type
        /// </summary>
        [HttpGet("available-forms")]
        public async Task<ActionResult<List<AvailableFormDto>>> GetAvailableForms(
            [FromQuery] string? saccoTypeId = null)
        {
            try
            {
                var formsQuery = _context.ReturnForms
                    .Where(f => f.IsActive);

                if (!string.IsNullOrEmpty(saccoTypeId))
                {
                    formsQuery = formsQuery.Where(f => f.SaccoTypeId == saccoTypeId);
                }

                var forms = await formsQuery
                    .Select(f => new AvailableFormDto
                    {
                        Id = f.Id,
                        FormName = f.FormName,
                        Code = f.Code,
                        SaccoTypeId = f.SaccoTypeId,
                        Category = f.Category
                    })
                    .OrderBy(f => f.FormName)
                    .ToListAsync();

                return Ok(forms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available forms");
                return StatusCode(500, new { error = "Failed to get available forms" });
            }
        }
    }

    // Additional DTO for year-based batch attachment
    public class BatchAttachToYearDto
    {
        public int YearId { get; set; }
        public List<string> ReturnFormIds { get; set; } = new();
        public int? FilingDeadlineDays { get; set; }
    }

    // DTO for available forms
    public class AvailableFormDto
    {
        public string Id { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string SaccoTypeId { get; set; } = null!;
        public Returns.Helpers.Enums.FormCategory Category { get; set; }
    }
}