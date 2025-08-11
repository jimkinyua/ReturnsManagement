using Returns.Helpers.Enums;
using Returns.DTOs.Returns.Returns_Submission;
using SubmissionStatusEnum = Returns.DTOs.Returns.Returns_Submission.SubmissionStatus;

namespace Returns.DTOs.Forms
{
    public class GroupedFormsDueDTO
    {
        public string FrequencyCode { get; set; } = null!; // MTH, QTR, FY, etc.
        public string FrequencyName { get; set; } = null!; // Monthly, Quarterly, etc.
        public int TotalFormsInGroup { get; set; }
        public string PeriodId { get; set; }
        public List<FormsDueByMonthDTO> Forms { get; set; } = new List<FormsDueByMonthDTO>();

        // Bulk submission guidance
        public int DraftCount => Forms.Count(f => f.SubmissionStatus == SubmissionStatusEnum.Draft.ToString());
        public bool IsIncomplete => TotalFormsInGroup != DraftCount;

    }

    public class FormsDueGroupedResponseDTO
    {
        public int TotalExpectedReturns { get; set; }   
        public List<GroupedFormsDueDTO> Frequency { get; set; } = new List<GroupedFormsDueDTO>();
    }
}