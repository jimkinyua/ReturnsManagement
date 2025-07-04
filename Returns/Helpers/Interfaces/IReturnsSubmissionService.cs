using Returns.DTOs.Returns;
using Returns.Models;

namespace Returns.Helpers.Interfaces
{
    public interface IReturnsSubmissionService
    {
        // Get expected returns for a SACCO with calculated status
        Task<List<ExpectedReturnDto>> GetExpectedReturnsAsync(string saccoId, string? periodId = null);
        
        // Get returns that are due or late
        Task<List<ExpectedReturnDto>> GetDueReturnsAsync(string saccoId);
        
        // Get expected returns for a specific month/year
        Task<List<ExpectedReturnDto>> GetExpectedReturnsByMonthAsync(string saccoId, int year, int month);
        
        // Get expected returns for a specific year
        Task<List<ExpectedReturnDto>> GetExpectedReturnsByYearAsync(string saccoId, int year);
        
        // Check if a SACCO can file returns for a specific period
        Task<ReturnFilingEligibility> CheckFilingEligibilityAsync(string saccoId, string periodId, List<string> formIds);
        
        // Submit returns for a period
        Task<ReturnSubmissionResult> SubmitReturnsAsync(NewReturnSubmissionDto submission);
        
        // Get submitted returns history
        Task<List<SubmittedReturnSummaryDto>> GetSubmittedReturnsAsync(string saccoId, int? year = null);
        
        // Waive a return requirement
        Task<bool> WaiveReturnAsync(string expectedReturnId, string reason, string waivedBy);
    }
    
    public class ReturnFilingEligibility
    {
        public bool CanFile { get; set; }
        public string? ReasonIfCannot { get; set; }
        public List<string> MissingForms { get; set; } = new();
        public List<string> AlreadyFiledForms { get; set; } = new();
        public string PeriodName { get; set; } = "";
        public DateTime DueDate { get; set; }
    }
    
    public class ReturnSubmissionResult
    {
        public bool Success { get; set; }
        public string? ReturnId { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, string> ProcessedForms { get; set; } = new(); // FormId -> Status
    }
}