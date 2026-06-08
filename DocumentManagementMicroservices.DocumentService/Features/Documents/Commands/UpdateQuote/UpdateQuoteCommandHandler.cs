using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote
{
    /// <summary>
    /// Command Handler responsabile dell'aggiornamento di un Preventivo (Quote) esistente.
    /// </summary>
    public class UpdateQuoteCommandHandler : IRequestHandler<UpdateQuoteCommand, QuoteUpdatedDto>
    {
        private readonly IDocumentRepository _repository;

        public UpdateQuoteCommandHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Elabora la richiesta di aggiornamento applicando le regole di business e garantendo la consistenza transazionale.
        /// </summary>
        public async Task<QuoteUpdatedDto> Handle(UpdateQuoteCommand request, CancellationToken cancellationToken)
        {
            // Ci assicuriamo che il documento estratto sia effettivamente del tipo atteso (Quote) prima di tentare qualsiasi manipolazione.
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

            // Persistenza con controllo della concorrenza ottimistica.
            // Passando l'ExpectedVersion ricevuta dal client, il repository verificherà in modo atomico se su MongoDB
            // la versione sul database coincide ancora con quella in memoria. In caso contrario, la scrittura fallisce,
            // prevenendo la sovrascrittura accidentale di dati.
            var success = await _repository.UpdateQuoteAsync(quote, request.ExpectedVersion);

            if (!success)
            {
                throw new DomainException("Concorrenza rilevata. Il preventivo è stato modificato da un altro utente.", "ConcurrencyConflict");
            }

            // Ritorno del DTO con la versione incrementata per permettere al client di allineare la sua interfaccia
            return new QuoteUpdatedDto(quote.Id, request.ValidityDays, request.ExpectedVersion + 1);
        }
    }
}
