using VaultIQ.Models;

namespace VaultIQ.Interfaces.Services
{
    public interface IGoogleAuthService
    {
        Task<(User user, string token)> GoogleSignInAsync(string idToken);
    }
}
