using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class NWDTDepositReturn:FormBase
    {
        public string RangeName { get; set; } = null!;
        public string DepositType { get; set; } = null!;
        public int NumberOfAccounts { get; set; }
        public decimal AmountInKshs000 { get; set; }
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy { get; set; }

        public Guid ReturnSubmissionId { get; set; }           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
