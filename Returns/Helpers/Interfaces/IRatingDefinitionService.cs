using Returns.DTOs.Rating_Defination;
using Returns.Models;

namespace Returns.Helpers.Interfaces
{
    public interface IRatingDefinitionService
    {
        Task<List<RatingDefination>> GetAllAsync(string saccoType);
        Task<RatingDefination> GetByIdAsync(string id);
        Task<RatingDefination> CreateAsync(RatingDefinitionCreateDTO dto);
        Task<RatingDefination> UpdateAsync(string id, RatingDefinitionUpdateDTO dto);
        Task<bool> DeleteAsync(string id);
        Task<List<ReturnForm>> GetAvailableFormsAsync(string saccoType);
    }
}
