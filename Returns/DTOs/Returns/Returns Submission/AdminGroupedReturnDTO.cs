using System;
using System.Collections.Generic;

namespace Returns.DTOs.Returns_Submission
{
    public class AdminGroupedReturnDTO
    {
        public string GroupId { get; set; } = null!; // RatingDefinitionId
        public string RatingName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string PeriodName { get; set; } = null!;
        public int Year { get; set; }
        public string Frequency { get; set; } = null!; // Monthly, Quarterly, etc.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public bool IsComplete { get; set; } // All required forms submitted
        public int TotalRequiredForms { get; set; }
        public int SubmittedForms { get; set; }
        public List<GroupedReturnFormDTO> Forms { get; set; } = new List<GroupedReturnFormDTO>();
    }

    public class GroupedReturnFormDTO
    {
        public string FormId { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string SubmissionId { get; set; } = null!;
        public DateTime? SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public bool IsSubmitted { get; set; }
        public bool IsLate { get; set; }
        public int DaysLate { get; set; }
    }

    public class AdminReturnFilterDTO
    {
        public int? Year { get; set; }
        public int? Month { get; set; } // 1-12
        public string? SaccoType { get; set; }
        public string? Frequency { get; set; } // Monthly, Quarterly, etc.
        public string? PeriodId { get; set; }
        public string? RatingDefinitionId { get; set; }
        public bool? IsComplete { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}