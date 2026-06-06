using DocumentManagementMicroservices.BuildingBlocks.Events;
using MassTransit;

namespace DocumentManagementMicroservices.IdentityService.Consumers
{
    /// <summary>
    /// Ascolta in background gli eventi di creazione documenti provenienti da altri microservizi.
    /// </summary>
    public class DocumentCreatedEventConsumer : IConsumer<DocumentCreatedEvent>
    {
        private readonly ILogger<DocumentCreatedEventConsumer> _logger;

        public DocumentCreatedEventConsumer(ILogger<DocumentCreatedEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<DocumentCreatedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "🚀 EVENTO RICEVUTO in IdentityService: Il cliente {CustomerId} ha creato il documento {DocumentNumber} (ID: {DocumentId}) alle {CreatedAt}",
                message.CustomerId,
                message.DocumentNumber,
                message.DocumentId,
                message.CreatedAt);

            return Task.CompletedTask;
        }
    }
}
