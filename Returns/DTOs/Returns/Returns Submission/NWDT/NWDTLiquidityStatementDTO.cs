namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTLiquidityStatementDTO:CommonFormDTO
    {
        public decimal LocalNotesAndCoins { get; set; }
        public decimal ForeignNotesAndCoins { get; set; }
        public decimal BalancesWithCommercialBanks { get; set; }
        public decimal TimeDepositsWithBanksMoreThan90Days { get; set; }
        public decimal OverdraftsAndMaturedLoans { get; set; }
        public decimal BalancesWithOtherSaccoSocieties { get; set; }
        public decimal BalancesWithOtherFinancialInstitutions { get; set; }
        public decimal BalancesDueToOtherSaccoSocieties { get; set; }
        public decimal BalancesDueToFinancialInstitutions { get; set; }
        public decimal TreasuryBills { get; set; }
        public decimal TreasuryBonds { get; set; }
        public decimal DepositsFromMembers { get; set; }
        public decimal DepositsFromOtherSources { get; set; }
        public decimal MaturedLiabilities { get; set; }
        public decimal LiabilitiesMaturing91Days { get; set; }

        // Calculated fields
        public decimal TotalNotesAndCoins { get; set; }
        public decimal NetBankBalances { get; set; }
        public decimal NetFinancialInstitutionBalances { get; set; }
        public decimal TotalGovernmentSecurities { get; set; }
        public decimal NetLiquidAssets { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalOtherLiabilities { get; set; }
        public decimal LiquidityRatio { get; set; }
        public decimal LiquidityRatioExcessDeficit { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
