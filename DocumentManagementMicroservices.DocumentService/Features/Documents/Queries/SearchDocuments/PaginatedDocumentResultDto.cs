namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.SearchDocuments
{
    public record DocumentSummaryDto(string Id, string DocumentNumber, string DocumentType, string Status, DateTime IssueDate, string CustomerId);

    public record PaginatedDocumentResultDto(IReadOnlyList<DocumentSummaryDto> Items, long TotalCount, int PageNumber, int PageSize);
}
