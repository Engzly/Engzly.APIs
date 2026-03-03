using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Engzly.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Serialize request data safely
        var requestData = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        logger.LogInformation("Handling request {RequestName} with data: {RequestData}", requestName, requestData);

        // Call the next delegate / handler
        var response = await next();

        // Serialize response safely
        var responseData = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        logger.LogInformation("Handled request {RequestName}. Response: {ResponseData}", requestName, responseData);

        return response;
    }
}