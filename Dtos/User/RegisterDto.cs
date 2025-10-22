using System.ComponentModel.DataAnnotations;

namespace VaultIQ.Dtos.User
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        public bool BiometricData { get; set; }
        public bool EnableIntrusionCamera { get; set; }
    }
}
