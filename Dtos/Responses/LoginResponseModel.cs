namespace VaultIQ.Dtos.Responses
{
    public class LoginResponseModel
    {
        public Guid Id { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
