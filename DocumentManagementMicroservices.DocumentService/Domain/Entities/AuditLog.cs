namespace DocumentManagementMicroservices.DocumentService.Domain.Entities
{
    public class AuditLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DocumentId { get; set; } = string.Empty;
        public string Action { get; set; } = "StatusChange";
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
