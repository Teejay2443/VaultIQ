namespace VaultIQ.Dtos.Auth
{
    public class GoogleSignInRequest
    {
        /// <summary>
        /// The ID token returned from Google after successful sign-in.
        /// </summary>
        public string IdToken { get; set; } = string.Empty;
    }
}

