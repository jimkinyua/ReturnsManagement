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
        
        // Bulk submission guidance
        public bool CanBulkSubmit => Forms.All(f => f.CanBulkSubmit) && Forms.Any();
        public int DraftCount => Forms.Count(f => f.SubmissionStatus == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.Draft);
        public int NotSubmittedCount => Forms.Count(f => f.SubmissionStatus == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.NotSubmitted);
        public int SubmittedCount => Forms.Count(f => f.SubmissionStatus == Returns.DTOs.Returns.Returns_Submission.SubmissionStatus.Submitted);
    }

    public class FormsDueGroupedResponseDTO
    {
        public int TotalExpectedReturns { get; set; }
        public int TotalFiled { get; set; }
        public int TotalDue { get; set; }
        public int TotalLate { get; set; }
        public int TotalWaived { get; set; }
        public List<GroupedFormsDueDTO> GroupedByFrequency { get; set; } = new List<GroupedFormsDueDTO>();
    }
}