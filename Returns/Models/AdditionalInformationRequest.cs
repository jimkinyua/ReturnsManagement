    using Returns.Models.Common;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Returns.Models
    {
        public class AdditionalInformationRequest: CommonFields
        {
            public string Status = "No Response";
            public string Description { get; set; } = null!;
            public string RequestedBy { get; set; } = null!;

            [ForeignKey("Returns")]
            public string ReturnId { get; set; } = null!;
            public bool IsResponded { get; set; } = false;
            public DateTime? RespondedAt { get; set; }
            public Return Return { get; set; } = null!;
            public ICollection<AdditionalInfoReponse> ReturnReponses { get; set; } = new List<AdditionalInfoReponse>();
        }
    }
