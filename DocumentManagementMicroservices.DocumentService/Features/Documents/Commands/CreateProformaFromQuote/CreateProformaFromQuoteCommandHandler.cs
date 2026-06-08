using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote
{
    /// <summary>
    /// Command Handler responsabile della transizione da Preventivo (Quote) a Fattura Proforma.
    /// </summary>
    public class CreateProformaFromQuoteCommandHandler : IRequestHandler<CreateProformaFromQuoteCommand, ProformaCreatedDto>
    {
        private readonly IDocumentRepository _repository;

        public CreateProformaFromQuoteCommandHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Esegue la validazione delle regole di dominio e genera la nuova entità derivata (Proforma).
        /// </summary>
        public async Task<ProformaCreatedDto> Handle(CreateProformaFromQuoteCommand request, CancellationToken cancellationToken)
        {
            // Estraiamo il documento base e verifichiamo che l'istanza deserializzata sia effettivamente un Quote.
            var document = await _repository.GetByIdAsync<DocumentBase>(request.QuoteId);

            if (document is null || document is not Quote quote)
            {
                throw new NotFoundException("Quote", request.QuoteId);
            }

            // Validazione del dominio: 
            // Un preventivo può evolvere in Proforma solo se ha raggiunto uno stato di validità commerciale accertata.
            if (quote.Status != DocumentStatus.Approved && quote.Status != DocumentStatus.Complete)
            {
                throw new DomainException($"Impossibile creare una Proforma da un Preventivo in stato: {quote.Status}");
            }

            // Costruzione della nuova entità derivata.
            // Applicazione della strategia di 'Referencing': invece di effettuare l'embedding (duplicazione) 
            // dell'intero payload di origine, manteniamo un riferimento esplicito.
            var proforma = new Proforma
            {
                DocumentNumber = $"PROF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4]}",
                IssueDate = DateTime.UtcNow,
                CustomerId = quote.CustomerId,
                Status = DocumentStatus.Draft,
                // Inizializzazione della versione per il controllo della concorrenza ottimistica
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                // Link di discendenza
                SourceQuoteId = quote.Id
            };

            await _repository.CreateAsync(proforma);

            return new ProformaCreatedDto(proforma.Id, proforma.DocumentNumber, proforma.Status);
        }
    }
}
