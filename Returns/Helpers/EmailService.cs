using Microsoft.Extensions.Options;
using Returns.Helpers.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;

namespace Returns.Helpers;

/// <summary>
/// One long-lived SmtpClient + a Semaphore to guarantee
/// only one send is in flight – fixes the “4.3.2 concurrent connections” problem.
/// </summary>
public sealed class EmailService : IEmailService, IDisposable
{
    private readonly SmtpClient _client;
    private readonly SemaphoreSlim _gate = new(1, 1);       // serialize sends
    private bool _disposed;

    public EmailService(IOptions<EmailSettings> options)
    {
        var cfg = options.Value;

        _client = new SmtpClient(cfg.Host, cfg.Port)
        {
            EnableSsl = cfg.EnableSsl,
            Credentials = new NetworkCredential(cfg.UserName, cfg.Password)
        };

        // keep the socket alive for 2 minutes (optional)
        _client.ServicePoint!.MaxIdleTime = 120_000;
    }

    public Task SendEmailAsync(string to, string subject, string body) =>
        SendAsync(BuildBasicMessage(to, subject, body));

    public Task SendEmailAsyncWithCC(
        string to, string subject, string body, IEnumerable<string> cc)
    {
        var msg = BuildBasicMessage(to, subject, body);

        if (cc != null)
            foreach (var addr in cc.Where(a => !string.IsNullOrWhiteSpace(a)))
                msg.CC.Add(addr.Trim());

        return SendAsync(msg);
    }

    public Task SendEmailWithAttachmentAsync(
        string to, string subject, string body, byte[] attachment, string attachmentName)
    {
        var msg = BuildBasicMessage(to, subject, body);
        msg.Attachments.Add(
            new Attachment(new MemoryStream(attachment), attachmentName,
                           MediaTypeNames.Application.Pdf));

        return SendAsync(msg);
    }

    private MailMessage BuildBasicMessage(string to, string subject, string body)
    {
        var msg = new MailMessage
        {
            From = _client.Credentials is NetworkCredential c
                           ? new MailAddress(c.UserName)
                           : new MailAddress("no-reply@example.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        msg.To.Add(to);
        return msg;
    }

    private async Task SendAsync(MailMessage msg)
    {
        await _gate.WaitAsync();
        try { await _client.SendMailAsync(msg); }
        finally
        {
            _gate.Release();
            msg.Dispose();            // dispose after send
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _client.Dispose();
        _gate.Dispose();
        _disposed = true;
    }
}
