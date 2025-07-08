using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DTCapitalAdequacyReturn : CommonFields
    {

        // CAPITAL COMPONENTS
        public decimal ShareCapital { get; set; }
        public decimal StatutoryReserves { get; set; }
        public decimal RetainedEarningsAccumulatedLosses { get; set; }
        public decimal NetSurplusAfterTaxCurrentYearToDate { get; set; }
        public decimal CapitalGrantsEquityInNature { get; set; }
        public decimal GeneralReserves { get; set; }
        public decimal OtherReserves { get; set; }
        public decimal SubTotalCoreCapital { get; set; }

        // DEDUCTIONS
        public decimal InvestmentsInSubsidiaryAndEquityInstruments { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal CoreCapital { get; set; }
        public decimal InstitutionalCapital { get; set; }

        //  ON-BALANCE SHEET ASSETS
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

        //  OFF-BALANCE SHEET ASSETS
        public decimal TotalOffBalanceSheetAssets { get; set; }

        // CAPITAL RATIO CALCULATIONS
        public decimal TotalAssets { get; set; }
        public decimal TotalDepositsLiabilities { get; set; }
        public decimal CoreCapitalToAssetsRatio { get; set; }
        public decimal MinimumCoreCapitalToAssetsRatio { get; set; } = 0.10M; // 10%
        public decimal CoreCapitalToAssetsRatioExcessDeficiency { get; set; }
        public decimal InstitutionalCapitalToAssetsRatio { get; set; }
        public decimal MinimumInstitutionalToAssetsRatio { get; set; } = 0.08M; // 8%
        public decimal InstitutionalCapitalToAssetsRatioExcessDeficiency { get; set; }
        public decimal CoreCapitalToDepositsRatio { get; set; }
        public decimal MinimumCoreCapitalToDepositsRatio { get; set; } = 0.08M; // 8%
        public decimal CoreCapitalToDepositsRatioExcessDeficiency { get; set; }

        // related entities
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string Year { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; } = null!;
        public int DaysLateBy { get; set; }

        public bool IsDraft { get; set; } = false;
        public bool IsCurrent { get; set; } = true;
        public bool IsAmended { get; set; } = false;
        public string? PreviousReturnId { get; set; }

    }
}
