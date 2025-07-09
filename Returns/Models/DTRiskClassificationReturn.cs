using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DTRiskClassificationReturn : FormBase
    {
        public string LoanType { get; set; } // "Regular" or "Rescheduled/Renegotiated"
        public string Classification { get; set; } // "Performing", "Watch", etc.
        public int? NumberOfAccounts { get; set; }
        public decimal? OutstandingLoanPortfolio { get; set; }
        public decimal? RequiredProvision { get; set; }
        public decimal? RequiredProvisionAmount { get; set; }
        public string Year { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
       /* public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy { get; set; }*/
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
