namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTLiquidityStatementDTO:CommonFormDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = null!;
        public int DaysLateBy { get; set; }
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
        public decimal TotalOtherLiabilities { get; set; }

        // Section 7: Liquidity Ratio
        public decimal NetLiquidAssets { get; set; }
        public decimal TotalShortTermLiabilities { get; set; }
        public decimal LiquidityRatio { get; set; }
        public decimal MinimumLiquidityRequirement { get; set; } = 15; // Default value
        public decimal LiquidityRatioExcessDeficit { get; set; }
    }
}
