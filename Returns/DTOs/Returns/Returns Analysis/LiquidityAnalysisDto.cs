using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Analysis
{
    public class LiquidityAnalysisDto
    {
        public decimal LiquidAssets { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal LiquidityRatio { get; set; }
        public int Rating { get; set; }
        public string RatingDescription => GetRatingDescription(Rating);

        private string GetRatingDescription(int rating) => rating switch
        {
            1 => "Strong Liquidity",
            2 => "Satisfactory Liquidity",
            3 => "Fair Liquidity",
            4 => "Marginal Liquidity",
            5 => "Unsatisfactory Liquidity",
            _ => "Unknown"
        };
    }
}
