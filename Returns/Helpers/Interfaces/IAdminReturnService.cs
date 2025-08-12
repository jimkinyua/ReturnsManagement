using Returns.DTOs.Returns_Submission;
using Returns.DTOs;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers.Interfaces
{
    public interface IAdminReturnService
    {
        Task<List<AdminGroupedReturnDTO>> GetSaccoGroupedReturnsAsync(SaccoReturnFilterDTO filter, LoggedInEntity loggedInSacco);
        Task<List<AdminGroupedReturnDTO>> GetGroupedReturnsAsync(AdminReturnFilterDTO filter);
        Task<AdminGroupedReturnDetailsDTO> GetGroupedReturnDetailsAsync(string groupId, string periodId, string saccoId);
        Task<AdminGroupedReturnDetailsDTO> GetGroupedReturnDetailsAsync(string groupId, string periodId, string saccoId, int? version);
        Task<List<ReturnVersionDTO>> GetAvailableVersionsAsync(string periodId, string saccoId, string? expectedReturnId = null);
        Task<List<string>> GetAvailableYearsAsync();
        Task<List<string>> GetAvailableSaccoTypesAsync();
        Task<List<string>> GetAvailableFrequenciesAsync();

        // Individual form versioning methods
        Task<List<FormVersionDTO>> GetAvailableFormVersionsAsync(string periodId, string saccoId, string expectedReturnId);
        Task<object?> GetFormDataByVersionAsync(string submissionId, string expectedReturnId, string? saccoType = null);
    }
}