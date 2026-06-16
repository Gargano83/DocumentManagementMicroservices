using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryHandlerTests
    {
        private readonly Mock<IDocumentRepository> _repositoryMock;
        private readonly GetDocumentByIdQueryHandler _handler;

        public GetDocumentByIdQueryHandlerTests()
        {
            _repositoryMock = new Mock<IDocumentRepository>();
            _handler = new GetDocumentByIdQueryHandler(_repositoryMock.Object, NullLogger<GetDocumentByIdQueryHandler>.Instance);
        }

        [Fact]
        public async Task Handle_WhenDocumentExists_ShouldReturnCorrectDto()
        {
            // Arrange
            var query = new GetDocumentByIdQuery("doc-123");
            var fakeDocument = new Quote
            {
                Id = "doc-123",
                DocumentNumber = "PREV-2026-001",
                CustomerId = "CUST-999",
                Status = DocumentStatus.Draft,
                IssueDate = DateTime.UtcNow
            };

            _repositoryMock.Setup(r => r.GetByIdAsync<DocumentBase>("doc-123")).ReturnsAsync(fakeDocument);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("doc-123", result.Id);
            Assert.Equal("PREV-2026-001", result.DocumentNumber);
            Assert.Equal("Quote", result.DocumentType);
        }

        [Fact]
        public async Task Handle_WhenDocumentDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var query = new GetDocumentByIdQuery("invalid-id");

            _repositoryMock.Setup(r => r.GetByIdAsync<DocumentBase>("invalid-id")).ReturnsAsync((DocumentBase?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }
    }
}
