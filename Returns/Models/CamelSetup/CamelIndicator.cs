using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Returns.Models.Common;

namespace Returns.Models.CamelSetup
{
    public class CamelIndicator : CommonFields
    {

        [ForeignKey("CamelCategory")]
        public string CategoryId { get; set; }  = null!;
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        public decimal Weight { get; set; }
        [Required]
        public Boolean BetterHigher { get; set; }
        public virtual CamelCategory CamelCategory { get; set; }
        public virtual ICollection<IndicatorRatingThreshold> RatingThresholds { get; set; } = null!;
    }
}
