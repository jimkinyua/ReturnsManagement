using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Models;
using Returns.Models.Data;
using System.Runtime.Intrinsics.X86;
using static Returns.Helpers.Constants;
using static Returns.Helpers.TokenHelper;
using Returns.Interfaces;
using Returns.DTOs.Returns.Returns_Submission;

namespace Returns.Helpers
{
    public class FormProcessingService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger _logger;
        private readonly IExcelImportService _excelService;
        private readonly IFormProcessorFactory _processorFactory;

        public FormProcessingService(
            ReturnsDbContext context, 
            ILogger logger,
            IExcelImportService excelService,
            IFormProcessorFactory processorFactory)
        {
            _context = context;
            _logger = logger;
            _excelService = excelService;
            _processorFactory = processorFactory;
        }

        public async Task<bool> ShouldConsistencyChecksBeDone(ReturnsDbContext dbContext, NewReturnDTO submissionDto)
        {
            // 1. collect all non-empty FormIds that the user submitted
            var formIds = submissionDto.FormUploads
                .Select(u => u.FormId)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            // no forms uploaded → skip consistency check
            if (!formIds.Any())
                return false;

            // 2. fetch only those ReturnForm records
            var forms = await dbContext.ReturnForms
                .Where(f => formIds.Contains(f.Id))
                .ToListAsync();

            // 3. Check if all required forms are present
            bool hasAllRequiredForms = true;
            forms.Any(f => f.IsCapitalAdequencyForm) &&
            forms.Any(f => f.IsLiquidityStatement) &&
            forms.Any(f => f.IsRiskClassification) &&
            forms.Any(f => f.IsInvestmentReturn) &&
            forms.Any(f => f.IsFinancialPosition) &&
            forms.Any(f => f.IsStatementOfComprehensiveIncome) &&
            forms.Any(f => f.IsDepositReturnForm);

            return hasAllRequiredForms;
        }

        public async Task<(bool Success, string ReturnId, List<string> ProcessingSummary)> ProcessFormBatchAsync(
            NewReturnDTO batchDTO, 
            LoggedInEntity loggedInSacco, 
            bool isConsistent, 
            List<string> consistencyErrors, 
            string periodToUse)
        {
            var processingSummary = new List<string>();
            string effectiveReturnId = string.Empty;

            try
            {
                // Create new return
                var newReturn = CreateNewReturn(
                    loggedInSacco,
                    isConsistent,
                    consistencyErrors,
                    batchDTO.SubmissionDate,
                    periodToUse);

                await _context.Returns.AddAsync(newReturn);
                await _context.SaveChangesAsync();
                effectiveReturnId = newReturn.Id;

                // Process each form in the batch
                foreach (var upload in batchDTO.FormUploads)
                {
                    if (upload.formFile == null) continue;

                    var form = await _context.ReturnForms
                        .Include(x => x.Period)
                        .FirstOrDefaultAsync(f => f.Id == upload.FormId);

                    if (form == null)
                    {
                        processingSummary.Add($"Form with ID {upload.FormId} was not found.");
                        continue;
                    }

                    // Process using new architecture
                    var result = await ProcessFormAsync(upload.formFile, form, effectiveReturnId, loggedInSacco.SaccoType);
                    
                    if (result.Status == SubmissionStatus.Success)
                    {
                        processingSummary.Add($"Successfully processed '{upload.formFile.FileName}'.");
                    }
                    else
                    {
                        processingSummary.Add($"File '{upload.formFile.FileName}' was not processed: {string.Join(", ", result.Messages)}");
                    }
                }

                return (true, effectiveReturnId, processingSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing form batch");
                processingSummary.Add($"Error processing batch: {ex.Message}");
                return (false, string.Empty, processingSummary);
            }
        }

        private Return CreateNewReturn(LoggedInEntity sacco, bool isConsistent, List<string> errors, DateTime submissionDate, string periodToUse)
        {
            return new Return
            {
                SaccoId = sacco.SaccoId,
                IsNotConsistent = isConsistent,
                ConsistentErrorMessage = string.Join(", ", errors),
                SaccoType = sacco.SaccoType,
                SaccoName = sacco.SaccoName,
                SubmittedAt = DateTime.Now,
                ReturnFor = submissionDate,
                Year = periodToUse,
                VersionNumber = 1,
                IsActiveVersion = true,
            };
        }

        public async Task<SubmissionResultDto> ProcessFormAsync(IFormFile formFile, ReturnForm form, string returnId, string saccoType)
        {
            try
            {
                // Determine form type
                string formType = GetFormTypeFromForm(form);
                if (formType == null)
                {
                    return new SubmissionResultDto
                    {
                        Status = SubmissionStatus.Failed,
                        Messages = { "Unknown form type" }
                    };
                }

                // Process using the new architecture
                return await _excelService.ImportFormAsync(formFile, formType, returnId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing form '{FileName}'", formFile.FileName);
                return new SubmissionResultDto
                {
                    Status = SubmissionStatus.Failed,
                    Messages = { $"Error processing '{formFile.FileName}': {ex.Message}" }
                };
            }
        }

        // Helper method to determine form type
        public string GetFormTypeFromForm(ReturnForm form)
        {
            if (form.IsCapitalAdequencyForm) return "CAPITAL_ADEQUACY";
            if (form.IsLiquidityStatement) return "LIQUIDITY_STATEMENT";
            if (form.IsDepositReturnForm) return "DEPOSIT_RETURN";
            if (form.IsRiskClassification) return "RISK_CLASSIFICATION";
            if (form.IsInvestmentReturn) return "INVESTMENT";
            if (form.IsFinancialPosition) return "FINANCIAL_POSITION";
            if (form.IsSectoralLending) return "SECTORAL_LENDING";
            if (form.IsDailyLiquidity) return "DAILY_LIQUIDITY";
            if (form.IsInsiderLending) return "INSIDER_LENDING";
            if (form.IsManagement) return "MANAGEMENT";
            if (form.IsStatementOfComprehensiveIncome) return "COMPREHENSIVE_INCOME";
            return null;
        }

        // Keep other existing methods like ExtractReportingEndDate, etc.
        // ... existing code ...
    }
}
