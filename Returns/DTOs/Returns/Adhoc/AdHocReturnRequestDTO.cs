namespace Returns.DTOs.Returns.Adhoc
{
    public class AdHocReturnRequestDTO
    {
        public string SaccoId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<IFormFile>? AttachmentFiles { get; set; }
    }
}
