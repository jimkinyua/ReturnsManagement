using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class MigrationOfReturnTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Returns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Returns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CapitalAdequacies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutoryReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsAccumulatedLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSurplusAfterTaxCurrentYearToDate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGrantsEquityInNature = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneralReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalCoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInSubsidiaryAndEquityInstruments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitutionalCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashLocalAndForeignCurrency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsAndBalancesAtOtherInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoansAndAdvances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Investments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOnBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssetsPerBalanceSheet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOffBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositsLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitutionalCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumInstitutionalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitutionalCapitalToAssetsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapitalAdequacies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapitalAdequacies_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RangeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepositType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfAccounts = table.Column<int>(type: "int", nullable: false),
                    AmountInKshs000 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonEarningAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandAndBuildings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxLandBuildingsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxFinancialInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToCoreCapitalExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxFinancialInvestmentsToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToDepositsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonEarningAssetsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxNonEarningAssetsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonEarningAssetsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidityReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LocalNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForeignNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithCommercialBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TimeDepositsWithBanksMoreThan90Days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverdraftsAndMaturedLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetBankBalances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherSaccoSocieties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToOtherSaccoSocieties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaturedLoansFromFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetFinancialInstitutionBalances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryBills = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryBonds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalGovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetLiquidAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsFromMembers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsFromOtherSources = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToSaccos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToOtherFinancialInst = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetDepositLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaturedLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiabilitiesMaturing91Days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalShortTermLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiquidityRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumLiquidityRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiquidityRatioExcessDeficit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidityReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidityReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskClassifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfAccounts = table.Column<int>(type: "int", nullable: true),
                    OutstandingLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredProvision = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredProvisionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskClassifications_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaccoAnalysis",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitutionalCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalRating = table.Column<int>(type: "int", nullable: false),
                    NonPerformingLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonPerformingLoanRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AssetQualityRating = table.Column<int>(type: "int", nullable: false),
                    GovernanceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ControlsScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManagementRating = table.Column<int>(type: "int", nullable: false),
                    NetIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnOnAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarningsRating = table.Column<int>(type: "int", nullable: false),
                    LiquidAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiquidityRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiquidityRating = table.Column<int>(type: "int", nullable: false),
                    OverallRating = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionRequired = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaccoAnalysis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaccoAnalysis_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementOfComprehensiveIncomeReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InterestOnLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeesAndCommissionOnLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsWithBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestExpenseOnDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostOfExternalBorrowings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherFinancialExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeesAndCommissionExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProvisionForLoanLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueOfLoansRecovered = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersonnelExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernanceExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarketingExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepreciationAndAmortization = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdministrativeExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Donations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementOfComprehensiveIncomeReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatementOfComprehensiveIncomeReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementOfFinancialPositionReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CashInHand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAtBank = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaymentsAndSundryReceivables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherSaccos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInCompanies = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowanceForLoanLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRecoverable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeferredTaxAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentProperties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaidLeaseRentals = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IntangibleAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShortTermDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonWithdrawableDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendsPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeferredTaxLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitsLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGrants = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriorYearsRetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentYearSurplus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutoryReserve = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RevaluationReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProposedDividends = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustmentToEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementOfFinancialPositionReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatementOfFinancialPositionReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapitalAdequacies_ReturnId",
                table: "CapitalAdequacies",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositReturns_ReturnId",
                table: "DepositReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentReturns_ReturnId",
                table: "InvestmentReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityReturns_ReturnId",
                table: "LiquidityReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskClassifications_ReturnId",
                table: "RiskClassifications",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SaccoAnalysis_ReturnId",
                table: "SaccoAnalysis",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementOfComprehensiveIncomeReturns_ReturnId",
                table: "StatementOfComprehensiveIncomeReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementOfFinancialPositionReturns_ReturnId",
                table: "StatementOfFinancialPositionReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapitalAdequacies");

            migrationBuilder.DropTable(
                name: "DepositReturns");

            migrationBuilder.DropTable(
                name: "InvestmentReturns");

            migrationBuilder.DropTable(
                name: "LiquidityReturns");

            migrationBuilder.DropTable(
                name: "RiskClassifications");

            migrationBuilder.DropTable(
                name: "SaccoAnalysis");

            migrationBuilder.DropTable(
                name: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropTable(
                name: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropTable(
                name: "Returns");
        }
    }
}
