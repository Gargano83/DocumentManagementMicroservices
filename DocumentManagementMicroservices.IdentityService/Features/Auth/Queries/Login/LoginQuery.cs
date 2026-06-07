using MediatR;

namespace DocumentManagementMicroservices.IdentityService.Features.Auth.Queries.Login
{
    public record LoginQuery(string Username, string Password) : IRequest<LoginResponseDto>;
}
