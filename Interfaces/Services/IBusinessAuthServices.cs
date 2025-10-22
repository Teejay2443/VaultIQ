using VaultIQ.Dtos.Business;
using VaultIQ.Dtos.Email;
using VaultIQ.Dtos.Requests;
using VaultIQ.Dtos.Responses;

namespace VaultIQ.Interfaces.Services
{
    public interface IBusinessAuthService
    {
        Task<ResponseModel<Guid>> RegisterBusinessAsync(BusinessRegisterDto dto);
        Task<ResponseModel<LoginResponseModel>> LoginBusinessAsync(BusinessLoginDto dto);
        Task<ResponseModel> VerifyBusinessEmailAsync(VerifyEmailDto dto);
        Task<ResponseModel<LoginResponseModel>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ResponseModel> ResendVerificationTokenAsync(string email);
        Task<ResponseModel<LoginResponseModel>> ResetPasswordAsync(BusinessResetPasswordDto dto);
        Task<ResponseModel> ForgotPasswordAsync(BusinessForgotPasswordDto dto);
    }
}
