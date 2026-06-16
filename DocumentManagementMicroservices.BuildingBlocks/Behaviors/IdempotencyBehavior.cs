using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DocumentManagementMicroservices.BuildingBlocks.Behaviors
{
    public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IIdempotentCommand<TResponse>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

        public IdempotencyBehavior(IDistributedCache cache, ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Se il client non passa la chiave, bypasso il controllo e proseguo l'esecuzione normalmente.
            if (string.IsNullOrEmpty(request.IdempotencyKey))
            {
                return await next();
            }

            // Prefisso per isolare le chiavi di idempotenza su Redis
            var cacheKey = $"idempotency:{request.IdempotencyKey}";

            // Interrogo Redis per vedere se questa chiave è già stata elaborata
            var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedResponse))
            {
                _logger.LogWarning("Idempotency Behavior: Rilevata richiesta duplicata per la chiave {IdempotencyKey}. Restituisco l'esito salvato in cache.", request.IdempotencyKey);

                // Deserializzo la risposta precedente e interrompo la pipeline, evitando che l'handler tocchi nuovamente il database.
                return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
            }

            // Primo passaggio: la chiave non esiste. Eseguo l'handler reale (es. CreateQuoteCommandHandler)
            var response = await next();

            // Se l'operazione si conclude con successo, memorizzo il risultato su Redis.
            // Imposto un Time-To-Live (TTL) di 10 minuti, sufficiente a coprire i retry di rete del client.
            var serializedResponse = JsonSerializer.Serialize(response);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(cacheKey, serializedResponse, cacheOptions, cancellationToken);

            return response;
        }
    }
}
