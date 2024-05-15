namespace groverale
{
    public class RetentionEvent
    {
        public RetentionEvent()
        {
            EventTime = DateTime.Now.ToUniversalTime();
        }

        public string? SiteUrl { get; set; }
        public string? ListName { get; set; }
        public string? FolderPath { get; set; }
        public string? DocumentName { get; set; }
        public string? SiteId { get; set; }
        public string? WebId { get; set; }
        public string? ListId { get; set; }
        public string? ItemUniqueId { get; set; }
        public string? ListItemId { get; set; }
        public string? LibraryName { get; set; }
        public string? WorkloadName { get; set; }
        public string? ExistingLabelId { get; set; }
        public string? UserId { get; set; }
        public DateTime EventTime { get; set; }
    }
}