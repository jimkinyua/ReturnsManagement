namespace Returns.DTOs.Returns.Adhoc
{
    public class PendingAdHocReturnRequestDTO
    {
        public string Id { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string RequestedById { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public string Description { get; set; } = null!;
        public List<string> AttachmentUrls { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
