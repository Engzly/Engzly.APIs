using Engzly.Application.Common.Bases;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
                if (failures.Count > 0)
                {
                    var errors = failures.Select(f => f.PropertyName + ": " + f.ErrorMessage).ToList();
                    if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Response<>))
                    {
                        var dataType = typeof(TResponse).GenericTypeArguments[0];
                        var responseType = typeof(Response<>).MakeGenericType(dataType);
                        var response = Activator.CreateInstance(responseType) as dynamic;

                        response.Succeeded = false;
                        response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request
                        response.Message = "Validation Failed";
                        response.Errors = errors;
                        var message = failures.Select(x => x.PropertyName + ": " + x.ErrorMessage).FirstOrDefault();

                        return (TResponse)response;
                    }

                    throw new ValidationException(errors.FirstOrDefault());

                }
            }
            return await next();
        }
    }

}
