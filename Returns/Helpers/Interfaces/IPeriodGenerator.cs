using Returns.DTOs.PeriodManagement;

namespace Returns.Helpers.Interfaces
{
    public interface IPeriodGenerator
    {
        /// <summary>
        /// Generates a preview of periods that would be created for the given year and frequencies
        /// </summary>
        Task<PeriodGenerationPreviewDto> PreviewPeriodsAsync(int yearId, List<int> frequencyIds);

        /// <summary>
        /// Generates and saves periods for the given year and frequencies
        /// </summary>
        Task<PeriodGenerationResultDto> GeneratePeriodsAsync(int yearId, List<int> frequencyIds, string createdBy);
    }
}