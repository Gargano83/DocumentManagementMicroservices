namespace DocumentManagementMicroservices.IdentityService.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            // BCrypt genera automaticamente un salt univoco e lo incorpora nell'hash risultante
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            // Estrae il salt dall'hash salvato e verifica se la password in chiaro corrisponde
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
