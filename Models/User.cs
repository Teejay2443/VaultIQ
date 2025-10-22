namespace VaultIQ.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool BiometricData { get; set; } 
        public bool EnableIntrusionCamera { get; set; } = false;
        public bool IsEmailVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? TokenGeneratedAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string AuthProvider { get; set; } = "Local";
        public ICollection<Document> Documents { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

