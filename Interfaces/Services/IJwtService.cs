using VaultIQ.Models;

namespace VaultIQ.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateBusinessToken(Business business);
        string GenerateRefreshToken();
    }
}
