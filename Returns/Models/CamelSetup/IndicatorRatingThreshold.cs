using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Returns.Models.Common;

namespace Returns.Models.CamelSetup
{
    public class IndicatorRatingThreshold: CommonFields
    {
        [ForeignKey("CamelIndicator")]
        public string IndicatorId { get; set; } = null!;
        [Required]
        public int RatingLevel { get; set; }
        [Required]
        public decimal ThresholdValue { get; set; }
        public virtual CamelIndicator CamelIndicator { get; set; }
    }
}
