using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocumentManagementMicroservices.BuildingBlocks.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                    Exception exception,
                                                    CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception has occurred.");

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Title = "Risorsa non trovata";
                    problemDetails.Detail = notFoundEx.Message;
                    problemDetails.Extensions["errorCode"] = notFoundEx.ErrorCode;
                    break;

                case DomainException domainEx:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Violazione delle regole di dominio";
                    problemDetails.Detail = domainEx.Message;
                    problemDetails.Extensions["errorCode"] = domainEx.ErrorCode;
                    break;

                case FluentValidation.ValidationException validationEx:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Validazione fallita";
                    problemDetails.Detail = "Si sono verificati uno o più errori di validazione.";
                    problemDetails.Extensions["errors"] = validationEx.Errors.GroupBy(x => x.PropertyName, x => x.ErrorMessage)
                                                                                .ToDictionary(x => x.Key, x => x.ToArray());
                    break;

                default:
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Errore interno del server";
                    problemDetails.Detail = "Si è verificato un errore inaspettato. Contattare il supporto tecnico se il problema persiste.";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
