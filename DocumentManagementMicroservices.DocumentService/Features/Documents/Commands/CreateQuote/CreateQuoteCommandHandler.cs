using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote
{
    /// <summary>
    /// Command Handler responsabile della creazione iniziale di un Preventivo (Quote).
    /// </summary>
    public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, QuoteCreatedDto>
    {
        private readonly IDocumentRepository _repository;

        public CreateQuoteCommandHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Istanzia e persiste una nuova entità Quote applicando le logiche di business iniziali.
        /// </summary>
        public async Task<QuoteCreatedDto> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
        {
            // Generazione della chiave di business univoca.
            var documentNumber = $"PREV-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            // Lo stato viene forzatamente impostato a 'Draft' (Bozza) in quanto è una regola di business immutabile alla creazione.
            var quote = new Quote
            {
                DocumentNumber = documentNumber,
                IssueDate = DateTime.UtcNow,
                CustomerId = request.CustomerId,
                Status = DocumentStatus.Draft,
                ValidUntil = DateTime.UtcNow.AddDays(request.ValidityDays),
                // TODO: In produzione, questo valore verrebbe estratto dall'HttpContext tramite un ICurrentUserService
                CreatedBy = "API_User"
            };

            // Salvataggio su MongoDB
            await _repository.CreateAsync(quote);

            // Mapping della risposta su un DTO.
            // Evitiamo rigorosamente l'esposizione dell'entità di dominio all'esterno,
            // restituendo solo i dati strettamente necessari al client per proseguire l'interazione.
            return new QuoteCreatedDto(quote.Id, quote.DocumentNumber);
        }
    }
}
