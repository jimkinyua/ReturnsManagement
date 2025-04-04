using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Returns.Helpers;
using Returns.Models.Data;
using Microsoft.Extensions.Options;
using Returns.Helpers.Interfaces;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        var app = builder.Build();

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
