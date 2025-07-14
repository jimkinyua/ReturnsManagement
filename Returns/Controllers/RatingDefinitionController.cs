using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Returns.DTOs.Rating_Defination;
using Returns.Helpers.Interfaces;

namespace Returns.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingDefinitionController : ControllerBase
    {
        private readonly IRatingDefinitionService _ratingDefinitionService;
        private readonly ILogger<RatingDefinitionController> _logger;

        public RatingDefinitionController(IRatingDefinitionService ratingDefinitionService, ILogger<RatingDefinitionController> logger)
        {
            _ratingDefinitionService = ratingDefinitionService;
            _logger = logger;
        }

        [HttpGet("{saccoType}")]
        public async Task<IActionResult> GetAllAsync(string saccoType)
        {
  
            var definitions = await _ratingDefinitionService.GetAllAsync(saccoType);
            return Ok(definitions);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var definition = await _ratingDefinitionService.GetByIdAsync(id);
            if (definition == null)
            {
                return NotFound($"RatingDefinition with ID {id} not found.");
            }
            return Ok(definition);
        }

        [HttpPost("CreateRatingForm")]
        public async Task<IActionResult> CreateAsync([FromBody] RatingDefinitionCreateDTO dto)
        {
        
            try
            {
                var definition = await _ratingDefinitionService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = definition.Id }, definition);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RatingDefinition.");
                return StatusCode(500, "An error occurred while creating the rating definition.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] RatingDefinitionUpdateDTO dto)
        {

            try
            {
                var definition = await _ratingDefinitionService.UpdateAsync(id, dto);
                if (definition == null)
                {
                    return NotFound($"RatingDefinition with ID {id} not found.");
                }
                return Ok(definition);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RatingDefinition with ID {Id}.", id);
                return StatusCode(500, "An error occurred while updating the rating definition.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {

            var success = await _ratingDefinitionService.DeleteAsync(id);
            if (!success)
            {
                return NotFound($"RatingDefinition with ID {id} not found.");
            }
            return NoContent();
        }

        [HttpGet("available-forms/{saccoType}")]
        public async Task<IActionResult> GetAvailableFormsAsync(string saccoType)
        {
            var forms = await _ratingDefinitionService.GetAvailableFormsAsync(saccoType);
            return Ok(forms);
        }

    }
}
