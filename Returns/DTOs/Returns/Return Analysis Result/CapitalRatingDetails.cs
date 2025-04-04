using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASRAXRBSS.Dto.Returns_Analysis
{
    public class CapitalRatingDetails
    {
        private decimal _actualValue;
        public decimal ActualValue
        {
            get { return _actualValue; }
            set { _actualValue = value * 100; }
        }    // The actual percentage/value
        public int Rating { get; set; }             // The rating (1-5)
        public decimal Weight { get; set; }         // The weight of this component
        public decimal WeightedScore { get; set; }  // Rating * Weight
    }
}
