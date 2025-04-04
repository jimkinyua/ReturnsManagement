using Returns.DTOs.Returns_Submission.DT;

namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTNewReturnDTO
    {
        public string? SaccoId { get; set; }
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public ICollection<ReturnFormUploadDTO> FormUploads { get; set; } = new List<ReturnFormUploadDTO>();

    }
}
