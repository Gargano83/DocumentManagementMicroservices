using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus
{
    public record ChangeDocumentStatusCommand(string DocumentId,
                                                DocumentStatus NewStatus,
                                                int ExpectedVersion 
                                                ) : IRequest<DocumentStatusChangedDto>;
}
