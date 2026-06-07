using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using MassTransit;
using MongoDB.Driver;

namespace DocumentManagementMicroservices.DocumentService.Features.AuditLogs.Consumers
{
    public class AuditLogConsumer : IConsumer<DocumentStatusChangedEvent>
    {
        private readonly IMongoCollection<AuditLog> _auditCollection;
        private readonly ILogger<AuditLogConsumer> _logger;

        // Sfrutto il client generico di MongoDB iniettato da Aspire per puntare a un DB diverso
        public AuditLogConsumer(IMongoClient mongoClient, ILogger<AuditLogConsumer> logger)
        {
            _logger = logger;

            // Creo la separazione logica: questo va sul database "DocumentManagement_Audit"
            var database = mongoClient.GetDatabase("auditlogdb");
            _auditCollection = database.GetCollection<AuditLog>("AuditLogs");
        }

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
