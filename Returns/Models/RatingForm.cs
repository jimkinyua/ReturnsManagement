using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class RatingForm:CommonFields
    {
        public string FormCode { get; set; } = null!;
        [ForeignKey("ReturnSubmission")]
        public string RatingDefinationId { get; set; } = null!;
        public RatingDefination Calculation { get; set; } = null!;
    }
}
