using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using Moq;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Commands.CreateProformaFromQuote
{
    public class CreateProformaFromQuoteCommandHandlerTests
    {
        private readonly Mock<IDocumentRepository> _repositoryMock;
        private readonly CreateProformaFromQuoteCommandHandler _handler;

        public CreateProformaFromQuoteCommandHandlerTests()
        {
            _repositoryMock = new Mock<IDocumentRepository>();
            _handler = new CreateProformaFromQuoteCommandHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WithApprovedQuote_ShouldCreateProforma()
        {
            // Arrange
            var quoteId = "QUOTE-123";
            var command = new CreateProformaFromQuoteCommand(QuoteId: quoteId);

            var existingQuote = new Quote
            {
                Id = quoteId,
                CustomerId = "CUST-999",
                Status = DocumentStatus.Approved
            };

            _repositoryMock.Setup(repo => repo.GetByIdAsync<DocumentBase>(id: quoteId)).ReturnsAsync(existingQuote);

            // Act
            var result = await _handler.Handle(request: command, cancellationToken: CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("PROF-", result.DocumentNumber);
            Assert.Equal(DocumentStatus.Draft, result.Status);

            // Verifico che il salvataggio sia avvenuto con l'Id originale impostato
            _repositoryMock.Verify(repo => repo.CreateAsync(It.Is<Proforma>(p =>
                p.SourceQuoteId == quoteId &&
                p.CustomerId == "CUST-999"
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDraftQuote_ShouldThrowDomainException()
        {
            // Arrange
            var quoteId = "QUOTE-123";
            var command = new CreateProformaFromQuoteCommand(QuoteId: quoteId);

            var draftQuote = new Quote
            {
                Id = quoteId,
                Status = DocumentStatus.Draft
            };

            _repositoryMock.Setup(repo => repo.GetByIdAsync<DocumentBase>(id: quoteId)).ReturnsAsync(draftQuote);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(request: command, cancellationToken: CancellationToken.None));

            Assert.Contains("Impossibile creare una Proforma da un Preventivo in stato: Draft", exception.Message);

            // Verifico che il repository non sia mai stato chiamato per il salvataggio
            _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Proforma>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentQuote_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new CreateProformaFromQuoteCommand(QuoteId: "NOT-EXISTING");

            _repositoryMock.Setup(repo => repo.GetByIdAsync<DocumentBase>(id: It.IsAny<string>())).ReturnsAsync((DocumentBase?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(request: command, cancellationToken: CancellationToken.None));
        }
    }
}
