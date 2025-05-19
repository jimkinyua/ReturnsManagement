namespace Returns.Helpers.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailAsyncWithCC(string to, string subject, string body, IEnumerable<string> ccAddresses);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentName);
    }
}
