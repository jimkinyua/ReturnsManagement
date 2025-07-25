using Returns.Models.Common;

namespace Returns.Models
{
    public enum AdHocReturnRequestStatus
    {
        Pending,
        Responded,
        Completed
    }

    public class AdHocReturnRequest:CommonFields
    {
        public string SaccoId { get; set; } = null!;
        public string RequestedById { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public string Description { get; set; } = null!;
        public string AttachmentUrlsJson { get; set; } = null!;
        public string? ResponseDescription { get; set; }
        public string? ResponseFileUrlsJson { get; set; }
        public string? RespondedById { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string Status { get; set; } = AdHocReturnRequestStatus.Pending.ToString();
    }
}
