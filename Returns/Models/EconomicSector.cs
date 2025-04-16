using Returns.Models.Common;

namespace Returns.Models
{
    public class EconomicSector:CommonFields
    {
        public string SectorName { get; set; } = null!;
        public string SectorCode { get; set; } = null!;
        public ICollection<EconomicSubSector> EconomicSubSectors { get; set; } = new List<EconomicSubSector>();
    }
}
