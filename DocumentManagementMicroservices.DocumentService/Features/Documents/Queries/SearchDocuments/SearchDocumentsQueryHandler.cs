using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.SearchDocuments
{
    /// <summary>
    /// Query Handler responsabile della ricerca paginata e filtrata dei documenti commerciali.
    /// </summary>
    public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, PaginatedDocumentResultDto>
    {
        private readonly IDocumentRepository _repository;

        public SearchDocumentsQueryHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Esegue la ricerca delegando l'applicazione dei filtri e restituisce un set di risultati paginato.
        /// </summary>
        public async Task<PaginatedDocumentResultDto> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
        {
            // Delego la complessità dell'interrogazione al repository.
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

            // Restituisco i DTO corredati di una paginazione standardizzata
            return new PaginatedDocumentResultDto(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
