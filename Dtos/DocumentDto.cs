namespace VaultIQ.Dtos
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }

}
