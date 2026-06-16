using DocumentManagementMicroservices.BuildingBlocks.Services;

namespace DocumentManagementMicroservices.DocumentService.Infrastracture.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Il NameIdentifier corrisponde al claim "sub" dell'IdentityService
        public string? UserId => _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].ToString();

        // Il Name corrisponde al claim "username" dell'IdentityService
        public string? UserName => _httpContextAccessor.HttpContext?.Request.Headers["X-User-Name"].ToString();
    }
}
