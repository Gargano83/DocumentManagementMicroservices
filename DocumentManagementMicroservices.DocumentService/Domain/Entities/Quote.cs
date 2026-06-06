namespace DocumentManagementMicroservices.DocumentService.Domain.Entities
{
    public class Quote : DocumentBase
    {
        /// <summary>
        /// Data di validità del preventivo
        /// </summary>
        public DateTime ValidUntil { get; set; }
    }
}
