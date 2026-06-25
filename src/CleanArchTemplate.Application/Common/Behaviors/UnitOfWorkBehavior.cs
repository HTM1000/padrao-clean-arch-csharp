using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Application.Common.Messaging;
using MediatR;

namespace CleanArchTemplate.Application.Common.Behaviors;

/// <summary>Após um comando bem-sucedido, confirma a unidade de trabalho (commit + dispatch de eventos).</summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();
        if (response.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
