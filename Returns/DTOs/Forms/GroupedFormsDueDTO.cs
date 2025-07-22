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
      /*  public int FiledCount { get; set; }
        public int DueCount { get; set; }
        public int LateCount { get; set; }*/
        public List<FormsDueByMonthDTO> Forms { get; set; } = new List<FormsDueByMonthDTO>();

        // Bulk submission guidance
        public bool CanBulkSubmit => Forms.All(f => f.SubmissionStatus == SubmissionStatusEnum.Draft) && Forms.Any();
        //public bool CanBulkSubmitNotSubmitted => Forms.All(f => f.SubmissionStatus == SubmissionStatusEnum.NotSubmitted) && Forms.Any();
        public int DraftCount => Forms.Count(f => f.SubmissionStatus == SubmissionStatusEnum.Draft);
        //public int NotSubmittedCount => Forms.Count(f => f.SubmissionStatus == SubmissionStatusEnum.NotSubmitted);
   /*     public int SubmittedCount => Forms.Count(f => f.SubmissionStatus == SubmissionStatusEnum.Submitted);
        public string BulkActionType => CanBulkSubmit ? "Submit All Drafts" :
                                      CanBulkSubmitNotSubmitted ? "Submit All New" :
                                      "Mixed Status";*/
    }

    public class FormsDueGroupedResponseDTO
    {
        public int TotalExpectedReturns { get; set; }   
        public List<GroupedFormsDueDTO> Frequency { get; set; } = new List<GroupedFormsDueDTO>();
    }
}