using Returns.Helpers.Enums;

namespace Returns.DTOs.Forms
{
    public class GroupedFormsDueDTO
    {
        public string FrequencyCode { get; set; } = null!; // MTH, QTR, FY, etc.
        public string FrequencyName { get; set; } = null!; // Monthly, Quarterly, etc.
        public int TotalFormsInGroup { get; set; }
        public int FiledCount { get; set; }
        public int DueCount { get; set; }
        public int LateCount { get; set; }
        public int WaivedCount { get; set; }
        public List<FormsDueByMonthDTO> Forms { get; set; } = new List<FormsDueByMonthDTO>();
        
        // Group-level submission properties
        public bool CanSubmitAll => Forms.Any() && Forms.All(f => f.CanSubmit);
        public int ReadyToSubmitCount => Forms.Count(f => f.CanSubmit);
        public int DraftCount => Forms.Count(f => f.CanContinue);
        public int LateDraftCount => Forms.Count(f => f.CanRequestEdit);
        public bool HasAnyDrafts => DraftCount > 0 || LateDraftCount > 0;
    }

    public class FormsDueGroupedResponseDTO
    {
        public int TotalExpectedReturns { get; set; }
        public int TotalFiled { get; set; }
        public int TotalDue { get; set; }
        public int TotalLate { get; set; }
        public int TotalWaived { get; set; }
        public List<GroupedFormsDueDTO> GroupedByFrequency { get; set; } = new List<GroupedFormsDueDTO>();
        
        // Summary for group-level actions
        public int TotalGroupsWithSubmitAll => GroupedByFrequency.Count(g => g.CanSubmitAll);
        public int TotalGroupsWithDrafts => GroupedByFrequency.Count(g => g.HasAnyDrafts);
    }
}