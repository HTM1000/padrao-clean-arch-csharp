using MediatR;

namespace CleanArchTemplate.Application.Common.Messaging;

/// <summary>Marcador comum para comandos (usado pelos behaviors de validação e unidade de trabalho).</summary>
public interface ICommandBase;

/// <summary>Comando sem valor de retorno.</summary>
public interface ICommand : IRequest<Result>, ICommandBase;

/// <summary>Comando que retorna um valor.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase;
