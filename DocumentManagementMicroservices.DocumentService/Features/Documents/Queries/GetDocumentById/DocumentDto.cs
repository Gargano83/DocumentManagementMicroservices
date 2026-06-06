namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    /// <summary>
    /// DTO per la lettura dei documenti.
    /// </summary>
    public class DocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
        public string? ShippingAddress { get; set; }
    }
}
