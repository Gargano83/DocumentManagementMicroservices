namespace DocumentManagementMicroservices.BuildingBlocks.Exceptions
{
    public class DomainException : Exception
    {
        public string ErrorCode { get; }

        public DomainException(string message, string errorCode = "DomainError") : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object key)
            : base($"Entity '{entityName}' ({key}) was not found.", "NotFound")
        {
        }
    }
}
