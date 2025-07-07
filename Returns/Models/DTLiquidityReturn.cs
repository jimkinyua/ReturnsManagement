using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DTLiquidityReturn : FormBase
    {
        // 1. Notes and Coins
        public decimal LocalNotesAndCoins { get; set; }
        public decimal ForeignNotesAndCoins { get; set; }
        public decimal TotalNotesAndCoins { get; set; }

        // 2. Bank Balances
        public decimal BalancesWithCommercialBanks { get; set; }
        public decimal TimeDepositsWithBanksMoreThan90Days { get; set; }
        public decimal OverdraftsAndMaturedLoans { get; set; }
        public decimal NetBankBalances { get; set; }

        // 3. Balances with Financial Institutions
        public decimal BalancesWithOtherSaccoSocieties { get; set; }
        public decimal BalancesWithOtherFinancialInstitutions { get; set; }
        public decimal BalancesDueToOtherSaccoSocieties { get; set; }
        public decimal BalancesDueToFinancialInstitutions { get; set; }
        public decimal MaturedLoansFromFinancialInstitutions { get; set; }
        public decimal NetFinancialInstitutionBalances { get; set; }

        // 4. Government Securities
        public decimal TreasuryBills { get; set; }
        public decimal TreasuryBonds { get; set; }
        public decimal TotalGovernmentSecurities { get; set; }

        // 5. Net Liquid Assets
        public decimal NetLiquidAssets { get; set; }

        // 6. Deposit Balances
        public decimal DepositsFromMembers { get; set; }
        public decimal DepositsFromOtherSources { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal BalancesDueToSaccos { get; set; }
        public decimal BalancesDueToBanks { get; set; }
        public decimal BalancesDueToOtherFinancialInst { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetDepositLiabilities { get; set; }

        // 7. Other Liabilities
        public decimal MaturedLiabilities { get; set; }
        public decimal LiabilitiesMaturing91Days { get; set; }
        public decimal TotalOtherLiabilities { get; set; }

        // 8. Liquidity Ratio
        public decimal TotalShortTermLiabilities { get; set; }
        public decimal LiquidityRatio { get; set; }
        public decimal MinimumLiquidityRequirement { get; set; } = 15;
        public decimal LiquidityRatioExcessDeficit { get; set; }
        public string Year { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy { get; set; }
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;

    }
}
