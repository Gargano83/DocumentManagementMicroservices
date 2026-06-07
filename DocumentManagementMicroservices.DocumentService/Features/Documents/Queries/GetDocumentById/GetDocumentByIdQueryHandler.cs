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

            // Uso HybridCache: cerca prima in RAM, poi in Redis, infine esegue la factory (MongoDB)
            var document = await _hybridCache.GetOrCreateAsync(
                cacheKey,
                async cancel => await _repository.GetByIdAsync<DocumentBase>(request.DocumentId),
                cancellationToken: cancellationToken
            );

            // Se il documento non esiste fisicamente a database
            if (document is null)
            {
                throw new NotFoundException("Document", request.DocumentId);
            }

            // Mappatura verso il DTO
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
