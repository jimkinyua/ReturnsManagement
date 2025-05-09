using System.Text;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers
{
    public static class ReportsHelper
    {
        public static string GenerateHtmlReport(List<ValidationError> ValidationErrors, string period)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<head><style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("h1 { color: #333; }");
            sb.AppendLine(".error { margin-bottom: 15px; padding: 10px; border-left: 4px solid #dc3545; background-color: #f8f9fa; }");
            sb.AppendLine(".error-category { font-weight: bold; color: #dc3545; }");
            sb.AppendLine(".error-desc { margin: 5px 0; }");
            sb.AppendLine(".error-details { margin-left: 20px; }");
            sb.AppendLine(".error-detail { margin: 3px 0; }");
            sb.AppendLine("</style></head>");
            sb.AppendLine("<body>");

            sb.AppendLine($"<h1>Validation Report for Period: {period}</h1>");
            sb.AppendLine($"<p>Validation Status: <strong>FAILED</strong></p>");
            sb.AppendLine($"<p>Total Errors Found: {ValidationErrors.Count}</p>");

            foreach (var error in ValidationErrors)
            {
                sb.AppendLine("<div class='error'>");
                sb.AppendLine($"<div class='error-category'>{error.Category}</div>");
                sb.AppendLine($"<div class='error-desc'>{error.Description}</div>");
                sb.AppendLine("<div class='error-details'>");

                foreach (var detail in error.Details)
                {
                    sb.AppendLine($"<div class='error-detail'><strong>{detail.Key}:</strong> {detail.Value}</div>");
                }

                sb.AppendLine("</div></div>");
            }

            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }
}
