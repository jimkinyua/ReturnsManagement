using Returns.DTOs.Returns_Submission;

namespace Returns.Helpers.Interfaces
{
    public interface IAdminReturnService
    {
        Task<List<AdminGroupedReturnDTO>> GetGroupedReturnsAsync(AdminReturnFilterDTO filter);
        Task<AdminGroupedReturnDetailsDTO> GetGroupedReturnDetailsAsync(string groupId, string periodId, string saccoId);
        Task<List<string>> GetAvailableYearsAsync();
        Task<List<string>> GetAvailableSaccoTypesAsync();
        Task<List<string>> GetAvailableFrequenciesAsync();
    }
}