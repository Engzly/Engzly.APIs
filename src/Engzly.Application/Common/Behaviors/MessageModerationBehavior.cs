using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Interfaces.Moderation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Common.Behaviors
{
    public sealed class MessageModerationBehavior<TRequest, TResponse>(
        IContentPolicy _contentPolicy,
        IProfanityFilter _profanityFilter,
        ILogger<MessageModerationBehavior<TRequest, TResponse>> _logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, IModeratedTextCommand
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var text = request.Text;
            if (string.IsNullOrWhiteSpace(text))
                return await next();

            var policyResult = _contentPolicy.Check(text);
            if (!policyResult.Allowed)
                return BuildBadRequest(policyResult.Reason ?? "Message blocked by content policy");

            try
            {
                var profanity = await _profanityFilter.CheckAsync(text, cancellationToken);
                if (profanity.Flagged)
                    return BuildBadRequest(profanity.Reason ?? "Message contains inappropriate language");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Profanity filter call failed — failing open for {Request}", typeof(TRequest).Name);
            }

            return await next();
        }

        private static TResponse BuildBadRequest(string message)
        {
            var responseType = typeof(TResponse);
            if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Response<>))
                throw new InvalidOperationException($"MessageModerationBehavior expects TResponse = Response<T>, got {responseType.Name}");

            var instance = Activator.CreateInstance(responseType)!;
            responseType.GetProperty(nameof(Response<object>.Succeeded))!.SetValue(instance, false);
            responseType.GetProperty(nameof(Response<object>.StatusCode))!.SetValue(instance, (int)System.Net.HttpStatusCode.BadRequest);
            responseType.GetProperty(nameof(Response<object>.Message))!.SetValue(instance, message);
            return (TResponse)instance;
        }
    }
}
