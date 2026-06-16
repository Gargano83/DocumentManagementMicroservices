using MediatR;

namespace DocumentManagementMicroservices.BuildingBlocks.Behaviors
{
    /// <summary>
    /// Contratto per identificare i comandi (es. POST di creazione) che richiedono 
    /// un controllo di idempotenza basato su chiave univoca fornita dal client.
    /// </summary>
    public interface IIdempotentCommand<out TResponse> : IRequest<TResponse>
    {
        /// <summary>
        /// Chiave univoca (es. un Guid generato dal frontend) che identifica questa specifica operazione.
        /// </summary>
        string IdempotencyKey { get; }
    }
}
