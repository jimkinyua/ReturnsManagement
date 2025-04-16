using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class EconomicSubSector: CommonFields
    {
        public string SubSectorName { get; set; } = null!;
        public string SubSectorCode { get; set; } = null!;
        [ForeignKey("EconomicSector")]
        public string SectorId { get; set; } = null!;
        public EconomicSector EconomicSector { get; set; } = null!;
        public ICollection<SubSectorData> SubSectorData { get; set; } = new List<SubSectorData>();
    }
}
