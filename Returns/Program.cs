using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Returns.Helpers;
using Returns.Models.Data;
using Microsoft.Extensions.Options;
using Returns.Helpers.Interfaces;
using Returns.Helpers.Interfaces.WorkFlow;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Returns.Helpers.Reminders;
using System.Globalization;
using TuesPechkin;

internal class Program
{
    [Obsolete]
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure culture settings
        ConfigureCulture();

        // Configure services
        ConfigureServices(builder);

        var app = builder.Build();

        // Configure middleware pipeline
        ConfigureMiddleware(app);

        // Initialize database
        InitializeDatabase(app);

        app.Run();
    }

    private static void ConfigureCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-KE");
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("ReturnsDbConnection");

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ALLOWED_ROUTES",
                policy => policy.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod());
        });

        // Add DbContext
        builder.Services.AddDbContext<ReturnsDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add API services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        // Register application services
        RegisterApplicationServices(builder);

        // Configure Hangfire
        ConfigureHangfire(builder);

        // Configure PDF generation service
        ConfigurePdfService(builder);
    }

    private static void RegisterApplicationServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

        // Core services
        builder.Services.AddScoped<DbInitializer>();
        builder.Services.AddScoped<ReturnsDbContext>();
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddTransient<IReturnAssignmentService, ReturnAssignmentService>();
        builder.Services.AddTransient<IComplianceService, RawSqlComplianceService>();
        builder.Services.AddTransient<IAdditionalInformationRequestService, AdditionalInformationRequestService>();
        builder.Services.AddTransient<IWorkflowTemplateAdminService, WorkflowTemplateService>();
        builder.Services.AddTransient<IWorkflowEngineService, WorkflowEngineService>();
        builder.Services.AddTransient<ICamelsAnalysisService, CamelsAnalysisService>();

        //builder.Services.AddScoped<FormProcessingService>();    
        //builder.Services.AddScoped<ReturnsReminderService>();
        builder.Services.AddLogging();

        //builder.Services.AddTransient<IPdfReportService, PdfReportService>();
    }

    private static void ConfigureHangfire(WebApplicationBuilder builder)
    {
        builder.Services.AddHangfire(cfg =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UseSqlServerStorage(
                    builder.Configuration.GetConnectionString("ReturnsDbConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });
        });

        builder.Services.AddHangfireServer();
    }

    private static void ConfigurePdfService(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConverter>(provider =>
            new ThreadSafeConverter(
                new RemotingToolset<PdfToolset>(
                        new TempFolderDeployment())));
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() }
        });

        // Schedule recurring jobs
        RecurringJob.AddOrUpdate<ReturnsReminderService>(
            recurringJobId: "returns-reminder-dev",
            methodCall: s => s.SendRemindersAsync(CancellationToken.None),
            cronExpression: Cron.MinuteInterval(1),
            options: new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local,           // or FindSystemTimeZoneById("E. Africa Standard Time")
                QueueName = "reminders"                  // still works up to 1.8.x
            });



        app.UseCors("ALLOWED_ROUTES");
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.MapControllers();
    }

    private static void InitializeDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ReturnsDbContext>();
            context.Database.Migrate();

            var dbInitializer = services.GetRequiredService<DbInitializer>();
            dbInitializer.IntialiseCamelData(context);
            dbInitializer.SeedPeriods(context);

            Console.WriteLine("Database migration and seeding complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        }
    }
}