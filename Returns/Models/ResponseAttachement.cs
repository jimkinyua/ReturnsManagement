using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ResponseAttachement: CommonFields
    {
        public string FileUrl { get; set; } = null!;
        [ForeignKey("AdditionalInfoReponse")]
        public string AdditionalInfoReponseId { get; set; } = null!;
        public AdditionalInfoReponse AdditionalInfoReponse { get; set; } = null!;

    }
}
