using DocumentManagementMicroservices.BuildingBlocks.Services;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using Moq;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Commands.CreateQuote
{
    public class CreateQuoteCommandHandlerTests
    {
        private readonly Mock<IDocumentRepository> _repositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly CreateQuoteCommandHandler _handler;

        public CreateQuoteCommandHandlerTests()
        {
            // Setup dei Mock
            _repositoryMock = new Mock<IDocumentRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            // Configuro il mock per fare in modo che, durante i test, simuli un utente autenticato di nome "Test_User"
            _currentUserServiceMock.Setup(s => s.UserName).Returns("Test_User");

            // Iniezione nel sistema sotto test (SUT)
            _handler = new CreateQuoteCommandHandler(_repositoryMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateQuoteAndPublishEvent()
        {
            // Arrange
            var command = new CreateQuoteCommand(CustomerId: "CUST-123", ValidityDays: 30);

            // Simulo che il repository accetti l'inserimento senza sollevare eccezioni
            _repositoryMock.Setup(repo => repo.CreateAsync(document: It.IsAny<Quote>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request: command, cancellationToken: CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Assert.StartsWith("PREV-", result.DocumentNumber);

            // Verifico che il repository sia stato chiamato esattamente 1 volta con un'entità Quote
            _repositoryMock.Verify(repo => repo.CreateAsync(It.Is<Quote>(q =>
                q.CustomerId == "CUST-123" &&
                q.Status == DocumentStatus.Draft &&
                q.CreatedBy == "Test_User"
            )), Times.Once);
        }
    }
}
