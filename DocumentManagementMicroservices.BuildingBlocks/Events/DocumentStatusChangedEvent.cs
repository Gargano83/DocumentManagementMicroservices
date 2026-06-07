namespace DocumentManagementMicroservices.BuildingBlocks.Events
{
    public record DocumentStatusChangedEvent(string DocumentId, string OldStatus, string NewStatus, DateTime Timestamp);
}
