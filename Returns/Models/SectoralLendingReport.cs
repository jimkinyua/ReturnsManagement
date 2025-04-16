using Returns.Models.Common;

namespace Returns.Models
{
    public class SectoralLendingReport: CommonFields
    {
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string Month { get; set; } = null!;
        public string Year { get; set; } = null!;
        public ICollection<SubSectorData> SubSectorData { get; set; } = new List<SubSectorData>();

    }
}
