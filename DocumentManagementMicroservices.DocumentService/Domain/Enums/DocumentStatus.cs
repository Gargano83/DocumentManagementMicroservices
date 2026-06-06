using System.ComponentModel;

namespace DocumentManagementMicroservices.DocumentService.Domain.Enums
{
    /// <summary>
    /// Rappresenta il ciclo di vita e gli stati possibili di un documento commerciale.
    /// </summary>
    public enum DocumentStatus
    {
        [Description("Bozza, documento incompleto o in fase di redazione")]
        Draft = 0,
        [Description("Documento completato e validato, pronto per l'invio")]
        Complete = 1,
        [Description("Documento inviato al cliente")]
        Sent = 2,
        [Description("Documento approvato/accettato dal cliente")]
        Approved = 3,
        [Description("Documento rifiutato")]
        Rejected = 4
    }
}
