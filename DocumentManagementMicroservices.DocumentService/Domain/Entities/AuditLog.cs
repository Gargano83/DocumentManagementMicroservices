namespace DocumentManagementMicroservices.DocumentService.Domain.Entities
{
    /// <summary>
    /// Entità che rappresenta una traccia di audit immutabile per gli eventi di dominio del sistema.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Identificativo univoco del record di audit.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Riferimento al documento (es. Preventivo, Ordine) che ha subito la mutazione.
        /// </summary>
        public string DocumentId { get; set; } = string.Empty;

        /// <summary>
        /// Tipologia dell'azione o dell'evento registrato.
        /// </summary>
        public string Action { get; set; } = "StatusChange";

        /// <summary>
        /// Payload o descrizione testuale della mutazione.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Istante temporale esatto in cui l'evento si è verificato sul server.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
