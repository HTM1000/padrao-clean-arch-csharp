using CleanArchTemplate.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.Api.Common;

/// <summary>Converte exceções não tratadas em ProblemDetails (RFC 7807).</summary>
public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        LogUnhandled(logger, exception);

        var (status, title, detail) = exception is DomainException domain
            ? (StatusCodes.Status400BadRequest, "Regra de domínio violada", domain.Message)
            : (StatusCodes.Status500InternalServerError, "Erro interno do servidor", "Ocorreu um erro inesperado.");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Exceção não tratada")]
    private static partial void LogUnhandled(ILogger logger, Exception exception);
}
