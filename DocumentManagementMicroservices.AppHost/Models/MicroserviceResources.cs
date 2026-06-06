namespace DocumentManagementMicroservices.AppHost.Models
{
    public record MicroserviceResources(
        IResourceBuilder<ProjectResource> IdentityService,
        IResourceBuilder<ProjectResource> DocumentService
    );
}
