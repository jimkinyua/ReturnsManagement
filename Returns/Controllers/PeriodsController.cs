using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs;
using Returns.Models;
using Returns.Models.CamelSetup;
using Returns.Models.Data;
using System.Globalization;

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

        [HttpGet]
        [HttpGet("GetPeriods")]
        public async Task<ActionResult<IEnumerable<PeriodDTO>>> GetPeriods()
        {
            var periodsList = await _context.Periods
                .ToListAsync();

            List<PeriodDTO> periods = periodsList.Select(period => new PeriodDTO
            {
                Id = period.Id,
                Name = period.Name,
            }).ToList();

            return Ok(periods);
        }

        private static string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }


        // GET: api/Periods/5
       /* [HttpGet("{id}")]
        public async Task<ActionResult<PeriodDTO>> GetPeriod(string id)
        {
            var period = await _context.Periods
                .Include(p => p.QuarterDates)
                .Include(p => p.ReturnForms)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (period == null)
            {
                return NotFound();
            }

            var periodDTO = new PeriodDTO
            {
                Id = period.Id,
                IsQuartely = period.IsQuaterly,
                Name = period.Name,
                Deadline = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(period.DeadlineMonth)} {period.DeadlineDay}",
                QuarterDates = period.QuarterDates?.Select(q => new QuarterDatesDTO
                (
                    q.StartMonth, q.StartDay, q.EndMonth, q.EndDay, q.DeadlineMonth, q.DeadlineDay
                )
                {
                    Id = q.Id,
                    PeriodId = q.PeriodId,
                    RespondedAt = q.RespondedAt
                }).ToList(),
                ReturnForms = period.ReturnForms?.Select(r => new ReturnFormDTO
                {
                    Id = r.Id,
                    FormName = r.FormName,
                    Code = r.Code,
                    TemplateUrl = r.TemplateUrl,
                    SaccoTypeId = r.SaccoTypeId,
                    IsCapitalAdequencyForm = r.IsCapitalAdequencyForm,
                    IsLiquidityStatement = r.IsLiquidityStatement,
                    IsRiskClassification = r.IsRiskClassification,
                    IsInvestmentReturn = r.IsInvestmentReturn,
                    IsFinancialPosition = r.IsFinancialPosition,
                    IsStatementOfComprehensiveIncome = r.IsStatementOfComprehensiveIncome,
                    IsDepositReturnForm = r.IsDepositReturnForm,
                    PeriodId = r.PeriodId,
                    RespondedAt = r.RespondedAt
                }).ToList()
            };

            return Ok(periodDTO);
        }*/

        // POST: api/Periods
       /* [HttpPost]
        public async Task<ActionResult<PeriodDTO>> CreatePeriod(PeriodDTO periodDTO)
        {
            // Create Period object
            var period = new Period
            {
                Id = Guid.NewGuid().ToString(),
                Name = periodDTO.Name,
                IsQuaterly = periodDTO.IsQuartely,
                DeadlineDay = periodDTO.Deadline != null ? int.Parse(periodDTO.Deadline.Split(' ')[1]) : 1,
                DeadlineMonth = periodDTO.Deadline != null
                    ? DateTime.ParseExact(periodDTO.Deadline.Split(' ')[0], "MMMM", CultureInfo.CurrentCulture).Month
                    : 1,
                RespondedAt = DateTime.UtcNow
            };

            // Initialize list for QuarterDates
            period.QuarterDates = new List<QuarterDates>();

            // Check if QuarterDates exist and process them
            if (periodDTO.QuarterDates != null && periodDTO.QuarterDates.Any())
            {
                foreach (var q in periodDTO.QuarterDates)
                {
                    var startParts = q.StartDate.Split(' ');
                    var endParts = q.EndDate.Split(' ');

                    var quarterDate = new QuarterDates
                    {
                        Id = Guid.NewGuid().ToString(),
                        StartMonth = DateTime.ParseExact(startParts[0], "MMMM", CultureInfo.CurrentCulture).Month,
                        StartDay = int.Parse(startParts[1]),
                        EndMonth = DateTime.ParseExact(endParts[0], "MMMM", CultureInfo.CurrentCulture).Month,
                        EndDay = int.Parse(endParts[1]),
                        DeadlineMonth = period.DeadlineMonth,
                        DeadlineDay = period.DeadlineDay,
                        PeriodId = period.Id,
                        RespondedAt = DateTime.UtcNow
                    };
                    _context.QuarterDates.Add(quarterDate); // Explicitly track QuarterDates
                    period.QuarterDates.Add(quarterDate);
                }
            }

            _context.Periods.Add(period);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPeriod), new { id = period.Id }, periodDTO);
        }*/


        // PUT: api/Periods/5
        /*[HttpPut("{id}")]
        public async Task<IActionResult> UpdatePeriod(string id, PeriodDTO periodDTO)
        {
            if (id != periodDTO.Id)
            {
                return BadRequest();
            }

            var period = await _context.Periods
                .Include(p => p.QuarterDates) // Include related QuarterDates
                .FirstOrDefaultAsync(p => p.Id == id);

            if (period == null)
            {
                return NotFound();
            }

            // Update main Period fields
            period.Name = periodDTO.Name;
            period.IsQuaterly = periodDTO.IsQuartely;
            period.DeadlineDay = int.Parse(periodDTO.Deadline.Split(' ')[1]);
            period.DeadlineMonth = DateTime.ParseExact(periodDTO.Deadline.Split(' ')[0], "MMMM", CultureInfo.CurrentCulture).Month;

            // ✅ Handle Quarterly Dates Update
            if (periodDTO.QuarterDates != null)
            {
                // Get existing quarter dates
                var existingQuarterDates = period.QuarterDates.ToList();

                // Remove quarter dates that are not in the DTO
                foreach (var existingQD in existingQuarterDates)
                {
                    if (!periodDTO.QuarterDates.Any(q => q.Id == existingQD.Id))
                    {
                        _context.QuarterDates.Remove(existingQD);
                    }
                }

                // Add or update quarter dates
                foreach (var q in periodDTO.QuarterDates)
                {
                    var existingQD = existingQuarterDates.FirstOrDefault(x => x.Id == q.Id);
                    var startParts = q.StartDate.Split(' ');
                    var endParts = q.EndDate.Split(' ');

                    if (existingQD != null)
                    {
                        // Update existing quarter date
                        existingQD.StartMonth = DateTime.ParseExact(startParts[0], "MMMM", CultureInfo.CurrentCulture).Month;
                        existingQD.StartDay = int.Parse(startParts[1]);
                        existingQD.EndMonth = DateTime.ParseExact(endParts[0], "MMMM", CultureInfo.CurrentCulture).Month;
                        existingQD.EndDay = int.Parse(endParts[1]);
                        existingQD.DeadlineMonth = period.DeadlineMonth;
                        existingQD.DeadlineDay = period.DeadlineDay;
                    }
                    else
                    {
                        // Add new quarter date
                        var newQuarterDate = new QuarterDates
                        {
                            Id = Guid.NewGuid().ToString(),
                            StartMonth = DateTime.ParseExact(startParts[0], "MMMM", CultureInfo.CurrentCulture).Month,
                            StartDay = int.Parse(startParts[1]),
                            EndMonth = DateTime.ParseExact(endParts[0], "MMMM", CultureInfo.CurrentCulture).Month,
                            EndDay = int.Parse(endParts[1]),
                            DeadlineMonth = period.DeadlineMonth,
                            DeadlineDay = period.DeadlineDay,
                            PeriodId = period.Id,
                            RespondedAt = DateTime.UtcNow
                        };

                        period.QuarterDates.Add(newQuarterDate);
                    }
                }
            }

            _context.Entry(period).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PeriodExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }*/


        // DELETE: api/Periods/5
      /*  [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeriod(string id)
        {
            var period = await _context.Periods.FindAsync(id);
            if (period == null)
            {
                return NotFound();
            }

            _context.Periods.Remove(period);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PeriodExists(string id)
        {
            return _context.Periods.Any(e => e.Id == id);
        }*/
    }
}
