namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    public record DocumentDto(string Id, string DocumentNumber, string Status, string CustomerId, DateTime IssueDate, string DocumentType);
}
