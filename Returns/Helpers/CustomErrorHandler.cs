using Microsoft.Extensions.Logging;

namespace Returns.Helpers
{
    public static class CustomErrorHandler
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger("CustomErrorHandler");

        public static void LogError(string errorMessage)
        {
            // Log the error
            Logger.LogError(errorMessage);
        }

        // Log Exceptions
        public static void LogException(Exception ex)
        {
            // Log the exception
            Logger.LogError(ex, ex.Message);
        }

        public static List<string> HandleException(Exception ex, bool includeStackTrace = true)
        {
            if (ex == null) return new List<string> { "Unknown error." };

            var messages = new List<string>();

            // Walk the entire InnerException chain
            for (var current = ex; current != null; current = current.InnerException)
            {
                messages.Add(current.Message);

                if (includeStackTrace && !string.IsNullOrWhiteSpace(current.StackTrace))
                {
                    if (messages.Count > 0)
                        messages.Add(current.StackTrace.Trim());
                }
            }

            return messages;
        }


    }
}
