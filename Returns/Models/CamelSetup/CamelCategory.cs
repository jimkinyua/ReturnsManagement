using Returns.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Returns.Models.CamelSetup
{
    public class CamelCategory : CommonFields
    {
       
        [MaxLength(1)]
        public string Code { get; set; } = null!;
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public virtual ICollection<CamelIndicator> Indicators { get; set; } = null!;
    }
}
