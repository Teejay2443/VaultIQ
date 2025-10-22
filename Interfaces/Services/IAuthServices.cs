using VaultIQ.Dtos.Email;
using VaultIQ.Dtos.Requests;
using VaultIQ.Dtos.Responses;
using VaultIQ.Dtos.User;
using VaultIQ.Models;

namespace VaultIQ.Interfaces.Services
{
    public interface IAuthServices
    {
        Task<ResponseModel<Guid>> RegisterUserAsync(RegisterDto dto);
        Task<ResponseModel<LoginResponseModel>> LoginUserAsync(LoginDto dto);
        Task<ResponseModel> VerifyEmailAsync(VerifyEmailDto dto);
        Task<ResponseModel> ResendVerificationTokenAsync(string email);
        Task<ResponseModel> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ResponseModel> ResetPasswordAsync(ResetPasswordDto dto);
        Task<ResponseModel<LoginResponseModel>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<object> GoogleSignInAsync(string idToken);
    }
}
