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

internal class Program
{
    [Obsolete]
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-KE");

        var connectionString = builder.Configuration.GetConnectionString("ReturnsDbConnection");

        var CORS_SPECIFICATIONS = "ALLOWED ROUTES";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: CORS_SPECIFICATIONS,
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        builder.Services.AddDbContext<ReturnsDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        // Register DbInitializer and ReturnsDbContext
        builder.Services.AddScoped<DbInitializer>();
        builder.Services.AddScoped<ReturnsDbContext>();

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddTransient<IReturnAssignmentService, ReturnAssignmentService>();
        builder.Services.AddTransient<IComplianceService, RawSqlComplianceService>();
        builder.Services.AddTransient<IAdditionalInformationRequestService, AdditionalInformationRequestService>();
        builder.Services.AddTransient<IWorkflowTemplateAdminService, WorkflowTemplateService>();
        builder.Services.AddTransient<IWorkflowEngineService, WorkflowEngineService>();
        builder.Services.AddTransient<ICamelsAnalysisService, CamelsAnalysisService>();



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

       


        var app = builder.Build();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() }
        });

        // Fire at 09:00 on the 5th–31st of every month
        RecurringJob.AddOrUpdate<ReturnsReminderService>(
            "returns-reminder",
            job => job.SendRemindersAsync(CancellationToken.None),
            "0 9 5-31 * *",     
            queue: "reminders");

        app.UseCors(CORS_SPECIFICATIONS);



        app.UseSwagger();
        app.UseSwaggerUI();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                ReturnsDbContext context = services.GetRequiredService<ReturnsDbContext>();
                context.Database.Migrate();
                Console.WriteLine("Migration complete");
                var dbInitializer = services.GetRequiredService<DbInitializer>();
                dbInitializer.IntialiseCamelData(context);
                dbInitializer.SeedPeriods(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while seeding the database: " + ex.Message.ToString());
            }
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        app.Run();
    }
}
