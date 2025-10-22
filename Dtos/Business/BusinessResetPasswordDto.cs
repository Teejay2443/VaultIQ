namespace VaultIQ.Dtos.Business
{
    public class BusinessResetPasswordDto
    {
        public string BusinessEmail { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
