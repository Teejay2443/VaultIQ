using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VaultIQ.Data;
using VaultIQ.Dtos.Email;
using VaultIQ.Dtos.Requests;
using VaultIQ.Dtos.Responses;
using VaultIQ.Dtos.User;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Interfaces.Services;
using VaultIQ.Models;

namespace VaultIQ.Services
{
    public class AuthService : IAuthServices
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailServices _emailServices;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly AppDbContext _context;
        public AuthService(
            IUserRepository userRepository,
            IEmailServices emailServices,
            IJwtService jwtService,
            ILogger<AuthService> logger,
            AppDbContext context)
        {
            _userRepository = userRepository;
            _emailServices = emailServices;
            _jwtService = jwtService;
            _logger = logger;
            _context = context;
        }

        public async Task<ResponseModel<Guid>> RegisterUserAsync(RegisterDto dto)
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", dto.Email);

            try
            {
                var existing = await _userRepository.GetByEmailAsync(dto.Email);
                if (existing != null)
                {
                    _logger.LogWarning("Registration failed - email already exists: {Email}", dto.Email);
                    return ResponseModel<Guid>.Failure("Email already exists.");
                }

                int randomCode = new Random().Next(100000, 999999);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    BiometricData = dto.BiometricData,
                    EnableIntrusionCamera = dto.EnableIntrusionCamera,
                    VerificationToken = randomCode.ToString(),
                    TokenGeneratedAt = DateTime.UtcNow,
                    IsEmailVerified = false,
                };

                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();

                string body = $@"
                    <h3>VaultIQ Email Verification</h3>
                    <p>Hi {user.FullName},</p>
                    <p>Your verification token is:</p>
                    <h2 style='color:#0078D4'>{user.VerificationToken}</h2>
                    <p>This token expires in 5 minutes.</p>";

                await _emailServices.SendEmailAsync(user.Email, "Verify Your VaultIQ Account", body);

                _logger.LogInformation("User registered successfully. Verification email sent to: {Email}", user.Email);
                return ResponseModel<Guid>.Success(user.Id, "User registered successfully. Please check your email for verification token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user with email: {Email}", dto.Email);
                return ResponseModel<Guid>.Failure("An error occurred while registering the user.");
            }
        }

        public async Task<ResponseModel<LoginResponseModel>> LoginUserAsync(LoginDto model)
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);

            try
            {
                var user = await _userRepository.GetByEmailAsync(model.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    if (user?.EnableIntrusionCamera == true)
                    {
                        _logger.LogWarning("[SECURITY ALERT] Intrusion detected for {Email} at {Time}", model.Email, DateTime.UtcNow);
                    }

                    return ResponseModel<LoginResponseModel>.Failure("Invalid email or password.");
                }

                if (!user.IsEmailVerified)
                {
                    return ResponseModel<LoginResponseModel>.Failure("Email not verified. Please verify your account.");
                }

                var accessToken = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateUserAsync(user);

                var response = new LoginResponseModel
                {
                    Id = user.Id,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                _logger.LogInformation("Login successful for user: {Email}", user.Email);
                return ResponseModel<LoginResponseModel>.Success(response, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", model.Email);
                return ResponseModel<LoginResponseModel>.Failure("An error occurred while logging in.");
            }
        }

        public async Task<ResponseModel> VerifyEmailAsync(VerifyEmailDto dto)
        {
            _logger.LogInformation("Attempting email verification for: {Email}", dto.Email);

            try
            {
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null)
                    return ResponseModel.Failure("User not found.");

                if (user.IsEmailVerified)
                    return ResponseModel.Failure("Account already verified.");

                if (user.VerificationToken != dto.Token)
                    return ResponseModel.Failure("Invalid verification token.");

                if (user.TokenGeneratedAt == null || DateTime.UtcNow > user.TokenGeneratedAt.Value.AddMinutes(15))
                    return ResponseModel.Failure("Verification token has expired.");

                user.IsEmailVerified = true;
                user.VerificationToken = null;
                user.TokenGeneratedAt = null;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Email verified successfully for user: {Email}", user.Email);
                return ResponseModel.Success("Email verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for: {Email}", dto.Email);
                return ResponseModel.Failure("An error occurred while verifying the email.");
            }
        }

        public async Task<ResponseModel> ResendVerificationTokenAsync(string email)
        {
            _logger.LogInformation("Resending verification token for: {Email}", email);

            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                    return ResponseModel.Failure("User not found.");

                if (user.IsEmailVerified)
                    return ResponseModel.Failure("Email already verified.");

                int newCode = new Random().Next(100000, 999999);
                user.VerificationToken = newCode.ToString();
                user.TokenGeneratedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                string subject = "VaultIQ - New Verification Code";
                string body = $"Your new verification token is: <b>{newCode}</b> (expires in 15 minutes).";

                await _emailServices.SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation("Verification code resent to: {Email}", user.Email);

                return ResponseModel.Success("A new verification token has been sent to your email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification token for: {Email}", email);
                return ResponseModel.Failure("An error occurred while resending the token.");
            }
        }

        public async Task<ResponseModel> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            _logger.LogInformation("Password reset request initiated for: {Email}", dto.Email);

            try
            {
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null)
                    return ResponseModel.Success("If the email exists, a reset code has been sent.");

                int resetCode = new Random().Next(100000, 999999);
                user.PasswordResetToken = resetCode.ToString();
                user.TokenGeneratedAt = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                string body = $@"
                    <h3>VaultIQ Password Reset</h3>
                    <p>Hi {user.FullName},</p>
                    <p>Your password reset token is:</p>
                    <h2 style='color:#D63384'>{resetCode}</h2>
                    <p>This token will expire in 15 minutes.</p>";

                await _emailServices.SendEmailAsync(user.Email, "VaultIQ Password Reset", body);
                _logger.LogInformation("Password reset email sent to: {Email}", user.Email);

                return ResponseModel.Success("If the email exists, a reset code has been sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for: {Email}", dto.Email);
                return ResponseModel.Failure("An error occurred while processing password reset request.");
            }
        }

        public async Task<ResponseModel> ResetPasswordAsync(ResetPasswordDto dto)
        {
            _logger.LogInformation("Attempting password reset for: {Email}", dto.Email);

            try
            {
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null)
                    return ResponseModel.Failure("Invalid email or token.");

                if (user.PasswordResetToken != dto.Token)
                    return ResponseModel.Failure("Invalid or expired token.");

                if (user.TokenGeneratedAt == null || DateTime.UtcNow > user.TokenGeneratedAt.Value.AddMinutes(15))
                    return ResponseModel.Failure("Token has expired. Please request a new one.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.PasswordResetToken = null;
                user.TokenGeneratedAt = null;
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Password reset successfully for user: {Email}", user.Email);
                return ResponseModel.Success("Password reset successfully. You can now log in.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for: {Email}", dto.Email);
                return ResponseModel.Failure("An error occurred while resetting the password.");
            }
        }

        public async Task<ResponseModel<LoginResponseModel>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return ResponseModel<LoginResponseModel>.Failure("Invalid or expired refresh token.");
            }

            var newAccessToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateUserAsync(user);

            var response = new LoginResponseModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return ResponseModel<LoginResponseModel>.Success(response, "Token refreshed successfully.");
        }
        public async Task<object> GoogleSignInAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            var email = payload.Email;
            var name = payload.Name;

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser == null)
            {
                // Create new user
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = name,
                    Email = email,
                    IsEmailVerified = true, // Google already verified
                    PasswordHash = null // no password since Google login
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                existingUser = newUser;
            }

            // Generate your app's JWT
            var token = _jwtService.GenerateToken(existingUser);

            return new
            {
                message = "Google sign-in successful",
                token,
                user = new
                {
                    id = existingUser.Id,
                    fullName = existingUser.FullName,
                    email = existingUser.Email
                }
            };
        }

    }
}
