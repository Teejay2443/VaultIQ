using System.ComponentModel.DataAnnotations;

namespace VaultIQ.Models
{
    public class Business
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string ContactName { get; set; }

        [Required, EmailAddress]
        public string BusinessEmail { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool EmailConfirmed { get; set; } = false;

        public string? EmailConfirmationToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? TokenGeneratedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

