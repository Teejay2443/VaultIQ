using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaultIQ.Data;
using VaultIQ.Dtos.Business;
using VaultIQ.Dtos.Email;
using VaultIQ.Dtos.Requests;
using VaultIQ.Interfaces.Services;

namespace VaultIQ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessAuthController : ControllerBase
    {
        private readonly IBusinessAuthService _businessAuthService;
        private readonly ILogger<BusinessAuthController> _logger;
        private readonly AppDbContext _context;

        public BusinessAuthController(IBusinessAuthService businessAuthService, ILogger<BusinessAuthController> logger,  AppDbContext context)
        {
            _businessAuthService = businessAuthService;
            _logger = logger;
            _context = context;

        }

        // ---------------------------
        // REGISTER BUSINESS
        // ---------------------------
        [HttpPost("register")]
        public async Task<IActionResult> RegisterBusiness([FromBody] BusinessRegisterDto dto)
        {
            _logger.LogInformation("Register endpoint hit for {Email}", dto.BusinessEmail);
            var result = await _businessAuthService.RegisterBusinessAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ---------------------------
        // LOGIN BUSINESS
        // ---------------------------
        [HttpPost("login")]
        public async Task<IActionResult> LoginBusiness([FromBody] BusinessLoginDto dto)
        {
            _logger.LogInformation("Login endpoint hit for {Email}", dto.BusinessEmail);
            var result = await _businessAuthService.LoginBusinessAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ---------------------------
        // VERIFY EMAIL
        // ---------------------------
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyBusinessEmail([FromBody] VerifyEmailDto dto)
        {
            _logger.LogInformation("Verify email endpoint hit for {Email}", dto.Email);
            var result = await _businessAuthService.VerifyBusinessEmailAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ---------------------------
        // RESEND VERIFICATION TOKEN
        // ---------------------------
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromQuery] string email)
        {
            _logger.LogInformation("Resend verification endpoint hit for {Email}", email);
            var result = await _businessAuthService.ResendVerificationTokenAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        // ---------------------------
        // FORGOT PASSWORD
        // ---------------------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] BusinessForgotPasswordDto dto)
        {
            _logger.LogInformation("Forgot password endpoint hit for {Email}", dto.BusinessEmail);
            var result = await _businessAuthService.ForgotPasswordAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ---------------------------
        // RESET PASSWORD
        // ---------------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] BusinessResetPasswordDto dto)
        {
            _logger.LogInformation("Reset password endpoint hit for {Email}", dto.BusinessEmail);
            var result = await _businessAuthService.ResetPasswordAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        // ---------------------------
        // REFRESH TOKEN
        // ---------------------------
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest dto)
        {
            _logger.LogInformation("Refresh token endpoint hit for {Email}", dto.Email);
            var result = await _businessAuthService.RefreshTokenAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness(Guid id)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
                return NotFound(new { message = "Business not found" });

            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Business deleted successfully" });
        }

    }
}
