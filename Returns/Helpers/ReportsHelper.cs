
using iText.Bouncycastleconnector;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Perfomance_Report;
using Returns.Models.Data;
using System.Globalization;
using System.Text;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers
{
    public static class ReportsHelper
    {
        public static byte[] GenerateConsistencyPdfReport(List<ValidationError> validationErrors, string period)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Create PDF document
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf, PageSize.A4);

                // Set document properties
                document.SetMargins(36, 36, 36, 36);

                // Define fonts and styles - using the correct Bold approach
                Style headerStyle = new Style()
                    .SetFontColor(ColorConstants.WHITE)
                    .SetBackgroundColor(new DeviceRgb(13, 91, 146)) // #0d5b92
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPaddingTop(20)
                    .SetPaddingBottom(20);

                Style titleStyle = new Style()
                    .SetFontColor(new DeviceRgb(13, 91, 146)) // #0d5b92
                    .SetFontSize(16);

                Style sectionTitleStyle = new Style()
                    .SetFontSize(14);

                Style errorCategoryStyle = new Style()
                    .SetFontColor(new DeviceRgb(220, 53, 69)) // #dc3545
                    .SetFontSize(12);

                Table metaTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                // Header with logo and title
                try
                {
                    // Replace with actual path to logo or embedded resource
                    ImageData logoData = ImageDataFactory.Create(GetSasraLogoBytes());
                    Image logo = new Image(logoData).SetHeight(60);

                    Paragraph headerPara = new Paragraph("Consistency Report")
                        .AddStyle(headerStyle);

                    Div headerDiv = new Div()
                        .SetBackgroundColor(new DeviceRgb(13, 91, 146))
                        .SetPadding(20)
                        .Add(new Paragraph().Add(logo).SetTextAlignment(TextAlignment.CENTER))
                        .Add(headerPara);

                    document.Add(headerDiv);
                }
                catch (Exception ex)
                {
                    // If logo loading fails, just add the text header
                    Paragraph headerPara = new Paragraph("Consistency Report")
                        .AddStyle(headerStyle);

                    Div headerDiv = new Div()
                        .SetBackgroundColor(new DeviceRgb(13, 91, 146))
                        .SetPadding(20)
                        .Add(headerPara);

                    document.Add(headerDiv);
                }

                // Report metadata
                document.Add(new Paragraph().SetMarginTop(20));
                document.Add(new Paragraph("Report Details").AddStyle(titleStyle).SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline());

                metaTable.AddCell(new Cell().Add(new Paragraph("Reporting Period:").SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline())).SetBorder(Border.NO_BORDER);
                metaTable.AddCell(new Cell().Add(new Paragraph(period)).SetBorder(Border.NO_BORDER));

                metaTable.AddCell(new Cell().Add(new Paragraph("Validation Status:").SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline())).SetBorder(Border.NO_BORDER);
                metaTable.AddCell(new Cell().Add(new Paragraph(validationErrors.Count() > 0 ? "INVALID" : "VALID")
                    .SetFontColor(validationErrors.Count() > 0 ? new DeviceRgb(114, 28, 36) : new DeviceRgb(21, 87, 36)))
                    .SetBorder(Border.NO_BORDER));


                // Replace .SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline()) with the appropriate method to simulate bold text
                metaTable.AddCell(new Cell().Add(new Paragraph("Errors Found:").SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline()).SetBorder(Border.NO_BORDER));
                metaTable.AddCell(new Cell().Add(new Paragraph("Errors Found:").SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline()).SetBorder(Border.NO_BORDER));
                metaTable.AddCell(new Cell().Add(new Paragraph(validationErrors.Count().ToString())).SetBorder(Border.NO_BORDER));

                document.Add(metaTable);

                // Error details section
                if (validationErrors.Count() > 0)
                {
                    document.Add(new Paragraph("Validation Errors").AddStyle(sectionTitleStyle).SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline().SetMarginTop(20));

                    foreach (var error in validationErrors)
                    {
                        Div errorDiv = new Div()
                            .SetMarginTop(15)
                            .SetMarginBottom(15)
                            .SetPadding(10)
                            .SetBackgroundColor(new DeviceRgb(248, 249, 250)) // #f8f9fa
                            .SetBorderLeft(new SolidBorder(new DeviceRgb(220, 53, 69), 4)); // #dc3545

                        errorDiv.Add(new Paragraph(error.Category).AddStyle(errorCategoryStyle).SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline());
                        errorDiv.Add(new Paragraph(error.Description).SetMarginTop(5));

                        // Create table for error details
                        Table detailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
                            .SetWidth(UnitValue.CreatePercentValue(100))
                            .SetMarginTop(10);

                        Cell headerCell1 = new Cell()
                            .Add(new Paragraph("Field").SetFontColor(ColorConstants.WHITE).SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline())
                            .SetBackgroundColor(new DeviceRgb(13, 91, 146))
                            .SetPadding(5);

                        Cell headerCell2 = new Cell()
                            .Add(new Paragraph("Value").SetFontColor(ColorConstants.WHITE).SetFontColor(ColorConstants.BLACK).SetFontSize(12).SetUnderline())
                            .SetBackgroundColor(new DeviceRgb(13, 91, 146))
                            .SetPadding(5);

                        detailsTable.AddHeaderCell(headerCell1);
                        detailsTable.AddHeaderCell(headerCell2);

                        foreach (var detail in error.Details)
                        {
                            detailsTable.AddCell(new Cell().Add(new Paragraph(detail.Key)).SetPadding(5));
                            detailsTable.AddCell(new Cell().Add(new Paragraph(detail.Value)).SetPadding(5));
                        }

                        errorDiv.Add(detailsTable);
                        document.Add(errorDiv);
                    }
                }

                // Add footer
                Paragraph footer = new Paragraph("SASRA - SACCO Societies Regulatory Authority\n" +
                                                 "Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(9)
                    .SetFontColor(new DeviceRgb(102, 102, 102)); // #666666

                document.Add(new LineSeparator(new SolidLine(1f))
                    .SetMarginTop(30)
                    .SetMarginBottom(10));
                document.Add(footer);

                // Close the document
                document.Close();

                return ms.ToArray();
            }
        }


        public static byte[] GenerateSaccoPerformancePdfReport(SaccoPerformanceReportDTO report)
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
                        AddReportHeader(document, headerColor, boldFont);

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
                        GenerateReportTable(tableForPage, report, periodsForPage, boldFont, regularFont, sectionBgColor, normalRowBgColor);

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
                    AddReportHeader(document, headerColor, boldFont);

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

                    GenerateReportTable(mainTable, report, report.Periods, boldFont, regularFont, sectionBgColor, normalRowBgColor);

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
        private static void AddReportHeader(Document document, DeviceRgb headerColor, PdfFont boldFont)
        {
            try
            {
                // Replace with actual path to logo or embedded resource
                ImageData logoData = ImageDataFactory.Create(GetSasraLogoBytes());
                Image logo = new Image(logoData).SetHeight(60);

                Paragraph headerPara = new Paragraph("SASRA - SACCO Performance Report")
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
                Paragraph headerPara = new Paragraph("SASRA - SACCO Performance Report")
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
        private static void GenerateReportTable(Table table, SaccoPerformanceReportDTO report, List<SaccoPerformanceReportDTO.PeriodData> periods,
            PdfFont boldFont, PdfFont regularFont, DeviceRgb sectionBgColor, DeviceRgb normalRowBgColor)
        {
            // Add table header row
            Cell emptyCell = new Cell(1, 1)
                .SetBackgroundColor(sectionBgColor)
                .SetPadding(10)
                .SetBorder(Border.NO_BORDER);
            table.AddCell(emptyCell);

            Cell standardHeader = new Cell(1, 1)
                .Add(new Paragraph("Prudential Standard").SetFont(boldFont))
                .SetBackgroundColor(sectionBgColor)
                .SetPadding(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER);
            table.AddCell(standardHeader);

            // Add period headers for only the periods we're including on this page
            foreach (var period in periods)
            {
                Cell periodHeader = new Cell(1, 1)
                    .Add(new Paragraph(period.PeriodLabel).SetFont(boldFont))
                    .SetBackgroundColor(sectionBgColor)
                    .SetPadding(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER);
                table.AddCell(periodHeader);
            }

            // Add section: CAPITAL ADEQUACY
            AddSectionHeader(table, "CAPITAL ADEQUACY", 2 + periods.Count, boldFont, sectionBgColor);

            // Add metrics for CAPITAL ADEQUACY
            AddMetricRow(table, "Core Capital",
                report.PrudentialStandards.ContainsKey("CoreCapital") ? report.PrudentialStandards["CoreCapital"] : "N/A",
                periods.Select(p => p.CoreCapital).ToList(),
                FormatType.Currency,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Core Capital/Total Assets",
                report.PrudentialStandards.ContainsKey("CoreCapitalToTotalAssets") ? report.PrudentialStandards["CoreCapitalToTotalAssets"] : "N/A",
                periods.Select(p => p.CoreCapitalToTotalAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Institutional Capital/Total Assets",
                report.PrudentialStandards.ContainsKey("InstitutionalCapitalToTotalAssets") ? report.PrudentialStandards["InstitutionalCapitalToTotalAssets"] : "N/A",
                periods.Select(p => p.InstitutionalCapitalToTotalAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            // Add section: ASSET QUALITY
            AddSectionHeader(table, "B. ASSET QUALITY", 2 + periods.Count, boldFont, sectionBgColor);

            // Add metrics for ASSET QUALITY
            AddMetricRow(table, "NPL\n(Substandard+Doubtful+Loss)",
                report.PrudentialStandards.ContainsKey("NPL") ? report.PrudentialStandards["NPL"] : "N/A",
                periods.Select(p => p.NonPerformingLoans).ToList(),
                FormatType.Currency,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Non-Earning Assets",
                report.PrudentialStandards.ContainsKey("NonEarningAssets") ? report.PrudentialStandards["NonEarningAssets"] : "N/A",
                periods.Select(p => p.NonEarningAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            // Add section: INVESTMENTS
            AddMetricRow(table, "Equity investments/Deposits",
                "<5%",
                periods.Select(p => p.EquityInvestmentsToDeposits * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Equity investments/Core capital",
                "<40%",
                periods.Select(p => p.EquityInvestmentsToCoreCapital * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            // Add section: EARNINGS RATING
            AddSectionHeader(table, "C. EARNINGS RATING", 2 + periods.Count, boldFont, sectionBgColor);

            // Add metrics for EARNINGS RATING
            AddMetricRow(table, "Yield on Gross Loans",
                "N/A",
                periods.Select(p => p.YieldOnGrossLoans * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Total Expense/Total Income",
                "N/A",
                periods.Select(p => p.TotalExpenseToTotalIncome * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Net Income/Average Assets (ROA)",
                "N/A",
                periods.Select(p => p.NetIncomeToAverageAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Operating Expenses/Financial Income (OPEX)",
                "N/A",
                periods.Select(p => p.OpertatingExpenseToFinancialOpex * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            // Add section: LIQUIDITY
            AddSectionHeader(table, "D. LIQUIDITY", 2 + periods.Count, boldFont, sectionBgColor);

            // Add metrics for LIQUIDITY
            AddMetricRow(table, "Liquid Assets/Short-term Liabilities",
                "N/A",
                periods.Select(p => p.LiquidAssetsToShortTermLiabilities * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "External Borrowing/Total Assets",
                "N/A",
                periods.Select(p => p.ExternalBorrowingToTotalAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Liquid Assets/Total Assets",
                "N/A",
                periods.Select(p => p.LiquidAssetsToTotalAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            // Add section: STRUCTURE/SENSITIVITY TO RISK
            AddSectionHeader(table, "E. STRUCTURE/SENSITIVITY TO RISK", 2 + periods.Count, boldFont, sectionBgColor);

            // Add metrics for STRUCTURE/SENSITIVITY TO RISK
            AddMetricRow(table, "Gross loans/Total Assets",
                "N/A",
                periods.Select(p => p.GrossLoansToTotalAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Gross loans/Deposits",
                "N/A",
                periods.Select(p => p.GrossLoansToDeposits * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Financial Investments/Total Assets",
                "N/A",
                periods.Select(p => p.FinancialInvestmentsToTotalAssets * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Dividends+Interest on Deposits/Total Income",
                "N/A",
                periods.Select(p => p.DividendsAndInterestOnDepositsToTotalIncome * 100).ToList(),  // Convert to percentage
                FormatType.Percentage,
                boldFont, regularFont, normalRowBgColor);

            // Add section: KEY FINANCIAL STATISTICS
            AddSectionHeader(table, "F. KEY FINANCIAL STATISTICS", 2 + periods.Count, boldFont, sectionBgColor);

            // Add raw data rows
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

            AddMetricRow(table, "Institutional Capital", "N/A",
                periods.Select(p => p.InstitutionalCapital).ToList(),
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
                periods.Select(p => p.EquityInvestmentsInNACCOS).ToList(),
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

            AddMetricRow(table, "Dividends+Interest on Deposits", "N/A",
                periods.Select(p => p.DividendsAndInterestOnDeposits).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Operating Expenses", "N/A",
                periods.Select(p => p.OperatingExpenses).ToList(),
                FormatType.Currency, boldFont, regularFont, normalRowBgColor);

            AddMetricRow(table, "Interest on Loan portfolio+Fees&Commission on Loan", "N/A",
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

        
    }
}
