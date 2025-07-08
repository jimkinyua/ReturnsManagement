using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Returns.Helpers.Excel
{
    public class ExcelImportService : IExcelImportService
    {
        private readonly ILogger<ExcelImportService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFormProcessorFactory _processorFactory;

        public ExcelImportService(
            ILogger<ExcelImportService> logger,
            IServiceProvider serviceProvider,
            IFormProcessorFactory processorFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _processorFactory = processorFactory;
        }

        public async Task<SubmissionResultDto> ImportFormAsync(IFormFile file, string formType, string returnId)
        {
            var result = new SubmissionResultDto
            {
                FormType = formType,
                Status = SubmissionStatus.Processing
            };

            try
            {
                // Get the processor for this form type
                var processor = _processorFactory.GetProcessor(formType);
                if (processor == null)
                {
                    throw new NotSupportedException($"No processor found for form type: {formType}");
                }

                // Process the form
                var processResult = await processor.ProcessAsync(file, returnId);
                
                result.Status = processResult.Success ? SubmissionStatus.Success : SubmissionStatus.Failed;
                result.Messages = processResult.Messages;
                result.Data = processResult.Data;
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error processing form {FormType}", formType);
                result.Status = SubmissionStatus.Failed;
                result.Messages.Add(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing form {FormType}", formType);
                result.Status = SubmissionStatus.Failed;
                result.Messages.Add($"Error processing form: {ex.Message}");
            }

            return result;
        }

        public async Task<T> ParseFormAsync<T>(IFormFile file, IFormConfiguration<T> configuration) where T : class, new()
        {
            // Validate file
            ValidateFile(file);

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            try
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = GetWorksheet(workbook, configuration.SheetName);

                // Create result object
                var result = new T();

                // Read metadata if T inherits from BaseFormStatement
                if (result is BaseFormStatement formStatement)
                {
                    ReadFormMetadata(worksheet, formStatement, configuration.MetadataLocations);
                }

                // Read data rows
                if (result is IFormWithRows<object> formWithRows)
                {
                    var rows = ReadDataRows(worksheet, configuration);
                    formWithRows.SetRows(rows);
                }

                return result;
            }
            catch (FileFormatException ex)
            {
                throw new ValidationException(
                    $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                    "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                    "Please save the sheet in .xlsx format and upload again.", ex);
            }
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file), "No file was provided for processing");
            }

            if (file.Length == 0)
            {
                throw new ArgumentException("The uploaded file is empty", nameof(file));
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                throw new ValidationException(
                    $"'{file.FileName}' is an *.xls* (Excel 97-2003) file. " +
                    "The system only accepts *.xlsx* workbooks (Excel 2007 or later). " +
                    "Please save the sheet in .xlsx format and upload again.");
            }
        }

        private IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName)
        {
            if (string.IsNullOrEmpty(sheetName))
            {
                return workbook.Worksheets.First();
            }

            var worksheet = workbook.Worksheets.FirstOrDefault(ws => 
                ws.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

            if (worksheet == null)
            {
                _logger.LogWarning("Worksheet '{SheetName}' not found, using first sheet", sheetName);
                return workbook.Worksheets.First();
            }

            return worksheet;
        }

        private void ReadFormMetadata(IXLWorksheet worksheet, BaseFormStatement form, FormMetadataLocation locations)
        {
            form.SaccoCsNumber = GetCellValueOrEmpty(worksheet.Cell(locations.SaccoCsNumberCell));
            form.Period = GetCellValueOrEmpty(worksheet.Cell(locations.PeriodCell));
            form.StartDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell(locations.StartDateCell))) ?? DateTime.Now;
            form.EndDate = ParseDateOrNull(GetCellValueOrEmpty(worksheet.Cell(locations.EndDateCell))) ?? DateTime.Now;
        }

        private List<object> ReadDataRows<T>(IXLWorksheet worksheet, IFormConfiguration<T> configuration) where T : class, new()
        {
            var rows = new List<object>();
            
            for (int rowNum = configuration.DataStartRow; rowNum <= configuration.DataEndRow; rowNum++)
            {
                var row = worksheet.Row(rowNum);
                
                if (configuration.RowMapper.ShouldSkipRow(row))
                    continue;

                try
                {
                    var mappedRow = configuration.RowMapper.MapRow(row, rowNum);
                    if (mappedRow != null)
                    {
                        rows.Add(mappedRow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error mapping row {RowNumber}", rowNum);
                    // Could collect errors here for reporting
                }
            }

            return rows;
        }

        // Helper methods
        public static string GetCellValueOrEmpty(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return string.Empty;

            return cell.GetString().Trim();
        }

        public static decimal? GetDecimalOrNull(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return null;

            var value = cell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(value) || value == "-")
                return null;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return null;
        }

        public static int? GetIntOrNull(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return null;

            if (int.TryParse(cell.GetString().Trim(), out int result))
                return result;

            return null;
        }

        public static DateTime? ParseDateOrNull(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParse(dateString, out DateTime result))
                return result;

            string[] formats = {
                "dd/MM/yyyy", "d/M/yyyy",
                "dd-MM-yyyy", "d-M-yyyy",
                "dd.MM.yyyy", "d.M.yyyy",
                "MM/dd/yyyy", "M/d/yyyy",
                "yyyy/MM/dd", "yyyy-MM-dd",
                "dd MMM yyyy", "dd MMMM yyyy",
                "MMM dd yyyy", "MMMM dd yyyy"
            };

            if (DateTime.TryParseExact(dateString, formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate))
            {
                return parsedDate;
            }

            return null;
        }
    }

    // Interface for forms that contain rows
    public interface IFormWithRows<T>
    {
        void SetRows(List<object> rows);
    }
}