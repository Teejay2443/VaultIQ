using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using VaultIQ.Dtos.Auth;
using VaultIQ.Dtos.Email;
using VaultIQ.Dtos.Requests;
using VaultIQ.Dtos.User;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Interfaces.Services;
using VaultIQ.Models;
using VaultIQ.Repositories;
using VaultIQ.Services;

namespace VaultIQ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAuthController : ControllerBase
    {
        private readonly IAuthServices _authService;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IGoogleAuthService _googleAuthService;

        public UserAuthController(IAuthServices authService, IUserRepository userRepository, IJwtService jwtService, IGoogleAuthService googleAuthService)
        {
            _authService = authService;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _googleAuthService = googleAuthService;
        }

        /// <summary>
        /// Registers a new user and sends verification token.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var response = await _authService.RegisterUserAsync(dto);
            if (!response.IsSuccessful)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Logs in a user and returns access + refresh tokens.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var response = await _authService.LoginUserAsync(dto);
            if (!response.IsSuccessful)
                return Unauthorized(response);

            return Ok(response);
        }

        /// <summary>
        /// Verifies a user’s email using a token.
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var response = await _authService.VerifyEmailAsync(dto);
            if (!response.IsSuccessful)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Resends a verification token to user’s email.
        /// </summary>
        [HttpPost("resend-token")]
        public async Task<IActionResult> ResendVerificationToken([FromQuery] string email)
        {
            var response = await _authService.ResendVerificationTokenAsync(email);
            if (!response.IsSuccessful)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Sends a password reset code to user’s email.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var response = await _authService.ForgotPasswordAsync(dto);
            return Ok(response);
        }

        /// <summary>
        /// Resets the user’s password using the reset token.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var response = await _authService.ResetPasswordAsync(dto);
            if (!response.IsSuccessful)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return StatusCode(result.StatusCode, result);
        }


        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [HttpPost("google-signin")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
        {
            var result = await _authService.GoogleSignInAsync(request.IdToken);
            return Ok(result);
        }


        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync();
            if (!result.Succeeded)
                return BadRequest("Google authentication failed.");

            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (email == null)
                return BadRequest("No email received from Google.");

            // Check if user already exists
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = name ?? "Google User",
                    Email = email,
                    PasswordHash = null, // Google users may not have password
                    IsEmailVerified = true
                };

                await _userRepository.AddUserAsync(user);
            }

            // Generate JWT for app login
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Google sign-in successful",
                token,
                user = new { user.FullName, user.Email }
            });
        }
    }
}
