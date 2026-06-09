using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using Moq;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Commands.UpdateQuote
{
    public class UpdateQuoteCommandHandlerTests
    {
        private readonly Mock<IDocumentRepository> _repositoryMock;
        private readonly UpdateQuoteCommandHandler _handler;

        public UpdateQuoteCommandHandlerTests()
        {
            _repositoryMock = new Mock<IDocumentRepository>();
            _handler = new UpdateQuoteCommandHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WhenConcurrencyConflictOccurs_ShouldThrowDomainException()
        {
            // Arrange
            var command = new UpdateQuoteCommand(QuoteId: "QUOTE-123", CustomerId: "NEW-CUST", ValidityDays: 15, ExpectedVersion: 1);

            var existingQuote = new Quote { Id = "QUOTE-123", Status = DocumentStatus.Draft, Version = 1 };

            _repositoryMock.Setup(repo => repo.GetByIdAsync<DocumentBase>(id: command.QuoteId)).ReturnsAsync(existingQuote);

            // Simulo che il database restituisca false (zero documenti modificati)
            _repositoryMock.Setup(repo => repo.UpdateQuoteAsync(quote: It.IsAny<Quote>(), expectedVersion: command.ExpectedVersion)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(request: command, cancellationToken: CancellationToken.None));

            Assert.Equal("ConcurrencyConflict", exception.ErrorCode);
        }
    }
}
