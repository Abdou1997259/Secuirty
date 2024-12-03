using FluentValidation;
using MediatR;


using System.Collections.Generic;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Secuirty.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);
            var validationsFailure = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context)));
            var failures = validationsFailure
                .Where(r => r.Errors.Count > 0)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                throw new FluentValidation.ValidationException(failures);
            }


            return await next();
        }
    }
}
