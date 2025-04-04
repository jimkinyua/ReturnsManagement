using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Analysis
{
    public class AssetQualityAnalysisDto
    {
        public decimal NonPerformingLoans { get; set; }
        public decimal TotalLoans { get; set; }
        public decimal NonPerformingLoanRatio { get; set; }
        public int Rating { get; set; }
        public string RatingDescription => GetRatingDescription(Rating);

        private string GetRatingDescription(int rating) => rating switch
        {
            1 => "Strong Asset Quality",
            2 => "Satisfactory Asset Quality",
            3 => "Fair Asset Quality",
            4 => "Marginal Asset Quality",
            5 => "Unsatisfactory Asset Quality",
            _ => "Unknown"
        };
    }
}
