using System.Runtime.Serialization;

namespace DocumentManagementMicroservices.BuildingBlocks.Exceptions
{
#pragma warning disable S3925 // "ISerializable should be implemented correctly". Ignorato perché la serializzazione basata su formatter è obsoleta in .NET 8+ (SYSLIB0051).
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
            : base($"L'entità '{entityName}' con chiave ({key}) non è stata trovata.", "NotFound")
        {
        }
    }
#pragma warning restore S3925
}
