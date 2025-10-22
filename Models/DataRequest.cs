using System.ComponentModel.DataAnnotations;

namespace VaultIQ.Models
{
    public class DataRequest
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }
        public string UserEmail { get; set; }
        public string FileName { get; set; }
        public string PurposeOfAccess { get; set; }
        public int AccessDurationInHours { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        public string Status { get; set; } = "Pending"; 


        public Business Business { get; set; } 
    }
}
