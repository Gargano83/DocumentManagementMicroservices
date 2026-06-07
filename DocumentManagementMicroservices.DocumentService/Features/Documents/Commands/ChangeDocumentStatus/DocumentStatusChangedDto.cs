namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus
{
    public record DocumentStatusChangedDto(string Id, string OldStatus, string NewStatus, int NewVersion);
}
