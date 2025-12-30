using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Shared.Enums;

namespace Trava.Application.Common.Behaviors
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
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(result => result.Errors)
                    .Where(failure => failure != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    throw new AppValidationException(failures);
                }
            }

            return await next(cancellationToken);
        }
    }

    public class AppValidationException : AppException
    {
        public AppValidationException(IEnumerable<ValidationFailure> failures)
            : base(
                CustomCode.ProvidedInformationIsInValid,
                failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}").ToList())
        {
        }
    }
}