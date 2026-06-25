# ADR 0002 — CQRS com MediatR e Result

- **Status:** Aceito
- **Data:** 2026-06-25

## Contexto

Comandos (escrita) e consultas (leitura) têm necessidades diferentes. Queremos handlers focados, validação e cross-cutting concerns sem poluir a regra de negócio, e fluxo de erro previsível.

## Decisão

Usar CQRS com **MediatR**: `ICommand`/`IQuery` sobre `Result<T>`. Comandos passam pelo domínio (carregam agregado, chamam método, salvam via `UnitOfWorkBehavior`). Queries leem o `DbContext` direto projetando DTOs, sem repositório de agregado. Erros esperados retornam `Result`/`Error` (sem exceptions). Behaviors aplicam logging, validação (FluentValidation) e unidade de trabalho.

Usamos **MediatR 12** e **FluentValidation 11** propositalmente (licença Apache-2.0), evitando as versões mais novas com licença comercial.

## Consequências

- (+) Handlers pequenos e testáveis; cross-cutting centralizado em behaviors.
- (+) Fluxo de erro explícito via `Result`.
- (−) Curva de aprendizado do pipeline do MediatR.
