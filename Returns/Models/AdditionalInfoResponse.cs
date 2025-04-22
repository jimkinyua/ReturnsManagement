using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class AdditionalInfoResponse: CommonFields
    {
        public string RespondedBy { get; set; } = null!;
        public string ReponseMessage { get; set; } = null!;

        [ForeignKey("RequstForAdditionalInfo")]
        public string RequestId { get; set; } = null!;
        public virtual AdditionalInformationRequest AdditionalInformationRequest { get; set; } = null!;
        public virtual ICollection<ResponseAttachement> ResponseAttachements { get; set; } = new List<ResponseAttachement>();

    }
}
