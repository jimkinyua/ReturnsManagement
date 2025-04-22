using Returns.DTOs.AdditionalInfo;
using Returns.Models.Data;

namespace Returns.Helpers.Interfaces
{
    public interface IAdditionalInformationRequestService
    {

        Task<AdditionalInformationRequestDto> RequestAdditionalInformationAsync(CreateAdditionalInformationRequestDto createAdditionalInformationRequestDto , string RequestedBy);
        Task<AdditionalInfoResponseDto> AddResponseAsync(CreateAdditionalInfoResponseDto dto, string RespondedBy);
    }
}
