namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTSubmittedReturnDTO
    {
        public string? Id { get; set; }
        public DateTime ReturnsFor { get; set; }
        public string? SaccoId { get; set; }
        public string? SaccoName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int DaysLateBy { get; set; }
        public string LateNessStatus { get; set; } = null!;
    }
}
