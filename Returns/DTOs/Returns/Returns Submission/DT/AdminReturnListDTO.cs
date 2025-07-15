using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class AdminReturnListDTO
    {
        public List<SubmittedReturnDTO> Returns { get; set; } = new List<SubmittedReturnDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        
        // Summary statistics
        public int TotalReturns { get; set; }
        public int TotalLateReturns { get; set; }
        public int TotalConsistentReturns { get; set; }
        public int TotalInconsistentReturns { get; set; }
    }
}