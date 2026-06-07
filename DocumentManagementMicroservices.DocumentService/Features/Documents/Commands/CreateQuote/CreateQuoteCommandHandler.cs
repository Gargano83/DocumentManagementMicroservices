using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MassTransit;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote
{
    public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, QuoteCreatedDto>
    {
        private readonly IDocumentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateQuoteCommandHandler(IDocumentRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<QuoteCreatedDto> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
        {
            // Genero un numero di documento fittizio
            var documentNumber = $"PREV-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var quote = new Quote
            {
                DocumentNumber = documentNumber,
                IssueDate = DateTime.UtcNow,
                CustomerId = request.CustomerId,
                Status = DocumentStatus.Draft,
                ValidUntil = DateTime.UtcNow.AddDays(request.ValidityDays),
                CreatedBy = "API_User"
            };

            // Salvataggio su MongoDB
            await _repository.CreateAsync(quote);

            // Pubblicazione dell'evento asincrono su RabbitMQ
            await _publishEndpoint.Publish(new DocumentCreatedEvent(
                quote.Id,
                quote.DocumentNumber,
                quote.CustomerId,
                quote.CreatedAt
            ), cancellationToken);

            // Restituisco il DTO invece della singola stringa
            return new QuoteCreatedDto(quote.Id, quote.DocumentNumber);
        }
    }
}
