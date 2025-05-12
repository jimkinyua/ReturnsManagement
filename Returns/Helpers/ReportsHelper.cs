using DocumentFormat.OpenXml.Drawing.Charts;
using System.Drawing.Printing;
using System.Text;
using TuesPechkin;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers
{
    public static class ReportsHelper
    {
        public static byte[] GeneratePdfReport(List<ValidationError> errors, string period)
        {
            var converter = new ThreadSafeConverter(
                new RemotingToolset<PdfToolset>(
                        new TempFolderDeployment()));

            try
            {
                // 2. Generate HTML content
                string htmlContent = GenerateHtmlReport(errors, period);

                // 3. Validate HTML content
                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    throw new ArgumentException("Generated HTML content is empty");
                }

                // 4. Configure PDF document settings
                var document = new HtmlToPdfDocument
                {
                    GlobalSettings =
            {
                PaperSize = PaperKind.A4,
                Orientation = GlobalSettings.PaperOrientation.Portrait,
                Margins = new MarginSettings { Top = 20, Bottom = 20, Left = 15, Right = 15 },
                DocumentTitle = $"SASRA Validation Report - {period}",
            },
                    Objects =
            {
                new ObjectSettings
                {
                    HtmlText = htmlContent,
                    WebSettings =
                    {
                        DefaultEncoding = "utf-8",
                        EnableJavascript = true,
                        PrintMediaType = true
                    },
                    HeaderSettings =
                    {
                        FontSize = 9,
                        CenterText = "Page [page] of [toPage]",
                        ContentSpacing = 5,
                        FontName = "Arial"
                    },
                    FooterSettings =
                    {
                        FontSize = 9,
                        CenterText = "SASRA - Confidential",
                        ContentSpacing = 5,
                        FontName = "Arial"
                    }
                }
            }
                };

                // 5. Convert to PDF
                byte[] pdfBytes = converter.Convert(document);

                // 6. Validate PDF output
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    throw new InvalidOperationException("PDF conversion returned empty result");
                }

                return pdfBytes;
            }
            
            catch (Exception ex)
            {
                // Fallback to simple PDF generation
                return GenerateSimplePdfFallback(errors, period, ex);
            }
        }

        private static byte[] GenerateSimplePdfFallback(List<ValidationError> errors, string period, Exception originalException)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms);

                document.Open();

                // Add title
                document.Add(new iTextSharp.text.Paragraph("SASRA Validation Report")
                {
                    Alignment = iTextSharp.text.Element.ALIGN_CENTER,
                    Font = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 18)
                });

                // Add period
                document.Add(new iTextSharp.text.Paragraph($"Period: {period}"));

                // Add error message
                document.Add(new iTextSharp.text.Paragraph(
                    $"WARNING: Could not generate full report. {originalException.Message}"));

                // Add errors
                foreach (var error in errors)
                {
                    document.Add(new iTextSharp.text.Paragraph(error.Description)
                    {
                        Font = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 12)
                    });

                    foreach (var detail in error.Details)
                    {
                        document.Add(new iTextSharp.text.Paragraph($"  {detail.Key}: {detail.Value}"));
                    }
                }

                document.Close();
                return ms.ToArray();
            }
        }
        public static string GenerateHtmlReport( List<ValidationError> validationErrors, string period)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset='UTF-8'>");
            sb.AppendLine("  <title>SASRA - Consistency Report</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: 'Arial', sans-serif; margin: 0; padding: 0; color: #333; }");
            sb.AppendLine("    .header { background-color: #0d5b92; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine("    .logo { height: 80px; margin-bottom: 10px; }");
            sb.AppendLine("    .container { padding: 20px 40px; }");
            sb.AppendLine("    h1 { color: #0d5b92; margin-top: 0; }");
            sb.AppendLine("    .report-meta { margin-bottom: 30px; }");
            sb.AppendLine("    .validation-status { padding: 10px; border-radius: 5px; font-weight: bold; }");
            sb.AppendLine("    .status-valid { background-color: #d4edda; color: #155724; }");
            sb.AppendLine("    .status-invalid { background-color: #f8d7da; color: #721c24; }");
            sb.AppendLine("    .error-section { margin-top: 30px; }");
            sb.AppendLine("    .error { margin-bottom: 20px; border-left: 4px solid #dc3545; padding: 10px 15px; background-color: #f8f9fa; }");
            sb.AppendLine("    .error-category { color: #dc3545; font-weight: bold; font-size: 1.1em; }");
            sb.AppendLine("    .error-desc { margin: 8px 0; }");
            sb.AppendLine("    .error-details { margin-left: 15px; }");
            sb.AppendLine("    .error-detail { margin: 5px 0; }");
            sb.AppendLine("    .footer { margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 0.9em; color: #666; text-align: center; }");
            sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin: 15px 0; }");
            sb.AppendLine("    th { background-color: #0d5b92; color: white; text-align: left; padding: 8px; }");
            sb.AppendLine("    td { padding: 8px; border-bottom: 1px solid #ddd; }");
            sb.AppendLine("    tr:nth-child(even) { background-color: #f2f2f2; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header with SASRA logo
            sb.AppendLine("  <div class='header'>");
            sb.AppendLine("    <img src='data:image/png;base64,[BASE64_LOGO_DATA]' class='logo' alt='SASRA Kenya Logo'>");
            sb.AppendLine("    <h1> Consistency Report</h1>");
            sb.AppendLine("  </div>");

            // Report content
            sb.AppendLine("  <div class='container'>");
            sb.AppendLine("    <div class='report-meta'>");
            sb.AppendLine($"      <p><strong>Reporting Period:</strong> {period}</p>");
            sb.AppendLine($"      <p><strong>Validation Status:</strong> ");
                   sb.AppendLine("        </span>");
            sb.AppendLine("      </p>");
            sb.AppendLine($"      <p><strong>Errors Found:</strong> {validationErrors.Count()}</p>");
            sb.AppendLine("    </div>");

            // Error details
            if (validationErrors.Count() > 0)
            {
                sb.AppendLine("    <div class='error-section'>");
                sb.AppendLine("      <h2>Validation Errors</h2>");

                foreach (var error in validationErrors)
                {
                    sb.AppendLine("      <div class='error'>");
                    sb.AppendLine($"        <div class='error-category'>{error.Category}</div>");
                    sb.AppendLine($"        <div class='error-desc'>{error.Description}</div>");

                    sb.AppendLine("        <table>");
                    sb.AppendLine("          <thead><tr><th>Field</th><th>Value</th></tr></thead>");
                    sb.AppendLine("          <tbody>");

                    foreach (var detail in error.Details)
                    {
                        sb.AppendLine("            <tr>");
                        sb.AppendLine($"              <td>{detail.Key}</td>");
                        sb.AppendLine($"              <td>{detail.Value}</td>");
                        sb.AppendLine("            </tr>");
                    }

                    sb.AppendLine("          </tbody>");
                    sb.AppendLine("        </table>");
                    sb.AppendLine("      </div>");
                }

                sb.AppendLine("    </div>");
            }

            // Footer
            sb.AppendLine("    <div class='footer'>");
            sb.AppendLine("      <p>SASRA - SACCO Societies Regulatory Authority</p>");
            sb.AppendLine("      <p>Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "</p>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /*        public static string GenerateHtmlReport(List<ValidationError> ValidationErrors, string period)
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
        */

        /*        public static string GenerateHtmlReport(List<ValidationError> validationErrors, string period)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("<!DOCTYPE html>");
                    sb.AppendLine("<html lang=\"en\">");
                    sb.AppendLine("<head>");
                    sb.AppendLine("    <meta charset=\"UTF-8\">");
                    sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                    sb.AppendLine("    <title>Validation Report</title>");
                    sb.AppendLine("    <style>");

                    sb.AppendLine("        :root {");
                    sb.AppendLine("            --primary-color: #0056b3;");
                    sb.AppendLine("            --error-color: #dc3545;");
                    sb.AppendLine("            --success-color: #28a745;");
                    sb.AppendLine("            --warning-color: #ffc107;");
                    sb.AppendLine("            --text-color: #333;");
                    sb.AppendLine("            --light-bg: #f8f9fa;");
                    sb.AppendLine("            --border-color: #dee2e6;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        body {");
                    sb.AppendLine("            font-family: 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;");
                    sb.AppendLine("            line-height: 1.5;");
                    sb.AppendLine("            color: var(--text-color);");
                    sb.AppendLine("            margin: 0;");
                    sb.AppendLine("            padding: 0;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .container {");
                    sb.AppendLine("            max-width: 1200px;");
                    sb.AppendLine("            margin: 0 auto;");
                    sb.AppendLine("            padding: 20px;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .header {");
                    sb.AppendLine("            background-color: var(--primary-color);");
                    sb.AppendLine("            color: white;");
                    sb.AppendLine("            padding: 20px;");
                    sb.AppendLine("            border-radius: 5px 5px 0 0;");
                    sb.AppendLine("            margin-bottom: 20px;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .header h1 {");
                    sb.AppendLine("            margin: 0;");
                    sb.AppendLine("            font-size: 24px;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .summary {");
                    sb.AppendLine("            background-color: var(--light-bg);");
                    sb.AppendLine("            padding: 15px;");
                    sb.AppendLine("            border-radius: 5px;");
                    sb.AppendLine("            margin-bottom: 20px;");
                    sb.AppendLine("            display: flex;");
                    sb.AppendLine("            align-items: center;");
                    sb.AppendLine("            justify-content: space-between;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .status {");
                    sb.AppendLine("            font-weight: bold;");
                    sb.AppendLine("            padding: 5px 10px;");
                    sb.AppendLine("            border-radius: 3px;");
                    sb.AppendLine("            display: inline-block;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .status-failed {");
                    sb.AppendLine("            background-color: var(--error-color);");
                    sb.AppendLine("            color: white;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .status-passed {");
                    sb.AppendLine("            background-color: var(--success-color);");
                    sb.AppendLine("            color: white;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-count {");
                    sb.AppendLine("            font-size: 18px;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-group {");
                    sb.AppendLine("            margin-bottom: 20px;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-category-header {");
                    sb.AppendLine("            background-color: var(--light-bg);");
                    sb.AppendLine("            border-left: 4px solid var(--error-color);");
                    sb.AppendLine("            padding: 10px 15px;");
                    sb.AppendLine("            font-weight: bold;");
                    sb.AppendLine("            font-size: 16px;");
                    sb.AppendLine("            margin-bottom: 10px;");
                    sb.AppendLine("            display: flex;");
                    sb.AppendLine("            justify-content: space-between;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-item {");
                    sb.AppendLine("            border: 1px solid var(--border-color);");
                    sb.AppendLine("            border-radius: 5px;");
                    sb.AppendLine("            margin-bottom: 10px;");
                    sb.AppendLine("            overflow: hidden;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-desc {");
                    sb.AppendLine("            padding: 10px 15px;");
                    sb.AppendLine("            border-bottom: 1px solid var(--border-color);");
                    sb.AppendLine("            background-color: white;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-details {");
                    sb.AppendLine("            padding: 0;");
                    sb.AppendLine("            margin: 0;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-detail {");
                    sb.AppendLine("            padding: 8px 15px;");
                    sb.AppendLine("            border-bottom: 1px solid var(--border-color);");
                    sb.AppendLine("            background-color: var(--light-bg);");
                    sb.AppendLine("            display: flex;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .error-detail:last-child {");
                    sb.AppendLine("            border-bottom: none;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .detail-key {");
                    sb.AppendLine("            font-weight: bold;");
                    sb.AppendLine("            width: 150px;");
                    sb.AppendLine("            flex-shrink: 0;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .detail-value {");
                    sb.AppendLine("            flex-grow: 1;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        .money-value {");
                    sb.AppendLine("            font-family: monospace;");
                    sb.AppendLine("            text-align: right;");
                    sb.AppendLine("        }");
                    sb.AppendLine("        @media print {");
                    sb.AppendLine("            .container { max-width: 100%; }");
                    sb.AppendLine("            .header { background-color: #eee; color: black; }");
                    sb.AppendLine("        }");
                    sb.AppendLine("    </style>");
                    sb.AppendLine("</head>");
                    sb.AppendLine("<body>");
                    sb.AppendLine("    <div class=\"container\">");

                    // Report header
                    sb.AppendLine("        <div class=\"header\">");
                    sb.AppendLine($"            <h1>Validation Report for Period: {period}</h1>");
                    sb.AppendLine("        </div>");

                    // Report summary
                    string statusClass = validationErrors.Count > 0 ? "status-failed" : "status-passed";
                    string statusText = validationErrors.Count > 0 ? "FAILED" : "PASSED";

                    sb.AppendLine("        <div class=\"summary\">");
                    sb.AppendLine($"            <div>Validation Status: <span class=\"status {statusClass}\">{statusText}</span></div>");
                    sb.AppendLine($"            <div class=\"error-count\">Total Errors Found: {validationErrors.Count}</div>");
                    sb.AppendLine("        </div>");

                    // Group errors by category for better organization
                    var errorGroups = validationErrors
                        .GroupBy(e => e.Category)
                        .OrderBy(g => g.Key);

                    foreach (var group in errorGroups)
                    {
                        sb.AppendLine($"        <div class=\"error-group\">");
                        sb.AppendLine($"            <div class=\"error-category-header\">");
                        sb.AppendLine($"                <span>{group.Key}</span>");
                        sb.AppendLine($"                <span>{group.Count()} issue{(group.Count() != 1 ? "s" : "")}</span>");
                        sb.AppendLine($"            </div>");

                        foreach (var error in group)
                        {
                            sb.AppendLine($"            <div class=\"error-item\">");
                            sb.AppendLine($"                <div class=\"error-desc\">{error.Description}</div>");
                            sb.AppendLine($"                <div class=\"error-details\">");

                            foreach (var detail in error.Details)
                            {
                                bool isMonetaryValue = detail.Value != null &&
                                                      detail.Value.ToString().Contains("Ksh") &&
                                                      decimal.TryParse(detail.Value.ToString().Replace("Ksh", "").Replace(",", ""), out _);

                                string valueClass = isMonetaryValue ? "detail-value money-value" : "detail-value";

                                sb.AppendLine($"                    <div class=\"error-detail\">");
                                sb.AppendLine($"                        <div class=\"detail-key\">{detail.Key}:</div>");
                                sb.AppendLine($"                        <div class=\"{valueClass}\">{detail.Value}</div>");
                                sb.AppendLine($"                    </div>");
                            }

                            sb.AppendLine($"                </div>");
                            sb.AppendLine($"            </div>");
                        }

                        sb.AppendLine($"        </div>");
                    }

                    sb.AppendLine("    </div>");
                    sb.AppendLine("</body>");
                    sb.AppendLine("</html>");

                    return sb.ToString();
                }
        */
    }
}
