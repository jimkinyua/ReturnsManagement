using Microsoft.AspNetCore.Http;
using Returns.DTOs.Returns.Returns_Submission;

namespace Returns.Interfaces
{
    public interface IFormProcessor
    {
        string FormType { get; }
        Task<FormProcessResult> ProcessAsync(IFormFile file, string returnId);
    }

    public interface IFormProcessorFactory
    {
        IFormProcessor GetProcessor(string formType);
        void RegisterProcessor(string formType, IFormProcessor processor);
    }

    public class FormProcessResult
    {
        public bool Success { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public object Data { get; set; }
        public string FormId { get; set; }
    }
}