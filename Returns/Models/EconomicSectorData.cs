using DocumentFormat.OpenXml.Bibliography;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class EconomicSectorData: FormBase
    {
        [ForeignKey("SectoralLendingReport")]
        public string SectoralLendingReportId { get; set; } = null!;
        public string ReturnSubmissionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string EconomicSectorCode { get; set; } = null!;
        //[ForeignKey("EconomicSector")]
        //public string EconomicSectorId { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string SubCategory { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public string EconomicSectorName { get; set; } = null!;
        public virtual SectoralLendingReport SectoralLendingReport { get; set; } = null!;
        //public virtual EconomicSector EconomicSector { get; set; } = null!;
    }
}
