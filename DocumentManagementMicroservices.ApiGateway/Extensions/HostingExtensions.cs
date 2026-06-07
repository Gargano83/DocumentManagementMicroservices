using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DocumentManagementMicroservices.ApiGateway.Extensions
{
    public static class HostingExtensions
    {
        public static WebApplicationBuilder AddGatewayConfiguration(this WebApplicationBuilder builder)
        {
            // Configurazione Autenticazione (Validazione JWT in ingresso)
            var jwtSecret = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing.");
            var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing.");
            var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing.");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                    };
                });

            // Definizione della Policy per richiedere il token sulle rotte protette
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireJwt", policy => policy.RequireAuthenticatedUser());
            });

            // Configurazione di YARP con Service Discovery di Aspire
            builder.Services.AddReverseProxy()
                            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                            .AddServiceDiscoveryDestinationResolver();

            return builder;
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.MapDefaultEndpoints();

            app.UseHttpsRedirection();

            // L'ordine è vitale: prima capisco chi sei (AuthN), poi vedo se hai i permessi (AuthZ)
            app.UseAuthentication();
            app.UseAuthorization();

            // Infine, passo la palla a YARP per l'instradamento
            app.MapReverseProxy();

            return app;
        }
    }
}
