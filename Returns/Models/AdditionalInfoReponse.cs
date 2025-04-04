using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class AdditionalInfoReponse: CommonFields
    {
        public string CreatedBy { get; set; } = null!;
        public string ReponseMessage { get; set; } = null!;

        [ForeignKey("RequstForAdditionalInfo")]
        public string RequestForInfoId { get; set; } = null!;
        public AdditionalInformationRequest AdditionalInformationRequest { get; set; } = null!;
        public ICollection<ResponseAttachement> ResponseAttachements { get; set; } = new List<ResponseAttachement>();

    }
}
