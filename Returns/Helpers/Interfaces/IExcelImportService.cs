using Returns.DTOs.Forms;
using Returns.Models;

namespace Returns.Helpers.Interfaces
{
    public interface IExcelImportService
    {
        Task<T> ImportFormDataAsync<T>(IFormFile file, ReturnForm formMetadata) where T : class, new();
        Task<object> ImportFormDataAsync(IFormFile file, ReturnForm formMetadata);
    }

    public interface IFormData
    {
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        string Period { get; set; }
        string SaccoCsNumber { get; set; }
        bool IsValid { get; }
        List<string> ValidationErrors { get; }
    }

    public interface IReturnChild
    {
        string Id { get; set; }
        string ReturnId { get; set; }
        string FormId { get; set; }
        DateTime CreatedAt { get; set; }
        bool RequiresResubmission { get; set; }
    }
}