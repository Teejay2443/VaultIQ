namespace VaultIQ.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;

        public Guid UserId { get; set; }    
        public User User { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
