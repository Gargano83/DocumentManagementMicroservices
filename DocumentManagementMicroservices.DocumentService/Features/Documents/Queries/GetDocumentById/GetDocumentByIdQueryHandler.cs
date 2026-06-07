using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
    {
        private readonly IDocumentRepository _repository;
        private readonly HybridCache _hybridCache;

        public GetDocumentByIdQueryHandler(IDocumentRepository repository, HybridCache hybridCache)
        {
            _repository = repository;
            _hybridCache = hybridCache;
        }

        public async Task<DocumentDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            // Definisco la chiave di cache univoca per questo documento
            var cacheKey = $"document:{request.DocumentId}";

            // Ora diciamo ad HybridCache di gestire e restituire direttamente il DocumentDto
            var documentDto = await _hybridCache.GetOrCreateAsync(
                cacheKey,
                async cancel =>
                {
                    // 1. Chiamata effettiva a MongoDB (eseguita solo se non è in cache)
                    var document = await _repository.GetByIdAsync<DocumentBase>(request.DocumentId);

                    // 2. Se non esiste, solleviamo subito l'eccezione per NON mettere in cache un null
                    if (document is null)
                    {
                        throw new NotFoundException("Document", request.DocumentId);
                    }

                    // 3. Mappiamo e restituiamo il DTO. Sarà questo record semplice a finire su Redis!
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
