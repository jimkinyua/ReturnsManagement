
using Microsoft.Extensions.Options;
using Returns.Helpers.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

namespace Returns.Helpers
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using (var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
            })
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.From),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Set to false if sending plain text
                };

                mailMessage.To.Add(to);
                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentName)
        {
            using (var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
            })
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.From),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                using (var stream = new MemoryStream(attachment))
                {
                    var attachmentItem = new Attachment(stream, attachmentName, MediaTypeNames.Application.Pdf);
                    mailMessage.Attachments.Add(attachmentItem);

                    await client.SendMailAsync(mailMessage);
                }
            }
        }
    }
}
