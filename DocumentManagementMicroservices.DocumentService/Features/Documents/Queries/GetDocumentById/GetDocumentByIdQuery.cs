using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    public record GetDocumentByIdQuery(string DocumentId) : IRequest<DocumentDto>;
}
