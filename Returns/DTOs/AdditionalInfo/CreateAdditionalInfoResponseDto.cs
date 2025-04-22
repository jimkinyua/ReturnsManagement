namespace Returns.DTOs.AdditionalInfo
{
    public class CreateAdditionalInfoResponseDto
    {
        public string Id { get; set; } = null!;
        public string ResponseMessage { get; set; } = null!;
        public List<AdditionalAttachment> Attachments { get; set; } = new List<AdditionalAttachment>();
    }
}
