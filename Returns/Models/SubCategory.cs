using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class SubCategory: CommonFields
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        [ForeignKey("Category")]
        public string CategoryId { get; set; } = null!; 
        public virtual Category Category { get; set; } = null!;
        public ICollection<EconomicSector> EconomicSectors { get; set; } = new List<EconomicSector>();

    }
}
