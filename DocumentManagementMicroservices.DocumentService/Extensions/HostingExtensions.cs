using Asp.Versioning;
using DocumentManagementMicroservices.BuildingBlocks.Behaviors;
using DocumentManagementMicroservices.BuildingBlocks.Middlewares;
using DocumentManagementMicroservices.BuildingBlocks.Services;
using DocumentManagementMicroservices.DocumentService.Features.AuditLogs.Consumers;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Data;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Services;
using FluentValidation;
using MassTransit;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

namespace DocumentManagementMicroservices.DocumentService.Extensions
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
            // MediatR e Pipeline Behaviors
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(IdempotencyBehavior<,>));
            });

            // FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            return builder;
        }

        public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            // MongoDB (tramite Aspire)
            builder.AddMongoDBClient("documentdb");
            builder.AddMongoDBClient("auditlogdb");

            // Repositories e Seeding
            builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
            builder.Services.AddHostedService<MongoDbSeeder>();

            // Cache (Redis tramite Aspire/HybridCache)
            builder.AddRedisDistributedCache("redis");
            builder.Services.AddHybridCache();

            // Identità utente loggato
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            return builder;
        }

        /// <summary>
        /// Configura il Message Broker (RabbitMQ) e registra i consumer asincroni per l'architettura Event-Driven.
        /// </summary>
        public static WebApplicationBuilder AddMessagingServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddMassTransit(x =>
            {
                // Standardizza i nomi degli endpoint su RabbitMQ usando il formato kebab-case 
                // (es. audit-log invece di AuditLog) per compatibilità e pulizia.
                x.SetKebabCaseEndpointNameFormatter();

                // Registrazione del consumer per la tracciabilità delle operazioni
                x.AddConsumer<AuditLogConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    // L'orchestrazione tramite .NET Aspire inietta automaticamente la stringa di connessione corretta.
                    var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        cfg.Host(connectionString);
                    }
                    // Auto-configurazione degli endpoint (code, exchange e binding) basata sui consumer registrati.
                    cfg.ConfigureEndpoints(context);
                });
            });

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
