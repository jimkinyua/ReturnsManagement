using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class NWDTLiquidityReturn : FormBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = null!;
/*        public string Frequency { get; set; } = null!;
*/        public int DaysLateBy { get; set; }
        // Section 1: Notes and Coins
        public decimal LocalNotesAndCoins { get; set; }
        public decimal ForeignNotesAndCoins { get; set; }

        // Section 2: Bank Balances
        public decimal BalancesWithCommercialBanks { get; set; }
        public decimal TimeDepositsWithBanksMoreThan90Days { get; set; }
        public decimal OverdraftsAndMaturedLoans { get; set; }

        // Section 3: Other Financial Institutions
        public decimal BalancesWithOtherSaccoSocieties { get; set; }
        public decimal BalancesWithOtherFinancialInstitutions { get; set; }
        public decimal BalancesDueToOtherSaccoSocieties { get; set; }
        public decimal BalancesDueToFinancialInstitutions { get; set; }
        public decimal MaturedLoansAndAdvances { get; set; }

        // Section 4: Government Securities
        public decimal TreasuryBills { get; set; }
        public decimal TreasuryBondsBearerBonds { get; set; }

        // Section 6: Other Liabilities
        public decimal MaturedLiabilities { get; set; }
        public decimal LiabilitiesMaturing91Days { get; set; }

        // Storage fields for calculated values
        private decimal _totalNotesAndCoins;
        private decimal _totalBankBalances;
        private decimal _totalOtherFinancialInstitutions;
        private decimal _totalGovernmentSecurities;
        private decimal _netLiquidAssets;
        private decimal _totalOtherLiabilities;
        private decimal _liquidityRatio;
        private decimal _liquidityRatioExcessDeficit;
        private decimal _netBankBalances;
        private decimal _netFinancialInstitutionBalances;

        // Calculated properties with database backing
        [NotMapped]
        public decimal NetBankBalances
        {
            get => BalancesWithCommercialBanks - (TimeDepositsWithBanksMoreThan90Days + OverdraftsAndMaturedLoans);
            set => _netBankBalances = value;
        }

        [NotMapped]
        public decimal NetFinancialInstitutionBalances
        {
            get => (BalancesWithOtherSaccoSocieties + BalancesWithOtherFinancialInstitutions) -
                   (BalancesDueToOtherSaccoSocieties + BalancesDueToFinancialInstitutions + MaturedLoansAndAdvances);
            set => _netFinancialInstitutionBalances = value;
        }

        [NotMapped]
        public decimal TotalNotesAndCoins
        {
            get => LocalNotesAndCoins + ForeignNotesAndCoins;
            set => _totalNotesAndCoins = value;
        }

        [NotMapped]
        public decimal TotalBankBalances
        {
            get => BalancesWithCommercialBanks - (TimeDepositsWithBanksMoreThan90Days + OverdraftsAndMaturedLoans);
            set => _totalBankBalances = value;
        }

        [NotMapped]
        public decimal TotalOtherFinancialInstitutions
        {
            get => (BalancesWithOtherSaccoSocieties + BalancesWithOtherFinancialInstitutions) -
                  (BalancesDueToOtherSaccoSocieties + BalancesDueToFinancialInstitutions + MaturedLoansAndAdvances);
            set => _totalOtherFinancialInstitutions = value;
        }

        [NotMapped]
        public decimal TotalGovernmentSecurities
        {
            get => TreasuryBills + TreasuryBondsBearerBonds;
            set => _totalGovernmentSecurities = value;
        }

        [NotMapped]
        public decimal NetLiquidAssets
        {
            get => TotalNotesAndCoins + TotalBankBalances + TotalOtherFinancialInstitutions + TotalGovernmentSecurities;
            set => _netLiquidAssets = value;
        }

        [NotMapped]
        public decimal TotalOtherLiabilities
        {
            get => MaturedLiabilities + LiabilitiesMaturing91Days;
            set => _totalOtherLiabilities = value;
        }

        [NotMapped]
        public decimal LiquidityRatio
        {
            get => TotalOtherLiabilities == 0 ? 0 : (NetLiquidAssets / TotalOtherLiabilities) * 100;
            set => _liquidityRatio = value;
        }

        public decimal MinimumRequirement { get; set; } = 10.0m; // Default 10%

        [NotMapped]
        public decimal LiquidityRatioExcessDeficit
        {
            get => LiquidityRatio - MinimumRequirement;
            set => _liquidityRatioExcessDeficit = value;
        }

        // Database columns for calculated values
        [Column("TotalNotesAndCoins")]
        public decimal StoredTotalNotesAndCoins
        {
            get => _totalNotesAndCoins == 0 ? TotalNotesAndCoins : _totalNotesAndCoins;
            set => _totalNotesAndCoins = value;
        }

        [Column("NetBankBalances")]
        public decimal StoredNetBankBalances
        {
            get => _netBankBalances == 0 ? NetBankBalances : _netBankBalances;
            set => _netBankBalances = value;
        }

        [Column("NetFinancialInstitutionBalances")]
        public decimal StoredNetFinancialInstitutionBalances
        {
            get => _netFinancialInstitutionBalances == 0 ? NetFinancialInstitutionBalances : _netFinancialInstitutionBalances;
            set => _netFinancialInstitutionBalances = value;
        }

        [Column("TotalBankBalances")]
        public decimal StoredTotalBankBalances
        {
            get => _totalBankBalances == 0 ? TotalBankBalances : _totalBankBalances;
            set => _totalBankBalances = value;
        }

        [Column("TotalOtherFinancialInstitutions")]
        public decimal StoredTotalOtherFinancialInstitutions
        {
            get => _totalOtherFinancialInstitutions == 0 ? TotalOtherFinancialInstitutions : _totalOtherFinancialInstitutions;
            set => _totalOtherFinancialInstitutions = value;
        }

        [Column("TotalGovernmentSecurities")]
        public decimal StoredTotalGovernmentSecurities
        {
            get => _totalGovernmentSecurities == 0 ? TotalGovernmentSecurities : _totalGovernmentSecurities;
            set => _totalGovernmentSecurities = value;
        }

        [Column("NetLiquidAssets")]
        public decimal StoredNetLiquidAssets
        {
            get => _netLiquidAssets == 0 ? NetLiquidAssets : _netLiquidAssets;
            set => _netLiquidAssets = value;
        }

        [Column("TotalOtherLiabilities")]
        public decimal StoredTotalOtherLiabilities
        {
            get => _totalOtherLiabilities == 0 ? TotalOtherLiabilities : _totalOtherLiabilities;
            set => _totalOtherLiabilities = value;
        }

        [Column("LiquidityRatio")]
        public decimal StoredLiquidityRatio
        {
            get => _liquidityRatio == 0 ? LiquidityRatio : _liquidityRatio;
            set => _liquidityRatio = value;
        }

        [Column("LiquidityRatioExcessDeficit")]
        public decimal StoredLiquidityRatioExcessDeficit
        {
            get => _liquidityRatioExcessDeficit == 0 ? LiquidityRatioExcessDeficit : _liquidityRatioExcessDeficit;
            set => _liquidityRatioExcessDeficit = value;
        }
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;

        public void CalculateAndStoreTotals()
        {
            StoredTotalNotesAndCoins = TotalNotesAndCoins;
            StoredNetBankBalances = NetBankBalances;
            StoredNetFinancialInstitutionBalances = NetFinancialInstitutionBalances;
            StoredTotalBankBalances = TotalBankBalances;
            StoredTotalOtherFinancialInstitutions = TotalOtherFinancialInstitutions;
            StoredTotalGovernmentSecurities = TotalGovernmentSecurities;
            StoredNetLiquidAssets = NetLiquidAssets;
            StoredTotalOtherLiabilities = TotalOtherLiabilities;
            StoredLiquidityRatio = LiquidityRatio;
            StoredLiquidityRatioExcessDeficit = LiquidityRatioExcessDeficit;
        }
    }
}