namespace DocumentManagementMicroservices.BuildingBlocks.Events
{
    /// <summary>
    /// Evento di integrazione pubblicato quando un nuovo documento viene creato.
    /// Utilizzo un record perché gli eventi devono essere immutabili.
    /// </summary>
    public record DocumentCreatedEvent(
        string DocumentId,
        string DocumentNumber,
        string CustomerId,
        DateTime CreatedAt
    );
}
