namespace DocumentManagementMicroservices.DocumentService.Domain.Entities
{
    public class SalesOrder : DocumentBase
    {
        /// <summary>
        /// Riferimento alla proforma da cui è stato generato
        /// </summary>
        public string? SourceProformaId { get; set; }

        /// <summary>
        /// Dati specifici dell'ordine che non sono presenti negli altri documenti
        /// </summary>
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
