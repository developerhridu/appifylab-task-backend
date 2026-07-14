using BuddyScript.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using ValidationException = BuddyScript.Application.Common.Exceptions.ValidationException;

namespace BuddyScript.Application.Common.Behaviours;

/// <summary>Runs all FluentValidation validators for a request before the handler; throws on failure.</summary>
public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var errors = failures
                    .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.Distinct().ToArray());

                throw new ValidationException(errors);
            }
        }

        return await next(cancellationToken);
    }
}
