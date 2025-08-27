using Returns.Models;

namespace Returns.DTOs.Returns.Adhoc
{
    public class AdHocReturnRequestDetailsDTO
    {
        public string Id { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string RequestedById { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public string Description { get; set; } = null!;
        public List<string> AttachmentUrls { get; set; } = null!;
        public string? ResponseDescription { get; set; }
        public List<string>? ResponseFileUrls { get; set; }
        public string? RespondedById { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string Status { get; set; } = null!;
    }
}
