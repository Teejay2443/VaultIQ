namespace VaultIQ.Dtos
{
    public class PagedDocumentsDto
    {
        public IEnumerable<object> Documents { get; set; } = new List<object>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
