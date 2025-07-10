using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DailyLiquidityReturn : FormBase
    {

        public string SACCOName { get; set; } = null!;
        public string CSNO { get; set; } = null!;
        public DateTime ReportDate { get; set; }
        public int DaysLateBy { get; set; } = 0;
        public int Version { get; set; } = 1;
        public string FilePath { get; set; } = string.Empty;

        // Opening Balances
        public decimal BankBalancesOpening { get; set; }
        public decimal ConsolidatedTreasuryCashBalancesOpening { get; set; }
        public decimal TellersBalancesOpening { get; set; }
        public decimal MobileMoneyChannelsOpening { get; set; }
        public decimal PlacementWithBanksOpening { get; set; }
        public decimal SubTotalOpening { get; set; }

        // Day Receipts
        public decimal DepositsFromMembers { get; set; }
        public decimal CashLoanRepayments { get; set; }
        public decimal OtherCashReceipts { get; set; }
        public decimal SubTotalReceipts { get; set; }
        public decimal TotalOpeningAndReceipts { get; set; }

        // Day Payments
        public decimal CashWithdrawalsByMembers { get; set; }
        public decimal CashPaymentsToMembers { get; set; }
        public decimal OtherCashPayments { get; set; }
        public decimal SubTotalPayments { get; set; }

        // Closing Balances
        public decimal BankBalancesClosing { get; set; }
        public decimal ConsolidatedTreasuryCashBalancesClosing { get; set; }
        public decimal TellersBalancesClosing { get; set; }
        public decimal MobileMoneyChannelsClosing { get; set; }
        public decimal PlacementWithBanksClosing { get; set; }
        public decimal TotalClosingBalance { get; set; }

        // Deposit Liabilities
        public decimal BOSADeposits { get; set; }
        public decimal FOSADeposits { get; set; }
        public decimal TotalDeposits { get; set; }

        // Liquidity Ratios
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalClosingBalanceToTotalDepositsRatio { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalClosingBalanceToFOSADepositsRatio { get; set; }
     
        // Computed properties that won't be stored in the database
        [NotMapped]
        public decimal ComputedSubTotalOpening =>
            BankBalancesOpening +
            ConsolidatedTreasuryCashBalancesOpening +
            TellersBalancesOpening +
            MobileMoneyChannelsOpening +
            PlacementWithBanksOpening;

        [NotMapped]
        public decimal ComputedSubTotalReceipts =>
            DepositsFromMembers +
            CashLoanRepayments +
            OtherCashReceipts;

        [NotMapped]
        public decimal ComputedTotalOpeningAndReceipts =>
            ComputedSubTotalOpening + ComputedSubTotalReceipts;

        [NotMapped]
        public decimal ComputedSubTotalPayments =>
            CashWithdrawalsByMembers +
            CashPaymentsToMembers +
            OtherCashPayments;

        [NotMapped]
        public decimal ComputedTotalClosingBalance =>
            BankBalancesClosing +
            ConsolidatedTreasuryCashBalancesClosing +
            TellersBalancesClosing +
            MobileMoneyChannelsClosing +
            PlacementWithBanksClosing;

        [NotMapped]
        public decimal ComputedTotalDeposits =>
            BOSADeposits + FOSADeposits;

        [NotMapped]
        public decimal ComputedTotalClosingBalanceToTotalDepositsRatio =>
            ComputedTotalDeposits != 0 ? ComputedTotalClosingBalance / ComputedTotalDeposits : 0;

        [NotMapped]
        public decimal ComputedTotalClosingBalanceToFOSADepositsRatio =>
            FOSADeposits != 0 ? ComputedTotalClosingBalance / FOSADeposits : 0;

        [ForeignKey("Returns")]
        public string ReturnSubmissionId { get; set; } = null!;
        public virtual ReturnSubmission Return { get; set; } = null!;
    }
}