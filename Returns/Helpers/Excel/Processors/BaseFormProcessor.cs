using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers.Excel.Processors
{
    public abstract class BaseFormProcessor<TConfig, TStatement> : IFormProcessor 
        where TConfig : IFormConfiguration<TStatement>, new()
        where TStatement : BaseFormStatement, new()
    {
        protected readonly ReturnsDbContext _context;
        protected readonly ILogger _logger;
        protected readonly IExcelImportService _excelService;
        protected readonly TConfig _configuration;

        public abstract string FormType { get; }

        protected BaseFormProcessor(
            ReturnsDbContext context,
            ILogger logger,
            IExcelImportService excelService)
        {
            _context = context;
            _logger = logger;
            _excelService = excelService;
            _configuration = new TConfig();
        }

        public virtual async Task<FormProcessResult> ProcessAsync(IFormFile file, string returnId)
        {
            var result = new FormProcessResult
            {
                Success = false
            };

            try
            {
                _logger.LogInformation("Processing {FormType} for return {ReturnId}", FormType, returnId);

                // Parse the Excel file
                var statement = await _excelService.ParseFormAsync(file, _configuration);
                
                // Validate the parsed data
                var validationErrors = await ValidateStatementAsync(statement, returnId);
                if (validationErrors.Any())
                {
                    result.Messages.AddRange(validationErrors);
                    return result;
                }

                // Save the file
                var filePath = await FormsHelper.SaveFileAsync(file, FormType);
                if (string.IsNullOrEmpty(filePath))
                {
                    result.Messages.Add("Failed to save file");
                    return result;
                }

                // Get form details
                var form = await GetFormByTypeAsync(FormType);
                if (form == null)
                {
                    result.Messages.Add($"Form type {FormType} not found in database");
                    return result;
                }

                // Save the data
                await SaveStatementAsync(statement, returnId, filePath, form);

                result.Success = true;
                result.Data = statement;
                result.FormId = form.Id;
                result.Messages.Add($"Successfully processed {FormType}");

                _logger.LogInformation("Successfully processed {FormType} for return {ReturnId}", FormType, returnId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {FormType} for return {ReturnId}", FormType, returnId);
                result.Messages.Add($"Error processing form: {ex.Message}");
            }

            return result;
        }

        protected virtual async Task<List<string>> ValidateStatementAsync(TStatement statement, string returnId)
        {
            var errors = new List<string>();

            // Basic validation
            if (string.IsNullOrEmpty(statement.SaccoCsNumber))
            {
                errors.Add("SACCO CS Number is required");
            }

            if (statement.EndDate <= statement.StartDate)
            {
                errors.Add("End date must be after start date");
            }

            // Check for duplicate submission
            var exists = await CheckDuplicateSubmissionAsync(returnId, statement.EndDate);
            if (exists)
            {
                errors.Add($"A submission for this period already exists");
            }

            return errors;
        }

        protected abstract Task SaveStatementAsync(TStatement statement, string returnId, string filePath, ReturnForm form);

        protected abstract Task<bool> CheckDuplicateSubmissionAsync(string returnId, DateTime endDate);

        protected async Task<ReturnForm> GetFormByTypeAsync(string formType)
        {
            // This is a simplified lookup - you might want to use a more sophisticated mapping
            return await _context.ReturnForms
                .FirstOrDefaultAsync(f => f.Code == formType || f.FormName.Contains(formType));
        }

        protected int CalculateDaysLate(ReturnForm form, DateTime submissionDate)
        {
            // Simplified calculation - you would use your actual business logic
            var dueDate = form.Period.Name switch
            {
                "Monthly" => new DateTime(submissionDate.Year, submissionDate.Month, 15),
                "Quarterly" => new DateTime(submissionDate.Year, ((submissionDate.Month - 1) / 3 + 1) * 3, 15),
                _ => submissionDate.AddDays(15)
            };

            return Math.Max(0, (DateTime.Now - dueDate).Days);
        }
    }
}