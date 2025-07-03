using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.PeriodManagement;
using Returns.Helpers.Interfaces;
using System.Security.Claims;

namespace Returns.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodGeneratorController : ControllerBase
    {
        private readonly IPeriodGenerator _periodGenerator;
        private readonly ILogger<PeriodGeneratorController> _logger;

        public PeriodGeneratorController(IPeriodGenerator periodGenerator, ILogger<PeriodGeneratorController> logger)
        {
            _periodGenerator = periodGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Preview periods that would be generated for a reporting year and selected frequencies
        /// </summary>
        /// <param name="request">The period generation request containing year and frequency IDs</param>
        /// <returns>A preview of periods that would be created</returns>
        [HttpPost("preview")]
        [ProducesResponseType(typeof(PeriodGenerationPreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PeriodGenerationPreviewDto>> PreviewPeriods([FromBody] PeriodGenerationRequestDto request)
        {
            try
            {
                if (request.FrequencyIds == null || !request.FrequencyIds.Any())
                {
                    return BadRequest("At least one frequency must be selected.");
                }

                var preview = await _periodGenerator.PreviewPeriodsAsync(request.YearId, request.FrequencyIds);

                _logger.LogInformation(
                    "Period preview generated for year {YearId} with {FrequencyCount} frequencies. Total periods to generate: {TotalPeriods}",
                    request.YearId, request.FrequencyIds.Count, preview.TotalPeriodsToGenerate);

                return Ok(preview);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for period preview");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating period preview");
                return StatusCode(500, "An error occurred while generating the period preview.");
            }
        }

        /// <summary>
        /// Confirm and generate periods for a reporting year and selected frequencies
        /// </summary>
        /// <param name="request">The period generation request containing year and frequency IDs</param>
        /// <returns>The result of the period generation operation</returns>
        [HttpPost("confirm")]
        [ProducesResponseType(typeof(PeriodGenerationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PeriodGenerationResultDto>> ConfirmAndGeneratePeriods([FromBody] PeriodGenerationRequestDto request)
        {
            try
            {
                if (request.FrequencyIds == null || !request.FrequencyIds.Any())
                {
                    return BadRequest("At least one frequency must be selected.");
                }

                var currentUser = User.Identity?.Name ?? "System";
                var result = await _periodGenerator.GeneratePeriodsAsync(request.YearId, request.FrequencyIds, currentUser);

                if (!result.Success)
                {
                    _logger.LogWarning("Period generation failed: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation(
                    "Successfully generated {CreatedCount} periods and skipped {SkippedCount} existing periods for year {YearId}",
                    result.TotalPeriodsCreated, result.TotalPeriodsSkipped, request.YearId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating periods");
                return StatusCode(500, "An error occurred while generating periods.");
            }
        }

        /// <summary>
        /// Get summary of existing periods for a reporting year
        /// </summary>
        /// <param name="yearId">The reporting year ID</param>
        /// <returns>Summary of existing periods grouped by frequency</returns>
        [HttpGet("summary/{yearId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetPeriodSummary(int yearId)
        {
            try
            {
                // This could be moved to a service method, but for now, let's create a simple summary
                var preview = await _periodGenerator.PreviewPeriodsAsync(yearId, new List<int>());

                return Ok(new
                {
                    YearId = preview.YearId,
                    Year = preview.Year,
                    StartDate = preview.StartDate,
                    EndDate = preview.EndDate,
                    Message = "Use the preview endpoint with frequency IDs to see period details."
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting period summary");
                return StatusCode(500, "An error occurred while retrieving the period summary.");
            }
        }
    }
}