using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.SearchDocuments
{
    public record SearchDocumentsQuery(string? CustomerId,
                                        string? Status,
                                        string? DocumentType,
                                        int PageNumber = 1,
                                        int PageSize = 10
                                        ) : IRequest<PaginatedDocumentResultDto>;
}
