using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ResponseAttachement: CommonFields
    {
        public string FileUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;
        [ForeignKey("AdditionalInfoReponse")]
        public string ResponseId { get; set; } = null!;
        public virtual AdditionalInfoResponse AdditionalInfoReponse { get; set; } = null!;

    }
}
