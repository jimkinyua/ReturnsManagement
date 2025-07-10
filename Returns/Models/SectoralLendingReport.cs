using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class SectoralLendingReport: FormBase
    {
        public string SaccoId { get; set; } = null!;
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;
        [ForeignKey("Returns")]
        public string SaccoName { get; set; } = null!;
        public string Month { get; set; } = null!;
        public string FilePath { get; set; } = "NULL"!;
        public string Year { get; set; } = null!;
        public int DaysLateBy { get; set; }
        public int Version { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<EconomicSectorData> SubSectorData { get; set; } = new List<EconomicSectorData>();
        public virtual ReturnSubmission ReturnSubmission { get; set; } = null!;

    }
}
