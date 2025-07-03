using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.PeriodManagement;
using Returns.Models.Data;

namespace Returns.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrequencyCatalogController : ControllerBase
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<FrequencyCatalogController> _logger;

        public FrequencyCatalogController(ReturnsDbContext context, ILogger<FrequencyCatalogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/FrequencyCatalog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FrequencyCatalogDto>>> GetFrequencyCatalogs()
        {
            var catalogs = await _context.FrequencyCatalogs
                .Where(f => f.IsActive)
                .OrderBy(f => f.IntervalDays)
                .Select(f => new FrequencyCatalogDto
                {
                    Id = f.Id,
                    Code = f.Code,
                    Name = f.Name,
                    IntervalDays = f.IntervalDays,
                    DefaultDeadlineOffset = f.DefaultDeadlineOffset,
                    LabelStrategy = f.LabelStrategy,
                    IsActive = f.IsActive
                })
                .ToListAsync();

            return Ok(catalogs);
        }

        // GET: api/FrequencyCatalog/all (includes inactive)
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<FrequencyCatalogDto>>> GetAllFrequencyCatalogs()
        {
            var catalogs = await _context.FrequencyCatalogs
                .OrderBy(f => f.IntervalDays)
                .Select(f => new FrequencyCatalogDto
                {
                    Id = f.Id,
                    Code = f.Code,
                    Name = f.Name,
                    IntervalDays = f.IntervalDays,
                    DefaultDeadlineOffset = f.DefaultDeadlineOffset,
                    LabelStrategy = f.LabelStrategy,
                    IsActive = f.IsActive
                })
                .ToListAsync();

            return Ok(catalogs);
        }

        // GET: api/FrequencyCatalog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FrequencyCatalogDto>> GetFrequencyCatalog(int id)
        {
            var catalog = await _context.FrequencyCatalogs
                .Where(f => f.Id == id)
                .Select(f => new FrequencyCatalogDto
                {
                    Id = f.Id,
                    Code = f.Code,
                    Name = f.Name,
                    IntervalDays = f.IntervalDays,
                    DefaultDeadlineOffset = f.DefaultDeadlineOffset,
                    LabelStrategy = f.LabelStrategy,
                    IsActive = f.IsActive
                })
                .FirstOrDefaultAsync();

            if (catalog == null)
            {
                return NotFound();
            }

            return Ok(catalog);
        }

        // GET: api/FrequencyCatalog/by-code/MTH
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<FrequencyCatalogDto>> GetFrequencyCatalogByCode(string code)
        {
            var catalog = await _context.FrequencyCatalogs
                .Where(f => f.Code == code.ToUpper())
                .Select(f => new FrequencyCatalogDto
                {
                    Id = f.Id,
                    Code = f.Code,
                    Name = f.Name,
                    IntervalDays = f.IntervalDays,
                    DefaultDeadlineOffset = f.DefaultDeadlineOffset,
                    LabelStrategy = f.LabelStrategy,
                    IsActive = f.IsActive
                })
                .FirstOrDefaultAsync();

            if (catalog == null)
            {
                return NotFound();
            }

            return Ok(catalog);
        }

        // PUT: api/FrequencyCatalog/5/toggle-active
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var catalog = await _context.FrequencyCatalogs.FindAsync(id);
            if (catalog == null)
            {
                return NotFound();
            }

            catalog.IsActive = !catalog.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Toggled active status for frequency {catalog.Code} to {catalog.IsActive}");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FrequencyCatalogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool FrequencyCatalogExists(int id)
        {
            return _context.FrequencyCatalogs.Any(e => e.Id == id);
        }
    }
}