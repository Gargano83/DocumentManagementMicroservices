using DocumentManagementMicroservices.BuildingBlocks.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManagementMicroservices.BuildingBlocks.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddBuildingBlocksExceptionHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
    }
}
