namespace VaultIQ.Dtos.Requests
{
    public class DataRequestResponseDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; }
        public string UserEmail { get; set; }
        public string FileName { get; set; }
        public string PurposeOfAccess { get; set; }
        public int AccessDurationInHours { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
