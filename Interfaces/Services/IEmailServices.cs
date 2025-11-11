

namespace VaultIQ.Interfaces.Services
{
    public interface IEmailServices
    {
       Task  SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
