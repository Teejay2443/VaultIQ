using Microsoft.Extensions.Options;
using Resend;
using System.Net.Mail;
using VaultIQ.Interfaces.Services;
using VaultIQ.Settings;

public class EmailServices : IEmailServices
{
    private readonly IResend _resend;
    private readonly ResendSettings _resendSettings;

    public EmailServices(IResend resend, IOptions<ResendSettings> settings)
    {
        _resend = resend;
        _resendSettings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var resp = await _resend.EmailSendAsync(new EmailMessage()
        {
            From = _resendSettings.From,
            To = to,
            Subject = subject,
            HtmlBody = body,
        });

        Console.WriteLine("✅ Email sent: " + resp.Content);
    }
}
