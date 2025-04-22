namespace Returns.DTOs.AdditionalInfo
{
    public class AdditionalInfoResponseDto
    {
        public string Id { get; set; } = null!;
        public string ResponseMessage { get; set; } = null!;
        public string RespondedBy { get; set; } = null!;
        public DateTime RespondedAt { get; set; }
        public List<AdditionalInfoAttachmentDto> Attachments { get; set; } = new List<AdditionalInfoAttachmentDto>();
    }
}
