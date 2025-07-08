using Returns.DTOs.Returns.Returns_Submission;

namespace Returns.Interfaces
{
    public interface IExcelImportService
    {
        Task<SubmissionResultDto> ImportFormAsync(IFormFile file, string formType, string returnId);
        Task<T> ParseFormAsync<T>(IFormFile file, IFormConfiguration<T> configuration) where T : class, new();
    }

    public interface IFormConfiguration<T> where T : class, new()
    {
        string FormType { get; }
        string SheetName { get; }
        FormMetadataLocation MetadataLocations { get; }
        int DataStartRow { get; }
        int DataEndRow { get; }
        IRowMapper<T> RowMapper { get; }
        bool ValidateConsistency { get; }
    }

    public interface IRowMapper<T> where T : class, new()
    {
        T MapRow(IXLRow row, int rowNumber);
        bool ShouldSkipRow(IXLRow row);
    }

    public class FormMetadataLocation
    {
        public string SaccoCsNumberCell { get; set; } = "D3";
        public string PeriodCell { get; set; } = "D4";
        public string StartDateCell { get; set; } = "D5";
        public string EndDateCell { get; set; } = "D6";
    }
}