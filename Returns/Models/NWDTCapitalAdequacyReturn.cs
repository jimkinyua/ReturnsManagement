using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class NWDTCapitalAdequacyReturn : FormBase
    {
        // Metadata
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public int DaysLateBy { get; set; }


        // Core Capital Components

        public decimal ShareCapital { get; set; }
        public decimal CapitalGrants { get; set; }
        public decimal RetainedEarnings { get; set; }
        public decimal NetSurplusAfterTax { get; set; }
        public decimal StatutoryReserves { get; set; }
        public decimal OtherReserves { get; set; }

        // Deductions

        public decimal InvestmentsInSubsidiary { get; set; }


        public decimal OtherDeductions { get; set; }

        // On-Balance Sheet Assets

        public decimal CashLocalForeign { get; set; }


        public decimal GovernmentSecurities { get; set; }


        public decimal DepositsBalancesAtOtherInstitutions { get; set; }


        public decimal LoansAndAdvances { get; set; }


        public decimal Investments { get; set; }


        public decimal PropertyAndEquipment { get; set; }


        public decimal OtherAssets { get; set; }


        public decimal TotalAssetsPerBalanceSheet { get; set; }

        // Off-Balance Sheet Assets

        public decimal OffBalanceSheetAssets { get; set; }

        // Minimum Requirements

        public decimal MinimumCoreCapitalToAssetsRatio { get; set; } = 8; // Default 8%


        public decimal MinimumRetainedEarningsToCoreCaptialRequirement { get; set; } = 50; // Default 50%


        public decimal MinimumCoreCapitalToDepositsRequirement { get; set; } = 5; // Default 5%


        public decimal TotalDepositsLiabilities { get; set; }

        // Private backing fields for calculated values
        private decimal _subTotalCoreCapital;
        private decimal _totalDeductions;
        private decimal _coreCapital;
        private decimal _retainedEarningsAndDisclosedReserves;
        private decimal _totalOnBalanceSheetAssets;
        private decimal _differenceInAssets;
        private decimal _totalAssets;
        private decimal _coreCapitalToAssetsRatio;
        private decimal _coreCapitalToAssetsExcessDeficiency;
        private decimal _retainedEarningsToCoreCaptialRatio;
        private decimal _retainedEarningsToCoreCaptialExcessDeficiency;
        private decimal _coreCapitalToDepositsRatio;
        private decimal _coreCapitalToDepositsExcessDeficiency;

        // Calculated properties with backing fields for storage
        [NotMapped]
        public decimal SubTotalCoreCapital
        {
            get => ShareCapital + CapitalGrants + RetainedEarnings +
                   NetSurplusAfterTax + StatutoryReserves + OtherReserves;
            set => _subTotalCoreCapital = value;
        }

        [NotMapped]
        public decimal TotalDeductions
        {
            get => InvestmentsInSubsidiary + OtherDeductions;
            set => _totalDeductions = value;
        }

        [NotMapped]
        public decimal CoreCapital
        {
            get => SubTotalCoreCapital - TotalDeductions;
            set => _coreCapital = value;
        }

        [NotMapped]
        public decimal RetainedEarningsAndDisclosedReserves
        {
            get => CapitalGrants + RetainedEarnings + NetSurplusAfterTax +
                   StatutoryReserves + OtherReserves;
            set => _retainedEarningsAndDisclosedReserves = value;
        }

        [NotMapped]
        public decimal TotalOnBalanceSheetAssets
        {
            get => CashLocalForeign + GovernmentSecurities +
                   DepositsBalancesAtOtherInstitutions + LoansAndAdvances +
                   Investments + PropertyAndEquipment + OtherAssets;
            set => _totalOnBalanceSheetAssets = value;
        }

        [NotMapped]
        public decimal DifferenceInAssets
        {
            get => TotalOnBalanceSheetAssets - TotalAssetsPerBalanceSheet;
            set => _differenceInAssets = value;
        }

        [NotMapped]
        public decimal TotalAssets
        {
            get => TotalAssetsPerBalanceSheet + OffBalanceSheetAssets;
            set => _totalAssets = value;
        }

        [NotMapped]
        public decimal CoreCapitalToAssetsRatio
        {
            get => TotalAssets == 0 ? 0 : (CoreCapital / TotalAssets) * 100;
            set => _coreCapitalToAssetsRatio = value;
        }

        [NotMapped]
        public decimal CoreCapitalToAssetsExcessDeficiency
        {
            get => CoreCapitalToAssetsRatio - MinimumCoreCapitalToAssetsRatio;
            set => _coreCapitalToAssetsExcessDeficiency = value;
        }

        [NotMapped]
        public decimal RetainedEarningsToCoreCaptialRatio
        {
            get => CoreCapital == 0 ? 0 : (RetainedEarningsAndDisclosedReserves / CoreCapital) * 100;
            set => _retainedEarningsToCoreCaptialRatio = value;
        }

        [NotMapped]
        public decimal RetainedEarningsToCoreCaptialExcessDeficiency
        {
            get => RetainedEarningsToCoreCaptialRatio - MinimumRetainedEarningsToCoreCaptialRequirement;
            set => _retainedEarningsToCoreCaptialExcessDeficiency = value;
        }

        [NotMapped]
        public decimal CoreCapitalToDepositsRatio
        {
            get => TotalDepositsLiabilities == 0 ? 0 : (CoreCapital / TotalDepositsLiabilities) * 100;
            set => _coreCapitalToDepositsRatio = value;
        }

        [NotMapped]
        public decimal CoreCapitalToDepositsExcessDeficiency
        {
            get => CoreCapitalToDepositsRatio - MinimumCoreCapitalToDepositsRequirement;
            set => _coreCapitalToDepositsExcessDeficiency = value;
        }

        // Database columns for calculated values
        [Column("SubTotalCoreCapital", TypeName = "decimal(18,2)")]
        public decimal StoredSubTotalCoreCapital
        {
            get => _subTotalCoreCapital == 0 ? SubTotalCoreCapital : _subTotalCoreCapital;
            set => _subTotalCoreCapital = value;
        }

        [Column("TotalDeductions", TypeName = "decimal(18,2)")]
        public decimal StoredTotalDeductions
        {
            get => _totalDeductions == 0 ? TotalDeductions : _totalDeductions;
            set => _totalDeductions = value;
        }

        [Column("CoreCapital", TypeName = "decimal(18,2)")]
        public decimal StoredCoreCapital
        {
            get => _coreCapital == 0 ? CoreCapital : _coreCapital;
            set => _coreCapital = value;
        }

        [Column("RetainedEarningsAndDisclosedReserves", TypeName = "decimal(18,2)")]
        public decimal StoredRetainedEarningsAndDisclosedReserves
        {
            get => _retainedEarningsAndDisclosedReserves == 0 ? RetainedEarningsAndDisclosedReserves : _retainedEarningsAndDisclosedReserves;
            set => _retainedEarningsAndDisclosedReserves = value;
        }

        [Column("TotalOnBalanceSheetAssets", TypeName = "decimal(18,2)")]
        public decimal StoredTotalOnBalanceSheetAssets
        {
            get => _totalOnBalanceSheetAssets == 0 ? TotalOnBalanceSheetAssets : _totalOnBalanceSheetAssets;
            set => _totalOnBalanceSheetAssets = value;
        }

        [Column("DifferenceInAssets", TypeName = "decimal(18,2)")]
        public decimal StoredDifferenceInAssets
        {
            get => _differenceInAssets == 0 ? DifferenceInAssets : _differenceInAssets;
            set => _differenceInAssets = value;
        }

        [Column("TotalAssets", TypeName = "decimal(18,2)")]
        public decimal StoredTotalAssets
        {
            get => _totalAssets == 0 ? TotalAssets : _totalAssets;
            set => _totalAssets = value;
        }

        [Column("CoreCapitalToAssetsRatio", TypeName = "decimal(18,2)")]
        public decimal StoredCoreCapitalToAssetsRatio
        {
            get => _coreCapitalToAssetsRatio == 0 ? CoreCapitalToAssetsRatio : _coreCapitalToAssetsRatio;
            set => _coreCapitalToAssetsRatio = value;
        }

        [Column("CoreCapitalToAssetsExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredCoreCapitalToAssetsExcessDeficiency
        {
            get => _coreCapitalToAssetsExcessDeficiency == 0 ? CoreCapitalToAssetsExcessDeficiency : _coreCapitalToAssetsExcessDeficiency;
            set => _coreCapitalToAssetsExcessDeficiency = value;
        }

        [Column("RetainedEarningsToCoreCaptialRatio", TypeName = "decimal(18,2)")]
        public decimal StoredRetainedEarningsToCoreCaptialRatio
        {
            get => _retainedEarningsToCoreCaptialRatio == 0 ? RetainedEarningsToCoreCaptialRatio : _retainedEarningsToCoreCaptialRatio;
            set => _retainedEarningsToCoreCaptialRatio = value;
        }

        [Column("RetainedEarningsToCoreCaptialExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredRetainedEarningsToCoreCaptialExcessDeficiency
        {
            get => _retainedEarningsToCoreCaptialExcessDeficiency == 0 ? RetainedEarningsToCoreCaptialExcessDeficiency : _retainedEarningsToCoreCaptialExcessDeficiency;
            set => _retainedEarningsToCoreCaptialExcessDeficiency = value;
        }

        [Column("CoreCapitalToDepositsRatio", TypeName = "decimal(18,2)")]
        public decimal StoredCoreCapitalToDepositsRatio
        {
            get => _coreCapitalToDepositsRatio == 0 ? CoreCapitalToDepositsRatio : _coreCapitalToDepositsRatio;
            set => _coreCapitalToDepositsRatio = value;
        }

        [Column("CoreCapitalToDepositsExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredCoreCapitalToDepositsExcessDeficiency
        {
            get => _coreCapitalToDepositsExcessDeficiency == 0 ? CoreCapitalToDepositsExcessDeficiency : _coreCapitalToDepositsExcessDeficiency;
            set => _coreCapitalToDepositsExcessDeficiency = value;
        }

        // Method to calculate and store all totals
        public void CalculateAndStoreTotals()
        {
            StoredSubTotalCoreCapital = SubTotalCoreCapital;
            StoredTotalDeductions = TotalDeductions;
            StoredCoreCapital = CoreCapital;
            StoredRetainedEarningsAndDisclosedReserves = RetainedEarningsAndDisclosedReserves;
            StoredTotalOnBalanceSheetAssets = TotalOnBalanceSheetAssets;
            StoredDifferenceInAssets = DifferenceInAssets;
            StoredTotalAssets = TotalAssets;
            StoredCoreCapitalToAssetsRatio = CoreCapitalToAssetsRatio;
            StoredCoreCapitalToAssetsExcessDeficiency = CoreCapitalToAssetsExcessDeficiency;
            StoredRetainedEarningsToCoreCaptialRatio = RetainedEarningsToCoreCaptialRatio;
            StoredRetainedEarningsToCoreCaptialExcessDeficiency = RetainedEarningsToCoreCaptialExcessDeficiency;
            StoredCoreCapitalToDepositsRatio = CoreCapitalToDepositsRatio;
            StoredCoreCapitalToDepositsExcessDeficiency = CoreCapitalToDepositsExcessDeficiency;
        }

        // Foreign key relationship
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        public string FilePath { get; set; } = null!;
    }
}
