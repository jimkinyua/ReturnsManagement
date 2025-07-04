using Returns.Models.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class NWDTInvestmentReturn : FormBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CoreCapital { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal NonEarningAssets { get; set; }

        // Financial assets
        public decimal SubsidiaryRelatedEntityInvestments { get; set; }
        public decimal EquityInvestments { get; set; }
        public decimal OtherInvestments { get; set; }
        public decimal OtherAssetsLandBuildingEquipment { get; set; }
        public decimal LandAndBuilding { get; set; }
        public decimal MaxLandBuildingEquipmentToTotalAssetRequirement { get; set; } = 0.10m; // 10%
        public decimal MaxLandBuildingToTotalAssetRequirement { get; set; } = 0.05m; // 5%
        public decimal MaxFinancialInvestmentsToCoreCapital { get; set; } = 0.40m; // 40%
        public decimal MaxEquityInvestmentsToTotalDeposits { get; set; } = 0.20m; // 20%
        public decimal MaxSubsidiaryInvestmentToTotalAssets { get; set; } = 0.50m; // 50%
        public decimal MaxOtherInvestmentsToCoreCapital { get; set; } = 0.30m; // 30%

        private decimal _financialAssets;
        private decimal _landBuildingEquipmentToTotalAssetsRatio;
        private decimal _landBuildingEquipmentExcessDeficiency;
        private decimal _landBuildingToTotalAssetsRatio;
        private decimal _landBuildingExcessDeficiency;
        private decimal _financialInvestmentsToCoreCapitalRatio;
        private decimal _financialInvestmentsExcessDeficiency;
        private decimal _equityInvestmentsToCoreCapitalRatio;
        private decimal _equityInvestmentsExcessDeficiency;
        private decimal _subsidiaryInvestmentsToCoreCapitalRatio;
        private decimal _subsidiaryInvestmentsExcessDeficiency;
        private decimal _otherInvestmentsToCoreCapitalRatio;
        private decimal _otherInvestmentsExcessDeficiency;

        // Calculated properties with backing fields for storage
        [NotMapped]
        public decimal FinancialAssets
        {
            get => SubsidiaryRelatedEntityInvestments + EquityInvestments + OtherInvestments;
            set => _financialAssets = value;
        }

        [NotMapped]
        public decimal LandBuildingEquipmentToTotalAssetsRatio
        {
            get => TotalAssets > 0 ? (OtherAssetsLandBuildingEquipment / TotalAssets) * 100 : 0;
            set => _landBuildingEquipmentToTotalAssetsRatio = value;
        }

        [NotMapped]
        public decimal LandBuildingEquipmentExcessDeficiency
        {
            get => LandBuildingEquipmentToTotalAssetsRatio - (MaxLandBuildingEquipmentToTotalAssetRequirement * 100);
            set => _landBuildingEquipmentExcessDeficiency = value;
        }

        [NotMapped]
        public decimal LandBuildingToTotalAssetsRatio
        {
            get => TotalAssets > 0 ? (LandAndBuilding / TotalAssets) * 100 : 0;
            set => _landBuildingToTotalAssetsRatio = value;
        }

        [NotMapped]
        public decimal LandBuildingExcessDeficiency
        {
            get => LandBuildingToTotalAssetsRatio - (MaxLandBuildingToTotalAssetRequirement * 100);
            set => _landBuildingExcessDeficiency = value;
        }

        [NotMapped]
        public decimal FinancialInvestmentsToCoreCapitalRatio
        {
            get => CoreCapital > 0 ? (FinancialAssets / CoreCapital) * 100 : 0;
            set => _financialInvestmentsToCoreCapitalRatio = value;
        }

        [NotMapped]
        public decimal FinancialInvestmentsExcessDeficiency
        {
            get => FinancialInvestmentsToCoreCapitalRatio - (MaxFinancialInvestmentsToCoreCapital * 100);
            set => _financialInvestmentsExcessDeficiency = value;
        }

        [NotMapped]
        public decimal EquityInvestmentsToCoreCapitalRatio
        {
            get => CoreCapital > 0 ? (EquityInvestments / CoreCapital) * 100 : 0;
            set => _equityInvestmentsToCoreCapitalRatio = value;
        }

        [NotMapped]
        public decimal EquityInvestmentsExcessDeficiency
        {
            get => EquityInvestmentsToCoreCapitalRatio - (MaxEquityInvestmentsToTotalDeposits * 100);
            set => _equityInvestmentsExcessDeficiency = value;
        }

        [NotMapped]
        public decimal SubsidiaryInvestmentsToCoreCapitalRatio
        {
            get => CoreCapital > 0 ? (SubsidiaryRelatedEntityInvestments / CoreCapital) * 100 : 0;
            set => _subsidiaryInvestmentsToCoreCapitalRatio = value;
        }

        [NotMapped]
        public decimal SubsidiaryInvestmentsExcessDeficiency
        {
            get => SubsidiaryInvestmentsToCoreCapitalRatio - (MaxSubsidiaryInvestmentToTotalAssets * 100);
            set => _subsidiaryInvestmentsExcessDeficiency = value;
        }

        [NotMapped]
        public decimal OtherInvestmentsToCoreCapitalRatio
        {
            get => CoreCapital > 0 ? (OtherInvestments / CoreCapital) * 100 : 0;
            set => _otherInvestmentsToCoreCapitalRatio = value;
        }

        [NotMapped]
        public decimal OtherInvestmentsExcessDeficiency
        {
            get => OtherInvestmentsToCoreCapitalRatio - (MaxOtherInvestmentsToCoreCapital * 100);
            set => _otherInvestmentsExcessDeficiency = value;
        }

        // Database columns for calculated values
        [Column("FinancialAssets", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialAssets
        {
            get => _financialAssets == 0 ? FinancialAssets : _financialAssets;
            set => _financialAssets = value;
        }

        [Column("LandBuildingEquipmentToTotalAssetsRatio", TypeName = "decimal(18,2)")]
        public decimal StoredLandBuildingEquipmentToTotalAssetsRatio
        {
            get => _landBuildingEquipmentToTotalAssetsRatio == 0 ? LandBuildingEquipmentToTotalAssetsRatio : _landBuildingEquipmentToTotalAssetsRatio;
            set => _landBuildingEquipmentToTotalAssetsRatio = value;
        }

        [Column("LandBuildingEquipmentExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredLandBuildingEquipmentExcessDeficiency
        {
            get => _landBuildingEquipmentExcessDeficiency == 0 ? LandBuildingEquipmentExcessDeficiency : _landBuildingEquipmentExcessDeficiency;
            set => _landBuildingEquipmentExcessDeficiency = value;
        }

        [Column("LandBuildingToTotalAssetsRatio", TypeName = "decimal(18,2)")]
        public decimal StoredLandBuildingToTotalAssetsRatio
        {
            get => _landBuildingToTotalAssetsRatio == 0 ? LandBuildingToTotalAssetsRatio : _landBuildingToTotalAssetsRatio;
            set => _landBuildingToTotalAssetsRatio = value;
        }

        [Column("LandBuildingExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredLandBuildingExcessDeficiency
        {
            get => _landBuildingExcessDeficiency == 0 ? LandBuildingExcessDeficiency : _landBuildingExcessDeficiency;
            set => _landBuildingExcessDeficiency = value;
        }

        [Column("FinancialInvestmentsToCoreCapitalRatio", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialInvestmentsToCoreCapitalRatio
        {
            get => _financialInvestmentsToCoreCapitalRatio == 0 ? FinancialInvestmentsToCoreCapitalRatio : _financialInvestmentsToCoreCapitalRatio;
            set => _financialInvestmentsToCoreCapitalRatio = value;
        }

        [Column("FinancialInvestmentsExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialInvestmentsExcessDeficiency
        {
            get => _financialInvestmentsExcessDeficiency == 0 ? FinancialInvestmentsExcessDeficiency : _financialInvestmentsExcessDeficiency;
            set => _financialInvestmentsExcessDeficiency = value;
        }

        [Column("EquityInvestmentsToCoreCapitalRatio", TypeName = "decimal(18,2)")]
        public decimal StoredEquityInvestmentsToCoreCapitalRatio
        {
            get => _equityInvestmentsToCoreCapitalRatio == 0 ? EquityInvestmentsToCoreCapitalRatio : _equityInvestmentsToCoreCapitalRatio;
            set => _equityInvestmentsToCoreCapitalRatio = value;
        }

        [Column("EquityInvestmentsExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredEquityInvestmentsExcessDeficiency
        {
            get => _equityInvestmentsExcessDeficiency == 0 ? EquityInvestmentsExcessDeficiency : _equityInvestmentsExcessDeficiency;
            set => _equityInvestmentsExcessDeficiency = value;
        }

        [Column("SubsidiaryInvestmentsToCoreCapitalRatio", TypeName = "decimal(18,2)")]
        public decimal StoredSubsidiaryInvestmentsToCoreCapitalRatio
        {
            get => _subsidiaryInvestmentsToCoreCapitalRatio == 0 ? SubsidiaryInvestmentsToCoreCapitalRatio : _subsidiaryInvestmentsToCoreCapitalRatio;
            set => _subsidiaryInvestmentsToCoreCapitalRatio = value;
        }

        [Column("SubsidiaryInvestmentsExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredSubsidiaryInvestmentsExcessDeficiency
        {
            get => _subsidiaryInvestmentsExcessDeficiency == 0 ? SubsidiaryInvestmentsExcessDeficiency : _subsidiaryInvestmentsExcessDeficiency;
            set => _subsidiaryInvestmentsExcessDeficiency = value;
        }

        [Column("OtherInvestmentsToCoreCapitalRatio", TypeName = "decimal(18,2)")]
        public decimal StoredOtherInvestmentsToCoreCapitalRatio
        {
            get => _otherInvestmentsToCoreCapitalRatio == 0 ? OtherInvestmentsToCoreCapitalRatio : _otherInvestmentsToCoreCapitalRatio;
            set => _otherInvestmentsToCoreCapitalRatio = value;
        }

        [Column("OtherInvestmentsExcessDeficiency", TypeName = "decimal(18,2)")]
        public decimal StoredOtherInvestmentsExcessDeficiency
        {
            get => _otherInvestmentsExcessDeficiency == 0 ? OtherInvestmentsExcessDeficiency : _otherInvestmentsExcessDeficiency;
            set => _otherInvestmentsExcessDeficiency = value;
        }

        // Method to calculate and store all values
        public void CalculateAndStoreTotals()
        {
            StoredFinancialAssets = FinancialAssets;

            StoredLandBuildingEquipmentToTotalAssetsRatio = LandBuildingEquipmentToTotalAssetsRatio;
            StoredLandBuildingEquipmentExcessDeficiency = LandBuildingEquipmentExcessDeficiency;

            StoredLandBuildingToTotalAssetsRatio = LandBuildingToTotalAssetsRatio;
            StoredLandBuildingExcessDeficiency = LandBuildingExcessDeficiency;

            StoredFinancialInvestmentsToCoreCapitalRatio = FinancialInvestmentsToCoreCapitalRatio;
            StoredFinancialInvestmentsExcessDeficiency = FinancialInvestmentsExcessDeficiency;

            StoredEquityInvestmentsToCoreCapitalRatio = EquityInvestmentsToCoreCapitalRatio;
            StoredEquityInvestmentsExcessDeficiency = EquityInvestmentsExcessDeficiency;

            StoredSubsidiaryInvestmentsToCoreCapitalRatio = SubsidiaryInvestmentsToCoreCapitalRatio;
            StoredSubsidiaryInvestmentsExcessDeficiency = SubsidiaryInvestmentsExcessDeficiency;

            StoredOtherInvestmentsToCoreCapitalRatio = OtherInvestmentsToCoreCapitalRatio;
            StoredOtherInvestmentsExcessDeficiency = OtherInvestmentsExcessDeficiency;
        }

        // Foreign key relationship
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        public string Period { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy { get; set; }
        
        public Guid ReturnSubmissionId { get; set; }           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}