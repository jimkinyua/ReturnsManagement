namespace Returns.DTOs.Returns.Returns_Submission
{
    public class AmendmentRequestDTO
    {
        public string SubmissionId { get; set; } = null!;
        public string AmendmentReason { get; set; } = null!;
        public IFormFile FormFile { get; set; } = null!;

    }
}
