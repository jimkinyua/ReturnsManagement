namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTCapitalAdequacyDTO : CommonFormDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysLateBy { get; set; }

        // Core Capital Components
        public decimal ShareCapital { get; set; }
        public decimal CapitalGrants { get; set; }
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
        public decimal RetainedEarningsAndDisclosedReserves { get; set; }

        // ON-BALANCE SHEET ASSETS
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

        // OFF-BALANCE SHEET ASSETS
        public decimal TotalOffBalanceSheetAssets { get; set; }

        // CAPITAL RATIO CALCULATIONS
        public decimal TotalAssets { get; set; }
        public decimal TotalDepositsLiabilitiesPerBalanceSheet { get; set; }
        public decimal CoreCapitalToAssetsRatio { get; set; }
        public decimal MinimumCoreCapitalToAssetsRatioRequirement { get; set; }
        public decimal MinimumRetainedEarningsAndDisclosedReservesToCoreCapitalRequirement { get; set; }
        public decimal MinimumCoreCapitalToDepositsRatioRequirement { get; set; }
        public decimal CoreCapitalToAssetsRatioExcessDeficiency { get; set; }
        public decimal RetainedEarningsAndDisclosedReservesToCoreCapital { get; set; }
        public decimal RetainedEarningsAndDisclosedReservesToCoreCapitalExcessDeficiency { get; set; }
        public decimal CoreCapitalToDepositsRatio { get; set; }
        public decimal CoreCapitalToDepositsRatioExcessDeficiency { get; set; }

    }
}
