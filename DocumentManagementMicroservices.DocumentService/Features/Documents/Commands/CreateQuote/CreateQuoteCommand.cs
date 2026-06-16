using DocumentManagementMicroservices.BuildingBlocks.Behaviors;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote
{
    /// <summary>
    /// Comando per la creazione di un preventivo. Implementa IIdempotentCommand per proteggere la POST di creazione.
    /// </summary>
    public record CreateQuoteCommand(string CustomerId, int ValidityDays = 30) : IIdempotentCommand<QuoteCreatedDto> 
    {
        /// <summary>
        /// Chiave di idempotenza estratta dall'header HTTP della richiesta.
        /// </summary>
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
