using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Features.AuditLogs.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.AuditLogs.Consumers
{
    public class AuditLogConsumerTests
    {
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<AuditLog>> _auditCollectionMock;
        private readonly Mock<ILogger<AuditLogConsumer>> _loggerMock;
        private readonly AuditLogConsumer _consumer;

        public AuditLogConsumerTests()
        {
            _mongoClientMock = new Mock<IMongoClient>();
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _auditCollectionMock = new Mock<IMongoCollection<AuditLog>>();
            _loggerMock = new Mock<ILogger<AuditLogConsumer>>();

            // Setup della catena di MongoDB (Client -> Database -> Collection)
            _mongoClientMock.Setup(c => c.GetDatabase("auditlogdb", null)).Returns(_mongoDatabaseMock.Object);

            _mongoDatabaseMock.Setup(d => d.GetCollection<AuditLog>("AuditLogs", null)).Returns(_auditCollectionMock.Object);

            _consumer = new AuditLogConsumer(_mongoClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_ShouldInsertAuditLogIntoDatabase()
        {
            // Arrange
            var message = new DocumentStatusChangedEvent(DocumentId: "DOC-999", OldStatus: "Draft", NewStatus: "Approved", Timestamp: DateTime.UtcNow);

            // Mock del contesto di MassTransit che incapsula il messaggio
            var consumeContextMock = new Mock<ConsumeContext<DocumentStatusChangedEvent>>();
            consumeContextMock.Setup(c => c.Message).Returns(message);
            consumeContextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            // Act
            await _consumer.Consume(consumeContextMock.Object);

            // Assert
            // Verifico che il metodo InsertOneAsync sia stato chiamato sulla collection corretta
            _auditCollectionMock.Verify(collection => collection.InsertOneAsync(
                It.Is<AuditLog>(log =>
                    log.DocumentId == "DOC-999" &&
                    log.Action == "StatusChange" &&
                    log.Details.Contains("Approved")),
                null,
                CancellationToken.None),
            Times.Once);
        }
    }
}
