using DocumentManagementMicroservices.IdentityService.Domain.Entities;
using DocumentManagementMicroservices.IdentityService.Services;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocumentManagementMicroservices.IdentityService.Features.Auth.Queries.Login
{
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

        public async Task<LoginResponseDto> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _usersCollection.Find(u => u.Username == request.Username).FirstOrDefaultAsync(cancellationToken);

            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenziali non valide.");
            }

            var claims = new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwtSecret = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing.");
            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing.");
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDto(tokenString, 7200);
        }
    }
}
