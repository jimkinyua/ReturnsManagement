using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASRAXRBSS.Dto.Returns_Analysis
{
    public class LiquidityRatingDetails
    {
        // Working Net Liquid Ratio (Min 15%)
        private decimal _wnliqValue;
        public decimal WNLIQValue
        {
            get { return _wnliqValue; }
            set { _wnliqValue = value * 100; }
        }
        public int WNLIQRating { get; set; }
        public decimal WNLIQWeight { get; set; }
        public decimal WNLIQWeightedScore { get; set; }

        // Technical Net Liquid Ratio
        private decimal _tnliqValue;
        public decimal TNLIQValue
        {
            get { return _tnliqValue; }
            set { _tnliqValue = value * 100; }
        }
        public int TNLIQRating { get; set; }
        public decimal TNLIQWeight { get; set; }
        public decimal TNLIQWeightedScore { get; set; }

        // External Borrowing Ratio (Max 25%)
        private decimal _ebValue;
        public decimal EBValue
        {
            get { return _ebValue; }
            set { _ebValue = value * 100; }
        }
        public int EBRating { get; set; }
        public decimal EBWeight { get; set; }
        public decimal EBWeightedScore { get; set; }

        // Liquid Assets to Total Assets Ratio
        private decimal _liqtoTAValue;
        public decimal LIQtoTAValue
        {
            get { return _liqtoTAValue; }
            set { _liqtoTAValue = value * 100; }
        }
        public int LIQtoTARating { get; set; }
        public decimal LIQtoTAWeight { get; set; }
        public decimal LIQtoTAWeightedScore { get; set; }

        public int FinalRating { get; set; }
        public string Period { get; set; }

    }
}
