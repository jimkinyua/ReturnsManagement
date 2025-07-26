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
//using Returns.Helpers.Reminders;
using System.Globalization;
using TuesPechkin;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.Net.Mail;
using System.Net;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Returns.DTOs.Compliance;
using Microsoft.Extensions.DependencyInjection;
using Returns.Helpers.Reminders;  // Added for IServiceCollection

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
        // Get connection string from configuration
        // Using ReturnsDbConnection which points to: Server=10.0.0.4;Database=Returns;User Id=erp;Password=Pass@7046.;
        var connectionString = builder.Configuration.GetConnectionString("ReturnsDbConnection");
        var emailCfg = builder.Configuration
                       .GetSection("EmailSettings")
                       .Get<EmailSettings>()!;
        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ALLOWED_ROUTES",
                policy => policy.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod());
        });

        builder.Services.AddHttpClient<IEnforcementService, EnforcementService>(c =>
        {
            c.BaseAddress = new Uri("https://sasra-backend.agilebiz.co.ke/gateway/api/enforcement/");
        });



        // Add DbContext with SQL Server connection
        builder.Services.AddDbContext<ReturnsDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add API services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddMemoryCache();


        // Register application services
        RegisterApplicationServices(builder);

        // Configure Hangfire (uncommented and fixed)
        ConfigureHangfire(builder);

        // Configure PDF generation service
        ConfigurePdfService(builder);

        builder.Services
       .AddFluentEmail(emailCfg.From)
       .AddSmtpSender(() =>
       {
           var client = new SmtpClient(emailCfg.Host, emailCfg.Port)
           {
               EnableSsl = emailCfg.EnableSsl,
               Credentials = new NetworkCredential(
                                 emailCfg.UserName,
                                 emailCfg.Password
                             )
           };
           // optional: tweak ServicePoint.MaxIdleTime if you like
           client.ServicePoint!.MaxIdleTime = 120_000;
           return client;
       });

    }

    private static void RegisterApplicationServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

        // Core services
        builder.Services.AddScoped<DbInitializer>();
        builder.Services.AddScoped<ReturnsDbContext>();
        //builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddTransient<IEmailService, FluentEmailService>();
        //builder.Services.AddSingleton<IEmailService, EmailService>();   // NOT AddScoped / AddTransient

        builder.Services.AddTransient<IReturnAssignmentService, ReturnAssignmentService>();
        builder.Services.AddTransient<IComplianceService, RawSqlComplianceService>();
        builder.Services.AddTransient<IAdditionalInformationRequestService, AdditionalInformationRequestService>();
        builder.Services.AddTransient<IWorkflowTemplateAdminService, WorkflowTemplateService>();
        builder.Services.AddTransient<IWorkflowEngineService, WorkflowEngineService>();
        builder.Services.AddTransient<ICamelsAnalysisService, CamelsAnalysisService>();
        builder.Services.AddTransient<IEnforcementService, EnforcementService>();
        builder.Services.AddTransient<IReturnChild, ChildGetterService>();
        builder.Services.AddTransient<IPeriodGenerator, PeriodGenerator>();
        builder.Services.AddTransient<IReturnFormAttachmentService, ReturnFormAttachmentService>();
        builder.Services.AddTransient<IRatingDefinitionService, RatingDefinitionService>();
        builder.Services.AddTransient<IConsistencyCheckService, ConsistencyCheckService>();
        builder.Services.AddTransient<IAdminReturnService, AdminReturnService>();
        builder.Services.AddTransient<IReturnAmendmentPolicy, CutOffPolicy>();
        builder.Services.AddTransient<IAmendmentService, AmendmentService>();

        // Add Excel parsing and return submission services
        builder.Services.AddTransient<IExcelParser, ExcelParserService>();
        builder.Services.AddTransient<IReturnSubmissionService, ReturnSubmissionService>();

        // Add ReturnsReminderService
        builder.Services.AddScoped<ReturnsReminderService>();

        //builder.Services.AddScoped<FormProcessingService>();    
        builder.Services.AddLogging();


        //builder.Services.AddTransient<IPdfReportService, PdfReportService>();
    }

    private static void ConfigureHangfire(WebApplicationBuilder builder)
    {
        var conn = builder.Configuration.GetConnectionString("HangfireDbConnection")
            ?? builder.Configuration.GetConnectionString("ReturnsDbConnection");

        builder.Services.AddHangfire(cfg =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UseSqlServerStorage(conn, new SqlServerStorageOptions
               {
                   // give long-running jobs breathing room
                   CommandBatchMaxTimeout = TimeSpan.FromMinutes(10),
                   SlidingInvisibilityTimeout = TimeSpan.FromMinutes(30),

                   // snappy queue polling so devs �see something happen�
                   QueuePollInterval = TimeSpan.FromSeconds(5),

                   // 1.8+ best-practice flags
                   UsePageLocksOnDequeue = true,
                   DisableGlobalLocks = true
               });
            cfg.UseFilter(new DisableConcurrentExecutionAttribute(300));
        });

        builder.Services.AddHangfireServer(opts =>
        {
            opts.ServerName = $"reminder-srv-{Environment.MachineName}";
            opts.WorkerCount = Math.Max(2, Environment.ProcessorCount * 4);
            opts.Queues = new[] { "reminders", "default" };
            opts.SchedulePollingInterval = TimeSpan.FromMinutes(40);    // re-check Cron schedule quickly
            opts.ShutdownTimeout = TimeSpan.FromMinutes(40);
        });
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

        RecurringJob.AddOrUpdate<ReturnsReminderService>(
         recurringJobId: "returns-reminder",
        methodCall: s => s.SendRemindersAsync(CancellationToken.None),
         cronExpression: "*/5 * * * *",                              // every 5 minutes; tweak as needed
        timeZone: TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"),
        queue: "reminders");


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
        var maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(5);

        var configuration = services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("ReturnsDbConnection");
        Console.WriteLine($"Attempting database connection with: {connectionString}");
        var dbserverName = connectionString.Split(';')[0].Split('=')[1];
        Console.WriteLine($"Database server name: {dbserverName}");

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var context = services.GetRequiredService<ReturnsDbContext>();
                context.Database.Migrate();

                var dbInitializer = services.GetRequiredService<DbInitializer>();
                dbInitializer.IntialiseCamelData(context);
                dbInitializer.SeedFrequencyCatalog(context);
                dbInitializer.SeedPeriods(context);
                dbInitializer.SeedRatingDefinitionsForSaccoTypeDTSaccos(context);
                dbInitializer.SeedRatingDefinitionsForSaccoTypeNWDTSaccos(context);
                Console.WriteLine("Database migration and seeding complete");
                return; // Success - exit the retry loop
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1) // Last attempt
                {
                    Console.WriteLine($"Failed to seed database after {maxRetries} attempts. Last error: {ex.Message}");
                    throw; // Re-throw on final attempt
                }

                Console.WriteLine($"Attempt {i + 1} failed, retrying in {retryDelay.TotalSeconds} seconds... Error: {ex.Message}");
                Thread.Sleep(retryDelay);
            }
        }
    }
}