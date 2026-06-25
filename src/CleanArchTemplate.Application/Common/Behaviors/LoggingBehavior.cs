using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchTemplate.Application.Common.Behaviors;

/// <summary>Loga início e fim de cada request do MediatR (via source-generated LoggerMessage).</summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        LogHandling(logger, requestName);
        var response = await next();
        LogHandled(logger, requestName);
        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Tratando {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tratado {RequestName}")]
    private static partial void LogHandled(ILogger logger, string requestName);
}
