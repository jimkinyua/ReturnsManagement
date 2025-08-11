using Returns.Helpers.Enums;

namespace Returns.DTOs.AdditionalInfo
{
    public class AdditionalInformationRequestDto
    {
        public string Id { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string RequestedBy { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public bool IsResponded { get; set; }
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string ReturnSubmissionId { get; set; } = null!;
        public string? ReturnType { get; set; }
        public List<AdditionalInfoResponseDto> Responses { get; set; } = new List<AdditionalInfoResponseDto>();
    }
}
