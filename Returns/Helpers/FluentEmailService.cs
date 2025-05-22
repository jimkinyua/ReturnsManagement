using FluentEmail.Core;
using Returns.Helpers.Interfaces;
using System.Net.Mail;
using System.Net.Mime;

namespace Returns.Helpers
{
    public class FluentEmailService : IEmailService
    {
        private readonly IFluentEmailFactory _factory;

        public FluentEmailService(IFluentEmailFactory factory)
        {
            _factory = factory;
        }

        public Task SendEmailAsync(string to, string subject, string body) =>
            _factory
                .Create()
                .To(to)
                .Subject(subject)
                .Body(body, isHtml: true)
                .SendAsync();

        public Task SendEmailAsyncWithCC( string to,string subject,string body,IEnumerable<string> ccAddresses)
        {
            var email = _factory
                .Create()
                .To(to)
                .Subject(subject)
                .Body(body, isHtml: true);

            if (ccAddresses != null)
            {
                foreach (var cc in ccAddresses
                                 .Where(a => !string.IsNullOrWhiteSpace(a)))
                {
                    email = email.CC(cc.Trim());
                }
            }

            return email.SendAsync();
        }


        public Task SendEmailWithAttachmentAsync(string to,string subject,string body,byte[] attachment,string attachmentName)
        {
            var ms = new MemoryStream(attachment);

            var email = _factory
                .Create()
                .To(to)
                .Subject(subject)
                .Body(body, isHtml: true)
                .Attach( new FluentEmail.Core.Models.Attachment
                {
                    Data = ms,
                    Filename = attachmentName,
                    ContentType = MediaTypeNames.Application.Pdf
                });

            return email.SendAsync();
        }

    }
}
