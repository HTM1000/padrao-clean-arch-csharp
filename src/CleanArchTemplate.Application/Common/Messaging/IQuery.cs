using MediatR;

namespace CleanArchTemplate.Application.Common.Messaging;

/// <summary>Consulta read-only que retorna um valor. Não passa pelo behavior de unidade de trabalho.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
