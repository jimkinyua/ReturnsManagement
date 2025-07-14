using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Rating_Defination;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class RatingDefinitionService : IRatingDefinitionService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<RatingDefinitionService> _logger;
        public RatingDefinitionService(ReturnsDbContext context, ILogger<RatingDefinitionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RatingDefination> CreateAsync(RatingDefinitionCreateDTO dto)
        {
            // Validate form codes
            var validFormCodes = await _context.ReturnForms
                .Where(rf => rf.SaccoTypeId == dto.SaccoType && rf.IsActive)
                .Select(rf => rf.Code)
                .ToListAsync();

            var invalidFormCodes = dto.FormCodes.Except(validFormCodes).ToList();
            if (invalidFormCodes.Any())
            {
                _logger.LogError("Invalid form codes provided: {InvalidCodes}", string.Join(", ", invalidFormCodes));
                throw new ArgumentException($"Invalid form codes: {string.Join(", ", invalidFormCodes)}");
            }

            var definition = new RatingDefination
            {
                RatingName = dto.RatingName,
                Description = dto.Description,
                SaccoType = dto.SaccoType,
                CreatedAt = DateTime.Now,
                RatingForms = dto.FormCodes.Select(fc => new RatingForm
                {
                    FormCode = fc,
                    CreatedAt = DateTime.Now,
                }).ToList()
            };

            await _context.RatingDefinations.AddAsync(definition);
            await _context.SaveChangesAsync();
            return definition;

        }

        public async Task<bool> DeleteAsync(string id)
        {
            var definition = await _context.RatingDefinations
                            .FirstOrDefaultAsync(rd => rd.Id == id);
            if (definition == null)
            {
                _logger.LogWarning("RatingDefinition with ID {Id} not found.", id);
                return false;
            }
            _context.RatingDefinations.Remove(definition);
            return true;
        }

        public async Task<List<RatingDefination>> GetAllAsync(string saccoType)
        {
            return await _context.RatingDefinations
                            .Include(rd => rd.RatingForms)
                            .Where(rd => rd.SaccoType == saccoType)
                            .ToListAsync();
        }

        public async Task<List<ReturnForm>> GetAvailableFormsAsync(string saccoType)
        {
            return await _context.ReturnForms
                            .Where(rf => rf.SaccoTypeId == saccoType && rf.IsActive)
                            .ToListAsync();
        }

        public async Task<RatingDefination> GetByIdAsync(string id)
        {
            var definition = await _context.RatingDefinations
                            .Include(rd => rd.RatingForms)
                            .FirstOrDefaultAsync(rd => rd.Id == id);

            if (definition == null)
            {
                _logger.LogWarning("RatingDefinition with ID {Id} not found.", id);
                return null;
            }
            return definition;
        }

        public async Task<RatingDefination> UpdateAsync(string id, RatingDefinitionUpdateDTO dto)
        {
            var definition = await _context.RatingDefinations
                            .Include(rd => rd.RatingForms)
                            .FirstOrDefaultAsync(rd => rd.Id == id);

            if (definition == null)
            {
                _logger.LogWarning("RatingDefinition with ID {Id} not found.", id);
                return null;
            }

            // Validate form codes
            var validFormCodes = await _context.ReturnForms
                .Where(rf => rf.SaccoTypeId == definition.SaccoType && rf.IsActive)
                .Select(rf => rf.Code)
                .ToListAsync();

            var invalidFormCodes = dto.FormCodes.Except(validFormCodes).ToList();
            if (invalidFormCodes.Any())
            {
                _logger.LogError("Invalid form codes provided: {InvalidCodes}", string.Join(", ", invalidFormCodes));
                throw new ArgumentException($"Invalid form codes: {string.Join(", ", invalidFormCodes)}");
            }

            definition.RatingName = dto.RatingName;
            definition.Description = dto.Description;

            // Update RatingForms (remove old, add new)
            _context.RatingForms.RemoveRange(definition.RatingForms);
            definition.RatingForms = dto.FormCodes.Select(fc => new RatingForm
            {
                FormCode = fc,
                RatingDefinationId = definition.Id,
                CreatedAt = DateTime.Now,
            }).ToList();

            await _context.SaveChangesAsync();
            return definition;
        }
    }
}
