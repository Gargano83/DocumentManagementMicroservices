using DocumentManagementMicroservices.BuildingBlocks.Behaviors;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    public record GetDocumentByIdQuery(string DocumentId) : ICacheableQuery<DocumentDto>
    {
        public string CacheKey => $"document:{DocumentId}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    }
}
