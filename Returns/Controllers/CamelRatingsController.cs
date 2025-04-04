using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs;
using Returns.Models.CamelSetup;
using Returns.Models.Data;

namespace Returns.Controllers
{
    [Route("api/returns")]
    [ApiController]
    public class CamelRatingsController : ControllerBase
    {
        private readonly ReturnsDbContext _context;

        public CamelRatingsController(ReturnsDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetCamelCategories")]
        public async Task<ActionResult<IEnumerable<CamelCategoryDTO>>> GetCategories()
        {
            List<CamelCategoryDTO> camelCategories = new List<CamelCategoryDTO>();
            var categories = await _context.CamelCategories
                .ToListAsync();
            foreach (var category in categories)
            {
                CamelCategoryDTO camelCategory = new CamelCategoryDTO
                {
                    Id = category.Id,
                    Code = category.Code,
                    Name = category.Name
                };

                camelCategories.Add(camelCategory);
            }

            return Ok(camelCategories);
        }

        // Create Camel Category
        [HttpPost("CreateCamelCategory")]
        public async Task<ActionResult<CamelCategoryDTO>> CreateCategory(CreateCamelCategoryDTO camelCategoryDTO)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            // ensure no duplicate code or name
            var existingCategory = await _context.CamelCategories
                .FirstOrDefaultAsync(c => c.Code == camelCategoryDTO.Code || c.Name == camelCategoryDTO.Name);
            if (existingCategory != null)
            {
                return BadRequest("Category with the same code or name already exists");
            }


            CamelCategory camelCategory = new CamelCategory
            {
                Code = camelCategoryDTO.Code,
                Name = camelCategoryDTO.Name
            };

            _context.CamelCategories.Add(camelCategory);
            await _context.SaveChangesAsync();

            return Ok(camelCategoryDTO);
        }

        //  edit Camel Category
        [HttpPut("EditCamelCategory/{categoryId}")]
        public async Task<ActionResult<CamelCategoryDTO>> EditCategory(string categoryId, CamelCategoryDTO camelCategoryDTO)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var existingCategory = await _context.CamelCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }

            // ensure no duplicate code or name
            var existingCategoryWithSameCodeOrName = await _context.CamelCategories
                .FirstOrDefaultAsync(c => (c.Code == camelCategoryDTO.Code || c.Name == camelCategoryDTO.Name) && c.Id != categoryId);
            if (existingCategoryWithSameCodeOrName != null)
            {
                return BadRequest("Category with the same code or name already exists");
            }

            existingCategory.Code = camelCategoryDTO.Code;
            existingCategory.Name = camelCategoryDTO.Name;

            _context.CamelCategories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return Ok(camelCategoryDTO);
        }

        // Delete Camel Category
        [HttpDelete("DeleteCamelCategory/{categoryId}")]
        public async Task<ActionResult> DeleteCategory(string categoryId)
        {
            var existingCategory = await _context.CamelCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }

            _context.CamelCategories.Remove(existingCategory);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("GetCategoryIndicators/{categoryId}")]
        public async Task<ActionResult<IEnumerable<CameLIndicatorDTO>>> GetIndicators(string categoryId)
        {
            List<CameLIndicatorDTO> cameLIndicators = new List<CameLIndicatorDTO>();
            var indicators = await _context.CamelIndicators
                .Where(i => i.CategoryId == categoryId)
                .ToListAsync();
            foreach (var indicator in indicators)
            {
                CameLIndicatorDTO cameLIndicatorDTO = new CameLIndicatorDTO
                {
                    CategoryId = indicator.CategoryId,
                    Id = indicator.Id,
                    Name = indicator.Name,
                    Weight = indicator.Weight,
                    BetterHigher = indicator.BetterHigher
                };

                cameLIndicators.Add(cameLIndicatorDTO);
            }
            return Ok(cameLIndicators);
        }

        // Create Camel Indicator
        [HttpPost("CreateCategoryIndicator")]
        public async Task<ActionResult<CameLIndicatorDTO>> CreateIndicator(CreateCamelIndicatorDTO camelIndicatorDTO)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            // ensure no duplicate name
            var existingIndicator = await _context.CamelIndicators
                .FirstOrDefaultAsync(i => i.Name == camelIndicatorDTO.Name);
            if (existingIndicator != null)
            {
                return BadRequest("Indicator with the same name already exists");
            }

            CamelIndicator camelIndicator = new CamelIndicator
            {
                CategoryId = camelIndicatorDTO.CategoryId,
                Name = camelIndicatorDTO.Name,
                Weight = camelIndicatorDTO.Weight,
                BetterHigher = camelIndicatorDTO.BetterHigher
            };

            _context.CamelIndicators.Add(camelIndicator);
            await _context.SaveChangesAsync();

            return Ok(camelIndicatorDTO);
        }

        // Edit Camel Indicator
        [HttpPut("EditCategoryIndicator/{indicatorId}")]
        public async Task<ActionResult<CameLIndicatorDTO>> EditIndicator(string indicatorId, CameLIndicatorDTO camelIndicatorDTO)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var existingIndicator = await _context.CamelIndicators
                .FirstOrDefaultAsync(i => i.Id == indicatorId);
            if (existingIndicator == null)
            {
                return NotFound("Indicator not found");
            }

            // ensure no duplicate name
            var existingIndicatorWithSameName = await _context.CamelIndicators
                .FirstOrDefaultAsync(i => i.Name == camelIndicatorDTO.Name && i.Id != indicatorId);
            if (existingIndicatorWithSameName != null)
            {
                return BadRequest("Indicator with the same name already exists");
            }

            existingIndicator.Name = camelIndicatorDTO.Name;
            existingIndicator.Weight = camelIndicatorDTO.Weight;
            existingIndicator.BetterHigher = camelIndicatorDTO.BetterHigher;

            _context.CamelIndicators.Update(existingIndicator);
            await _context.SaveChangesAsync();

            return Ok(camelIndicatorDTO);
        }

        // Delete Camel Indicator
        [HttpDelete("DeleteCategoryIndicator/{indicatorId}")]
        public async Task<ActionResult> DeleteIndicator(string indicatorId)
        {
            var existingIndicator = await _context.CamelIndicators
                .FirstOrDefaultAsync(i => i.Id == indicatorId);
            if (existingIndicator == null)
            {
                return NotFound("Indicator not found");
            }

            _context.CamelIndicators.Remove(existingIndicator);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Get Indicator Thresholds

        [HttpGet("GetIndicatorThresholds/{indicatorId}")]
        public async Task<ActionResult<IEnumerable<CamelIndicatorThresholdDTO>>> GetIndicatorThresholds(string indicatorId)
        {
            List<CamelIndicatorThresholdDTO> camelIndicatorThresholds = new List<CamelIndicatorThresholdDTO>();
            var thresholds = await _context.IndicatorRatingThresholds
                .Where(t => t.IndicatorId == indicatorId)
                .ToListAsync();
            foreach (var threshold in thresholds)
            {
                CamelIndicatorThresholdDTO camelIndicatorThreshold = new CamelIndicatorThresholdDTO
                {
                    Id = threshold.Id,
                    IndicatorId = threshold.IndicatorId,
                    RatingLevel = threshold.RatingLevel,
                    ThresholdValue = threshold.ThresholdValue
                };

                camelIndicatorThresholds.Add(camelIndicatorThreshold);
            }

            return Ok(camelIndicatorThresholds);
        }

        // Create Indicator Threshold
        [HttpPost("CreateIndicatorThreshold")]
        public async Task<ActionResult<CamelIndicatorThresholdDTO>> CreateThreshold(CreateCamelIndicatorThresholdDTO camelIndicatorThresholdDTO)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            // ensure no duplicate rating level
            var existingThreshold = await _context.IndicatorRatingThresholds
                .FirstOrDefaultAsync(t => t.IndicatorId == camelIndicatorThresholdDTO.IndicatorId && t.RatingLevel == camelIndicatorThresholdDTO.RatingLevel);
            if (existingThreshold != null)
            {
                return BadRequest("Threshold with the same rating level already exists");
            }

            IndicatorRatingThreshold indicatorRatingThreshold = new IndicatorRatingThreshold
            {
                IndicatorId = camelIndicatorThresholdDTO.IndicatorId,
                RatingLevel = camelIndicatorThresholdDTO.RatingLevel,
                ThresholdValue = camelIndicatorThresholdDTO.ThresholdValue
            };

            _context.IndicatorRatingThresholds.Add(indicatorRatingThreshold);
            await _context.SaveChangesAsync();

            return Ok(camelIndicatorThresholdDTO);
        }

        // Edit Indicator Threshold
        [HttpPut("EditIndicatorThreshold")]
        public async Task<ActionResult<CamelIndicatorThresholdDTO>> EditThreshold(CamelIndicatorThresholdDTO camelIndicatorThresholdDTO)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var existingThreshold = await _context.IndicatorRatingThresholds
                .FirstOrDefaultAsync(t => t.Id == camelIndicatorThresholdDTO.Id);
            if (existingThreshold == null)
            {
                return NotFound("Threshold not found");
            }

            // ensure no duplicate rating level
            var existingThresholdWithSameRatingLevel = await _context.IndicatorRatingThresholds
                .FirstOrDefaultAsync(t => t.IndicatorId == camelIndicatorThresholdDTO.IndicatorId && t.RatingLevel == camelIndicatorThresholdDTO.RatingLevel && t.Id != camelIndicatorThresholdDTO.Id);
            if (existingThresholdWithSameRatingLevel != null)
            {
                return BadRequest("Threshold with the same rating level already exists");
            }

            existingThreshold.RatingLevel = camelIndicatorThresholdDTO.RatingLevel;
            existingThreshold.ThresholdValue = camelIndicatorThresholdDTO.ThresholdValue;

            _context.IndicatorRatingThresholds.Update(existingThreshold);
            await _context.SaveChangesAsync();

            return Ok(camelIndicatorThresholdDTO);
        }

        // Delete Indicator Threshold
        [HttpDelete("DeleteIndicatorThreshold/{thresholdId}")]
        public async Task<ActionResult> DeleteThreshold(string thresholdId)
        {
            var existingThreshold = await _context.IndicatorRatingThresholds
                .FirstOrDefaultAsync(t => t.Id == thresholdId);
            if (existingThreshold == null)
            {
                return NotFound("Threshold not found");
            }

            _context.IndicatorRatingThresholds.Remove(existingThreshold);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
