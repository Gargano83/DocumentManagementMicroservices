using DocumentManagementMicroservices.IdentityService.Domain.Entities;
using DocumentManagementMicroservices.IdentityService.Services;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocumentManagementMicroservices.IdentityService.Features.Auth.Queries.Login
{
    /// <summary>
    /// Query Handler responsabile dell'autenticazione degli utenti e dell'emissione dei token JWT.
    /// </summary>
    public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponseDto>
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;

        public LoginQueryHandler(
            IMongoCollection<User> usersCollection,
            IPasswordHasher passwordHasher,
            IConfiguration configuration)
        {
            _usersCollection = usersCollection;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        /// <summary>
        /// Elabora le credenziali in ingresso, verificandone la validità crittografica, e restituisce un payload di accesso.
        /// </summary>
        public async Task<LoginResponseDto> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            // 1. Recupero dell'utente tramite indice univoco (Username)
            var user = await _usersCollection.Find(u => u.Username == request.Username).FirstOrDefaultAsync(cancellationToken);

            // 2. Viene restituito un messaggio generico ("Credenziali non valide") identico sia in caso di utente 
            // inesistente sia di password errata. In questo modo si evita di rivelare a un potenziale attaccante 
            // quale delle due informazioni sia scorretta.
            if (user is null || !_passwordHasher.Verify(password: request.Password, hash: user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenziali non valide.");
            }

            // 3. Costruzione dei Claims:
            // Informazioni fondamentali (Sub, Name, Role) necessarie al Document Service per applicare il controllo degli accessi basato sui ruoli.
            // Il claim JWT ID assicura l'univocità crittografica del token.
            var claims = new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 4. Se i segreti per la firma del token mancano,
            // l'handler interrompe immediatamente l'esecuzione sollevando un'eccezione, 
            // impedendo a monte l'emissione di token vulnerabili.
            var jwtSecret = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing.");
            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing.");
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing.");

            // 5. Firma Crittografica
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Restituisco token e il TTL (Time to live) esatto, permettendo al client 
            // di gestire timer locali per il redirect o il refresh della sessione.
            return new LoginResponseDto(tokenString, 7200);
        }
    }
}
