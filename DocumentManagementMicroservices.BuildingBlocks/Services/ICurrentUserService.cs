namespace DocumentManagementMicroservices.BuildingBlocks.Services
{
    /// <summary>
    /// Contratto per astrarre la lettura dell'utente correntemente autenticato sulla pipeline HTTP.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// L'Id univoco dell'utente.
        /// </summary>
        string? UserId { get; }

        /// <summary>
        /// Lo username leggibile dell'utente.
        /// </summary>
        string? UserName { get; }
    }
}
