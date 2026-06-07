using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.SearchDocuments
{
    public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, PaginatedDocumentResultDto>
    {
        private readonly IDocumentRepository _repository;

        public SearchDocumentsQueryHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDocumentResultDto> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
        {
            // Il repository si occuperà di costruire i filtri MongoDB e restituire i dati crudi
            var (items, totalCount) = await _repository.SearchAsync(
                request.CustomerId,
                request.Status,
                request.DocumentType,
                request.PageNumber,
                request.PageSize);

            // Mappo le entità di dominio in DTO di riepilogo
            var dtos = items.Select(doc => new DocumentSummaryDto(
                Id: doc.Id,
                DocumentNumber: doc.DocumentNumber,
                DocumentType: doc.GetType().Name,
                Status: doc.Status.ToString(),
                IssueDate: doc.IssueDate,
                CustomerId: doc.CustomerId
            )).ToList();

            return new PaginatedDocumentResultDto(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
