
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
        private static readonly DeviceRgb SASRA_PRIMARY_COLOR = new DeviceRgb(13, 91, 146); // #0d5b92 - Main blue
        private static readonly DeviceRgb SASRA_SECONDARY_COLOR = new DeviceRgb(214, 164, 39); // #D6A427 - Gold accent (from logo)
        private static readonly DeviceRgb SASRA_LIGHT_GRAY = new DeviceRgb(229, 231, 235); // #e5e7eb - Light gray for backgrounds
        private static readonly DeviceRgb SASRA_DARK_GRAY = new DeviceRgb(102, 102, 102); // #666666 - Dark gray for text
        private static readonly DeviceRgb SASRA_ERROR_COLOR = new DeviceRgb(220, 53, 69); // #dc3545 - Red for errors/negative values
        private static readonly DeviceRgb SASRA_SUCCESS_COLOR = new DeviceRgb(21, 87, 36); // #155724 - Green for success/positive indicators



        public static byte[] GenerateConsistencyPdfReport(List<ValidationError> validationErrors, string period)
        {
        using var ms = new MemoryStream();

        PdfWriter writer = new PdfWriter(ms);
        PdfDocument pdf = new PdfDocument(writer);
        Document doc = new Document(pdf, PageSize.A4);
        doc.SetMargins(36, 36, 36, 36);

        PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        DeviceRgb sasraGold = new DeviceRgb(211, 158, 11);   // #D39E0B
        DeviceRgb sasraNavy = new DeviceRgb(13, 59, 102);   // #0D3B66
        DeviceRgb sasraGrey = new DeviceRgb(102, 102, 102);   // #666666
        DeviceRgb errorRed = new DeviceRgb(153, 0, 0);   // dark red

        Style bigTitleStyle = new Style().SetFont(boldFont).SetFontColor(sasraNavy)
                                              .SetFontSize(18)
                                              .SetTextAlignment(TextAlignment.CENTER);

        Style labelStyle = new Style().SetFont(boldFont).SetFontSize(12);
        Style valueStyle = new Style().SetFont(regularFont).SetFontSize(12);

        Style sectionTitleStyle = new Style().SetFont(boldFont).SetFontColor(sasraNavy)
                                              .SetFontSize(13);

        Style errorCategoryStyle = new Style().SetFont(boldFont).SetFontColor(errorRed)
                                              .SetFontSize(12);


        try
        {
            ImageData logoData = ImageDataFactory.Create(GetSasraLogoBytes());
            Image logo = new Image(logoData).SetHeight(60).SetAutoScale(true);

            Table hdr = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER);

            hdr.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                                  .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                  .Add(logo));

            Paragraph orgName = new Paragraph("SACCO Societies Regulatory Authority (SASRA)")
                                    .SetFont(boldFont)
                                    .SetFontColor(sasraNavy)
                                    .SetFontSize(16)
                                    .SetMarginBottom(3);

            Paragraph tagline = new Paragraph("Securing SACCO Funds")
                                    .SetFont(regularFont)
                                    .SetFontColor(sasraGrey)
                                    .SetFontSize(11)
                                    .SetMarginBottom(6);

            Paragraph contacts = new Paragraph(
                "UAP Old Mutual Tower, 19ᵗʰ Floor, Upper Hill Road, Nairobi, Kenya\n" +
                "P.O. Box 25089 – 00100 Nairobi  |  Tel: +254 (20) 293 5100/101  |  Toll-Free: 0800 724 422\n" +
                "Email: info@sasra.go.ke  |  www.sasra.go.ke")
                .SetFont(regularFont)
                .SetFontSize(9)
                .SetFontColor(sasraGrey);

            hdr.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                                  .Add(orgName)
                                  .Add(tagline)
                                  .Add(contacts));
            doc.Add(hdr);
        }
        catch
        {
            // fallback if logo fails
            doc.Add(new Paragraph("SACCO Societies Regulatory Authority (SASRA)")
                    .SetFont(boldFont)
                    .SetFontColor(sasraNavy)
                    .SetFontSize(16));
        }

        // thin gold rule
        doc.Add(new LineSeparator(new SolidLine())
                   .SetStrokeColor(sasraGold)
                   .SetMarginTop(5)
                   .SetMarginBottom(12));

        // ---------------------------------------------------------------------
        // Page title
        // ---------------------------------------------------------------------
        doc.Add(new Paragraph("Consistency Report").AddStyle(bigTitleStyle).SetMarginBottom(20));

        // ---------------------------------------------------------------------
        // Report metadata
        // ---------------------------------------------------------------------
        doc.Add(new Paragraph("Report Details")
                .SetFont(boldFont)
                .SetFontColor(sasraNavy)
                .SetFontSize(14)
                .SetMarginBottom(10));

        Table metaTbl = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
            .SetWidth(UnitValue.CreatePercentValue(100));

        void AddMetaRow(string label, string value, bool colourise = false)
        {
            metaTbl.AddCell(new Cell().Add(new Paragraph(label).AddStyle(labelStyle))
                                      .SetBorder(Border.NO_BORDER));

            Paragraph valPara = new Paragraph(value).AddStyle(valueStyle);
            if (colourise)
                valPara.SetFontColor(value == "VALID" ? sasraNavy : errorRed)
                       .SetFont(boldFont);

            metaTbl.AddCell(new Cell().Add(valPara).SetBorder(Border.NO_BORDER));
        }

        AddMetaRow("Reporting Period", period);
        AddMetaRow("Validation Status", validationErrors.Any() ? "INVALID" : "VALID", colourise: true);
        AddMetaRow("Errors Found", validationErrors.Count.ToString());

        doc.Add(metaTbl);

        // ---------------------------------------------------------------------
        // Validation errors
        // ---------------------------------------------------------------------
        if (validationErrors.Any())
        {
            doc.Add(new Paragraph("Validation Errors")
                        .AddStyle(sectionTitleStyle)
                        .SetMarginTop(20));

            foreach (var err in validationErrors)
            {
                Div errDiv = new Div()
                    .SetMarginTop(12)
                    .SetPadding(10)
                    .SetBackgroundColor(new DeviceRgb(248, 249, 250))   // light grey
                    .SetBorderLeft(new SolidBorder(errorRed, 4));

                errDiv.Add(new Paragraph(err.Category).AddStyle(errorCategoryStyle));
                errDiv.Add(new Paragraph(err.Description).SetMarginTop(5));

                Table detTbl = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginTop(8);

                detTbl.AddHeaderCell(new Cell().Add(new Paragraph("Field")
                                                    .SetFont(boldFont)
                                                    .SetFontColor(ColorConstants.WHITE))
                                               .SetBackgroundColor(sasraGold)
                                               .SetPadding(5));
                detTbl.AddHeaderCell(new Cell().Add(new Paragraph("Value")
                                                    .SetFont(boldFont)
                                                    .SetFontColor(ColorConstants.WHITE))
                                               .SetBackgroundColor(sasraGold)
                                               .SetPadding(5));

                foreach (var d in err.Details)
                {
                    detTbl.AddCell(new Cell().Add(new Paragraph(d.Key).SetFont(regularFont)).SetPadding(4));
                    detTbl.AddCell(new Cell().Add(new Paragraph(d.Value).SetFont(regularFont)).SetPadding(4));
                }

                errDiv.Add(detTbl);
                doc.Add(errDiv);
            }
        }

        // ---------------------------------------------------------------------
        // Footer
        // ---------------------------------------------------------------------
        doc.Add(new LineSeparator(new SolidLine())
                    .SetStrokeColor(sasraGold)
                    .SetMarginTop(28)
                    .SetMarginBottom(8));

        doc.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                .SetFont(regularFont)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(9)
                .SetFontColor(sasraGrey));

        doc.Close();
        return ms.ToArray();
    }



        public static byte[] GenerateSaccoPerformancePdfReport(SaccoPerformanceReportDTO report, string components)
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
                        AddReportHeader(document, SASRA_PRIMARY_COLOR, boldFont);

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
                        GenerateReportTable(tableForPage, report, periodsForPage, components, boldFont, regularFont, SASRA_LIGHT_GRAY, normalRowBgColor);

                        document.Add(tableForPage);

                        AddApprovalComments(document, report, boldFont,
                          PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE),
                          regularFont);

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
                    AddReportHeader(document, SASRA_PRIMARY_COLOR, boldFont);

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

                    GenerateReportTable(mainTable, report, report.Periods, components, boldFont, regularFont, SASRA_LIGHT_GRAY, normalRowBgColor);

                    document.Add(mainTable);
                    AddApprovalComments(document, report, boldFont,
                          PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE),
                          regularFont);

                    // Add footer
                    AddReportFooter(document);
                }

                // Close the document
                document.Close();

                return ms.ToArray();
            }
        }

        private static void AddReportHeader(Document document, DeviceRgb headerColor, PdfFont boldFont)
        {
            // local supporting colours & regular font
            DeviceRgb sasraNavy = new DeviceRgb(13, 59, 102);   // #0D3B66
            DeviceRgb sasraGrey = new DeviceRgb(102, 102, 102); // #666666
            PdfFont regular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            try
            {
                // ─── Logo ─────────────────────────────────────────────────────────
                ImageData logoData = ImageDataFactory.Create(GetSasraLogoBytes());
                Image logo = new Image(logoData)
                                        .SetHeight(60)
                                        .SetAutoScale(true);

                // ─── Right-hand block (org name, tagline, contacts) ───────────────
                Paragraph orgName = new Paragraph("SACCO Societies Regulatory Authority (SASRA)")
                                        .SetFont(boldFont)
                                        .SetFontColor(ColorConstants.BLACK)
                                        .SetFontSize(16)
                                        .SetMarginBottom(3);

                Paragraph tagline = new Paragraph("Securing SACCO Funds")
                                        .SetFont(regular)
                                        .SetFontColor(ColorConstants.BLACK)
                                        .SetFontSize(11)
                                        .SetMarginBottom(6);

                Paragraph contacts = new Paragraph(
                        "UAP Old Mutual Tower, 19ᵗʰ Floor, Upper Hill Road, Nairobi, Kenya\n" +
                        "P.O. Box 25089 – 00100 Nairobi  |  Tel: +254 (20) 293 5100/101  |  Toll-Free: 0800 724 422\n" +
                        "Email: info@sasra.go.ke  |  www.sasra.go.ke")
                    .SetFont(regular)
                    .SetFontColor(ColorConstants.BLACK)
                    .SetFontSize(9);

                // ─── Two-column table layout ──────────────────────────────────────
                Table hdr = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4 }))
                                .SetWidth(UnitValue.CreatePercentValue(100))
                                .SetBorder(Border.NO_BORDER);

                hdr.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(logo));

                hdr.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .Add(orgName)
                    .Add(tagline)
                    .Add(contacts));

                // wrap table in coloured div for padding
                Div headerDiv = new Div()
                    //.SetBackgroundColor(sasraGrey)     
                    .SetPadding(16)
                    .Add(hdr);

                document.Add(headerDiv);

                // thin separator below header
                document.Add(new LineSeparator(new SolidLine())
                                 .SetStrokeColor(headerColor)
                                 .SetMarginTop(6)
                                 .SetMarginBottom(12));
            }
            catch
            {
                document.Add(
                    new Div()
                        .SetBackgroundColor(headerColor)
                        .SetPadding(18)
                        .Add(new Paragraph("SACCO Societies Regulatory Authority (SASRA)")
                                 .SetFont(boldFont)
                                 .SetFontColor(ColorConstants.WHITE)
                                 .SetFontSize(16)
                                 .SetTextAlignment(TextAlignment.CENTER))
                );
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
                .SetFontColor(SASRA_DARK_GRAY); // #666666

            document.Add(footer);
        }

        private static void AddApprovalComments(Document document, SaccoPerformanceReportDTO report,
         PdfFont boldFont, PdfFont italicFont, PdfFont regularFont)
        {
            // Only add the section if there are approval comments
            if (report.ApprovalActions != null && report.ApprovalActions.Count > 0)
            {
                document.Add(new Paragraph().SetMarginTop(30));

                // Section header
                Paragraph approvalHeader = new Paragraph("Approval Comments")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetFontColor(SASRA_PRIMARY_COLOR)
                    .SetMarginBottom(10);
                document.Add(approvalHeader);

                // Create a table for approval comments
                Table commentsTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 3, 1.5f }))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                // Add table headers
                Cell[] headerCells = new Cell[]
                {
                    new Cell().Add(new Paragraph("Date").SetFont(boldFont)),
                    new Cell().Add(new Paragraph("User").SetFont(boldFont)),
                    new Cell().Add(new Paragraph("Comment").SetFont(boldFont)),
                    new Cell().Add(new Paragraph("Action").SetFont(boldFont))
                };

                foreach (var cell in headerCells)
                {
                    cell.SetBackgroundColor(SASRA_LIGHT_GRAY)
                        .SetPadding(8)
                        .SetTextAlignment(TextAlignment.CENTER);
                    commentsTable.AddHeaderCell(cell);
                }

                // Add comments to table
                foreach (var comment in report.ApprovalActions)
                {
                    // Date cell
                    commentsTable.AddCell(new Cell()
                        .Add(new Paragraph(comment.CreatedAt.ToString("dd-MM-yyyy HH:mm"))
                        .SetFont(regularFont))
                        .SetPadding(8));

                    

                    // Step/RoleName cell
                    commentsTable.AddCell(new Cell()
                        .Add(new Paragraph(comment.UserName)
                        .SetFont(regularFont))
                        .SetPadding(8));

                    // Comment cell
                    commentsTable.AddCell(new Cell()
                        .Add(new Paragraph(comment.Comment)
                        .SetFont(regularFont))
                        .SetPadding(8));

                    // Status cell with appropriate color
                    var statusCell = new Cell()
                        .Add(new Paragraph(comment.Status)
                        .SetFont(boldFont))
                        .SetPadding(8)
                        .SetTextAlignment(TextAlignment.CENTER);

                    // Set status cell color based on status
                    if (comment.Status.ToUpper() == "APPROVED")
                    {
                        statusCell.SetFontColor(SASRA_SUCCESS_COLOR);
                    }
                    else if (comment.Status.ToUpper() == "REJECTED")
                    {
                        statusCell.SetFontColor(SASRA_ERROR_COLOR);
                    }

                    commentsTable.AddCell(statusCell);
                }

                document.Add(commentsTable);
            }
        }

        // Helper method to generate table for a given set of periods
        /// <summary>
        /// Builds a dynamic CAMELS table (or any subset, e.g. “CAEL”, “CAMEL”)
        /// for the generic SaccoPerformanceReportDTO.
        /// </summary>
        private static void GenerateReportTable(
            Table table,
            SaccoPerformanceReportDTO report,
            List<SaccoPerformanceReportDTO.PeriodData> periods,
            string components,                              // NEW – e.g. "CAMELS", "CAEL"
            PdfFont boldFont,
            PdfFont regularFont,
            DeviceRgb sectionBgColor,
            DeviceRgb normalRowBgColor)
        {
            /* ---------- static header row ---------- */
            table.AddCell(new Cell().SetBackgroundColor(sectionBgColor)
                                    .SetPadding(10)
                                    .SetBorder(Border.NO_BORDER));

            table.AddCell(new Cell().Add(new Paragraph("Prudential Standard").SetFont(boldFont))
                                    .SetBackgroundColor(sectionBgColor)
                                    .SetPadding(10)
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .SetBorder(Border.NO_BORDER));

            foreach (var p in periods)
            {
                table.AddCell(new Cell().Add(new Paragraph(p.PeriodLabel).SetFont(boldFont))
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
                    report.PrudentialStandards.TryGetValue("CoreCapitalToTotalAssets", out var ccta) ? ccta : "N/A",
                    periods.Select(p => p.CoreCapitalToTotalAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Institutional Capital/Total Assets",
                    report.PrudentialStandards.TryGetValue("InstitutionalCapitalToTotalAssets", out var icta) ? icta : "N/A",
                    periods.Select(p => p.InstitutionalCapitalToTotalAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== A – ASSET QUALITY ===== */
            if (components.Contains('A'))
            {
                AddSectionHeader(table, "B. ASSET QUALITY", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "NPL (Substandard+Doubtful+Loss)",
                    report.PrudentialStandards.TryGetValue("NPL", out var npl) ? npl : "N/A",
                    periods.Select(p => p.NonPerformingLoans).ToList(),
                    FormatType.Currency, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Non-Earning Assets",
                    report.PrudentialStandards.TryGetValue("NonEarningAssets", out var nea) ? nea : "N/A",
                    periods.Select(p => p.NonEarningAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Equity investments/Deposits",
                    "<5%", periods.Select(p => p.EquityInvestmentsToDeposits * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Equity investments/Core capital",
                    "<40%", periods.Select(p => p.EquityInvestmentsToCoreCapital * 100).ToList(),
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
                    periods.Select(p => p.TotalExpenseToTotalIncome * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Net Income/Average Assets (ROA)", "N/A",
                    periods.Select(p => p.NetIncomeToAverageAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Operating Expenses/Financial Income (OPEX)", "N/A",
                    periods.Select(p => p.OpertatingExpenseToFinancialOpex * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== L – LIQUIDITY ===== */
            if (components.Contains('L'))
            {
                AddSectionHeader(table, "E. LIQUIDITY", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "Liquid Assets/Short-term Liabilities", "N/A",
                    periods.Select(p => p.LiquidAssetsToShortTermLiabilities * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "External Borrowing/Total Assets", "N/A",
                    periods.Select(p => p.ExternalBorrowingToTotalAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Liquid Assets/Total Assets", "N/A",
                    periods.Select(p => p.LiquidAssetsToTotalAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== S – STRUCTURE / SENSITIVITY ===== */
            if (components.Contains('S'))
            {
                AddSectionHeader(table, "F. STRUCTURE / SENSITIVITY TO RISK", 2 + periods.Count, boldFont, sectionBgColor);

                AddMetricRow(table, "Gross loans/Total Assets", "N/A",
                    periods.Select(p => p.GrossLoansToTotalAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Gross loans/Deposits", "N/A",
                    periods.Select(p => p.GrossLoansToDeposits * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Financial Investments/Total Assets", "N/A",
                    periods.Select(p => p.FinancialInvestmentsToTotalAssets * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);

                AddMetricRow(table, "Dividends+Interest on Deposits/Total Income", "N/A",
                    periods.Select(p => p.DividendsAndInterestOnDepositsToTotalIncome * 100).ToList(),
                    FormatType.Percentage, boldFont, regularFont, normalRowBgColor);
            }

            /* ===== RAW FINANCIAL STATISTICS (always) ===== */
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
                    valuePara.SetFontColor(SASRA_ERROR_COLOR);
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
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Images", "sasraimage.jpg");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Logo not found at {path}");
            }

            return File.ReadAllBytes(path);
        }



        public static string GenerateHtmlReport(
            List<ValidationError> validationErrors,
            string period)
        {
            var sb = new StringBuilder();

            var logoBytes = GetSasraLogoBytes();
            var logoBase64 = Convert.ToBase64String(logoBytes);
            var logoDataUri = $"data:image/jpeg;base64,{logoBase64}";
            const string sasraNavy = "#0D3B66";
            const string sasraGold = "#D39E0B";
            const string sasraGrey = "#666666";
            const string errorRed = "#990000";
            const string lightGrey = "#f8f9fa";

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset='UTF-8'>");
            sb.AppendLine("  <title>SASRA – Consistency Report</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: Helvetica, Arial, sans-serif; color: " + sasraGrey + "; margin:0; padding:0; }");
            sb.AppendLine("    .header { background-color: " + sasraNavy + "; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine("    .logo { height: 60px; vertical-align: middle; }");
            sb.AppendLine("    .org-info { display: inline-block; text-align: left; margin-left: 15px; vertical-align: middle; }");
            sb.AppendLine("    .org-info h2 { margin: 0; font-size: 18px; }");
            sb.AppendLine("    .org-info p { margin: 2px 0; font-size: 12px; color: " + lightGrey + "; }");
            sb.AppendLine("    .gold-rule { border: none; height: 3px; background-color: " + sasraGold + "; margin: 0; }");
            sb.AppendLine("    .container { padding: 20px 40px; }");
            sb.AppendLine("    h1 { color: " + sasraNavy + "; font-size: 20px; margin-top: 10px; }");
            sb.AppendLine("    .report-meta { margin-bottom: 25px; }");
            sb.AppendLine("    .report-meta p { margin: 4px 0; }");
            sb.AppendLine("    .validation-status { font-weight: bold; }");
            sb.AppendLine("    .status-valid   { color: " + sasraNavy + "; }");
            sb.AppendLine("    .status-invalid { color: " + errorRed + "; }");
            sb.AppendLine("    .error-section { margin-top: 30px; }");
            sb.AppendLine("    .error-section h2 { color: " + sasraNavy + "; font-size: 16px; }");
            sb.AppendLine("    .error { background-color: " + lightGrey + "; border-left: 4px solid " + errorRed + "; padding: 10px 15px; margin-bottom: 20px; }");
            sb.AppendLine("    .error-category { color: " + errorRed + "; font-weight: bold; font-size: 1.1em; }");
            sb.AppendLine("    .error-desc     { margin: 8px 0; }");
            sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine("    th { background-color: " + sasraGold + "; color: white; text-align: left; padding: 8px; }");
            sb.AppendLine("    td { padding: 8px; border-bottom: 1px solid #ddd; }");
            sb.AppendLine("    tr:nth-child(even) { background-color: #f2f2f2; }");
            sb.AppendLine("    .footer { margin-top: 40px; border-top: 1px solid #ddd; padding-top: 15px; font-size: 0.85em; color: " + sasraGrey + "; text-align:center; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header with logo + org name/tagline
            sb.AppendLine("  <div class='header'>");
            sb.AppendLine($"    <img src='{logoDataUri}' class='logo' alt='SASRA Logo' />");
            sb.AppendLine("    <div class='org-info'>");
            sb.AppendLine("      <h2>SACCO Societies Regulatory Authority</h2>");
            sb.AppendLine("      <p>Securing SACCO Funds</p>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("  <hr class='gold-rule' />");

            // Body
            sb.AppendLine("  <div class='container'>");
            sb.AppendLine("    <h1>Consistency Report</h1>");
            sb.AppendLine("    <div class='report-meta'>");
            sb.AppendLine($"      <p><strong>Reporting Period:</strong> {period}</p>");
            sb.AppendLine($"      <p><strong>Validation Status:</strong> " +
                $"<span class='validation-status {(validationErrors.Any() ? "status-invalid" : "status-valid")}'>{(validationErrors.Any() ? "INVALID" : "VALID")}</span></p>");
            sb.AppendLine($"      <p><strong>Errors Found:</strong> {validationErrors.Count}</p>");
            sb.AppendLine("    </div>");

            // Errors table
            if (validationErrors.Any())
            {
                sb.AppendLine("    <div class='error-section'>");
                sb.AppendLine("      <h2>Validation Errors</h2>");

                foreach (var err in validationErrors)
                {
                    sb.AppendLine("      <div class='error'>");
                    sb.AppendLine($"        <div class='error-category'>{err.Category}</div>");
                    sb.AppendLine($"        <div class='error-desc'>{err.Description}</div>");
                    sb.AppendLine("        <table>");
                    sb.AppendLine("          <thead><tr><th>Field</th><th>Value</th></tr></thead>");
                    sb.AppendLine("          <tbody>");
                    foreach (var d in err.Details)
                    {
                        sb.AppendLine("            <tr>");
                        sb.AppendLine($"              <td>{d.Key}</td>");
                        sb.AppendLine($"              <td>{d.Value}</td>");
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
            sb.AppendLine("      <p>UAP Old Mutual Tower, 19th Floor, Upper Hill Road, Nairobi, Kenya | P.O. Box 25089 – 00100 Nairobi</p>");
            sb.AppendLine("      <p>Tel: +254 (20) 293 5100/101  |  Toll-Free: 0800 724 422  |  Email: info@sasra.go.ke  |  www.sasra.go.ke</p>");
            sb.AppendLine($"      <p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("    </div>");

            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }


    }
}
