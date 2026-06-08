using DocumentManagementMicroservices.BuildingBlocks.Events;
using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MassTransit;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus
{
    /// <summary>
    /// Command Handler responsabile della transizione di stato nel ciclo di vita dei documenti.
    /// </summary>
    public class ChangeDocumentStatusCommandHandler : IRequestHandler<ChangeDocumentStatusCommand, DocumentStatusChangedDto>
    {
        private readonly IDocumentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ChangeDocumentStatusCommandHandler(IDocumentRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Elabora la transizione di stato garantendo la protezione contro scritture concorrenti.
        /// </summary>
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

            document.Status = request.NewStatus;

            // Persistenza con Concorrenza Ottimistica.
            // Se due client tentano simultaneamente di modificare lo stato partendo dalla stessa versione (ExpectedVersion),
            // solo il primo update avrà successo su MongoDB. Il secondo fallirà, proteggendo l'integrità del documento.
            var updateSuccess = await _repository.UpdateStatusWithConcurrencyAsync(
                document.Id,
                request.NewStatus,
                request.ExpectedVersion);

            if (!updateSuccess)
            {
                throw new DomainException($"Concorrenza rilevata. Il documento è stato modificato da un altro utente.", "ConcurrencyConflict");
            }

            // Pubblicazione dell'evento di integrazione asincrono.
            // Disaccoppia la logica core (il cambio stato) dalle operazioni di contorno (storicizzazione nell'Audit DB).
            await _publishEndpoint.Publish(new DocumentStatusChangedEvent(document.Id,
                                                                            oldStatus.ToString(),
                                                                            request.NewStatus.ToString(),
                                                                            DateTime.UtcNow
                                                                            ), cancellationToken);

            var newVersion = request.ExpectedVersion + 1;

            return new DocumentStatusChangedDto(document.Id, oldStatus.ToString(), request.NewStatus.ToString(), newVersion);
        }

        /// <summary>
        /// Definisce le transizioni consentite per i documenti commerciali.
        /// Sfrutta il pattern matching di C# per una definizione dichiarativa e leggibile delle regole consentite (passaggi di stato consentiti).
        /// </summary>
        private static void ValidateStateTransition(DocumentStatus currentStatus, DocumentStatus newStatus)
        {
            if (currentStatus == newStatus) return;

            bool isValid = (currentStatus, newStatus) switch
            {
                (DocumentStatus.Draft, DocumentStatus.Complete) => true,
                (DocumentStatus.Complete, DocumentStatus.Sent) => true,
                (DocumentStatus.Complete, DocumentStatus.Draft) => true,
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
