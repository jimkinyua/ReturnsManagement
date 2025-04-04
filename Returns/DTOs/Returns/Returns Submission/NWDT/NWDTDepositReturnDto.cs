using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTDepositReturnDto
    {
        public string? RangeName { get; set; }
        public string? DepositType { get; set; }
        public int NumberOfAccounts { get; set; }
        public decimal Amount { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
