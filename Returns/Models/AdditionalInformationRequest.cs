using Returns.Helpers.Enums;
using Returns.Models.Common;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Returns.Models
    {
        public class AdditionalInformationRequest: CommonFields
        {
            public string Description { get; set; } = null!;
            public string RequestedBy { get; set; } = null!;
            public string RequestStatus { get; set; } = AdditionalInfoRequestStatus.NoResponse.ToString();

            [ForeignKey("ReturnSubmissions")]
            public string ReturnSubmissionId { get; set; } = null!;
            public string SaccoId { get; set; } = null!;
            public bool IsResponded { get; set; } = false;
            public DateTime? RespondedAt { get; set; }
            public virtual ReturnSubmission ReturnSubmission { get; set; } = null!;
            public virtual ICollection<AdditionalInfoResponse> ReturnReponses { get; set; } = new List<AdditionalInfoResponse>();
        }
    }
