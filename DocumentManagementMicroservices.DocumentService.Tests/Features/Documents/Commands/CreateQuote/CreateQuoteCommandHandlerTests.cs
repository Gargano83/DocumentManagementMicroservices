using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MassTransit;
using Moq;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Commands.CreateQuote
{
    public class CreateQuoteCommandHandlerTests
    {
        private readonly Mock<IDocumentRepository> _repositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly CreateQuoteCommandHandler _handler;

        public CreateQuoteCommandHandlerTests()
        {
            // Setup dei Mock
            _repositoryMock = new Mock<IDocumentRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();

            // Iniezione nel sistema sotto test (SUT)
            _handler = new CreateQuoteCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateQuoteAndPublishEvent()
        {
            // Arrange
            var command = new CreateQuoteCommand("CUST-123", 30);

            // Simulo che il repository accetti l'inserimento senza sollevare eccezioni
            _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Quote>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Assert.StartsWith("PREV-", result.DocumentNumber);

            // Verifico che il repository sia stato chiamato esattamente 1 volta con un'entità Quote
            _repositoryMock.Verify(repo => repo.CreateAsync(It.Is<Quote>(q =>
                q.CustomerId == "CUST-123" &&
                q.Status == DocumentManagementMicroservices.DocumentService.Domain.Enums.DocumentStatus.Draft
            )), Times.Once);

            // Verifico che l'evento MassTransit sia stato pubblicato
            _publishEndpointMock.Verify(endpoint => endpoint.Publish(
                It.Is<DocumentCreatedEvent>(e => e.CustomerId == "CUST-123"),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }
    }
}
