using Returns.DTOs.Forms;

namespace Returns.Helpers.Interfaces
{
    public interface IReturnFormAttachmentService
    {
        // Preview what will be created without saving
        Task<ReturnFormAttachmentPreviewDto> PreviewAttachmentAsync(AttachReturnFormsDto request);

        // Confirm and execute the attachment with transaction
        Task<ReturnFormAttachmentResultDto> ConfirmAttachmentAsync(ConfirmReturnFormAttachmentDto request);

        // Get current attachments for a period
        Task<List<AttachedReturnFormDto>> GetAttachedFormsAsync(string periodId);

        // Remove attachment (soft delete)
        Task<bool> RemoveAttachmentAsync(string expectedReturnId);

        // Bulk update filing deadlines
        Task<bool> UpdateFilingDeadlinesAsync(UpdateFilingDeadlinesDto request);
    }
}