using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using MassTransit;
using MongoDB.Driver;

namespace DocumentManagementMicroservices.DocumentService.Features.AuditLogs.Consumers
{
    /// <summary>
    /// Consumer asincrono responsabile dell'ascolto degli eventi di dominio e della storicizzazione delle operazioni (Audit).
    /// </summary>
    public class AuditLogConsumer : IConsumer<DocumentStatusChangedEvent>
    {
        private readonly IMongoCollection<AuditLog> _auditCollection;
        private readonly ILogger<AuditLogConsumer> _logger;

        public AuditLogConsumer(IMongoClient mongoClient, ILogger<AuditLogConsumer> logger)
        {
            _logger = logger;

            // Creazione della separazione logica: le tracce di audit risiedono in un database isolato
            var database = mongoClient.GetDatabase("auditlogdb");
            _auditCollection = database.GetCollection<AuditLog>("AuditLogs");
        }

        /// <summary>
        /// Elabora il messaggio in ingresso e persiste il log.
        /// </summary>
        public async Task Consume(ConsumeContext<DocumentStatusChangedEvent> context)
        {
            var message = context.Message;

            var auditEntry = new AuditLog
            {
                DocumentId = message.DocumentId,
                Action = "StatusChange",
                Details = $"Stato modificato da {message.OldStatus} a {message.NewStatus}",
                Timestamp = message.Timestamp
            };

            // Salvataggio asincrono sul database di Audit
            await _auditCollection.InsertOneAsync(auditEntry, cancellationToken: context.CancellationToken);

            _logger.LogInformation("Audit Log salvato con successo per il documento {DocumentId}. Nuova transizione: {NewStatus}", message.DocumentId, message.NewStatus);
        }
    }
}
