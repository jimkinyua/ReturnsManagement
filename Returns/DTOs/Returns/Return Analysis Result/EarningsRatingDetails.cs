using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASRAXRBSS.Dto.Returns_Analysis
{
    public class EarningsRatingDetails
    {
        // ROA Details
        private decimal _roaValue;
        public decimal ROAValue
        {
            get { return _roaValue; }
            set { _roaValue = value * 100; }
        }            // Actual ROA percentage
        public int ROARating { get; set; }               // Rating for ROA
        public decimal ROAWeight { get; set; }           // Weight for ROA component

        // Cost to Income Ratio Details
        private decimal _costToIncomeValue;
        public decimal CostToIncomeValue
        {
            get { return _costToIncomeValue; }
            set { _costToIncomeValue = value * 100; }
        }   // Actual C/I ratio
        public int CostToIncomeRating { get; set; }      // Rating for C/I
        public decimal CostToIncomeWeight { get; set; }  // Weight for C/I component

        // Operating Efficiency Details
        private decimal _oeValue;
        public decimal OEValue
        {
            get { return _oeValue; }
            set { _oeValue = value * 100; }
        }             // Actual OE ratio
        public int OERating { get; set; }                // Rating for OE
        public decimal OEWeight { get; set; }            // Weight for OE component

        public int FinalRating { get; set; }             // Final weighted rating
        public string Period { get; set; }

    }
}
