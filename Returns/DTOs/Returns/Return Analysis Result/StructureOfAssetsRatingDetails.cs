using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASRAXRBSS.Dto.Returns_Analysis
{
    public class StructureOfAssetsRatingDetails
    {
        // LB ratio - Land and Buildings ratio (25% weight)
        private decimal _lbRatioValue;
        public decimal LBRatioValue
        {
            get { return _lbRatioValue; }
            set { _lbRatioValue = value * 100; }
        }
        public int LBRatioRating { get; set; }
        public decimal LBRatioWeight { get; set; } = 0.25m;
        public decimal LBRatioWeightedScore { get; set; }

        // FICC - Private investments to Core Capital ratio (25% weight)
        private decimal _ficcValue;
        public decimal FICCValue
        {
            get { return _ficcValue; }
            set { _ficcValue = value * 100; }
        }
        public int FICCRating { get; set; }
        public decimal FICCWeight { get; set; } = 0.25m;
        public decimal FICCWeightedScore { get; set; }

        // FITD - Private investments to deposits ratio (25% weight)
        private decimal _fitdValue;
        public decimal FITDValue
        {
            get { return _fitdValue; }
            set { _fitdValue = value * 100; }
        }
        public int FITDRating { get; set; }
        public decimal FITDWeight { get; set; } = 0.25m;
        public decimal FITDWeightedScore { get; set; }

        // NEA - Non-Earning Assets ratio (25% weight)
        private decimal _neaValue;
        public decimal NEAValue
        {
            get { return _neaValue; }
            set { _neaValue = value * 100; }
        }
        public int NEARating { get; set; }
        public decimal NEAWeight { get; set; } = 0.25m;
        public decimal NEAWeightedScore { get; set; }

        public int FinalRating { get; set; }
        public string Period { get; set; }

    }
}
