# CleanArchTemplate — Design / Spec

- **Data:** 2026-06-25
- **Autor:** Henrique (com Claude Code + superpowers)
- **Status:** Aprovado
- **Objetivo:** Template `dotnet new` de backend C# (.NET 10) com o melhor padrão de código possível — Clean Architecture + DDD + CQRS (MediatR), rigor máximo (analyzers, CPM, Architecture.Tests, CI). Reaproveita os padrões já consolidados no projeto MegaStore (`D:\www\MegaStore\.claude`).

---

## 1. Decisões aprovadas (resumo)

| Tema | Decisão |
|---|---|
| Nome raiz | `CleanArchTemplate` (parametrizável via `sourceName`) |
| .NET | **10** |
| CQRS | **MediatR** (`IRequest<Result<T>>`, behaviors) |
| Formato | **`dotnet new` template** (`.template.config/template.json`) |
| Fatia de exemplo | **1 fatia completa** (agregado `Product` ponta-a-ponta), removível com `--no-sample` |
| Persistência | **EF Core multi-provider** — Postgres **e** SQL Server prontos, selecionáveis por config (Abordagem A) |
| API | **Controllers** REST finos sobre MediatR |
| Auth | **JWT** com endpoints `register`/`login` + exemplo `[Authorize]` |
| Rigor | **Máximo** — `.editorconfig` estrito, CPM, `Nullable`+`TreatWarningsAsErrors`, analyzers, CI, husky |

---

## 2. Estrutura da solução

```
CleanArchTemplate/
├── .template.config/template.json      # mecânica do `dotnet new`
├── Directory.Build.props               # Nullable, warnings-as-errors, analyzers, langversion
├── Directory.Packages.props            # Central Package Management (todas as versões aqui)
├── .editorconfig                       # regras de estilo estritas (severidade = error)
├── .gitignore
├── CleanArchTemplate.slnx
├── README.md                           # guia de uso + padrões + link ddd-cheatsheet
├── .github/workflows/ci.yml
├── docs/
│   ├── superpowers/specs/              # este spec
│   └── adr/                            # ADRs iniciais
├── src/
│   ├── CleanArchTemplate.Domain/         # núcleo, ZERO dependências de infra
│   ├── CleanArchTemplate.Application/    # CQRS, depende só de Domain
│   ├── CleanArchTemplate.Infrastructure/ # EF Core, auth — depende de Application
│   └── CleanArchTemplate.Api/            # Controllers, composition root
└── tests/
    ├── CleanArchTemplate.Domain.Tests/
    ├── CleanArchTemplate.Application.Tests/
    ├── CleanArchTemplate.Infrastructure.Tests/
    └── CleanArchTemplate.Architecture.Tests/
```

**Regra de dependência** (validada por Architecture.Tests):
`Domain ← Application ← Infrastructure → Api`. Domain não referencia nenhum outro projeto/infra. Api é o composition root (único que conhece tudo).

---

## 3. Domain (building blocks)

- Bases: `Entity<TId>`, `AggregateRoot<TId>` (com `RaiseDomainEvent` / `IReadOnlyCollection<IDomainEvent> DomainEvents` / `ClearDomainEvents`), `ValueObject` (equality por componentes), `IDomainEvent`, `DomainException`.
- IDs fortemente tipados como `record struct` (ex.: `ProductId(Guid Value)`).
- Value Objects de exemplo: `Money` (decimal + currency, operações validadas), `Sku`.
- Ports de repositório declarados **aqui** (`IProductRepository`, `IUnitOfWork`), implementados na Infrastructure.
- Invariantes: setters privados, construtor privado + factory estática `Create(...)` validando, mutação só por métodos de domínio. Eventos de domínio em mudanças importantes.

## 4. Application (CQRS com MediatR)

- `Result` / `Result<T>` + `Error` tipado (código + mensagem + tipo: Validation/NotFound/Conflict/...). Sem exceptions para fluxo esperado.
- Interfaces marcadoras: `ICommand<T> : IRequest<Result<T>>`, `IQuery<T> : IRequest<Result<T>>`.
- **Pipeline behaviors** (ordem): `LoggingBehavior` → `ValidationBehavior` (FluentValidation) → `UnitOfWorkBehavior` (commit + dispatch de domain events — **só em commands**).
- Vertical slice por caso de uso: `Application/<Contexto>/Commands|Queries/<UseCase>/`.
- **Queries** leem `DbContext` direto projetando DTO — **não** usam repositório de agregado (padrão MegaStore).
- `DependencyInjection.AddApplication()` registra MediatR, validators e behaviors.

## 5. Infrastructure (EF Core multi-provider + auth)

- `AppDbContext` único.
- **Multi-provider (Abordagem A):** `DbProviderSelector` lê `Database:Provider` (`Postgres` | `SqlServer`) de config/env e aplica `UseNpgsql(...)` ou `UseSqlServer(...)` com `MigrationsAssembly`/`MigrationsHistoryTable` apropriado.
- **Migrations separadas por provider:** `Migrations/Postgres/` e `Migrations/SqlServer/`. `DesignTimeDbContextFactory` lê `--provider` para `dotnet ef migrations add <Nome> -- --provider Postgres|SqlServer`.
- `IEntityTypeConfiguration<T>` por entidade (sem atributos EF no Domain). Repositórios concretos + `UnitOfWork` (dispatch de domain events no SaveChanges).
- **Auth:** `JwtTokenGenerator` (`IJwtTokenGenerator`), `PasswordHasher` (`IPasswordHasher`, PBKDF2/identity), entidade/tabela `User`, casos de uso `RegisterUser`/`LoginUser` na Application.
- `DependencyInjection.AddInfrastructure(config)` registra DbContext, provider, repos, auth.

## 6. Api (Controllers finos)

- `BaseApiController` com helper `result.ToActionResult()` → mapeia `Error.Type` para 400/401/404/409/422.
- `ProductsController` (`CreateProduct`, `GetProductById`) + `AuthController` (`register`, `login`). Todos com `[ProducesResponseType]`.
- `GlobalExceptionHandler : IExceptionHandler` → ProblemDetails (RFC 7807).
- Swagger/OpenAPI com esquema Bearer, health checks (`/health`), Serilog (request logging).
- `Program.cs` minimal: `AddApplication()` + `AddInfrastructure(config)` + pipeline.

## 7. Fatia de exemplo (referência ponta-a-ponta)

Agregado **`Product`**: `ProductId`, `Sku Sku`, `string Name`, `Money Price`, factory `Create`, evento `ProductCreated`.
- `CreateProductCommand` + `CreateProductCommandValidator` + `CreateProductCommandHandler` → `Result<ProductId>`.
- `GetProductByIdQuery` + `ProductDto` (projeção direta do DbContext).
- Endpoints no `ProductsController`.
- Testes nas 4 camadas cobrindo a fatia.
- Todo o conteúdo de exemplo é envolvido por símbolo condicional do template; `--no-sample` o remove na geração.

## 8. Rigor máximo (qualidade enforced)

- `Directory.Build.props`: `Nullable=enable`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `LangVersion=latest`, `ImplicitUsings=enable`, `GenerateDocumentationFile` onde fizer sentido.
- Analyzers (via CPM): **Roslynator.Analyzers**, **SonarAnalyzer.CSharp**, **Meziantou.Analyzer**.
- `.editorconfig` estrito: naming conventions, `var` rules, ordenação de usings, `dotnet_diagnostic.*.severity = error` nas regras-chave.
- **CI (GitHub Actions `ci.yml`):** restore → build → `dotnet format --verify-no-changes` → test (matriz Postgres + SqlServer via Testcontainers) → coleta de cobertura.
- **husky.net** + commitlint (Conventional Commits) + `dotnet format` no pre-commit.

## 9. Testes (4 projetos)

- **Domain.Tests** — xUnit: invariantes, factories, VOs (`Money`, `Sku`).
- **Application.Tests** — xUnit + **NSubstitute** + **FluentAssertions**: handlers com repos mockados; ≥3 testes por handler (sucesso, falha de validação, falha de regra).
- **Infrastructure.Tests** — **Testcontainers** (Postgres e SqlServer): repos e migrations reais.
- **Architecture.Tests** — **NetArchTest**: Domain sem deps externas; Application não referencia Infrastructure; handlers `sealed` e sufixo `Handler`; commands `*Command`, queries `*Query`; controllers só dependem de `ISender`/`IMediator`.

## 10. Mecânica do `dotnet new`

- `.template.config/template.json`:
  - `sourceName: "CleanArchTemplate"`, `shortName: "cleanarch"`.
  - Parâmetros: `--provider` (choice: Postgres|SqlServer|Both, default Postgres), `--no-sample` (bool, default false), `--auth` (bool, default true).
- Instalação local: `dotnet new install .` → geração: `dotnet new cleanarch -n MinhaLoja`.
- `README.md`: uso, padrões (link ao `ddd-cheatsheet.md` do MegaStore), ADRs iniciais.

---

## 11. Critérios de sucesso (Definition of Done)

1. `dotnet build` limpo com warnings-as-errors (zero warnings).
2. `dotnet test` verde nos 4 projetos (com Testcontainers disponíveis).
3. `dotnet format --verify-no-changes` passa.
4. Architecture.Tests verdes (regras de camada respeitadas).
5. `dotnet new install .` + `dotnet new cleanarch -n Demo` gera projeto que compila e roda (`/health` 200, Swagger ok).
6. Troca de provider por config funciona (Postgres ↔ SqlServer).
7. CI verde no GitHub Actions.

## 12. Fora de escopo (YAGNI)

- Frontend.
- Mensageria/event bus externo (domain events ficam in-process).
- Multi-tenancy (o template é genérico; o MegaStore tem `StoreId`, aqui não).
- Múltiplos bounded contexts (uma fatia de referência basta).
