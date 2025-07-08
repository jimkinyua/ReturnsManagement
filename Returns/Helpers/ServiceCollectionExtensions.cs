using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Returns.DTOs.Returns.Returns_Submission.DT;
using Returns.Helpers.Excel;
using Returns.Helpers.Excel.Configurations;
using Returns.Helpers.Excel.Processors;
using Returns.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Returns.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExcelImportServices(this IServiceCollection services)
        {
            // Register core services
            services.AddScoped<IExcelImportService, ExcelImportService>();
            services.AddSingleton<IFormProcessorFactory, FormProcessorFactory>();

            // Register form configurations
            services.AddSingleton<IFormConfiguration<CapitalAdequacyStatement>, CapitalAdequacyConfiguration>();
            services.AddSingleton<IFormConfiguration<LiquidityStatement>, LiquidityStatementConfiguration>();
            services.AddSingleton<IFormConfiguration<DepositReturnDto>, DepositReturnConfiguration>();
            // Add more configurations as needed...

            // Register form processors
            services.AddScoped<CapitalAdequacyProcessor>();
            services.AddScoped<LiquidityStatementProcessor>();
            services.AddScoped<DepositReturnProcessor>();
            // Add more processors as needed...

            // Register processors with factory during startup
            services.AddHostedService<FormProcessorRegistrationService>();

            return services;
        }

        // Alternative: fluent configuration
        public static IServiceCollection AddFormProcessor<TProcessor>(
            this IServiceCollection services, 
            string formType) where TProcessor : class, IFormProcessor
        {
            services.AddScoped<TProcessor>();
            
            // Register with factory
            services.Configure<FormProcessorOptions>(options =>
            {
                options.Processors[formType] = typeof(TProcessor);
            });

            return services;
        }
    }

    // Background service to register processors with factory
    public class FormProcessorRegistrationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public FormProcessorRegistrationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IFormProcessorFactory>();

            // Register all processors
            RegisterProcessor<CapitalAdequacyProcessor>(factory);
            RegisterProcessor<LiquidityStatementProcessor>(factory);
            RegisterProcessor<DepositReturnProcessor>(factory);
            // Add more as needed...

            return Task.CompletedTask;
        }

        private void RegisterProcessor<T>(IFormProcessorFactory factory) where T : IFormProcessor
        {
            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<T>();
            factory.RegisterProcessor(processor.FormType, processor);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    // Configuration options
    public class FormProcessorOptions
    {
        public Dictionary<string, Type> Processors { get; set; } = new();
    }
}