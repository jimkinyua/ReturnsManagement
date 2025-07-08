using Returns.DTOs.Returns.Returns_Submission;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Returns.Helpers.Enums;

namespace Returns.Helpers.Interfaces
{
    public interface IExcelParser
    {
        Task<ExcelParseResult> ParseAsync(IFormFile file, FormCategory formCategory, string SaccoType);
    }

    public class ExcelParseResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<IParsedRow> Rows { get; set; } = new List<IParsedRow>();
        public string FormType { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public interface IParsedRow
    {
        string ReturnSubmissionId { get; set; }
        object ToEntity();
    }
}