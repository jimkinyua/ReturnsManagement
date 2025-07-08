using Microsoft.Extensions.DependencyInjection;
using Returns.Interfaces;

namespace Returns.Helpers.Excel
{
    public class FormProcessorFactory : IFormProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _processorTypes = new();

        public FormProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFormProcessor GetProcessor(string formType)
        {
            if (_processorTypes.TryGetValue(formType.ToUpperInvariant(), out var processorType))
            {
                return (IFormProcessor)_serviceProvider.GetRequiredService(processorType);
            }

            throw new NotSupportedException($"No processor registered for form type: {formType}");
        }

        public void RegisterProcessor(string formType, IFormProcessor processor)
        {
            _processorTypes[formType.ToUpperInvariant()] = processor.GetType();
        }

        public void RegisterProcessor<TProcessor>(string formType) where TProcessor : IFormProcessor
        {
            _processorTypes[formType.ToUpperInvariant()] = typeof(TProcessor);
        }
    }

    // Extension method for easy registration
    public static class FormProcessorExtensions
    {
        public static IServiceCollection AddFormProcessors(this IServiceCollection services)
        {
            // Register factory
            services.AddSingleton<IFormProcessorFactory, FormProcessorFactory>();

            // Scan and register all form processors
            var processorTypes = typeof(FormProcessorFactory).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IFormProcessor).IsAssignableFrom(t));

            foreach (var type in processorTypes)
            {
                services.AddScoped(type);
            }

            return services;
        }
    }
}