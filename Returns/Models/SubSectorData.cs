using DocumentFormat.OpenXml.Bibliography;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class SubSectorData: CommonFields
    {
        public string SubSectorName { get; set; } = null!;
        public string SubSectorCode { get; set; } = null!;
        [ForeignKey("EconomicSubSector")]
        public string SubSectorId { get; set; } = null!;
        public decimal Amount { get; set; }
        [ForeignKey("ReportId")]
        public virtual SectoralLendingReport SectoralLendingReport { get; set; } = null!;
        public virtual EconomicSubSector EconomicSubSector { get; set; } = null!;
    }
}
