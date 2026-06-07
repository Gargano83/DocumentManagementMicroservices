using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MassTransit;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus
{
    public class ChangeDocumentStatusCommandHandler : IRequestHandler<ChangeDocumentStatusCommand, DocumentStatusChangedDto>
    {
        private readonly IDocumentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ChangeDocumentStatusCommandHandler(IDocumentRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<DocumentStatusChangedDto> Handle(ChangeDocumentStatusCommand request, CancellationToken cancellationToken)
        {
            var document = await _repository.GetByIdAsync<DocumentBase>(request.DocumentId);

            if (document is null)
            {
                throw new NotFoundException("Document", request.DocumentId);
            }

            var oldStatus = document.Status;

            // Controllo di Dominio: Verifica se la transizione di stato è valida
            ValidateStateTransition(oldStatus, request.NewStatus);

            // Aggiornamento in memoria
            document.Status = request.NewStatus;

            // Persistenza con Concorrenza Ottimistica
            // Passiamo l'ExpectedVersion ricevuta dal comando. Il repository si occuperà dell'incremento.
            var updateSuccess = await _repository.UpdateStatusWithConcurrencyAsync(
                document.Id,
                request.NewStatus,
                request.ExpectedVersion);

            if (!updateSuccess)
            {
                // Questa eccezione verrà mappata come HTTP 409 Conflict nel GlobalExceptionHandler (da aggiungere se desiderato)
                throw new DomainException($"Concorrenza rilevata. Il documento è stato modificato da un altro utente.", "ConcurrencyConflict");
            }

            // Pubblicazione dell'evento
            await _publishEndpoint.Publish(new DocumentStatusChangedEvent(document.Id,
                                                                            oldStatus.ToString(),
                                                                            request.NewStatus.ToString(),
                                                                            DateTime.UtcNow
                                                                            ), cancellationToken);

            // Calcoliamo la nuova versione per il DTO di ritorno
            var newVersion = request.ExpectedVersion + 1;

            return new DocumentStatusChangedDto(document.Id, oldStatus.ToString(), request.NewStatus.ToString(), newVersion);
        }

        private void ValidateStateTransition(DocumentStatus currentStatus, DocumentStatus newStatus)
        {
            if (currentStatus == newStatus) return;

            bool isValid = (currentStatus, newStatus) switch
            {
                (DocumentStatus.Draft, DocumentStatus.Complete) => true,
                (DocumentStatus.Complete, DocumentStatus.Sent) => true,
                (DocumentStatus.Complete, DocumentStatus.Draft) => true, // Riportare in bozza
                (DocumentStatus.Sent, DocumentStatus.Approved) => true,
                (DocumentStatus.Sent, DocumentStatus.Rejected) => true,
                _ => false
            };

            if (!isValid)
            {
                throw new DomainException($"Transizione di stato non consentita da {currentStatus} a {newStatus}");
            }
        }
    }
}
