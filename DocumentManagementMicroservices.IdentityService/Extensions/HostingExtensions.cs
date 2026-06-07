using Asp.Versioning;
using DocumentManagementMicroservices.BuildingBlocks.Behaviors;
using DocumentManagementMicroservices.BuildingBlocks.Middlewares;
using DocumentManagementMicroservices.IdentityService.Domain.Entities;
using DocumentManagementMicroservices.IdentityService.Infrastructure.Data;
using DocumentManagementMicroservices.IdentityService.Services;
using FluentValidation;
using MongoDB.Driver;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

namespace DocumentManagementMicroservices.IdentityService.Extensions
{
    public static class HostingExtensions
    {
        public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers()
                            .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                            });

            // Configurazione Versionamento
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Configurazione OpenAPI
            builder.Services.AddOpenApi();

            // Configurazione Gestione Errori Centralizzata
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            return builder;
        }

        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
        {
            // Registrazione del servizio di Hashing come Singleton
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // Registrazione di MediatR (scansiona l'assembly corrente per trovare gli Handler)
            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // Registrazione di FluentValidation (scansiona l'assembly per trovare i Validator)
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            return builder;
        }

        public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            // MongoDB (tramite Aspire)
            builder.AddMongoDBClient("identitydb");

            // Repositories e Seeding
            builder.Services.AddSingleton(sp =>
            {
                var mongoClient = sp.GetRequiredService<IMongoClient>();
                var database = mongoClient.GetDatabase("identitydb");
                return database.GetCollection<User>("Users");
            });
            builder.Services.AddHostedService<IdentityDbSeeder>();

            return builder;
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseExceptionHandler();
            app.MapDefaultEndpoints();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
                app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
