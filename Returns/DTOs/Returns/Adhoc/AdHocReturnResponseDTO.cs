namespace Returns.DTOs.Returns.Adhoc
{
    public class AdHocReturnResponseDTO
    {
        public string RequestId { get; set; } = null!;
        public string ResponseDescription { get; set; } = null!;
        public List<IFormFile> ResponseFiles { get; set; } = null!;
    }
}
