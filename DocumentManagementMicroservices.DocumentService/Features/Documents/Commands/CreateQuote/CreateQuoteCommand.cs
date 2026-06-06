using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MassTransit;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote
{
    /// <summary>
    /// IL COMANDO: Rappresenta "l'intenzione" dell'utente.
    /// Dichiaro IRequest<string> perché mi aspetto che, una volta eseguito, restituisca l'ID di MongoDB (stringa).
    /// </summary>
    public class CreateQuoteCommand : IRequest<string>
    {
        public string CustomerId { get; set; } = string.Empty;
        public int ValidityDays { get; set; } = 30;
    }

    /// <summary>
    /// L'HANDLER: Contiene la logica di business per gestire esattamente e solo questo comando.
    /// </summary>
    public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, string>
    {
        private readonly IDocumentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        // Inietto solo le dipendenze necessarie per questa specifica slice
        public CreateQuoteCommandHandler(IDocumentRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<string> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
        {
            // Genero un numero di documento fittizio
            // TODO: in produzione meglio adottare una soluzione differente
            var documentNumber = $"PREV-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

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

            // Restituisco l'Id autogenerato da MongoDB
            return quote.Id;
        }
    }
}
