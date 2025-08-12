using System;
using System.Collections.Generic;

namespace Returns.DTOs.Returns_Submission
{
    public class AdminGroupedReturnDTO
    {
        public string GroupId { get; set; } = null!; // RatingDefinitionId or "standalone" for ungrouped
        public string SaccoType { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string PeriodName { get; set; } = null!;
        public int Year { get; set; }
        public string Frequency { get; set; } = null!; // Monthly, Quarterly, etc.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public bool IsComplete { get; set; } // All required forms submitted
        public int SubmittedForms { get; set; }
        public List<GroupedReturnFormDTO> Forms { get; set; } = new List<GroupedReturnFormDTO>();
        public ReturnGroupType GroupType { get; set; } = ReturnGroupType.Grouped; // New property

        // Lateness information for admin visibility
        public bool HasLateSubmissions { get; set; } // True if any form in the group was submitted late
        public int TotalDaysLate { get; set; } // Total days late across all forms
        public int LateFormsCount { get; set; } // Number of forms that were submitted late
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

    public class SaccoReturnFilterDTO
    {
        private int? _year;
        private int? _month;
        private bool _hasAnyFilter = false;

        /// <summary>
        /// Filter by the year when returns were DUE (period end date), not when they were submitted
        /// </summary>
        public int? Year
        {
            get => _hasAnyFilter ? _year : (_year ?? DateTime.Now.Year);
            set
            {
                _year = value;
                _hasAnyFilter = true;
            }
        }

        /// <summary>
        /// Filter by the month when returns were DUE (period end date), not when they were submitted
        /// </summary>
        public int? Month
        {
            get;// _hasAnyFilter ? _month : (_month ?? DateTime.Now.Month);
            set;
            /*{
                *//*_month = value;
                _hasAnyFilter = true;*//*
            }*/
        }

        public string? SaccoType
        {
            get;
            set;
        }

        public string? Frequency { get; set; } // Monthly, Quarterly, etc.
        public string? PeriodId { get; set; }
        public string? RatingDefinitionId { get; set; }
        public bool? IsComplete { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Constructor to set default values only when no filters are provided
        public SaccoReturnFilterDTO()
        {
            // Only set defaults if no other filters are provided
            if (string.IsNullOrEmpty(SaccoType) &&
                string.IsNullOrEmpty(Frequency) &&
                string.IsNullOrEmpty(PeriodId) &&
                string.IsNullOrEmpty(RatingDefinitionId) &&
                !IsComplete.HasValue)
            {
                _year = DateTime.Now.Year;
                _month = DateTime.Now.Month;
            }
        }
    }


    public class AdminReturnFilterDTO
    {
        private int? _year;
        private int? _month;
        private bool _hasAnyFilter = false;

        /// <summary>
        /// Filter by the year when returns were DUE (period end date), not when they were submitted
        /// </summary>
        public int? Year
        {
            get => _hasAnyFilter ? _year : (_year ?? DateTime.Now.Year);
            set
            {
                _year = value;
                _hasAnyFilter = true;
            }
        }

        /// <summary>
        /// Filter by the month when returns were DUE (period end date), not when they were submitted
        /// </summary>
        public int? Month
        {
            get;// _hasAnyFilter ? _month : (_month ?? DateTime.Now.Month);
            set;
            /*{
                *//*_month = value;
                _hasAnyFilter = true;*//*
            }*/
        }

        public string? SaccoType
        {
            get;
            set;
        }

        public string? Frequency { get; set; } // Monthly, Quarterly, etc.
        public string? PeriodId { get; set; }
        public string? RatingDefinitionId { get; set; }
        public bool? IsComplete { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Constructor to set default values only when no filters are provided
        public AdminReturnFilterDTO()
        {
            // Only set defaults if no other filters are provided
            if (string.IsNullOrEmpty(SaccoType) &&
                string.IsNullOrEmpty(Frequency) &&
                string.IsNullOrEmpty(PeriodId) &&
                string.IsNullOrEmpty(RatingDefinitionId) &&
                !IsComplete.HasValue)
            {
                _year = DateTime.Now.Year;
                _month = DateTime.Now.Month;
            }
        }
    }
}