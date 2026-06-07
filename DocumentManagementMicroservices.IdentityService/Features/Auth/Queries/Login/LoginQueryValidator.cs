using FluentValidation;

namespace DocumentManagementMicroservices.IdentityService.Features.Auth.Queries.Login
{
    public class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("L'username è obbligatorio.");

            RuleFor(x => x.Password).NotEmpty().WithMessage("La password è obbligatoria.");
        }
    }
}
