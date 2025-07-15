using System;
using System.Collections.Generic;

namespace Returns.DTOs.Returns_Submission
{
    public class AdminGroupedReturnDTO
    {
        public string GroupId { get; set; } = null!; // RatingDefinitionId or "standalone" for ungrouped
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
        public int ReturnCount { get; set; } // Count of returns in this group
        public List<GroupedReturnFormDTO> Forms { get; set; } = new List<GroupedReturnFormDTO>();
        public ReturnGroupType GroupType { get; set; } = ReturnGroupType.Grouped; // New property
    }

    public enum ReturnGroupType
    {
        Grouped,    // Returns that need to be read together (e.g., CAMELS)
        Standalone  // Individual returns that don't need grouping
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
        private int? _year;
        private int? _month;

        public int? Year 
        { 
            get => _year ?? DateTime.Now.Year;
            set => _year = value;
        }
        
        public int? Month 
        { 
            get => _month ?? DateTime.Now.Month;
            set => _month = value;
        }
        
        public string? SaccoType { get; set; }
        public string? Frequency { get; set; } // Monthly, Quarterly, etc.
        public string? PeriodId { get; set; }
        public string? RatingDefinitionId { get; set; }
        public bool? IsComplete { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}