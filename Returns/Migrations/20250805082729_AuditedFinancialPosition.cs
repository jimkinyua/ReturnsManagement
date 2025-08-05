using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AuditedFinancialPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditedFinancialPositions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReturnSubmissionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaccoCsNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InterestRateOnDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RebateRateOnShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAndCashEquivalent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashInHand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashInMobileWallets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashBalanceHeldInOtherSACCOs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAtBankCurrentAccountsKsh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAtBankFixedAccountsKsh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAtBankDollarDenominated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaymentsAndSundryReceivables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecuritiesTreasuryBillsBonds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsDepositsAtKUSCCO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoneyMarketAtCIC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoneyMarketAtCooperativeBank = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsDepositsAtKenyaTeachersAssociationKETSA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoneyMarketOthers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesAtCooperativeBankAndCoopHoldings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesAtCIC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesAtKUSCCO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesInCooperativeAllianceOfKenyaCAK = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesInCODIC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesInKenyaTeachersAssociationKETSA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentInCompaniesAllSharesTradedAtNSE = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentInSACCOSubsidiaries = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentInKMRC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInSaccoCentral = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnyOtherInvestmentsNotListedAbove = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowanceForLoanLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountsReceivables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRecoverable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeferredTaxAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyEquipmentAndOtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentProperties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaidLeaseRentals = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IntangibleAssetsManagementInformationSystemCoreBanking = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IntangibleAssetsOthers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsDepositsWithdrawableDepositsFOSA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShortTermDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonWithdrawableDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountsPayableAndOtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendsPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeferredTaxLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitsLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromCommercialBanksAndMicroFinanceBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromKUSCCO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromWomenEnterpriseFunds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromMESPT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromAgricultureFinanceCorporationAFC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromYouthFund = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromKMRC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowingsFromOtherInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGrants = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriorYearsRetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentYearsSurplus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherEquityAccounts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutoryReserve = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RevaluationReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProposedDividends = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustmentToEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLiabilitiesAndEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    FormId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresResubmission = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditedFinancialPositions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditedFinancialPositions");
        }
    }
}
