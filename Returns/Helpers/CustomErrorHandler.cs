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

        public static List<string> HandleException(Exception ex)
        {
            var errorMessages = new List<string>();
            if (ex.InnerException != null)
            {
                errorMessages.Add(ex.InnerException.Message);
                if (ex.InnerException.InnerException != null)
                {
                    errorMessages.Add(ex.InnerException.InnerException.Message);
                }
            }
            else
            {
                errorMessages.Add(ex.Message);
            }

            return errorMessages;
        }


    }
}
