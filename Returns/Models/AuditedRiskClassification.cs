using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class AuditedRiskClassification:FormBase
    {
        public string LoanType { get; set; } = string.Empty; // "Regular" or "Rescheduled/Renegotiated"
        public string Classification { get; set; }  = string.Empty; // "Performing", "Watch", etc.
        public int? NumberOfAccounts { get; set; }
        public decimal? OutstandingLoanPortfolio { get; set; }
        public decimal? RequiredProvision { get; set; }
        public decimal? RequiredProvisionAmount { get; set; }
        public string Year { get; set; } = null!;

        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
        public string SaccoCsNumber { get; internal set; }
    }
}
