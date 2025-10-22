using System.ComponentModel.DataAnnotations;

namespace VaultIQ.Dtos.User
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
