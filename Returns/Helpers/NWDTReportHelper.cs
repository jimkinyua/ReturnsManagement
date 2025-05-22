using iText.Bouncycastleconnector;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using Returns.DTOs.Perfomance_Report.NWDT;
using System.Globalization;
using iText.Layout;
using iText.Layout.Borders;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Draw;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
namespace Returns.Helpers
{
    public static class NWDTReportHelper
    {

    public static byte[] GenerateNwdtSaccoPerformancePdfReport(NWDTPerformanceReportDTO report, string components)
    {
        

        // Determine if we need pagination based on period count
        bool needsPagination = report.Periods.Count > 3; // Threshold for pagination
        int periodsPerPage = 3; // Number of periods to show per page

        using (MemoryStream ms = new MemoryStream())
        {
            // Create PDF document
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf, PageSize.A4.Rotate()); // Use landscape orientation

            // Set document properties
            document.SetMargins(36, 36, 36, 36);

            // Define fonts
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Colors
            DeviceRgb headerColor = new DeviceRgb(13, 91, 146); // #0d5b92
            DeviceRgb sectionBgColor = new DeviceRgb(229, 231, 235); // Light gray background
            DeviceRgb normalRowBgColor = new DeviceRgb(240, 240, 240); // Lighter gray for normal rows

            // If we need pagination, create multiple tables
            if (needsPagination)
            {
                // Split periods into groups
                for (int pageIndex = 0; pageIndex < Math.Ceiling((double)report.Periods.Count / periodsPerPage); pageIndex++)
                {
                    // Get periods for this page
                    var periodsForPage = report.Periods
                        .Skip(pageIndex * periodsPerPage)
                        .Take(periodsPerPage)
                        .ToList();

                    // Add header to each page
                    AddReportHeader(document, headerColor, boldFont, report.SaccoName);

                    // Add report metadata with page indicator
                    document.Add(new Paragraph().SetMarginTop(20));
                    string pageIndicator = needsPagination ? $" (Page {pageIndex + 1} of {Math.Ceiling((double)report.Periods.Count / periodsPerPage)})" : "";
                    Paragraph reportDatePara = new Paragraph($"Report Date: {report.ReportDate:dd MMMM, yyyy}{pageIndicator}")
                        .SetFont(boldFont)
                        .SetFontSize(12)
                        .SetMarginBottom(20);
                    document.Add(reportDatePara);

                    // Create table for this page's periods
                    int columnCount = 2 + periodsForPage.Count; // Metric name, standard, and period columns
                    Table tableForPage = new Table(UnitValue.CreatePercentArray(columnCount))
                        .SetWidth(UnitValue.CreatePercentValue(100));

                    // Generate table with only these periods
                    GenerateNwdtReportTable(tableForPage, report, periodsForPage, components, boldFont, regularFont, sectionBgColor, normalRowBgColor);

                    document.Add(tableForPage);

                    // Add footer
                    AddReportFooter(document);

                    // Add page break if not the last page
                    if (pageIndex < Math.Ceiling((double)report.Periods.Count / periodsPerPage) - 1)
                    {
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }
                }
            }
            else
            {
                // Add header
                AddReportHeader(document, headerColor, boldFont, report.SaccoName);

                // Add report metadata
                document.Add(new Paragraph().SetMarginTop(20));
                Paragraph reportDatePara = new Paragraph($"Report Date: {report.ReportDate:dd MMMM, yyyy}")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetMarginBottom(20);
                document.Add(reportDatePara);

                // Create a single table with all periods
                int columnCount = 2 + report.Periods.Count;
                Table mainTable = new Table(UnitValue.CreatePercentArray(columnCount))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                GenerateNwdtReportTable(mainTable, report, report.Periods, components, boldFont, regularFont, sectionBgColor, normalRowBgColor);

                document.Add(mainTable);

                // Add footer
                AddReportFooter(document);
            }

            // Close the document
            document.Close();

            return ms.ToArray();
        }
    }

    // Add report header
    private static void AddReportHeader(Document document, DeviceRgb headerColor, PdfFont boldFont, string saccoName)
    {
        try
        {
            // Replace with actual path to logo or embedded resource
            ImageData logoData = ImageDataFactory.Create(GetSasraLogoBytes());
            Image logo = new Image(logoData).SetHeight(60);

            Paragraph headerPara = new Paragraph($"SASRA - {saccoName} Performance Report")
                .SetFont(boldFont)
                .SetFontColor(ColorConstants.WHITE)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);

            Div headerDiv = new Div()
                .SetBackgroundColor(headerColor)
                .SetPadding(20)
                .Add(new Paragraph().Add(logo).SetTextAlignment(TextAlignment.CENTER))
                .Add(headerPara);

            document.Add(headerDiv);
        }
        catch (Exception ex)
        {
            // If logo loading fails, just add the text header
            Paragraph headerPara = new Paragraph($"SASRA - {saccoName} Performance Report")
                .SetFont(boldFont)
                .SetFontColor(ColorConstants.WHITE)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);

            Div headerDiv = new Div()
                .SetBackgroundColor(headerColor)
                .SetPadding(20)
                .Add(headerPara);

            document.Add(headerDiv);
        }
    }

    // Add report footer
    private static void AddReportFooter(Document document)
    {
        document.Add(new Paragraph().SetMarginTop(20));
        Paragraph footer = new Paragraph("SASRA - SACCO Societies Regulatory Authority\n" +
                                         "Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(9)
            .SetFontColor(new DeviceRgb(102, 102, 102)); // #666666

        document.Add(footer);
    }

        // Helper method to generate table for a given set of periods
        /// <summary>
        /// Builds the CAMELS (or CAEL, CAMEL, etc.) table dynamically,
        /// including Management (“M”) and gating every block by the caller-supplied
        /// <paramref name="components"/> string.
        /// </summary>
        private static void GenerateNwdtReportTable(
            Table table,
            NWDTPerformanceReportDTO report,
            List<NWDTPerformanceReportDTO.NWDTPeriodData> periods,
            string components,                              // e.g. "CAMELS", "CAEL"
            PdfFont boldFont,
            PdfFont regularFont,
            DeviceRgb sectionBgColor,
            DeviceRgb normalRowBgColor)
        {
            /* ---------- fixed header row ---------- */
            table.AddCell(new Cell().SetBackgroundColor(sectionBgColor)
                                    .SetPadding(10)
                                    .SetBorder(Border.NO_BORDER));

            table.AddCell(new Cell().Add(new Paragraph("Prudential Standard").SetFont(boldFont))
                                    .SetBackgroundColor(sectionBgColor)
                                    .SetPadding(10)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .SetBorder(Border.NO_BORDER));

            foreach (var period in periods)
            {
                table.AddCell(new Cell().Add(new Paragraph(period.PeriodLabel).SetFont(boldFont))
                                        .SetBackgroundColor(sectionBgColor)
                                        .SetPadding(10)
                                        .SetTextAlignment(TextAlignment.CENTER)
                                        .SetBorder(Border.NO_BORDER));
            }

            /* ===== C – CAPITAL ADEQUACY ===== */
            if (components.Contains('C'))
            {
                AddSectionHeader(table, "A. CAPITAL ADEQUACY", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "Core Capital",
                    report.PrudentialStandards.TryGetValue("CoreCapital", out var cc) ? cc : "N/A",
                    periods.Select(p => p.CoreCapital).ToList(),
                    FormatType.Currency, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Core Capital/Total Assets",
                    report.PrudentialStandards.TryGetValue("CoreCapita/Total Assets", out var ccta) ? ccta : "N/A",
                    periods.Select(p => p.CoreCapitalToTotalAssetsRatio).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== A – ASSET QUALITY ===== */
            if (components.Contains('A'))
            {
                AddSectionHeader(table, "B. ASSET QUALITY", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "NPL (Substandard+Doubtful+Loss)",
                    report.PrudentialStandards.TryGetValue("Non-Performing Loans", out var npl) ? npl : "N/A",
                    periods.Select(p => p.NonPerformingLoans).ToList(),
                    FormatType.Currency, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Non-Earning Assets",
                    report.PrudentialStandards.TryGetValue("Non-Earning Assets", out var nea) ? nea : "N/A",
                    periods.Select(p => p.NonEarningAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Equity investments/Deposits",
                    report.PrudentialStandards.TryGetValue("Equity Investments to Core Capital Ratio", out var eid) ? eid : "N/A",
                    periods.Select(p => p.EquityInvestmentsToDeposits * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Equity investments/Core capital",
                    report.PrudentialStandards.TryGetValue("Equity Investments to Core Capital Ratio", out var eic) ? eic : "N/A",
                    periods.Select(p => p.EquityInvestmentsToCoreCapitalRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Subsidiary & Related Investments/Core capital",
                    report.PrudentialStandards.TryGetValue("Subsidary Investments to Total Assets", out var sri) ? sri : "N/A",
                    periods.Select(p => p.SubsidiaryAndRelatedInvestmentToCoreCapitalRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Other Financial Investments/Core capital",
                    report.PrudentialStandards.TryGetValue("Other Financial investments to Core Capital Ratio ", out var ofi) ? ofi : "N/A",
                    periods.Select(p => p.OtherFinancialInvestmentsToCoreCapitalRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== M – MANAGEMENT ===== */
            if (components.Contains('M'))
            {
                AddSectionHeader(table, "C. MANAGEMENT", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "Governance Structure Score", "N/A",
                    periods.Select(p => p.GovernanceStructureScore).ToList(),
                    FormatType.Number, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Internal Controls Score", "N/A",
                    periods.Select(p => p.InternalControlsScore).ToList(),
                    FormatType.Number, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Compliance-with-Laws Score", "N/A",
                    periods.Select(p => p.ComplianceWithLawsScore).ToList(),
                    FormatType.Number, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Member Protection Score", "N/A",
                    periods.Select(p => p.MemberProtectionScore).ToList(),
                    FormatType.Number, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== E – EARNINGS ===== */
            if (components.Contains('E'))
            {
                AddSectionHeader(table, "D. EARNINGS RATING", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "Yield on Gross Loans", "N/A",
                    periods.Select(p => p.YieldOnGrossLoans * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Total Expense/Total Income", "N/A",
                    periods.Select(p => p.TotalExpenseToTotalIncomeRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Net Income/Average Assets (ROA)", "N/A",
                    periods.Select(p => p.NetIncomeToAverageAssetsRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Operating Expenses/Financial Income (OPEX)", "N/A",
                    periods.Select(p => p.OperatingExpenseToFinancialIncomeRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== L – LIQUIDITY ===== */
            if (components.Contains('L'))
            {
                AddSectionHeader(table, "E. LIQUIDITY", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "Liquid Assets/Short-term Liabilities",
                    report.PrudentialStandards.TryGetValue("Liquid Assets/Short-term Liabilities", out var lasl) ? lasl : "N/A",
                    periods.Select(p => p.LiquidAssetsToShortTermLiabilitiesRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "External Borrowing/Total Assets",
                    report.PrudentialStandards.TryGetValue("External Borrowing to Total Assets", out var ebta) ? ebta : "N/A",
                    periods.Select(p => p.ExternalBorrowingToTotalAssetsRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Liquid Assets/Total Assets", "N/A",
                    periods.Select(p => p.LiquidAssetsToTotalAssetsRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== S – STRUCTURE / SENSITIVITY ===== */
            if (components.Contains('S'))
            {
                AddSectionHeader(table, "F. STRUCTURE / SENSITIVITY TO RISK", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "Financial Investments/Total Assets", "N/A",
                    periods.Select(p => p.FinancialInvestmentsToTotalAssetsRatio * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== RAW FINANCIAL STATISTICS (always shown) ===== */
            AddSectionHeader(table, "G. KEY FINANCIAL STATISTICS", 2 + periods.Count, boldFont, sectionBgColor);

            AddMetricRow(table, "Total Assets", "N/A",
                periods.Select(p => p.TotalAssets).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Average Assets", "N/A",
                periods.Select(p => p.AverageAssets).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Total Deposits", "N/A",
                periods.Select(p => p.TotalDeposits).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Gross Loans from Form 4", "N/A",
                periods.Select(p => p.GrossLoansForm4).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Gross Loans from Form 6", "N/A",
                periods.Select(p => p.GrossLoansForm6).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Core Capital", "N/A",
                periods.Select(p => p.CoreCapital).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Non-performing Loans", "N/A",
                periods.Select(p => p.NonPerformingLoans).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Non-Earning Assets", "N/A",
                periods.Select(p => p.NonEarningAssets * 100).ToList(),
                FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Property and Equipment", "N/A",
                periods.Select(p => p.PropertyAndEquipment).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Equity Investments", "N/A",
                periods.Select(p => p.EquityInvestments).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Financial Investments", "N/A",
                periods.Select(p => p.FinancialInvestments).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Equity Investments in NACCOS", "N/A",
                periods.Select(p => p.EquityInvestmentsInNaccos).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Liquid Assets", "N/A",
                periods.Select(p => p.LiquidAssets).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Short-term Liabilities", "N/A",
                periods.Select(p => p.ShortTermLiabilities).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "External Borrowing", "N/A",
                periods.Select(p => p.ExternalBorrowing).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Average Gross Loans", "N/A",
                periods.Select(p => p.AverageGrossLoans).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Total Income", "N/A",
                periods.Select(p => p.TotalIncome).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Net Financial Income/Loss", "N/A",
                periods.Select(p => p.NetFinancialIncome).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Dividends + Interest on Deposits", "N/A",
                periods.Select(p => p.DividendsAndInterestOnDeposits).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Operating Expenses", "N/A",
                periods.Select(p => p.OperatingExpenses).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Interest on Loan Portfolio + Fees & Commission on Loan", "N/A",
                periods.Select(p => p.InterestOnLoanPortfolioAndFeesCommission).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Total Expenses", "N/A",
                periods.Select(p => p.TotalExpenses).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Net Income", "N/A",
                periods.Select(p => p.NetIncome).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);
        }

        // Define format types
        public enum FormatType
    {
        Currency,
        Percentage,
        Number
    }

    // Add a section header to the table
    private static void AddSectionHeader(Table table, string sectionName, int columnCount, PdfFont boldFont, DeviceRgb bgColor)
    {
        Cell sectionHeader = new Cell(1, columnCount)
            .Add(new Paragraph(sectionName).SetFont(boldFont))
            .SetBackgroundColor(bgColor)
            .SetPadding(10)
            .SetBorder(Border.NO_BORDER);
        table.AddCell(sectionHeader);
    }

    // Add a metric row to the table
    private static void AddMetricRow(Table table, string metricName, string standard, List<decimal> values,
        FormatType formatType, PdfFont boldFont, PdfFont regularFont, DeviceRgb bgColor)
    {
        // Add metric name cell
        Cell metricCell = new Cell(1, 1)
            .Add(new Paragraph(metricName).SetFont(regularFont))
            .SetBackgroundColor(bgColor)
            .SetPadding(8)
            .SetBorder(Border.NO_BORDER);
        table.AddCell(metricCell);

        // Add standard cell
        Cell standardCell = new Cell(1, 1)
            .Add(new Paragraph(standard).SetFont(regularFont))
            .SetBackgroundColor(bgColor)
            .SetPadding(8)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetBorder(Border.NO_BORDER);
        table.AddCell(standardCell);

        // Add value cells
        foreach (var value in values)
        {
            string formattedValue;

            // Format the value based on type
            switch (formatType)
            {
                case FormatType.Currency:
                    formattedValue = FormatCurrency(value);
                    break;
                case FormatType.Percentage:
                    formattedValue = FormatPercentage(value);
                    break;
                default:
                    formattedValue = value.ToString("0.00");
                    break;
            }

            Paragraph valuePara = new Paragraph(formattedValue)
                .SetFont(regularFont)
                .SetTextAlignment(TextAlignment.RIGHT);

            // Color negative values in red
            if (value < 0)
            {
                valuePara.SetFontColor(new DeviceRgb(220, 53, 69));
            }

            Cell valueCell = new Cell(1, 1)
                .Add(valuePara)
                .SetBackgroundColor(bgColor)
                .SetPadding(8)
                .SetBorder(Border.NO_BORDER);

            table.AddCell(valueCell);
        }
    }

    // Format decimal as currency
    private static string FormatCurrency(decimal value)
    {
        // Show as is without rounding to match the displayed format in images
        return value.ToString("N2", CultureInfo.InvariantCulture);
    }

    // Format decimal as percentage
    private static string FormatPercentage(decimal value)
    {
        // Show as is without rounding to match the displayed format in images
        return value.ToString("0.00") + "%";
    }

    // Helper method to get logo bytes
    private static byte[] GetSasraLogoBytes()
    {
        // Use a placeholder image for testing
        return Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");

        // TODO: Replace with your actual logo
        // return File.ReadAllBytes("path/to/logo.png");
        // OR
        // return Convert.FromBase64String("YOUR_BASE64_STRING");
    }
}
}
