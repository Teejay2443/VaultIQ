//using Microsoft.Extensions.Options;
//using Resend;
//using System.Net.Mail;
//using VaultIQ.Interfaces.Services;
//using VaultIQ.Settings;

//public class EmailServices : IEmailServices
//{
//    private readonly IResend _resend;
//    private readonly ResendSettings _resendSettings;

//    public EmailServices(IResend resend, IOptions<ResendSettings> settings)
//    {
//        _resend = resend;
//        _resendSettings = settings.Value;
//    }

//    public async Task SendEmailAsync(string to, string subject, string body)
//    {
//        ResendResponse resp;
//        try
//        {
//            resp = await _resend.EmailSendAsync(new EmailMessage()
//            {
//                From = _resendSettings.From,
//                To = to,
//                Subject = subject,
//                HtmlBody = body,
//            });
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("✅ Email sent: " + ex.Message);
//        }
//    }
//}


//using System.Net;
//using System.Net.Mail;
//using Microsoft.Extensions.Options;

//public interface IEmailServices
//{
//    Task SendEmailAsync(string to, string subject, string body);
//}

//public class EmailServices : IEmailServices
//{
//    private readonly SmtpSettings _smtp;

//    public EmailServices(IOptions<SmtpSettings> smtpSettings)
//    {
//        _smtp = smtpSettings.Value;
//    }

//    public async Task SendEmailAsync(string to, string subject, string body)
//    {
//        var mail = new MailMessage()
//        {
//            From = new MailAddress(_smtp.From),
//            Subject = subject,
//            Body = body,
//            IsBodyHtml = true
//        };

//        mail.To.Add(to);

//        using var smtp = new SmtpClient(_smtp.Host, _smtp.Port)
//        {
//            Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password),
//            EnableSsl = _smtp.EnableSsl
//        };

//        await smtp.SendMailAsync(mail);
//    }
//}

using MailKit.Net.Smtp;
using MimeKit;
using VaultIQ.Interfaces.Services;

public class EmailServices : IEmailServices
{
    private readonly IConfiguration _config;

    public EmailServices(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("VaultIQ", _config["SmtpSettings:Username"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["SmtpSettings:Host"],
                int.Parse(_config["SmtpSettings:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _config["SmtpSettings:Username"],
                _config["SmtpSettings:Password"]
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (SmtpCommandException ex)
        {
            // SMTP-level error (bad auth, invalid email, etc.)
            throw new Exception($"SMTP command error: {ex.Message} - StatusCode: {ex.StatusCode}");
        }
        catch (SmtpProtocolException ex)
        {
            // Protocol failure (TLS, malformed message, etc.)
            throw new Exception($"SMTP protocol error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Any other error
            throw new Exception($"Email sending failed: {ex.Message}");
        }
    }
}



public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string From { get; set; }
}
