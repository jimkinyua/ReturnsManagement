using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class AdminReturnFilterDTO
    {
        public int? Month { get; set; } // Optional: filter by month (1-12)
        public int? Year { get; set; } // Optional: filter by year
        public string? SaccoId { get; set; } // Optional: filter by specific SACCO
        public string? SaccoType { get; set; } // Optional: filter by SACCO type (DT/NWDT)
        public string? Period { get; set; } // Optional: filter by period (Monthly/Quarterly)
        public int Page { get; set; } = 1; // Pagination
        public int PageSize { get; set; } = 20; // Items per page
        public string? SortBy { get; set; } = "SubmittedAt"; // Sort field
        public string? SortOrder { get; set; } = "desc"; // asc/desc
    }
}