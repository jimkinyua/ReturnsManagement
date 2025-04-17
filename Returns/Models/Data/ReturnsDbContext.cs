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
        public DbSet<StatementOfComprehensiveIncomeReturn> StatementOfComprehensiveIncomeReturns { get; set; }
        public DbSet<StatementOfFinancialPositionReturn> StatementOfFinancialPositionReturns { get; set; }
        public DbSet<InvestmentReturn> InvestmentReturns { get; set; }
        public DbSet<RiskClassificationReturn> RiskClassifications { get; set; }
        public DbSet<LiquidityReturn> LiquidityReturns { get; set; }
        public DbSet<CapitalAdequacy> CapitalAdequacies { get; set; }
        public DbSet<CamelCategory> CamelCategories { get; set; }
        public DbSet<CamelIndicator> CamelIndicators { get; set; }
        public DbSet<IndicatorRatingThreshold> IndicatorRatingThresholds { get; set; }

        // NWDT Returns
        public DbSet<NDWTCapitalAdequacyReturn> NDWTCapitalAdequacyReturns { get; set; }
        public DbSet<NWDTLiquidityReturn> NDWTLiquidityReturns { get; set; }
        public DbSet<NWDTDepositReturn> NWDTDepositReturns { get; set; }
        public DbSet<NWDTRiskClassificationReturn> NWDTRiskClassificationReturns { get; set; }
        public DbSet<NWDTInvestmentReturn> NWDTInvestmentReturns { get; set; }
        public DbSet<NWDTFinancialPositionReturn> NWDTFinancialPositionReturns { get; set; }
        public DbSet<NWDTComprehensiveIncomeReturn> NWDTComprehensiveIncomeReturns { get; set; }
        
        public DbSet<ReturnsAssigment> ReturnsAssigments { get; set; }
        public DbSet<AdditionalInformationRequest> AdditionalInformationRequests { get; set; }
        public DbSet<AdditionalInfoReponse> AdditionalInfoReponses { get; set; }
        public DbSet<ResponseAttachement> ResponseAttachements { get; set; }

        public DbSet<SectoralLendingReport> SectoralLendingReports { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<EconomicSector> EconomicSectors { get; set; }
        public DbSet<EconomicSectorData> SectoralLendingData { get; set; }


    }   

}
