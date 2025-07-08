using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.ComponentModel.DataAnnotations;

namespace Returns.Helpers
{
    public class ReturnSubmissionService : IReturnSubmissionService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<ReturnSubmissionService> _logger;
        private readonly IExcelImportService _excelImportService;
        private readonly IFormProcessorFactory _processorFactory;

        public ReturnSubmissionService(
            ReturnsDbContext context,
            ILogger<ReturnSubmissionService> logger,
            IExcelImportService excelImportService,
            IFormProcessorFactory processorFactory)
        {
            _context = context;
            _logger = logger;
            _excelImportService = excelImportService;
            _processorFactory = processorFactory;
        }

        public async Task<SubmissionResultDto> ProcessFormSubmissionAsync(
            IFormFile file, 
            string returnId, 
            string formType,
            string userId)
        {
            _logger.LogInformation("Processing form submission for Return: {ReturnId}, Form: {FormType}", returnId, formType);

            try
            {
                // Create the submission record first
                var submission = new ReturnSubmission
                {
                    ReturnId = returnId,
                    FormType = formType,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    Status = SubmissionStatus.Processing.ToString(),
                    SubmittedBy = userId,
                    SubmittedAt = DateTime.Now
                };

                _context.ReturnSubmissions.Add(submission);
                await _context.SaveChangesAsync();

                // Process the form using the new architecture
                var result = await _excelImportService.ImportFormAsync(file, formType, returnId);
                
                // Update submission with results
                submission.Status = result.Status.ToString();
                submission.ProcessedAt = result.ProcessedAt;
                
                if (result.Status == SubmissionStatus.Success)
                {
                    submission.IsLatest = true;
                    // Mark previous submissions as not latest
                    await _context.ReturnSubmissions
                        .Where(s => s.ReturnId == returnId && 
                                   s.FormType == formType && 
                                   s.Id != submission.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsLatest, false));
                }
                else if (result.Status == SubmissionStatus.Failed || result.Status == SubmissionStatus.ValidationError)
                {
                    submission.ErrorMessages = string.Join("; ", result.Messages);
                }

                await _context.SaveChangesAsync();

                // Add submission ID to result
                result.SubmissionId = submission.Id.ToString();
                
                return result;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error in form submission");
                return new SubmissionResultDto
                {
                    FormType = formType,
                    FormFileName = file.FileName,
                    Status = SubmissionStatus.ValidationError,
                    Messages = new List<string> { vex.Message }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing form submission");
                return new SubmissionResultDto
                {
                    FormType = formType,
                    FormFileName = file.FileName,
                    Status = SubmissionStatus.Failed,
                    Messages = new List<string> { "An error occurred while processing the form." }
                };
            }
        }

        public async Task<List<ReturnSubmissionDto>> GetSubmissionsAsync(string returnId)
        {
            return await _context.ReturnSubmissions
                .Where(s => s.ReturnId == returnId)
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new ReturnSubmissionDto
                {
                    Id = s.Id,
                    FormType = s.FormType,
                    FileName = s.FileName,
                    FileSize = s.FileSize,
                    Status = s.Status,
                    IsLatest = s.IsLatest,
                    SubmittedBy = s.SubmittedBy,
                    SubmittedAt = s.SubmittedAt,
                    ProcessedAt = s.ProcessedAt,
                    ErrorMessages = s.ErrorMessages
                })
                .ToListAsync();
        }

        public async Task<SubmissionStatusDto> GetReturnStatusAsync(string returnId)
        {
            var submissions = await _context.ReturnSubmissions
                .Where(s => s.ReturnId == returnId && s.IsLatest)
                .Select(s => new { s.FormType, s.Status })
                .ToListAsync();

            var requiredForms = GetRequiredFormsForReturn(returnId);
            var statusDto = new SubmissionStatusDto
            {
                ReturnId = returnId,
                TotalFormsRequired = requiredForms.Count,
                FormsSubmitted = submissions.Count,
                FormsCompleted = submissions.Count(s => s.Status == SubmissionStatus.Success.ToString()),
                FormStatuses = new Dictionary<string, string>()
            };

            foreach (var form in requiredForms)
            {
                var submission = submissions.FirstOrDefault(s => s.FormType == form);
                statusDto.FormStatuses[form] = submission?.Status ?? "Not Submitted";
            }

            statusDto.OverallStatus = statusDto.FormsCompleted == statusDto.TotalFormsRequired 
                ? "Complete" 
                : statusDto.FormsSubmitted > 0 
                    ? "In Progress" 
                    : "Not Started";

            return statusDto;
        }

        private List<string> GetRequiredFormsForReturn(string returnId)
        {
            // This should be configured based on your business rules
            // For now, returning a standard set
            return new List<string>
            {
                "CAPITAL_ADEQUACY",
                "LIQUIDITY_STATEMENT",
                "STATEMENT_OF_FINANCIAL_POSITION",
                "INCOME_STATEMENT",
                "CASH_FLOW_STATEMENT"
            };
        }
    }

    // DTOs
    public class ReturnSubmissionDto
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string Status { get; set; }
        public bool IsLatest { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string ErrorMessages { get; set; }
    }

    public class SubmissionStatusDto
    {
        public string ReturnId { get; set; }
        public int TotalFormsRequired { get; set; }
        public int FormsSubmitted { get; set; }
        public int FormsCompleted { get; set; }
        public string OverallStatus { get; set; }
        public Dictionary<string, string> FormStatuses { get; set; }
    }
}
