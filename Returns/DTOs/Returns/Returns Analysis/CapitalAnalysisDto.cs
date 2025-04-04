using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Analysis
{
    public class CapitalAnalysisDto
    {
        public decimal CoreCapital { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal CoreCapitalToAssetsRatio { get; set; }
        public decimal InstitutionalCapitalRatio { get; set; }
        public int Rating { get; set; }
        public string RatingDescription => GetRatingDescription(Rating);

        private string GetRatingDescription(int rating) => rating switch
        {
            1 => "Strong Capital",
            2 => "Satisfactory Capital",
            3 => "Fair Capital",
            4 => "Marginal Capital",
            5 => "Unsatisfactory Capital",
            _ => "Unknown"
        };
    }
}
