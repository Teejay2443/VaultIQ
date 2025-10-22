namespace VaultIQ.Interfaces.Services
{
        public interface IEmailServices
        {
            Task SendEmailAsync(string to, string subject, string htmlContent);
        }
}
