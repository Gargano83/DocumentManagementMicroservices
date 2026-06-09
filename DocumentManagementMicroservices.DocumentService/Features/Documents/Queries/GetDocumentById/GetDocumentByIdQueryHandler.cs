using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    /// <summary>
    /// Query Handler è responsabile del recupero in lettura di un documento tramite il suo identificativo.
    /// </summary>
    public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
    {
        private readonly IDocumentRepository _repository;
        private readonly HybridCache _hybridCache;

        public GetDocumentByIdQueryHandler(IDocumentRepository repository, HybridCache hybridCache)
        {
            _repository = repository;
            _hybridCache = hybridCache;
        }

        /// <summary>
        /// Interroga la cache multi-livello e, in caso di miss, esegue il fallback sul database.
        /// </summary>
        public async Task<DocumentDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            // Definizione di una cache key univoca e partizionata per dominio
            var cacheKey = $"document:{request.DocumentId}";

            // L'approccio 'GetOrCreateAsync' previene nativamente i problemi di:
            // richieste concorrenti per la stessa chiave scaduta colpiscono simultaneamente il DB.
            var documentDto = await _hybridCache.GetOrCreateAsync(
                cacheKey,
                async cancel =>
                {
                    // 1. Chiamata effettiva a MongoDB (eseguita solo se non è in cache)
                    var document = await _repository.GetByIdAsync<DocumentBase>(id: request.DocumentId);

                    // 2. Se non esiste, solleviamo subito l'eccezione per NON mettere in cache un null
                    if (document is null)
                    {
                        throw new NotFoundException("Document", request.DocumentId);
                    }

                    // 3. Serializzazione DTO:
                    // La strategia di salvare direttamente un DTO migliora le performance legate alla deserializzazione JSON all'interno di Redis.
                    // Il dato conservato in cache è così ottimizzato e pronto per essere servito al client.
                    return new DocumentDto(
                        Id: document.Id,
                        DocumentNumber: document.DocumentNumber,
                        Status: document.Status.ToString(),
                        CustomerId: document.CustomerId,
                        IssueDate: document.IssueDate,
                        DocumentType: document.GetType().Name
                    );
                },
                cancellationToken: cancellationToken
            );

            return documentDto;
        }
    }
}
