using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class InsiderLendingHeader: FormBase
    {
        //public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string CSNO { get; set; } = null!;
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public int DaysLateBy { get; set; } = 0;
        public int Version { get; set; } = 1;
        public string FilePath { get; set; } = string.Empty;
        public ICollection<InsiderLoan> InsiderLoans { get; set; } = new List<InsiderLoan>();
        [ForeignKey("Return")]
        public string ReturnId { get; set; } = null!;
        public Return Return { get; set; } = null!;

    }
}
