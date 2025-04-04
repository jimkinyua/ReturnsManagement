using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class RiskClassificationDTO
    {
        public string LoanType { get; set; }
        public string Classification { get; set; }
        public int? NumberOfAccounts { get; set; }
        public decimal? OutstandingLoanPortfolio { get; set; }
        public decimal? RequiredProvision { get; set; }
        public decimal? RequiredProvisionAmount { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
