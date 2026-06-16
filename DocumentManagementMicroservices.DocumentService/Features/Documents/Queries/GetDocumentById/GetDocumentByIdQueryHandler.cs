using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    /// <summary>
    /// Query Handler è responsabile del recupero in lettura di un documento tramite il suo identificativo.
    /// </summary>
    public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
    {
        private readonly IDocumentRepository _repository;
        private readonly ILogger<GetDocumentByIdQueryHandler> _logger;

        public GetDocumentByIdQueryHandler(IDocumentRepository repository, ILogger<GetDocumentByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Interroga la cache multi-livello e, in caso di miss, esegue il fallback sul database.
        /// </summary>
        public async Task<DocumentDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("⚠️ [DATABASE HIT] Il dato non era in cache. Sto interrogando fisicamente MongoDB per l'ID: {Id}", request.DocumentId);

            // Chiamata effettiva a MongoDB (eseguita solo in caso di cache miss)
            var document = await _repository.GetByIdAsync<DocumentBase>(id: request.DocumentId);

            // Se non esiste, sollevo subito l'eccezione per non mettere in cache un null
            if (document is null)
            {
                throw new NotFoundException("Document", request.DocumentId);
            }

            // Mapping del DTO ottimizzato: verrà intercettato dal CachingBehavior e salvato su Redis
            return new DocumentDto(
                Id: document.Id,
                DocumentNumber: document.DocumentNumber,
                Status: document.Status.ToString(),
                CustomerId: document.CustomerId,
                IssueDate: document.IssueDate,
                DocumentType: document.GetType().Name
            );
        }
    }
}
