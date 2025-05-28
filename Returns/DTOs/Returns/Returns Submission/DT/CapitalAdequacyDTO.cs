using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class CapitalAdequacyDTO: CommonFormDTO
    {
        public decimal ShareCapital { get; set; }
        public decimal StatutoryReserves { get; set; }
        public decimal RetainedEarningsAccumulatedLosses { get; set; }
        public decimal NetSurplusAfterTaxCurrentYearToDate { get; set; }
        public decimal CapitalGrantsEquityInNature { get; set; }
        public decimal GeneralReserves { get; set; }
        public decimal OtherReserves { get; set; }
        public decimal SubTotalCoreCapital { get; set; }

        // Deductions
        public decimal InvestmentsInSubsidiaryAndEquityInstruments { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal CoreCapital { get; set; }
        public decimal InstitutionalCapital { get; set; }

        // Balance Sheet Assets
        public decimal CashLocalAndForeignCurrency { get; set; }
        public decimal GovernmentSecurities { get; set; }
        public decimal DepositsAndBalancesAtOtherInstitutions { get; set; }
        public decimal LoansAndAdvances { get; set; }
        public decimal Investments { get; set; }
        public decimal PropertyAndEquipment { get; set; }
        public decimal OtherAssets { get; set; }
        public decimal TotalOnBalanceSheetAssets { get; set; }
        public decimal TotalAssetsPerBalanceSheet { get; set; }
        public decimal Difference { get; set; }

        // Ratios
        public decimal CoreCapitalToAssetsRatio { get; set; }
        public decimal CoreCapitalToAssetsRatioExcessDeficiency { get; set; }
        public decimal InstitutionalCapitalToAssetsRatio { get; set; }
        public decimal InstitutionalCapitalToAssetsRatioExcessDeficiency { get; set; }
        public decimal CoreCapitalToDepositsRatio { get; set; }
        public decimal CoreCapitalToDepositsRatioExcessDeficiency { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
