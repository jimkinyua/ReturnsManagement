using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class EconomicSector: CommonFields
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;

        [ForeignKey("SubCategory")]
        public string SubCategoryId { get; set; } = null!;
        public virtual SubCategory SubCategory { get; set; } = null!;
        public ICollection<EconomicSectorData> SubSectorData { get; set; } = new List<EconomicSectorData>();

    }
}
