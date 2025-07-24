namespace Returns.DTOs.Returns.Returns_Submission
{
    public class SaccoAmendmentResponseDTO
    {
        public string ReturnSubmissionId { get; set; } = null!;
        public IFormFile FormFile { get; set; } = null!;
    }
}
