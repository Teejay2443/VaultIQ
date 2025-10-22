using VaultIQ.Dtos.Business;
using VaultIQ.Dtos.Email;
using VaultIQ.Dtos.Requests;
using VaultIQ.Dtos.Responses;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Interfaces.Services;
using VaultIQ.Models;
using VaultIQ.Repositories;

namespace VaultIQ.Services
{
    public class BusinessAuthService : IBusinessAuthService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IEmailServices _emailServices;
        private readonly IJwtService _jwtService;
        private readonly ILogger<BusinessAuthService> _logger;

        public BusinessAuthService(
            IBusinessRepository businessRepository,
            IEmailServices emailServices,
            IJwtService jwtService,
            ILogger<BusinessAuthService> logger)
        {
            _businessRepository = businessRepository;
            _emailServices = emailServices;
            _jwtService = jwtService;
            _logger = logger;
        }

        // ---------------------------
        // BUSINESS REGISTRATION
        // ---------------------------
        public async Task<ResponseModel<Guid>> RegisterBusinessAsync(BusinessRegisterDto dto)
        {
            _logger.LogInformation("Attempting to register business with email: {Email}", dto.BusinessEmail);

            try
            {
                var existing = await _businessRepository.GetByEmailAsync(dto.BusinessEmail);
                if (existing != null)
                {
                    _logger.LogWarning("Registration failed - email already exists: {Email}", dto.BusinessEmail);
                    return ResponseModel<Guid>.Failure("Business email already exists.");
                }

                if (dto.Password != dto.ConfirmPassword)
                    return ResponseModel<Guid>.Failure("Passwords do not match.");

                int verificationCode = new Random().Next(100000, 999999);

                var business = new Business
                {
                    Id = Guid.NewGuid(),
                    CompanyName = dto.CompanyName,
                    ContactName = dto.ContactName,
                    BusinessEmail = dto.BusinessEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    EmailConfirmationToken = verificationCode.ToString(),
                    EmailConfirmed = false,
                    TokenGeneratedAt = DateTime.UtcNow
                };

                await _businessRepository.AddBusinessAsync(business);
                await _businessRepository.SaveChangesAsync();

                string emailBody = $@"
                    <h3>VaultIQ Business Verification</h3>
                    <p>Hi {business.ContactName},</p>
                    <p>Your verification token is:</p>
                    <h2 style='color:#0078D4'>{business.EmailConfirmationToken}</h2>
                    <p>This token expires in 15 minutes.</p>";

                await _emailServices.SendEmailAsync(business.BusinessEmail, "Verify Your Business Account", emailBody);

                _logger.LogInformation("Business registered successfully. Verification email sent to: {Email}", business.BusinessEmail);
                return ResponseModel<Guid>.Success(business.Id, "Business registered successfully. Please check your email for verification token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering business with email: {Email}", dto.BusinessEmail);
                return ResponseModel<Guid>.Failure("An error occurred while registering the business.");
            }
        }

        // ---------------------------
        // BUSINESS LOGIN
        // ---------------------------
        public async Task<ResponseModel<LoginResponseModel>> LoginBusinessAsync(BusinessLoginDto dto)
        {
            _logger.LogInformation("Business login attempt: {Email}", dto.BusinessEmail);

            try
            {
                var business = await _businessRepository.GetByEmailAsync(dto.BusinessEmail);
                if (business == null || !BCrypt.Net.BCrypt.Verify(dto.Password, business.PasswordHash))
                {
                    _logger.LogWarning("Invalid credentials for {Email}", dto.BusinessEmail);
                    return ResponseModel<LoginResponseModel>.Failure("Invalid email or password.");
                }

                if (!business.EmailConfirmed)
                    return ResponseModel<LoginResponseModel>.Failure("Email not verified. Please verify your account.");

                var accessToken = _jwtService.GenerateBusinessToken(business);
                var refreshToken = _jwtService.GenerateRefreshToken();

                business.RefreshToken = refreshToken;
                business.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _businessRepository.UpdateBusinessAsync(business);
                await _businessRepository.SaveChangesAsync();

                var response = new LoginResponseModel
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                _logger.LogInformation("Business login successful for {Email}", dto.BusinessEmail);
                return ResponseModel<LoginResponseModel>.Success(response, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during business login: {Email}", dto.BusinessEmail);
                return ResponseModel<LoginResponseModel>.Failure("An error occurred during login.");
            }
        }

        // ---------------------------
        // VERIFY EMAIL
        // ---------------------------
        public async Task<ResponseModel> VerifyBusinessEmailAsync(VerifyEmailDto dto)
        {
            _logger.LogInformation("Verifying business email: {Email}", dto.Email);

            try
            {
                var business = await _businessRepository.GetByEmailAsync(dto.Email);
                if (business == null)
                    return ResponseModel.Failure("Business not found.");

                if (business.EmailConfirmed)
                    return ResponseModel.Failure("Account already verified.");

                if (business.EmailConfirmationToken != dto.Token)
                    return ResponseModel.Failure("Invalid verification token.");

                if (business.TokenGeneratedAt == null || DateTime.UtcNow > business.TokenGeneratedAt.Value.AddMinutes(15))
                    return ResponseModel.Failure("Verification token has expired.");

                business.EmailConfirmed = true;
                business.EmailConfirmationToken = null;
                business.TokenGeneratedAt = null;
                await _businessRepository.UpdateBusinessAsync(business);
                await _businessRepository.SaveChangesAsync();

                _logger.LogInformation("Business email verified successfully: {Email}", dto.Email);
                return ResponseModel.Success("Business email verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying business email: {Email}", dto.Email);
                return ResponseModel.Failure("An error occurred during verification.");
            }
        }
        public async Task<ResponseModel> ForgotPasswordAsync(BusinessForgotPasswordDto dto)
        {
            var business = await _businessRepository.GetByEmailAsync(dto.BusinessEmail);
            if (business == null)
                return ResponseModel.Failure("No account found with this email.");

            business.PasswordResetToken = new Random().Next(100000, 999999).ToString();
            business.TokenGeneratedAt = DateTime.UtcNow;

            await _businessRepository.SaveChangesAsync();

            string body = $@"
            <h3>VaultIQ Business Password Reset</h3>
            <p>Hi {business.ContactName},</p>
            <p>Your password reset token is:</p>
            <h2 style='color:#D63384'>{business.PasswordResetToken}</h2>
            <p>This token will expire in 15 minutes.</p>";

            await _emailServices.SendEmailAsync(business.BusinessEmail, "VaultIQ Password Reset", body);

            return ResponseModel.Success("Password reset token sent successfully.");
        }

        public async Task<ResponseModel<LoginResponseModel>> ResetPasswordAsync(BusinessResetPasswordDto dto)
        {
            var business = await _businessRepository.GetByEmailAsync(dto.BusinessEmail);
            if (business == null)
                return ResponseModel<LoginResponseModel>.Failure("Invalid email or token.");

            if (business.PasswordResetToken != dto.Token)
                return ResponseModel<LoginResponseModel>.Failure("Invalid or expired token.");

            if (business.TokenGeneratedAt == null || business.TokenGeneratedAt.Value.AddMinutes(15) < DateTime.UtcNow)
                return ResponseModel<LoginResponseModel>.Failure("Token has expired. Please request a new one.");

            business.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            business.PasswordResetToken = null;
            business.TokenGeneratedAt = null;

            await _businessRepository.SaveChangesAsync();

            var accessToken = _jwtService.GenerateBusinessToken(business);
            var refreshToken = _jwtService.GenerateRefreshToken();

            business.RefreshToken = refreshToken;
            business.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _businessRepository.UpdateBusinessAsync(business);

            var response = new LoginResponseModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return ResponseModel<LoginResponseModel>.Success(response, "Password reset successfully. You can now log in.");
        }

        public async Task<ResponseModel> ResendVerificationTokenAsync(string email)
        {
            _logger.LogInformation("Resending verification token for: {Email}", email);

            try
            {
                var business = await _businessRepository.GetByEmailAsync(email);
                if (business == null)
                    return ResponseModel.Failure("User not found.");

                if (business.EmailConfirmed)
                    return ResponseModel.Failure("Email already verified.");

                int newCode = new Random().Next(100000, 999999);
                business.EmailConfirmationToken = newCode.ToString();
                business.TokenGeneratedAt = DateTime.UtcNow;
                await _businessRepository.UpdateBusinessAsync(business);

                string subject = "VaultIQ - New Verification Code";
                string body = $"Your new verification token is: <b>{newCode}</b> (expires in 15 minutes).";

                await _emailServices.SendEmailAsync(business.BusinessEmail, subject, body);
                _logger.LogInformation("Verification code resent to: {Email}", business.BusinessEmail);

                return ResponseModel.Success("A new verification token has been sent to your email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification token for: {Email}", email);
                return ResponseModel.Failure("An error occurred while resending the token.");
            }
        }

        public async Task<ResponseModel<LoginResponseModel>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var business = await _businessRepository.GetByEmailAsync(request.Email);
            if (business == null || business.RefreshToken != request.RefreshToken || business.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return ResponseModel<LoginResponseModel>.Failure("Invalid or expired refresh token.");
            }

            var newAccessToken = _jwtService.GenerateBusinessToken(business);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            business.RefreshToken = newRefreshToken;
            business.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _businessRepository.UpdateBusinessAsync(business);

            var response = new LoginResponseModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return ResponseModel<LoginResponseModel>.Success(response, "Token refreshed successfully.");
        }
    }
}
