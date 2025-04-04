using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Analysis
{
    public class ManagementAnalysisDto
    {
        public decimal GovernanceScore { get; set; }
        public decimal ControlsScore { get; set; }
        public decimal ComplianceScore { get; set; }
        public int Rating { get; set; }
        public string RatingDescription => GetRatingDescription(Rating);

        private string GetRatingDescription(int rating) => rating switch
        {
            1 => "Strong Management",
            2 => "Satisfactory Management",
            3 => "Fair Management",
            4 => "Marginal Management",
            5 => "Unsatisfactory Management",
            _ => "Unknown"
        };
    }
}
