using System.ComponentModel.DataAnnotations;

namespace VaultIQ.Dtos.User
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
