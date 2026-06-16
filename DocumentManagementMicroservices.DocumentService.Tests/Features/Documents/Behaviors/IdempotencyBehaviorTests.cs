using DocumentManagementMicroservices.BuildingBlocks.Behaviors;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace DocumentManagementMicroservices.DocumentService.Tests.Features.Documents.Behaviors
{
    public class IdempotencyBehaviorTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<IdempotencyBehavior<CreateQuoteCommand, QuoteCreatedDto>>> _loggerMock;
        private readonly IdempotencyBehavior<CreateQuoteCommand, QuoteCreatedDto> _behavior;

        public IdempotencyBehaviorTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<IdempotencyBehavior<CreateQuoteCommand, QuoteCreatedDto>>>();
            _behavior = new IdempotencyBehavior<CreateQuoteCommand, QuoteCreatedDto>(_cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenIdempotencyKeyIsEmpty_ShouldBypassCacheAndCallNext()
        {
            // Arrange
            var command = new CreateQuoteCommand("CUST-123", 30) { IdempotencyKey = string.Empty };
            var expectedResponse = new QuoteCreatedDto("ID-123", "PREV-2026-XXXX");

            RequestHandlerDelegate<QuoteCreatedDto> nextDelegate = (ct) => Task.FromResult(expectedResponse);

            // Act
            var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

            // Assert
            Assert.Equal(expectedResponse, result);
            // Verifico che non sia mai stato interrogato il database di cache di Redis
            _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenCacheMiss_ShouldExecuteHandlerAndStoreResult()
        {
            // Arrange
            var command = new CreateQuoteCommand("CUST-123", 30) { IdempotencyKey = "unique-key-111" };
            var expectedResponse = new QuoteCreatedDto("ID-123", "PREV-2026-XXXX");

            // Simulo un Cache Miss (Redis restituisce null, ovvero chiave mai vista)
            _cacheMock.Setup(c => c.GetAsync("idempotency:unique-key-111", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

            RequestHandlerDelegate<QuoteCreatedDto> nextDelegate = (ct) => Task.FromResult(expectedResponse);

            // Act
            var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

            // Assert
            Assert.Equal(expectedResponse, result);

            // Verifico che il comportamento abbia salvato l'esito su Redis per le chiamate future
            _cacheMock.Verify(c => c.SetAsync(
                "idempotency:unique-key-111",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenCacheHit_ShouldReturnCachedResultWithoutExecutingHandler()
        {
            // Arrange
            var command = new CreateQuoteCommand("CUST-123", 30) { IdempotencyKey = "unique-key-222" };
            var cachedResponseDto = new QuoteCreatedDto("ID-OLD", "PREV-2026-OLD");
            var serializedResponse = JsonSerializer.Serialize(cachedResponseDto);
            var cachedBytes = Encoding.UTF8.GetBytes(serializedResponse);

            // Simulo un Cache Hit (Redis ha già la risposta pronta in memoria)
            _cacheMock.Setup(c => c.GetAsync("idempotency:unique-key-222", It.IsAny<CancellationToken>())).ReturnsAsync(cachedBytes);

            // Se il delegato 'next' viene chiamato, il test fallirà perché significa che l'handler è stato rieseguito
            RequestHandlerDelegate<QuoteCreatedDto> nextDelegate = (ct) => {
                throw new Exception("L'handler non doveva essere eseguito in caso di Cache Hit!");
            };

            // Act
            var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cachedResponseDto.Id, result.Id);
            Assert.Equal(cachedResponseDto.DocumentNumber, result.DocumentNumber);
        }
    }
}
