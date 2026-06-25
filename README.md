# CleanArchTemplate

Template de backend **C# / .NET 10** com **Clean Architecture + DDD + CQRS**, rigor máximo de qualidade e uma fatia de exemplo (`Product`) ponta-a-ponta. Inclui autenticação JWT e persistência EF Core **multi-provider** (PostgreSQL **e** SQL Server).

## Camadas

```
Domain  ←  Application  ←  Infrastructure  →  Api
```

- **Domain** — núcleo sem dependências de infra. Entidades ricas, Value Objects, eventos de domínio, ports de repositório. Invariantes protegidas por factories e setters privados.
- **Application** — CQRS com MediatR (`ICommand`/`IQuery` sobre `Result<T>`), validação (FluentValidation) e behaviors (logging, validação, unidade de trabalho). Queries leem o `DbContext` direto projetando DTOs.
- **Infrastructure** — EF Core (provider selecionável), repositórios, `UnitOfWork` que faz commit + dispatch de eventos de domínio, e autenticação (JWT + PBKDF2).
- **Api** — controllers finos sobre `ISender`, `Result → IActionResult`, ProblemDetails (RFC 7807), Swagger com Bearer, health checks, Serilog.

A regra de dependência é **verificada por testes** (NetArchTest) em `tests/CleanArchTemplate.Architecture.Tests`.

## Gerar um projeto a partir do template

```bash
dotnet new install .
dotnet new cleanarch -n MinhaLoja
```

O `dotnet new` renomeia os namespaces de `CleanArchTemplate` para o nome informado.

> O template sai **completo e funcional** (fatia `Product` + auth) como referência. Para um projeto do zero, basta apagar as pastas `Products`/`Users` correspondentes em cada camada e remover os respectivos `DbSet`/registros.

## Rodar localmente

1. Suba um banco (exemplo Postgres):
   ```bash
   docker run -d --name cat-pg -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=cleanarch -p 5432:5432 postgres:16-alpine
   ```
2. Aplique as migrations:
   ```bash
   dotnet ef database update --project src/CleanArchTemplate.Infrastructure --startup-project src/CleanArchTemplate.Api -- --provider Postgres
   ```
3. Rode a API:
   ```bash
   dotnet run --project src/CleanArchTemplate.Api
   ```
4. Acesse o Swagger em `https://localhost:<porta>/swagger` e o health em `/health`.

## Trocar de provider (Postgres ↔ SQL Server)

Ambos já estão configurados. Selecione em `appsettings.json`:

```json
"Database": { "Provider": "SqlServer" },
"ConnectionStrings": { "Default": "Server=localhost;Database=cleanarch;Trusted_Connection=True;TrustServerCertificate=True" }
```

**Sobre as migrations:** o código (DbContext, repositórios, DI) é igual para os dois bancos — só as *migrations* são específicas de provider (o SQL gerado difere: `uuid` no Postgres vs `uniqueidentifier` no SQL Server). O EF Core rastreia migrations por `DbContext`, então **um único conjunto por vez**. O template já inclui as migrations do **Postgres** (provider default) em `Infrastructure/Persistence/Migrations/Postgres`.

Para usar **SQL Server**, regenere as migrations:

```bash
# 1. defina Database:Provider = SqlServer no appsettings
# 2. remova as migrations do Postgres
rm -r src/CleanArchTemplate.Infrastructure/Persistence/Migrations/Postgres
# 3. gere as do SQL Server
dotnet ef migrations add Initial --project src/CleanArchTemplate.Infrastructure --startup-project src/CleanArchTemplate.Api --output-dir Persistence/Migrations/SqlServer -- --provider SqlServer
```

> Para manter **os dois bancos simultaneamente** com migrations versionadas (ex.: produção em SQL Server e dev em Postgres), o padrão é separar as migrations em **assemblies dedicados por provider**. O template deixa o `DesignTimeDbContextFactory` e a seleção por configuração prontos para essa evolução.

## Qualidade (rigor máximo)

- `Nullable` + `TreatWarningsAsErrors` + `EnforceCodeStyleInBuild`.
- Central Package Management (`Directory.Packages.props`).
- Analyzers: Roslynator, SonarAnalyzer.CSharp, Meziantou.Analyzer.
- `.editorconfig` estrito (supressões pontuais documentadas com justificativa).
- CI em `.github/workflows/ci.yml`: build + `dotnet format --verify-no-changes` + testes.

## Testes

```bash
dotnet test
```

- **Domain.Tests** — invariantes, factories, Value Objects.
- **Application.Tests** — handlers (NSubstitute + FluentAssertions) e queries (EF InMemory).
- **Infrastructure.Tests** — roundtrip real com **Testcontainers** (Postgres). Requer Docker; marcados como `Category=Integration`.
- **Architecture.Tests** — regras de camada e convenções de nome (NetArchTest).

## Padrões de DDD

Veja a cola de DDD do projeto MegaStore: `D:\www\MegaStore\docs\ddd-cheatsheet.md`, e os ADRs em [`docs/adr`](docs/adr).
