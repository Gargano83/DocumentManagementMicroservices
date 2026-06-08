using FluentValidation;
using MediatR;

namespace DocumentManagementMicroservices.BuildingBlocks.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
#pragma warning disable CA2016 // Il delegato RequestHandlerDelegate di MediatR non accetta il CancellationToken
                return await next();
#pragma warning restore CA2016
            }

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

#pragma warning disable CA2016 // Il delegato RequestHandlerDelegate di MediatR non accetta il CancellationToken
            return await next();
#pragma warning restore CA2016
        }
    }
}
