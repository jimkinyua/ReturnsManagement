using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.PeriodManagement;
using Returns.Models;
using Returns.Models.Data;
using System.Security.Claims;

namespace Returns.Controllers
{
    [Route("api/returns/[controller]")]
    [ApiController]
    public class ReportingYearsController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<ReportingYearsController> _logger;

        public ReportingYearsController(ReturnsDbContext context, ILogger<ReportingYearsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ReportingYears
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportingYearDto>>> GetReportingYears()
        {
            var years = await _context.ReportingYears
                .Include(y => y.Periods)
                .OrderByDescending(y => y.Year)
                .Select(y => new ReportingYearDto
                {
                    Id = y.Id,
                    Year = y.Year,
                    IsActive = y.IsActive,
                    CreatedAt = y.CreatedAt,
                    CreatedBy = y.CreatedBy,
                    PeriodCount = y.Periods.Count,
                    EndDateDate = y.EndDateDate,
                    StartDate = y.StartDate

                })
                .ToListAsync();

            return Ok(years);
        }

        // GET: api/ReportingYears/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportingYearDto>> GetReportingYear(int id)
        {
            var year = await _context.ReportingYears
                .Include(y => y.Periods)
                .Where(y => y.Id == id)
                .Select(y => new ReportingYearDto
                {
                    Id = y.Id,
                    Year = y.Year,
                    IsActive = y.IsActive,
                    CreatedAt = y.CreatedAt,
                    CreatedBy = y.CreatedBy,
                    PeriodCount = y.Periods.Count,
                    EndDateDate = y.EndDateDate,
                    StartDate = y.StartDate
                })
                .FirstOrDefaultAsync();

            if (year == null)
            {
                return NotFound();
            }

            return Ok(year);
        }

        // POST: api/ReportingYears
        [HttpPost]
        public async Task<ActionResult<ReportingYearDto>> CreateReportingYear(CreateYearDto createYearDto)
        {
            try
            {
                // Check if year already exists
                var existingYear = await _context.ReportingYears.FirstOrDefaultAsync(y => y.Year == createYearDto.Year);

                if (existingYear != null)
                {
                    return BadRequest($"Year {createYearDto.Year} already exists.");
                }

                var currentUser = User.Identity?.Name ?? "System";

                var reportingYear = new ReportingYear
                {
                    Year = createYearDto.Year,
                    IsActive = createYearDto.IsActive,
                    CreatedBy = currentUser,
                    CreatedAt = DateTime.Now,
                    StartDate = createYearDto.StartDate,
                    EndDateDate = createYearDto.EndDateDate
                };

                _context.ReportingYears.Add(reportingYear);
                await _context.SaveChangesAsync();

                var result = new ReportingYearDto
                {
                    Id = reportingYear.Id,
                    Year = reportingYear.Year,
                    IsActive = reportingYear.IsActive,
                    CreatedAt = reportingYear.CreatedAt,
                    CreatedBy = reportingYear.CreatedBy,
                    StartDate = reportingYear.StartDate,
                    EndDateDate = reportingYear.EndDateDate,
                    PeriodCount = 0
                };

                _logger.LogInformation($"Created reporting year {reportingYear.Year} by user {currentUser}");

                return CreatedAtAction(nameof(GetReportingYear), new { id = reportingYear.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reporting year");
                return StatusCode(500, "An error occurred while creating the reporting year.");
            }
        }

        // PUT: api/ReportingYears/5/toggle-active
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var year = await _context.ReportingYears.FindAsync(id);
            if (year == null)
            {
                return NotFound();
            }

            year.IsActive = !year.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Toggled active status for year {year.Year} to {year.IsActive}");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!YearExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/ReportingYears/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReportingYear(int id)
        {
            var year = await _context.ReportingYears
                .Include(y => y.Periods)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (year == null)
            {
                return NotFound();
            }

            if (year.Periods.Any())
            {
                return BadRequest("Cannot delete a year that has periods. Please delete all periods first.");
            }

            _context.ReportingYears.Remove(year);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted reporting year {year.Year}");

            return NoContent();
        }

        private bool YearExists(int id)
        {
            return _context.ReportingYears.Any(e => e.Id == id);
        }
    }
}