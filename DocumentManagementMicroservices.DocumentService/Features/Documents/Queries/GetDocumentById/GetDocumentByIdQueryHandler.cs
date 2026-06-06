using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
    {
        private readonly IDocumentRepository _repository;
        private readonly HybridCache _cache;

        public GetDocumentByIdQueryHandler(IDocumentRepository repository, HybridCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"document:{request.Id}";

            return await _cache.GetOrCreateAsync(
                cacheKey,
                async cancelToken =>
                {
                    // Recupero l'entità di dominio dal DB
                    var entity = await _repository.GetByIdAsync<DocumentBase>(request.Id);

                    if (entity == null) return null;

                    // Mappo l'entità nel DTO concreto
                    return new DocumentDto
                    {
                        Id = entity.Id,
                        DocumentNumber = entity.DocumentNumber,
                        IssueDate = entity.IssueDate,
                        CustomerId = entity.CustomerId,
                        Status = entity.Status.ToString(),
                        DocumentType = entity.GetType().Name,

                        // Pattern matching per estrarre in sicurezza i campi delle classi derivate
                        ValidUntil = (entity as Quote)?.ValidUntil,
                        ShippingAddress = (entity as SalesOrder)?.ShippingAddress
                    };
                },
                cancellationToken: cancellationToken
            );
        }
    }
}
