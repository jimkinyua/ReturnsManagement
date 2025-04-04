using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Analysis
{
    public class EarningsAnalysisDto
    {
        public decimal NetIncome { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal ReturnOnAssets { get; set; }
        public int Rating { get; set; }
        public string RatingDescription => GetRatingDescription(Rating);

        private string GetRatingDescription(int rating) => rating switch
        {
            1 => "Strong Earnings",
            2 => "Satisfactory Earnings",
            3 => "Fair Earnings",
            4 => "Marginal Earnings",
            5 => "Unsatisfactory Earnings",
            _ => "Unknown"
        };
    }
}
