using Microsoft.EntityFrameworkCore;
using Returns.Models.CamelSetup;

namespace Returns.Models.Data
{
    public class ReturnsDbContext : DbContext
    {
        public ReturnsDbContext(DbContextOptions<ReturnsDbContext> options) : base(options) { }
        public ReturnsDbContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .AddJsonFile($"appsettings.{environment}.json", optional: true)
           .AddEnvironmentVariables()
           .Build();

            optionsBuilder.UseSqlServer(config.GetConnectionString("ReturnsDbConnection"));
            // var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            // optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        }

        public DbSet<Period> Periods { get; set; }
        //public DbSet<QuarterDates> QuarterDates { get; set; }
        public DbSet<ReturnForm> ReturnForms { get; set; }
        public DbSet<OtherReturn> OtherReturns { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<DepositReturn> DepositReturns { get; set; }
        public DbSet<SaccoAnalysis> SaccoAnalysis { get; set; }
        public DbSet<DTComprehensiveIncomeReturn> DTComprehensiveIncomeReturns { get; set; }
        public DbSet<DTFinancialPositionReturn> DTFinancialPositionReturns { get; set; }
        public DbSet<DTInvestmentReturn> DTInvestmentReturns { get; set; }
        public DbSet<DTRiskClassificationReturn> DTRiskClassificationReturns { get; set; }
        public DbSet<DTLiquidityReturn> DTLiquidityReturns { get; set; }
        public DbSet<DTCapitalAdequacyReturn> DTCapitalAdequacyReturns { get; set; }
        public DbSet<CamelCategory> CamelCategories { get; set; }
        public DbSet<CamelIndicator> CamelIndicators { get; set; }
        public DbSet<IndicatorRatingThreshold> IndicatorRatingThresholds { get; set; }

        // NWDT Return
        public DbSet<NWDTCapitalAdequacyReturn> NWDTCapitalAdequacyReturns { get; set; }
        public DbSet<NWDTLiquidityReturn> NDWTLiquidityReturns { get; set; }
        public DbSet<NWDTDepositReturn> NWDTDepositReturns { get; set; }
        public DbSet<NWDTRiskClassificationReturn> NWDTRiskClassificationReturns { get; set; }
        public DbSet<NWDTInvestmentReturn> NWDTInvestmentReturns { get; set; }
        public DbSet<NWDTFinancialPositionReturn> NWDTFinancialPositionReturns { get; set; }
        public DbSet<NWDTComprehensiveIncomeReturn> NWDTComprehensiveIncomeReturns { get; set; }
        
        public DbSet<ReturnsAssigment> ReturnsAssigments { get; set; }
        public DbSet<AdditionalInformationRequest> AdditionalInformationRequests { get; set; }
        public DbSet<AdditionalInfoResponse> AdditionalInfoResponses  { get; set; }
        public DbSet<ResponseAttachement> ResponseAttachements { get; set; }

        public DbSet<SectoralLendingReport> SectoralLendingReports { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<EconomicSector> EconomicSectors { get; set; }
        public DbSet<EconomicSectorData> SectoralLendingData { get; set; }
        public DbSet<DailyLiquidityReturn> DailyLiquidityReturns { get; set; }
        public DbSet<InsiderLendingHeader> InsiderLendingHeaders { get; set; }
        public DbSet<InsiderLoan> InsiderLoans { get; set; }
        public DbSet<ManagementReturn> ManagementReturns { get; set; }
        public DbSet<WorkFlowTemplate> WorkFlowTemplates { get; set; }
        public DbSet<WorkFlowStep> WorkFlowSteps { get; set; }
        public DbSet<ApprovalAction> ApprovalActions { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<FormResubmissionRequest> FormResubmissionRequests { get; set; }

    }   

}
