namespace VaultIQ.Dtos.Requests
{
    public class CreateDataRequestDto
    {
        public string UserEmail { get; set; }
        public string FileName { get; set; }
        public string PurposeOfAccess { get; set; }
        public int AccessDurationInHours { get; set; }
    }
}
