using MediatR;

namespace DocumentManagementMicroservices.BuildingBlocks.Behaviors
{
    /// <summary>
    /// Contratto per identificare le Query (letture) che richiedono il caching automatico dei dati.
    /// </summary>
    public interface ICacheableQuery<out TResponse> : IRequest<TResponse>
    {
        /// <summary>
        /// La chiave di cache univoca per questa specifica richiesta.
        /// </summary>
        string CacheKey { get; }

        /// <summary>
        /// Durata personalizzata della cache (TTL). Se lasciata null, verrà usato il default di 5 minuti.
        /// </summary>
        TimeSpan? Expiration { get; }
    }
}
