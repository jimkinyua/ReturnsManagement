using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASRAXRBSS.Dto.Returns_Analysis
{
    public class AssetQualityRatingDetails
    {
        // NPL30 Details
        private decimal _npl30Value;
        public decimal NPL30Value
        {
            get { return _npl30Value; }
            set { _npl30Value = value * 100; }
        }          // The actual NPL30 ratio
        public int NPL30Rating { get; set; }             // Rating (1-5) for NPL30
        public decimal NPL30Weight { get; set; }         // Weight (0.70)
        public decimal NPL30WeightedScore { get; set; }  // Rating * Weight

        // Adjusted/Lagged NPL30 Details
        private decimal _adjustedNPL30Value;
        public decimal AdjustedNPL30Value
        {
            get { return _adjustedNPL30Value; }
            set { _adjustedNPL30Value = value * 100; }
        }          // The actual adjusted ratio
        public int AdjustedNPL30Rating { get; set; }             // Rating (1-5) for adjusted
        public decimal AdjustedNPL30Weight { get; set; }         // Weight (0.30)
        public decimal AdjustedNPL30WeightedScore { get; set; }  // Rating * Weight

        public int FinalRating { get; set; }             // Final weighted rating
        public string Period { get; set; }

        public AssetQualityRatingDetails()
        {
            NPL30Weight = 0.70m;
            AdjustedNPL30Weight = 0.30m;
        }
    }
}
