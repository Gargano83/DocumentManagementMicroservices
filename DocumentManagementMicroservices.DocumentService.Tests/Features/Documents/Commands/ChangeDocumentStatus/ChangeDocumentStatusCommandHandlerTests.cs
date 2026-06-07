using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MassTransit;
using Moq;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Commands.ChangeDocumentStatus
{
    public class ChangeDocumentStatusCommandHandlerTests
    {
        private readonly Mock<IDocumentRepository> _repositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly ChangeDocumentStatusCommandHandler _handler;

        public ChangeDocumentStatusCommandHandlerTests()
        {
            _repositoryMock = new Mock<IDocumentRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _handler = new ChangeDocumentStatusCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidTransition_ShouldUpdateStatusAndPublishEvent()
        {
            // Arrange
            var command = new ChangeDocumentStatusCommand("DOC-123", DocumentStatus.Complete, 1);
            var existingDoc = new Quote { Id = "DOC-123", Status = DocumentStatus.Draft, Version = 1 };

            _repositoryMock.Setup(repo => repo.GetByIdAsync<DocumentBase>("DOC-123"))
                .ReturnsAsync(existingDoc);

            _repositoryMock.Setup(repo => repo.UpdateStatusWithConcurrencyAsync("DOC-123", DocumentStatus.Complete, 1)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(DocumentStatus.Complete.ToString(), result.NewStatus);
            Assert.Equal(2, result.NewVersion);

            _publishEndpointMock.Verify(endpoint => endpoint.Publish(
                It.Is<DocumentStatusChangedEvent>(e =>
                    e.DocumentId == "DOC-123" &&
                    e.OldStatus == "Draft" &&
                    e.NewStatus == "Complete"),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidTransition_ShouldThrowDomainException()
        {
            // Arrange: Provo a passare da Draft a Sent (non permesso dal nostro switch di dominio)
            var command = new ChangeDocumentStatusCommand("DOC-123", DocumentStatus.Sent, 1);
            var existingDoc = new Quote { Id = "DOC-123", Status = DocumentStatus.Draft, Version = 1 };

            _repositoryMock.Setup(repo => repo.GetByIdAsync<DocumentBase>("DOC-123")).ReturnsAsync(existingDoc);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Contains("Transizione di stato non consentita", exception.Message);

            // Verifico che nulla venga salvato o pubblicato
            _repositoryMock.Verify(repo => repo.UpdateStatusWithConcurrencyAsync(It.IsAny<string>(), It.IsAny<DocumentStatus>(), It.IsAny<int>()), Times.Never);
            _publishEndpointMock.Verify(endpoint => endpoint.Publish(It.IsAny<DocumentStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
