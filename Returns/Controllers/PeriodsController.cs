using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs;
using Returns.Models;
using Returns.Models.CamelSetup;
using Returns.Models.Data;
using System.Globalization;
using System.Linq;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class PeriodsController : ControllerBase
    {
        private readonly ReturnsDbContext _context;

        public PeriodsController(ReturnsDbContext context)
        {
            _context = context;
        }

        public record PeriodDTO(string Id, string Name);
        public record FrequencyGroupDTO(string FrequencyName, List<PeriodDTO> Periods);

        [HttpGet("GetPeriods/{yearid}")]
        public async Task<ActionResult<IEnumerable<PeriodDTO>>> GetPeriods(int yearid)
        {
            var periods = await _context.ReturnPeriods
            .Include(p => p.FrequencyCatalog)
                                   .Where(p => p.YearId == yearid)
                                   .ToListAsync();
            var result = periods
                .GroupBy(p => p.FrequencyCatalog.Name)
                .OrderByDescending(g => g.Max(p => p.EndDate))
          .Select(g => new FrequencyGroupDTO(
              g.Key,
              g.OrderByDescending(p => p.EndDate)      // newest period first
               .Select(p => new PeriodDTO(p.Id, p.Name))
               .ToList()
          ))
          .ToList();
            return Ok(result);
        }

        private static string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }


    }
}
