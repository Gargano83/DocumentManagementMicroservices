using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote
{
    public class UpdateQuoteCommandHandler : IRequestHandler<UpdateQuoteCommand, QuoteUpdatedDto>
    {
        private readonly IDocumentRepository _repository;

        public UpdateQuoteCommandHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<QuoteUpdatedDto> Handle(UpdateQuoteCommand request, CancellationToken cancellationToken)
        {
            var document = await _repository.GetByIdAsync<DocumentBase>(request.QuoteId);

            if (document is null || document is not Quote quote)
            {
                throw new NotFoundException("Quote", request.QuoteId);
            }

            // Regola di Dominio: Solo i documenti in Draft possono essere modificati liberamente
            if (quote.Status != DocumentStatus.Draft)
            {
                throw new DomainException($"Impossibile modificare un preventivo in stato {quote.Status}. Solo le bozze (Draft) sono modificabili.");
            }

            // Applico le modifiche in memoria
            quote.CustomerId = request.CustomerId;
            quote.ValidUntil = DateTime.UtcNow.AddDays(request.ValidityDays);
            quote.UpdatedAt = DateTime.UtcNow;

            // Persistenza tramite un metodo di update con concorrenza
            var success = await _repository.UpdateQuoteAsync(quote, request.ExpectedVersion);

            if (!success)
            {
                throw new DomainException("Concorrenza rilevata. Il preventivo è stato modificato da un altro utente.", "ConcurrencyConflict");
            }

            return new QuoteUpdatedDto(quote.Id, request.ValidityDays, request.ExpectedVersion + 1);
        }
    }
}
