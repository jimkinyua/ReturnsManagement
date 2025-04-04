using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASRAXRBSS.Dto.Returns_Analysis
{
    public class CapitalAnalysisResult
    {
        public CapitalRatingDetails MinimumCC { get; set; }
        public CapitalRatingDetails CCA { get; set; }
        public CapitalRatingDetails ICA { get; set; }
        public CapitalRatingDetails CCD { get; set; }
        public CapitalRatingDetails AdjustedCCA { get; set; }
        public int FinalRating { get; set; }
        public string Period { get; set; }

        public CapitalAnalysisResult()
        {
            MinimumCC = new CapitalRatingDetails {};     // 1%
            CCA = new CapitalRatingDetails { };           // 20%
            ICA = new CapitalRatingDetails {  };           // 20%
            CCD = new CapitalRatingDetails {  };           // 20%
            AdjustedCCA = new CapitalRatingDetails {  };   // 39%
        }
    }
}
