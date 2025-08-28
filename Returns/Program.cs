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
using System.Globalization;
using TuesPechkin;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.Net.Mail;
using System.Net;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Returns.DTOs.Compliance;
using Microsoft.Extensions.DependencyInjection;
using Returns.Helpers.Reminders;

var builder = WebApplication.CreateBuilder(args);

// Configure culture settings
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-KE");

// Configure services
var connectionString = builder.Configuration.GetConnectionString("ReturnsDbConnection");
var emailCfg = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()!;

builder.Services.AddCors(options =>
{
    options.AddPolicy("ALLOWED_ROUTES",
        policy => policy.AllowAnyOrigin() // Consider restricting this; see security section below
                       .AllowAnyHeader()
                       .AllowAnyMethod());
});

builder.Services.AddHttpClient<IEnforcementService, EnforcementService>(c =>
{
    c.BaseAddress = new Uri("https://sasra-backend.agilebiz.co.ke/gateway/api/enforcement/");
});

builder.Services.AddDbContext<ReturnsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Register application services (unchanged, but moved inline)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<ReturnsDbContext>();
builder.Services.AddTransient<IEmailService, FluentEmailService>();
builder.Services.AddTransient<IReturnAssignmentService, ReturnAssignmentService>();
//builder.Services.AddTransient<IComplianceService, RawSqlComplianceService>();
builder.Services
  .AddHttpClient<IComplianceService, RawSqlComplianceService>(client =>
  {
      client.BaseAddress = new Uri(builder.Configuration["GatewayConfigs:GatewayURL"]!);
  });
builder.Services.AddSingleton<IBackgroundJobClient>(sp => new BackgroundJobClient(sp.GetRequiredService<JobStorage>()));
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
builder.Services.AddTransient<IExcelParser, ExcelParserService>();
builder.Services.AddTransient<ISaccoAssignmentService, SaccoAssignmentService>();
builder.Services.AddTransient<IReturnSubmissionService, ReturnSubmissionService>();
builder.Services.AddTransient<IAdhocReturnsService, AdhocReturnsService>();
builder.Services.AddTransient<ISaccoTierCalculationService, SaccoTierCalculationService>();
builder.Services.AddScoped<ReturnsReminderService>();
builder.Services.AddLogging();

// Configure Hangfire
var conn = builder.Configuration.GetConnectionString("HangfireDbConnection")
    ?? builder.Configuration.GetConnectionString("ReturnsDbConnection");

builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(conn, new SqlServerStorageOptions
       {
           CommandBatchMaxTimeout = TimeSpan.FromMinutes(10),
           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(30),
           QueuePollInterval = TimeSpan.FromSeconds(5),
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
    opts.SchedulePollingInterval = TimeSpan.FromMinutes(40);
    opts.ShutdownTimeout = TimeSpan.FromMinutes(40);
});

// Configure PDF service
builder.Services.AddSingleton<IConverter>(provider =>
    new ThreadSafeConverter(
        new RemotingToolset<PdfToolset>(
            new TempFolderDeployment())));

// FluentEmail
builder.Services
    .AddFluentEmail(emailCfg.From)
    .AddSmtpSender(() =>
    {
        var client = new SmtpClient(emailCfg.Host, emailCfg.Port)
        {
            EnableSsl = emailCfg.EnableSsl,
            Credentials = new NetworkCredential(emailCfg.UserName, emailCfg.Password)
        };
        client.ServicePoint!.MaxIdleTime = 120_000;
        return client;
    });

var app = builder.Build();

// Configure middleware
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() }
});

RecurringJob.AddOrUpdate<ReturnsReminderService>(
    "returns-reminder",
    s => s.SendRemindersAsync(CancellationToken.None),
    "0 0 * * *",  // Changed to daily at midnight; see scheduling section below
    TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"),
    "reminders");

app.UseCors("ALLOWED_ROUTES");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

// Initialize database
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var maxRetries = 3;
var retryDelay = TimeSpan.FromSeconds(5);

var configuration = services.GetRequiredService<IConfiguration>();
var connString = configuration.GetConnectionString("ReturnsDbConnection");
Console.WriteLine($"Attempting database connection to server: {connString.Split(';')[0].Split('=')[1]}");  // Masked password; see security section

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
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}. Retrying in {retryDelay.TotalSeconds} seconds...");
        if (i == maxRetries - 1) throw;
        Thread.Sleep(retryDelay);
    }
}

app.Run();