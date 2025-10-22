using System.ComponentModel.DataAnnotations;

namespace VaultIQ.Dtos.User
{

        public class ResetPasswordDto
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Token { get; set; } = string.Empty;

            [Required]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
}

