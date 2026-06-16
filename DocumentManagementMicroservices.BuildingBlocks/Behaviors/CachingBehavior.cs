using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DocumentManagementMicroservices.BuildingBlocks.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICacheableQuery<TResponse>
    {
        private readonly HybridCache _hybridCache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(HybridCache hybridCache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _hybridCache = hybridCache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Configuro le opzioni di scadenza basandomi sulla query stessa o sul valore di default
            var options = new HybridCacheEntryOptions
            {
                Expiration = request.Expiration ?? TimeSpan.FromMinutes(5)
            };

            _logger.LogInformation("CachingBehavior: Verifico la presenza in cache per la chiave '{CacheKey}'", request.CacheKey);

            // Il GetOrCreateAsync si occuperà di fare il "next(cancel)" (ovvero chiamare l'handler) solo se la chiave non esiste su Redis.
            return await _hybridCache.GetOrCreateAsync(
                request.CacheKey,
                async cancel => await next(),
                options,
                cancellationToken: cancellationToken
            );
        }
    }
}
