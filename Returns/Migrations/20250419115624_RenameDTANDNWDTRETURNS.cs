using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RenameDTANDNWDTRETURNS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapitalAdequacies");

            migrationBuilder.DropTable(
                name: "InvestmentReturns");

            migrationBuilder.DropTable(
                name: "LiquidityReturns");

            migrationBuilder.DropTable(
                name: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropTable(
                name: "RiskClassifications");

            migrationBuilder.DropTable(
                name: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropTable(
                name: "StatementOfFinancialPositionReturns");

            migrationBuilder.CreateTable(
                name: "DTCapitalAdequacyReturns",
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
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DTCapitalAdequacyReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DTCapitalAdequacyReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DTComprehensiveIncomeReturns",
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
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DTComprehensiveIncomeReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DTComprehensiveIncomeReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DTFinancialPositionReturns",
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
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DTFinancialPositionReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DTFinancialPositionReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DTInvestmentReturns",
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
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DTInvestmentReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DTInvestmentReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DTLiquidityReturns",
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
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DTLiquidityReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DTLiquidityReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DTRiskClassificationReturns",
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
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DTRiskClassificationReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DTRiskClassificationReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NWDTCapitalAdequacyReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGrants = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSurplusAfterTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutoryReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInSubsidiary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashLocalForeign = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsBalancesAtOtherInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoansAndAdvances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Investments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssetsPerBalanceSheet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OffBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumRetainedEarningsToCoreCaptialRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToDepositsRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositsLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalCoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsAndDisclosedReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOnBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DifferenceInAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsToCoreCaptialRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsToCoreCaptialExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NWDTCapitalAdequacyReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NWDTCapitalAdequacyReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DTCapitalAdequacyReturns_ReturnId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTComprehensiveIncomeReturns_ReturnId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTFinancialPositionReturns_ReturnId",
                table: "DTFinancialPositionReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTInvestmentReturns_ReturnId",
                table: "DTInvestmentReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTLiquidityReturns_ReturnId",
                table: "DTLiquidityReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTRiskClassificationReturns_ReturnId",
                table: "DTRiskClassificationReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTCapitalAdequacyReturns_ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DTCapitalAdequacyReturns");

            migrationBuilder.DropTable(
                name: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropTable(
                name: "DTFinancialPositionReturns");

            migrationBuilder.DropTable(
                name: "DTInvestmentReturns");

            migrationBuilder.DropTable(
                name: "DTLiquidityReturns");

            migrationBuilder.DropTable(
                name: "DTRiskClassificationReturns");

            migrationBuilder.DropTable(
                name: "NWDTCapitalAdequacyReturns");

            migrationBuilder.CreateTable(
                name: "CapitalAdequacies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CapitalGrantsEquityInNature = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashLocalAndForeignCurrency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    DepositsAndBalancesAtOtherInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitutionalCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitutionalCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitutionalCapitalToAssetsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Investments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInSubsidiaryAndEquityInstruments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    LoansAndAdvances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumInstitutionalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSurplusAfterTaxCurrentYearToDate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsAccumulatedLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatutoryReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalCoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssetsPerBalanceSheet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositsLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOffBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOnBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "InvestmentReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToCoreCapitalExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToDepositsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    LandAndBuildings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxFinancialInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxFinancialInvestmentsToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxLandBuildingsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxNonEarningAssetsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonEarningAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonEarningAssetsRatioExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonEarningAssetsToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BalancesDueToBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToOtherFinancialInst = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToOtherSaccoSocieties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToSaccos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithCommercialBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherSaccoSocieties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    DepositsFromMembers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsFromOtherSources = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForeignNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    LiabilitiesMaturing91Days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiquidityRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiquidityRatioExcessDeficit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LocalNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaturedLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaturedLoansFromFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumLiquidityRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetBankBalances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetDepositLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetFinancialInstitutionBalances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetLiquidAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverdraftsAndMaturedLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeDepositsWithBanksMoreThan90Days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalGovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalShortTermLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryBills = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryBonds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "NDWTCapitalAdequacyReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CapitalGrants = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashLocalForeign = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    DepositsBalancesAtOtherInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Investments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInSubsidiary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    LoansAndAdvances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToDepositsRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumRetainedEarningsToCoreCaptialRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSurplusAfterTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OffBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatutoryReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapitalToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DifferenceInAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsAndDisclosedReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsToCoreCaptialExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsToCoreCaptialRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalCoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOnBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssetsPerBalanceSheet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositsLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NDWTCapitalAdequacyReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NDWTCapitalAdequacyReturns_Returns_ReturnId",
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
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfAccounts = table.Column<int>(type: "int", nullable: true),
                    OutstandingLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredProvision = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredProvisionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "StatementOfComprehensiveIncomeReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdministrativeExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostOfExternalBorrowings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    DepositsWithBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepreciationAndAmortization = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Donations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FeesAndCommissionExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeesAndCommissionOnLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovernanceExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestExpenseOnDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestOnLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    MarketingExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherFinancialExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersonnelExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvisionForLoanLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueOfLoansRecovered = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdjustmentToEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowanceForLoanLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherSaccos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGrants = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAtBank = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashInHand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentYearSurplus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    DeferredTaxAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeferredTaxLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendsPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalBorrowings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IntangibleAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentProperties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInCompanies = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    NonWithdrawableDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaidLeaseRentals = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaymentsAndSundryReceivables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriorYearsRetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProposedDividends = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitsLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RevaluationReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShortTermDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatutoryReserve = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRecoverable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "IX_InvestmentReturns_ReturnId",
                table: "InvestmentReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityReturns_ReturnId",
                table: "LiquidityReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NDWTCapitalAdequacyReturns_ReturnId",
                table: "NDWTCapitalAdequacyReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskClassifications_ReturnId",
                table: "RiskClassifications",
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
    }
}
