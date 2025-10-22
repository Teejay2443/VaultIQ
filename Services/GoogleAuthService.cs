using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using VaultIQ.Data;
using VaultIQ.Interfaces.Services;
using VaultIQ.Models;

namespace VaultIQ.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public GoogleAuthService(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<(User user, string token)> GoogleSignInAsync(string idToken)
        {
            // 1️⃣ Verify token with Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            // 2️⃣ Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (existingUser == null)
            {
                // 3️⃣ Create a new user
                var newUser = new User
                {
                    FullName = payload.Name,
                    Email = payload.Email,
                    PasswordHash = string.Empty, // no password for Google users
                    AuthProvider = "Google",
                    IsEmailVerified = true // Google already verified it
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                existingUser = newUser;
            }

            // 4️⃣ Generate a JWT for authentication
            var token = _jwtService.GenerateToken(existingUser);

            return (existingUser, token);
        }
    }
}
