using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Analysis
{
    public class ReturnsAnalysisDto
    {
        public DateTime AnalysisDate { get; set; }
        public long ReturnId { get; set; }

        // Capital (C)
        public CapitalAnalysisDto Capital { get; set; }

        // Asset Quality (A)
        public AssetQualityAnalysisDto AssetQuality { get; set; }

        // Management (M)
        public ManagementAnalysisDto Management { get; set; }

        // Earnings (E)
        public EarningsAnalysisDto Earnings { get; set; }

        // Liquidity (L)
        public LiquidityAnalysisDto Liquidity { get; set; }

        // Final Results
        public int OverallRating { get; set; }
        public string RiskLevel { get; set; }
        public string ActionRequired { get; set; }
    }
}
