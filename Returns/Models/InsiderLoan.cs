using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class InsiderLoan:FormBase
    {
        
        public string NameOfBorrower { get; set; }=null!;
        public string LoanCategory { get; set; } = null!;
        public string MemberNumber { get; set; }   =null!;    
        public string PositionHeld { get; set; }  =null!;    
        public string LoanTypeName { get; set; }  =null!;    
        public decimal AmountAppliedFor { get; set; }      
        public decimal AmountGranted { get; set; }        
        public DateTime DateApprovedOrRatified { get; set; }       
        public decimal AmountOfBosaDeposits { get; set; }        
        public string NatureOfSecurity { get; set; }=null!;      
        public DateTime RepaymentCommencementDate { get; set; }
        public string RepaymentPeriod { get; set; }=null!;
        public string? OtherRemarks { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public string? PerfomanceCategory { get; set; }
        public string RepaymentStatus { get; set; }
        [ForeignKey("InsiderLendingHeader")]
        public string InsiderLendingHeaderId { get; set; } = null!;
        public string ReturnId { get; set; } = null!;
        public string ReturnSubmissionId { get; set; } = null!;
        public InsiderLendingHeader InsiderLendingHeader { get; set; } = null!;
    }
}
