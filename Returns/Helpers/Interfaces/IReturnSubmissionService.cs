using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers.Interfaces
{
    public interface IReturnSubmissionService
    {
        Task SendSubmissionConfirmationEmailAsync(string saccoId, string periodId);
        Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto, LoggedInEntity loggedInSacco, Boolean IsAmendment=false, Boolean NeedFileToSave = true, string SavedFileUrl = "");
        Task<IList<SubmissionResultDto>> SubmitFinalAsync(string submissionId);
        Task<Dictionary<string, (SubmissionStatus Status, bool HasBeenSubmitted, string? SubmissionId, DateTime? SubmittedAt, string? FileUrl)>> GetSubmissionStatusesAsync(List<string> expectedReturnIds, string SaccoId, bool useCache = false);
        bool IsFormLate(DateTime filingDeadline, SubmissionStatus status);
        Task<BulkSubmissionResultDTO> BulkSubmitByPeriodAsync(string periodId, string saccoId, string saccoType);
    }
}
