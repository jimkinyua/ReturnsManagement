using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class SubmittedReturnDTO
    {
        public string Id { get; set; }
        public string ReturnsFor { get; set; }
        public string SaccoId { get; set; }
        public string SaccoName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int DaysLateBy { get; set; }
        public string LateNessStatus { get; set; } = null!;
        public Boolean IsConsistent { get; set; } = false;
        public string? ConsistentErrorMessage { get; set; }
        public int TotalReturns { get; set; }
        public int TotalLateReturns { get; set; }
        public int VersionNumber { get; set; }
        public List<string> PreviousVersionIds { get; set; } = new List<string>();

    }
}
