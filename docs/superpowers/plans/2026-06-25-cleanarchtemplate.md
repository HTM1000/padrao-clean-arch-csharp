# CleanArchTemplate Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construir um template `dotnet new` (shortName `cleanarch`) de backend C# .NET 10 com Clean Architecture + DDD + CQRS, rigor máximo, EF Core multi-provider (Postgres + SQL Server), JWT auth e uma fatia de exemplo `Product` ponta-a-ponta.

**Architecture:** Quatro camadas com regra de dependência `Domain ← Application ← Infrastructure → Api`, validada por testes de arquitetura (NetArchTest). CQRS com MediatR sobre `Result<T>`; persistência via EF Core com provider selecionável por configuração; Api de controllers finos. No final, embrulhado como template `dotnet new`.

**Tech Stack:** .NET 10, C# latest, MediatR, FluentValidation, EF Core (Npgsql + SqlServer), xUnit, NSubstitute, FluentAssertions, Testcontainers, NetArchTest, Roslynator/SonarAnalyzer/Meziantou, Serilog, husky.net, GitHub Actions.

## Global Constraints

- **TFM:** `net10.0` em todos os projetos. Verbatim no csproj: `<TargetFramework>net10.0</TargetFramework>`.
- **Nullable:** `enable` em todos os projetos (via `Directory.Build.props`).
- **Warnings as errors:** `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` global. Zero warnings é DoD.
- **Central Package Management:** todas as versões de pacote em `Directory.Packages.props`. csproj usa `<PackageReference Include="X" />` sem `Version`.
- **Namespace raiz / sourceName:** `CleanArchTemplate` (NÃO renomear manualmente — é o `sourceName` do template).
- **Domain sem dependências:** o projeto `CleanArchTemplate.Domain` não referencia nenhum pacote de infra (sem EF, sem MediatR).
- **Result, não exceptions:** fluxo esperado retorna `Result`/`Result<T>`. `DomainException` só para violação de invariante.
- **Commits:** Conventional Commits (`feat:`, `test:`, `chore:`, `docs:`, `build:`). Um commit por task no mínimo.
- **Working dir:** `D:\www\html-solutions\CleanArchTemplate` (git já inicializado, branch `main`).

---

### Task 1: Esqueleto da solução + configuração de build

**Files:**
- Create: `CleanArchTemplate.slnx`
- Create: `Directory.Build.props`
- Create: `Directory.Packages.props`
- Create: `.editorconfig`
- Create: `.gitignore`
- Create: `src/CleanArchTemplate.Domain/CleanArchTemplate.Domain.csproj`
- Create: `src/CleanArchTemplate.Application/CleanArchTemplate.Application.csproj`
- Create: `src/CleanArchTemplate.Infrastructure/CleanArchTemplate.Infrastructure.csproj`
- Create: `src/CleanArchTemplate.Api/CleanArchTemplate.Api.csproj`
- Create: `tests/CleanArchTemplate.Domain.Tests/CleanArchTemplate.Domain.Tests.csproj`
- Create: `tests/CleanArchTemplate.Application.Tests/CleanArchTemplate.Application.Tests.csproj`
- Create: `tests/CleanArchTemplate.Infrastructure.Tests/CleanArchTemplate.Infrastructure.Tests.csproj`
- Create: `tests/CleanArchTemplate.Architecture.Tests/CleanArchTemplate.Architecture.Tests.csproj`

**Interfaces:**
- Produces: estrutura de projetos + grafo de referências `Api→Infrastructure→Application→Domain`; CPM ativo; props globais.

- [ ] **Step 1: Criar projetos e solução**

```bash
cd "D:/www/html-solutions/CleanArchTemplate"
dotnet new sln -n CleanArchTemplate --format slnx
dotnet new classlib -n CleanArchTemplate.Domain        -o src/CleanArchTemplate.Domain        -f net10.0
dotnet new classlib -n CleanArchTemplate.Application   -o src/CleanArchTemplate.Application   -f net10.0
dotnet new classlib -n CleanArchTemplate.Infrastructure -o src/CleanArchTemplate.Infrastructure -f net10.0
dotnet new webapi   -n CleanArchTemplate.Api           -o src/CleanArchTemplate.Api           -f net10.0 --use-controllers
dotnet new xunit    -n CleanArchTemplate.Domain.Tests        -o tests/CleanArchTemplate.Domain.Tests        -f net10.0
dotnet new xunit    -n CleanArchTemplate.Application.Tests   -o tests/CleanArchTemplate.Application.Tests   -f net10.0
dotnet new xunit    -n CleanArchTemplate.Infrastructure.Tests -o tests/CleanArchTemplate.Infrastructure.Tests -f net10.0
dotnet new xunit    -n CleanArchTemplate.Architecture.Tests  -o tests/CleanArchTemplate.Architecture.Tests  -f net10.0
# remover Class1.cs/UnitTest1.cs default
find src tests -name "Class1.cs" -delete
find tests -name "UnitTest1.cs" -delete
```

- [ ] **Step 2: Referências de projeto (regra de dependência)**

```bash
dotnet add src/CleanArchTemplate.Application reference src/CleanArchTemplate.Domain
dotnet add src/CleanArchTemplate.Infrastructure reference src/CleanArchTemplate.Application
dotnet add src/CleanArchTemplate.Api reference src/CleanArchTemplate.Infrastructure
dotnet add tests/CleanArchTemplate.Domain.Tests reference src/CleanArchTemplate.Domain
dotnet add tests/CleanArchTemplate.Application.Tests reference src/CleanArchTemplate.Application
dotnet add tests/CleanArchTemplate.Infrastructure.Tests reference src/CleanArchTemplate.Infrastructure
dotnet add tests/CleanArchTemplate.Architecture.Tests reference src/CleanArchTemplate.Api
# adicionar todos à solução
dotnet sln add (Get-ChildItem -r *.csproj)   # PowerShell; em bash: dotnet sln add $(find . -name *.csproj)
```

- [ ] **Step 3: `Directory.Packages.props` (CPM — versões centralizadas)**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="MediatR" Version="12.4.1" />
    <PackageVersion Include="FluentValidation" Version="11.10.0" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.10.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
    <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.2.0" />
    <PackageVersion Include="Serilog.AspNetCore" Version="8.0.3" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="7.2.0" />
    <!-- analyzers -->
    <PackageVersion Include="Roslynator.Analyzers" Version="4.12.9" />
    <PackageVersion Include="SonarAnalyzer.CSharp" Version="10.4.0.108396" />
    <PackageVersion Include="Meziantou.Analyzer" Version="2.0.182" />
    <!-- testes -->
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
    <PackageVersion Include="FluentAssertions" Version="6.12.2" />
    <PackageVersion Include="NetArchTest.Rules" Version="1.3.2" />
    <PackageVersion Include="Testcontainers.PostgreSql" Version="4.1.0" />
    <PackageVersion Include="Testcontainers.MsSql" Version="4.1.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>
</Project>
```

> Nota ao implementador: confirme as versões disponíveis com `dotnet add package <X> --prerelease` se alguma 10.0.0 ainda não tiver GA no momento da execução; ajuste o número, mantendo o pacote.

- [ ] **Step 4: `Directory.Build.props` (rigor global)**

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <!-- analyzers em todos os projetos -->
  <ItemGroup>
    <PackageReference Include="Roslynator.Analyzers" PrivateAssets="all" />
    <PackageReference Include="SonarAnalyzer.CSharp" PrivateAssets="all" />
    <PackageReference Include="Meziantou.Analyzer" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

- [ ] **Step 5: `.editorconfig` estrito** (criar com regras de naming, `var`, usings ordenados e severidades-chave como `error`; bloco mínimo abaixo, expandir conforme dotnet default)

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
dotnet_sort_system_directives_first = true
csharp_using_directive_placement = outside_namespace:error
csharp_style_namespace_declarations = file_scoped:error
dotnet_style_qualification_for_field = false:error
csharp_prefer_braces = true:error
dotnet_diagnostic.IDE0005.severity = error   # usings desnecessários
dotnet_diagnostic.CA1822.severity = warning
dotnet_naming_rule.private_fields_underscore.severity = error
dotnet_naming_rule.private_fields_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_underscore.style = underscore_prefix
dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private
dotnet_naming_style.underscore_prefix.required_prefix = _
dotnet_naming_style.underscore_prefix.capitalization = camel_case
```

- [ ] **Step 6: `.gitignore`**

```bash
dotnet new gitignore
```

- [ ] **Step 7: Build de verificação**

Run: `dotnet build`
Expected: build **SUCCEEDED**, 0 warnings, 0 errors.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "build: scaffold da solução com 4 camadas, CPM, analyzers e editorconfig"
```

---

### Task 2: Building blocks do Domain

**Files:**
- Create: `src/CleanArchTemplate.Domain/Common/Entity.cs`
- Create: `src/CleanArchTemplate.Domain/Common/AggregateRoot.cs`
- Create: `src/CleanArchTemplate.Domain/Common/ValueObject.cs`
- Create: `src/CleanArchTemplate.Domain/Common/IDomainEvent.cs`
- Create: `src/CleanArchTemplate.Domain/Common/DomainException.cs`
- Test: `tests/CleanArchTemplate.Domain.Tests/Common/AggregateRootTests.cs`
- Test: `tests/CleanArchTemplate.Domain.Tests/Common/ValueObjectTests.cs`

**Interfaces:**
- Produces: `Entity<TId>`, `AggregateRoot<TId>` (`RaiseDomainEvent`, `IReadOnlyCollection<IDomainEvent> DomainEvents`, `ClearDomainEvents()`), `ValueObject` (`GetEqualityComponents()`), `IDomainEvent` (`: MediatR.INotification`? NÃO — Domain não conhece MediatR; ver nota), `DomainException`.

> **Decisão:** `IDomainEvent` é uma interface vazia própria do Domain. A ponte com MediatR.INotification é feita na Infrastructure no dispatch (um wrapper), mantendo o Domain limpo.

- [ ] **Step 1: Test de ValueObject (falha)**

```csharp
using CleanArchTemplate.Domain.Common;
using FluentAssertions;
using Xunit;

namespace CleanArchTemplate.Domain.Tests.Common;

public sealed class ValueObjectTests
{
    private sealed class Pair(int a, int b) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return a;
            yield return b;
        }
    }

    [Fact]
    public void Equal_when_components_match()
    {
        new Pair(1, 2).Should().Be(new Pair(1, 2));
        (new Pair(1, 2) == new Pair(1, 2)).Should().BeTrue();
    }

    [Fact]
    public void NotEqual_when_components_differ()
    {
        new Pair(1, 2).Should().NotBe(new Pair(2, 1));
    }
}
```

- [ ] **Step 2: Rodar e ver falhar**

Run: `dotnet test tests/CleanArchTemplate.Domain.Tests`
Expected: FAIL — `ValueObject` não existe.

- [ ] **Step 3: Implementar building blocks**

`ValueObject.cs`:
```csharp
namespace CleanArchTemplate.Domain.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && GetType() == other.GetType()
        && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => obj is ValueObject vo && Equals(vo);

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (h, c) => HashCode.Combine(h, c));

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
```

`IDomainEvent.cs`:
```csharp
namespace CleanArchTemplate.Domain.Common;

public interface IDomainEvent;
```

`Entity.cs`:
```csharp
namespace CleanArchTemplate.Domain.Common;

public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>>
    where TId : struct
{
    public TId Id { get; } = id;

    public bool Equals(Entity<TId>? other) =>
        other is not null && GetType() == other.GetType() && Id.Equals(other.Id);

    public override bool Equals(object? obj) => obj is Entity<TId> e && Equals(e);
    public override int GetHashCode() => Id.GetHashCode();
}
```

`AggregateRoot.cs`:
```csharp
namespace CleanArchTemplate.Domain.Common;

public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id)
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

`DomainException.cs`:
```csharp
namespace CleanArchTemplate.Domain.Common;

public sealed class DomainException(string message) : Exception(message);
```

- [ ] **Step 4: Test de AggregateRoot (domain events)**

```csharp
using CleanArchTemplate.Domain.Common;
using FluentAssertions;
using Xunit;

namespace CleanArchTemplate.Domain.Tests.Common;

public sealed class AggregateRootTests
{
    private sealed record Created : IDomainEvent;
    private sealed class Sample() : AggregateRoot<Guid>(Guid.NewGuid())
    {
        public void Do() => RaiseDomainEvent(new Created());
    }

    [Fact]
    public void Raises_and_clears_domain_events()
    {
        var s = new Sample();
        s.Do();
        s.DomainEvents.Should().ContainSingle();
        s.ClearDomainEvents();
        s.DomainEvents.Should().BeEmpty();
    }
}
```

- [ ] **Step 5: Rodar e ver passar**

Run: `dotnet test tests/CleanArchTemplate.Domain.Tests`
Expected: PASS (todos).

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(domain): building blocks (Entity, AggregateRoot, ValueObject, DomainException)"
```

---

### Task 3: Result / Error na Application

**Files:**
- Create: `src/CleanArchTemplate.Application/Common/Result.cs`
- Create: `src/CleanArchTemplate.Application/Common/Error.cs`
- Test: `tests/CleanArchTemplate.Application.Tests/Common/ResultTests.cs`

**Interfaces:**
- Produces: `Error(string Code, string Message, ErrorType Type)` com `enum ErrorType { Validation, NotFound, Conflict, Unauthorized, Failure }` e estáticos `Error.NotFound/Conflict/...`; `Result` (`IsSuccess`, `Error`, `Success()`, `Failure(Error)`); `Result<T>` (`Value`, conversões implícitas, `Success(T)`).

- [ ] **Step 1: Test (falha)**

```csharp
using CleanArchTemplate.Application.Common;
using FluentAssertions;
using Xunit;

namespace CleanArchTemplate.Application.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_has_no_error()
    {
        var r = Result<int>.Success(5);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(5);
    }

    [Fact]
    public void Failure_carries_error()
    {
        var r = Result<int>.Failure(Error.NotFound("X", "not found"));
        r.IsSuccess.Should().BeFalse();
        r.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Implicit_from_value_is_success()
    {
        Result<int> r = 42;
        r.IsSuccess.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Rodar/falhar** — `dotnet test tests/CleanArchTemplate.Application.Tests` → FAIL.

- [ ] **Step 3: Implementar**

`Error.cs`:
```csharp
namespace CleanArchTemplate.Application.Common;

public enum ErrorType { Validation, NotFound, Conflict, Unauthorized, Failure }

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
}
```

`Result.cs`:
```csharp
namespace CleanArchTemplate.Application.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Sucesso não pode ter erro.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Falha precisa de erro.");
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;
    private Result(T value) : base(true, Error.None) => _value = value;
    private Result(Error error) : base(false, error) => _value = default;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não há valor em um resultado de falha.");

    public static new Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
}
```

- [ ] **Step 4: Rodar/passar** — `dotnet test tests/CleanArchTemplate.Application.Tests` → PASS.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(application): Result/Error pattern"
```

---

### Task 4: Abstrações CQRS + pipeline behaviors + DI

**Files:**
- Create: `src/CleanArchTemplate.Application/Common/Messaging/ICommand.cs`
- Create: `src/CleanArchTemplate.Application/Common/Messaging/IQuery.cs`
- Create: `src/CleanArchTemplate.Application/Common/Behaviors/ValidationBehavior.cs`
- Create: `src/CleanArchTemplate.Application/Common/Behaviors/LoggingBehavior.cs`
- Create: `src/CleanArchTemplate.Application/Common/Behaviors/UnitOfWorkBehavior.cs`
- Create: `src/CleanArchTemplate.Application/Common/Abstractions/IUnitOfWork.cs`
- Create: `src/CleanArchTemplate.Application/DependencyInjection.cs`
- Test: `tests/CleanArchTemplate.Application.Tests/Common/Behaviors/ValidationBehaviorTests.cs`

**Interfaces:**
- Consumes: `Result`, `Result<T>`, `Error` (Task 3).
- Produces: `ICommand<T> : IRequest<Result<T>>`, `ICommand : IRequest<Result>`, `IQuery<T> : IRequest<Result<T>>`, marcador `ICommandBase`; `IUnitOfWork.SaveChangesAsync(CancellationToken)`; `AddApplication(IServiceCollection)`.

- [ ] **Step 1: Messaging markers**

`ICommand.cs`:
```csharp
using MediatR;
using CleanArchTemplate.Application.Common;

namespace CleanArchTemplate.Application.Common.Messaging;

public interface ICommandBase;
public interface ICommand : IRequest<Result>, ICommandBase;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase;
```

`IQuery.cs`:
```csharp
using MediatR;
using CleanArchTemplate.Application.Common;

namespace CleanArchTemplate.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
```

`IUnitOfWork.cs`:
```csharp
namespace CleanArchTemplate.Application.Common.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Test do ValidationBehavior (falha)**

```csharp
using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Behaviors;
using CleanArchTemplate.Application.Common.Messaging;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace CleanArchTemplate.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record Cmd(int X) : ICommand<int>;
    private sealed class CmdValidator : AbstractValidator<Cmd>
    {
        public CmdValidator() => RuleFor(c => c.X).GreaterThan(0);
    }

    [Fact]
    public async Task Returns_validation_failure_when_invalid()
    {
        var behavior = new ValidationBehavior<Cmd, Result<int>>([new CmdValidator()]);
        RequestHandlerDelegate<Result<int>> next = (_) => Task.FromResult(Result<int>.Success(1));

        var result = await behavior.Handle(new Cmd(0), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Calls_next_when_valid()
    {
        var behavior = new ValidationBehavior<Cmd, Result<int>>([new CmdValidator()]);
        RequestHandlerDelegate<Result<int>> next = (_) => Task.FromResult(Result<int>.Success(99));

        var result = await behavior.Handle(new Cmd(5), next, CancellationToken.None);

        result.Value.Should().Be(99);
    }
}
```

- [ ] **Step 3: Rodar/falhar** → FAIL.

- [ ] **Step 4: Implementar behaviors**

`ValidationBehavior.cs` (só valida `ICommandBase`; cria `Result`/`Result<T>` de falha por reflexão do tipo de retorno):
```csharp
using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Messaging;
using FluentValidation;
using MediatR;

namespace CleanArchTemplate.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any())
            return await next(ct);

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(ct);

        var error = Error.Validation("Validation", string.Join("; ", failures.Select(f => f.ErrorMessage)));
        return CreateFailure(error);
    }

    private static TResponse CreateFailure(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failure = typeof(Result<>).MakeGenericType(valueType)
            .GetMethod(nameof(Result<object>.Failure))!
            .Invoke(null, [error])!;
        return (TResponse)failure;
    }
}
```

`LoggingBehavior.cs`:
```csharp
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchTemplate.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling {Request}", name);
        var response = await next(ct);
        logger.LogInformation("Handled {Request}", name);
        return response;
    }
}
```

`UnitOfWorkBehavior.cs` (só em commands; salva após sucesso):
```csharp
using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Application.Common.Messaging;
using MediatR;

namespace CleanArchTemplate.Application.Common.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next(ct);
        if (response.IsSuccess)
            await unitOfWork.SaveChangesAsync(ct);
        return response;
    }
}
```

`Microsoft.Extensions.Logging.Abstractions` é trazido transitivamente via MediatR/DI; se faltar, adicionar PackageReference. Adicione ao csproj da Application: `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`.

- [ ] **Step 5: `DependencyInjection.cs`**

```csharp
using System.Reflection;
using CleanArchTemplate.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchTemplate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        return services;
    }
}
```

- [ ] **Step 6: Rodar/passar** → PASS.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat(application): CQRS markers + validation/logging/uow behaviors + DI"
```

---

### Task 5: Agregado de exemplo `Product` (Domain)

**Files:**
- Create: `src/CleanArchTemplate.Domain/Products/ProductId.cs`
- Create: `src/CleanArchTemplate.Domain/Products/ValueObjects/Money.cs`
- Create: `src/CleanArchTemplate.Domain/Products/ValueObjects/Sku.cs`
- Create: `src/CleanArchTemplate.Domain/Products/Product.cs`
- Create: `src/CleanArchTemplate.Domain/Products/Events/ProductCreated.cs`
- Create: `src/CleanArchTemplate.Domain/Products/IProductRepository.cs`
- Test: `tests/CleanArchTemplate.Domain.Tests/Products/ProductTests.cs`
- Test: `tests/CleanArchTemplate.Domain.Tests/Products/ValueObjects/MoneyTests.cs`

**Interfaces:**
- Consumes: building blocks (Task 2).
- Produces: `ProductId(Guid Value)` record struct; `Money(decimal Amount, string Currency)`; `Sku(string Value)`; `Product` com `Create(Sku, string name, Money price) -> Product`, props `Id/Sku/Name/Price`, evento `ProductCreated(ProductId, Sku, Money)`; `IProductRepository.AddAsync(Product, CancellationToken)`, `GetByIdAsync(ProductId, CancellationToken) -> Product?`.

- [ ] **Step 1: Test de Money e Product (falha)** — `MoneyTests.cs`:

```csharp
using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace CleanArchTemplate.Domain.Tests.Products.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_rejects_negative()
    {
        var act = () => Money.Create(-1m, "BRL");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equal_by_value()
    {
        Money.Create(10m, "BRL").Should().Be(Money.Create(10m, "BRL"));
    }
}
```

`ProductTests.cs`:
```csharp
using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.Events;
using CleanArchTemplate.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace CleanArchTemplate.Domain.Tests.Products;

public sealed class ProductTests
{
    [Fact]
    public void Create_sets_props_and_raises_event()
    {
        var p = Product.Create(Sku.Create("ABC-1"), "Camisa", Money.Create(50m, "BRL"));
        p.Name.Should().Be("Camisa");
        p.DomainEvents.Should().ContainSingle(e => e is ProductCreated);
    }

    [Fact]
    public void Create_rejects_empty_name()
    {
        var act = () => Product.Create(Sku.Create("ABC-1"), " ", Money.Create(50m, "BRL"));
        act.Should().Throw<DomainException>();
    }
}
```

- [ ] **Step 2: Rodar/falhar** → FAIL.

- [ ] **Step 3: Implementar**

`ProductId.cs`:
```csharp
namespace CleanArchTemplate.Domain.Products;

public readonly record struct ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());
}
```

`Money.cs`:
```csharp
using CleanArchTemplate.Domain.Common;

namespace CleanArchTemplate.Domain.Products.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("Valor não pode ser negativo.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Moeda obrigatória.");
        return new Money(amount, currency.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

`Sku.cs`:
```csharp
using CleanArchTemplate.Domain.Common;

namespace CleanArchTemplate.Domain.Products.ValueObjects;

public sealed class Sku : ValueObject
{
    public string Value { get; }
    private Sku(string value) => Value = value;

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException("SKU obrigatório.");
        return new Sku(value.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
}
```

`ProductCreated.cs`:
```csharp
using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products.ValueObjects;

namespace CleanArchTemplate.Domain.Products.Events;

public sealed record ProductCreated(ProductId ProductId, Sku Sku, Money Price) : IDomainEvent;
```

`Product.cs`:
```csharp
using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products.Events;
using CleanArchTemplate.Domain.Products.ValueObjects;

namespace CleanArchTemplate.Domain.Products;

public sealed class Product : AggregateRoot<ProductId>
{
    public Sku Sku { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }

    private Product(ProductId id, Sku sku, string name, Money price) : base(id)
    {
        Sku = sku;
        Name = name;
        Price = price;
    }

    // ctor para o EF (sem eventos)
    private Product() : base(default)
    {
        Sku = null!;
        Name = null!;
        Price = null!;
    }

    public static Product Create(Sku sku, string name, Money price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nome obrigatório.");
        var product = new Product(ProductId.New(), sku, name.Trim(), price);
        product.RaiseDomainEvent(new ProductCreated(product.Id, sku, price));
        return product;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nome obrigatório.");
        Name = name.Trim();
    }
}
```

`IProductRepository.cs`:
```csharp
namespace CleanArchTemplate.Domain.Products;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Rodar/passar** → PASS.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(domain): agregado Product com Money/Sku, factory, evento e repo port"
```

---

### Task 6: Slice `CreateProduct` (Application)

**Files:**
- Create: `src/CleanArchTemplate.Application/Products/Commands/CreateProduct/CreateProductCommand.cs`
- Create: `src/CleanArchTemplate.Application/Products/Commands/CreateProduct/CreateProductCommandValidator.cs`
- Create: `src/CleanArchTemplate.Application/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`
- Test: `tests/CleanArchTemplate.Application.Tests/Products/Commands/CreateProductCommandHandlerTests.cs`

**Interfaces:**
- Consumes: `ICommand<T>`, `Result<T>`, `Error`, `IProductRepository`, `Product`, `Money`, `Sku`, `ProductId`.
- Produces: `CreateProductCommand(string Sku, string Name, decimal Price, string Currency) : ICommand<Guid>`; handler retorna `Result<Guid>` com `product.Id.Value`.

- [ ] **Step 1: Test do handler (falha)**

```csharp
using CleanArchTemplate.Application.Products.Commands.CreateProduct;
using CleanArchTemplate.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CleanArchTemplate.Application.Tests.Products.Commands;

public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _repo = Substitute.For<IProductRepository>();

    [Fact]
    public async Task Creates_product_and_returns_id()
    {
        var sut = new CreateProductCommandHandler(_repo);
        var result = await sut.Handle(new CreateProductCommand("ABC-1", "Camisa", 50m, "BRL"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 2: Rodar/falhar** → FAIL.

- [ ] **Step 3: Implementar**

`CreateProductCommand.cs`:
```csharp
using CleanArchTemplate.Application.Common.Messaging;

namespace CleanArchTemplate.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Sku, string Name, decimal Price, string Currency)
    : ICommand<Guid>;
```

`CreateProductCommandValidator.cs`:
```csharp
using FluentValidation;

namespace CleanArchTemplate.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
```

`CreateProductCommandHandler.cs`:
```csharp
using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Messaging;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.ValueObjects;
using MediatR;

namespace CleanArchTemplate.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductRepository products)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            Sku.Create(command.Sku),
            command.Name,
            Money.Create(command.Price, command.Currency));

        await products.AddAsync(product, cancellationToken);
        return product.Id.Value;
    }
}
```

> Nota: o `SaveChanges` é responsabilidade do `UnitOfWorkBehavior` — o handler não chama `SaveChanges`.

- [ ] **Step 4: Rodar/passar** → PASS.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(application): slice CreateProduct (command + validator + handler)"
```

---

### Task 7: Slice `GetProductById` (Application)

**Files:**
- Create: `src/CleanArchTemplate.Application/Common/Abstractions/IApplicationDbContext.cs`
- Create: `src/CleanArchTemplate.Application/Products/Queries/GetProductById/GetProductByIdQuery.cs`
- Create: `src/CleanArchTemplate.Application/Products/Queries/GetProductById/ProductDto.cs`
- Create: `src/CleanArchTemplate.Application/Products/Queries/GetProductById/GetProductByIdQueryHandler.cs`
- Test: `tests/CleanArchTemplate.Application.Tests/Products/Queries/GetProductByIdQueryHandlerTests.cs`

**Interfaces:**
- Consumes: `IQuery<T>`, `Result<T>`, `Error`.
- Produces: `IApplicationDbContext` expõe `DbSet<Product> Products` (read model) — implementado pelo `AppDbContext` na Task 8; `GetProductByIdQuery(Guid Id) : IQuery<ProductDto>`; `ProductDto(Guid Id, string Sku, string Name, decimal Price, string Currency)`.

> **Decisão (padrão MegaStore):** Query lê o DbContext direto projetando DTO, **sem** usar `IProductRepository`. Para testabilidade sem banco, a query usa um `IApplicationDbContext` mockável com EF in-memory no teste.

- [ ] **Step 1: `IApplicationDbContext`**

```csharp
using CleanArchTemplate.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Common.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

> Isso adiciona `Microsoft.EntityFrameworkCore` à Application. É aceitável (EF Core abstractions na Application é padrão comum); o Domain continua limpo. Architecture.Tests (Task 13) permitirá EF na Application mas proíbe no Domain.

- [ ] **Step 2: Test (falha) usando EF InMemory**

Adicionar ao csproj de Application.Tests: `Microsoft.EntityFrameworkCore.InMemory` (add ao CPM: `<PackageVersion Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />`).

```csharp
using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Application.Products.Queries.GetProductById;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace CleanArchTemplate.Application.Tests.Products.Queries;

public sealed class GetProductByIdQueryHandlerTests
{
    private static IApplicationDbContext ContextWith(params Product[] seed)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var ctx = new TestDbContext(options);
        ctx.Products.AddRange(seed);
        ctx.SaveChanges();
        return ctx;
    }

    private sealed class TestDbContext(DbContextOptions options) : DbContext(options), IApplicationDbContext
    {
        public DbSet<Product> Products => Set<Product>();
        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Product>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Id).HasConversion(id => id.Value, v => new ProductId(v));
                e.OwnsOne(p => p.Sku);
                e.OwnsOne(p => p.Price);
            });
        }
    }

    [Fact]
    public async Task Returns_dto_when_found()
    {
        var product = Product.Create(Sku.Create("ABC-1"), "Camisa", Money.Create(50m, "BRL"));
        var ctx = ContextWith(product);
        var sut = new GetProductByIdQueryHandler(ctx);

        var result = await sut.Handle(new GetProductByIdQuery(product.Id.Value), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Camisa");
    }

    [Fact]
    public async Task Returns_notfound_when_missing()
    {
        var sut = new GetProductByIdQueryHandler(ContextWith());
        var result = await sut.Handle(new GetProductByIdQuery(Guid.NewGuid()), default);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
```

- [ ] **Step 3: Rodar/falhar** → FAIL.

- [ ] **Step 4: Implementar**

`ProductDto.cs`:
```csharp
namespace CleanArchTemplate.Application.Products.Queries.GetProductById;

public sealed record ProductDto(Guid Id, string Sku, string Name, decimal Price, string Currency);
```

`GetProductByIdQuery.cs`:
```csharp
using CleanArchTemplate.Application.Common.Messaging;

namespace CleanArchTemplate.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
```

`GetProductByIdQueryHandler.cs`:
```csharp
using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var dto = await context.Products
            .Where(p => p.Id == new Domain.Products.ProductId(query.Id))
            .Select(p => new ProductDto(p.Id.Value, p.Sku.Value, p.Name, p.Price.Amount, p.Price.Currency))
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Error.NotFound("Product.NotFound", $"Produto {query.Id} não encontrado.")
            : dto;
    }
}
```

- [ ] **Step 5: Rodar/passar** → PASS.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(application): slice GetProductById (query lê DbContext projetando DTO)"
```

---

### Task 8: Infrastructure — DbContext multi-provider, configs, repos, UoW, DI

**Files:**
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/AppDbContext.cs`
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/DatabaseProvider.cs`
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/Repositories/ProductRepository.cs`
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/UnitOfWork.cs`
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/DomainEventDispatcher.cs`
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
- Create: `src/CleanArchTemplate.Infrastructure/DependencyInjection.cs`
- Modify: `src/CleanArchTemplate.Infrastructure/CleanArchTemplate.Infrastructure.csproj` (pacotes EF + providers)

**Interfaces:**
- Consumes: `IApplicationDbContext`, `IUnitOfWork`, `IProductRepository`, `Product`, `IDomainEvent`.
- Produces: `AppDbContext : DbContext, IApplicationDbContext`; `enum DatabaseProvider { Postgres, SqlServer }`; `AddInfrastructure(IServiceCollection, IConfiguration)`.

- [ ] **Step 1: csproj de Infrastructure** — adicionar `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Options`.

- [ ] **Step 2: `ProductConfiguration.cs`**

```csharp
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchTemplate.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, v => new ProductId(v)).ValueGeneratedNever();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.OwnsOne(p => p.Sku, b => b.Property(s => s.Value).HasColumnName("sku").HasMaxLength(50).IsRequired());
        builder.OwnsOne(p => p.Price, b =>
        {
            b.Property(m => m.Amount).HasColumnName("price_amount").HasPrecision(18, 2);
            b.Property(m => m.Currency).HasColumnName("price_currency").HasMaxLength(3);
        });
    }
}
```

- [ ] **Step 3: `AppDbContext.cs`**

```csharp
using System.Reflection;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
```

- [ ] **Step 4: `UnitOfWork` + `DomainEventDispatcher`** (commit + dispatch dos eventos via MediatR)

`DatabaseProvider.cs`:
```csharp
namespace CleanArchTemplate.Infrastructure.Persistence;

public enum DatabaseProvider { Postgres, SqlServer }
```

`DomainEventDispatcher.cs`:
```csharp
using CleanArchTemplate.Domain.Common;
using MediatR;

namespace CleanArchTemplate.Infrastructure.Persistence;

internal sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;

internal sealed class DomainEventDispatcher(IPublisher publisher)
{
    public async Task DispatchAsync(IEnumerable<AggregateRootMarker> roots, CancellationToken ct)
    {
        var events = roots.SelectMany(r => r.PullDomainEvents()).ToList();
        foreach (var e in events)
            await publisher.Publish(new DomainEventNotification(e), ct);
    }
}
```

> **Decisão de implementação:** para iterar agregados de forma genérica, `UnitOfWork` percorre `ChangeTracker.Entries()` filtrando entidades cujo tipo herda de `AggregateRoot<>`, lê `DomainEvents` e chama `ClearDomainEvents()`. Como `AggregateRoot<TId>` é genérico, usa-se reflexão leve OU uma interface não-genérica `IHasDomainEvents { IReadOnlyCollection<IDomainEvent> DomainEvents; void ClearDomainEvents(); }` implementada por `AggregateRoot<TId>`. **Adote a interface** (mais limpo que reflexão) — adicionar `IHasDomainEvents` ao Domain (Task 2 retrofit) e `AggregateRoot<TId> : ..., IHasDomainEvents`. Atualize o teste da Task 2 se necessário.

`IHasDomainEvents.cs` (Domain/Common):
```csharp
namespace CleanArchTemplate.Domain.Common;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

`UnitOfWork.cs`:
```csharp
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Common;
using MediatR;

namespace CleanArchTemplate.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext context, IPublisher publisher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var roots = context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var events = roots.SelectMany(r => r.DomainEvents).ToList();
        roots.ForEach(r => r.ClearDomainEvents());

        var affected = await context.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in events)
            await publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);

        return affected;
    }
}
```

> Remova o esboço anterior de `DomainEventDispatcher` com `AggregateRootMarker` (não usado); mantenha apenas `DomainEventNotification` no arquivo `DomainEventDispatcher.cs` ou mova-o para junto do `UnitOfWork`.

`ProductRepository.cs`:
```csharp
using CleanArchTemplate.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await context.Products.AddAsync(product, cancellationToken);

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default) =>
        await context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
```

- [ ] **Step 5: `DesignTimeDbContextFactory.cs`** (lê `--provider` para `dotnet ef`)

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanArchTemplate.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var provider = ParseProvider(args);
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        if (provider == DatabaseProvider.SqlServer)
            builder.UseSqlServer("Server=localhost;Database=cat_design;Trusted_Connection=True;TrustServerCertificate=True",
                o => o.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        else
            builder.UseNpgsql("Host=localhost;Database=cat_design;Username=postgres;Password=postgres",
                o => o.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        return new AppDbContext(builder.Options);
    }

    private static DatabaseProvider ParseProvider(string[] args)
    {
        var idx = Array.IndexOf(args, "--provider");
        return idx >= 0 && idx + 1 < args.Length
               && args[idx + 1].Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
            ? DatabaseProvider.SqlServer
            : DatabaseProvider.Postgres;
    }
}
```

> Migrations por provider: usar `--output-dir Persistence/Migrations/Postgres` e `.../SqlServer` (Task 9). Para isolar histórico, configurar `MigrationsHistoryTable` distinto por provider no `DependencyInjection`.

- [ ] **Step 6: `DependencyInjection.cs`**

```csharp
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Infrastructure.Persistence;
using CleanArchTemplate.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = Enum.Parse<DatabaseProvider>(configuration["Database:Provider"] ?? "Postgres", ignoreCase: true);
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default ausente.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider == DatabaseProvider.SqlServer)
                options.UseSqlServer(connectionString, o =>
                {
                    o.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    o.MigrationsHistoryTable("__ef_migrations_sqlserver");
                });
            else
                options.UseNpgsql(connectionString, o =>
                {
                    o.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    o.MigrationsHistoryTable("__ef_migrations_postgres");
                });
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }
}
```

- [ ] **Step 7: Build** — `dotnet build` → SUCCEEDED, 0 warnings.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat(infrastructure): AppDbContext multi-provider, configs, repo, UoW com dispatch de eventos"
```

---

### Task 9: Migrations Postgres + SQL Server

**Files:**
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/Migrations/Postgres/*` (geradas)
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/Migrations/SqlServer/*` (geradas)

- [ ] **Step 1: Migration Postgres**

```bash
dotnet ef migrations add Initial \
  --project src/CleanArchTemplate.Infrastructure \
  --startup-project src/CleanArchTemplate.Api \
  --output-dir Persistence/Migrations/Postgres \
  -- --provider Postgres
```
Expected: cria `InitialProducts`/`Initial` em `Migrations/Postgres`.

- [ ] **Step 2: Migration SqlServer**

```bash
dotnet ef migrations add Initial \
  --project src/CleanArchTemplate.Infrastructure \
  --startup-project src/CleanArchTemplate.Api \
  --output-dir Persistence/Migrations/SqlServer \
  -- --provider SqlServer
```
Expected: cria migration em `Migrations/SqlServer`.

> Se o EF reclamar de conflito de nomes de classe entre as duas pastas, renomeie as classes geradas (ex.: `InitialPostgres` / `InitialSqlServer`) ou use namespaces distintos por pasta. Ambas as migrations devem coexistir e ser selecionadas pelo provider em runtime.

- [ ] **Step 3: Build** → SUCCEEDED.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat(infrastructure): migrations Initial para Postgres e SqlServer"
```

---

### Task 10: Infrastructure.Tests com Testcontainers

**Files:**
- Create: `tests/CleanArchTemplate.Infrastructure.Tests/PostgresProductRepositoryTests.cs`
- Create: `tests/CleanArchTemplate.Infrastructure.Tests/Fixtures/PostgresFixture.cs`

**Interfaces:**
- Consumes: `AppDbContext`, `ProductRepository`, `Product`.

- [ ] **Step 1: Fixture com Testcontainers (Postgres)**

```csharp
using CleanArchTemplate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace CleanArchTemplate.Infrastructure.Tests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build();

    public AppDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString()).Options;
        Context = new AppDbContext(options);
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
    }
}
```

- [ ] **Step 2: Teste de roundtrip do repositório**

```csharp
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.ValueObjects;
using CleanArchTemplate.Infrastructure.Persistence.Repositories;
using CleanArchTemplate.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace CleanArchTemplate.Infrastructure.Tests;

public sealed class PostgresProductRepositoryTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Persists_and_reads_product()
    {
        var repo = new ProductRepository(fixture.Context);
        var product = Product.Create(Sku.Create("ABC-1"), "Camisa", Money.Create(50m, "BRL"));

        await repo.AddAsync(product);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var found = await repo.GetByIdAsync(product.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Camisa");
        found.Price.Amount.Should().Be(50m);
    }
}
```

- [ ] **Step 3: Rodar (requer Docker)**

Run: `dotnet test tests/CleanArchTemplate.Infrastructure.Tests`
Expected: PASS (com Docker rodando). Se Docker indisponível, marcar `[Trait("Category","Integration")]` e documentar skip.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "test(infrastructure): roundtrip ProductRepository com Testcontainers Postgres"
```

---

### Task 11: Auth — User, JWT, password hasher, register/login

**Files:**
- Create: `src/CleanArchTemplate.Domain/Users/UserId.cs`, `User.cs`, `IUserRepository.cs`
- Create: `src/CleanArchTemplate.Application/Common/Abstractions/IJwtTokenGenerator.cs`, `IPasswordHasher.cs`
- Create: `src/CleanArchTemplate.Application/Users/Commands/Register/RegisterCommand.cs` (+Validator, +Handler)
- Create: `src/CleanArchTemplate.Application/Users/Commands/Login/LoginCommand.cs` (+Handler)
- Create: `src/CleanArchTemplate.Infrastructure/Authentication/JwtTokenGenerator.cs`, `PasswordHasher.cs`, `JwtOptions.cs`
- Create: `src/CleanArchTemplate.Infrastructure/Persistence/Configurations/UserConfiguration.cs`, `Repositories/UserRepository.cs`
- Test: `tests/CleanArchTemplate.Application.Tests/Users/RegisterCommandHandlerTests.cs`

**Interfaces:**
- Produces: `User.Create(email, passwordHash) -> User`; `IJwtTokenGenerator.Generate(User) -> string`; `IPasswordHasher.Hash(string) -> string` / `Verify(string password, string hash) -> bool`; `RegisterCommand(string Email, string Password) : ICommand<Guid>`; `LoginCommand(string Email, string Password) : ICommand<string>` (retorna JWT).

- [ ] **Step 1: Domain User** (entidade rica, `UserId` record struct, `Email`/`PasswordHash`, factory). Adicionar `IUserRepository.GetByEmailAsync/AddAsync`. Adicionar `DbSet<User> Users` ao `IApplicationDbContext` e `AppDbContext`.

- [ ] **Step 2: Test RegisterCommandHandler (falha)** — verifica que email duplicado → `Error.Conflict`, e que senha é hasheada via `IPasswordHasher` (mock NSubstitute), retornando `Result<Guid>`.

```csharp
[Fact]
public async Task Fails_when_email_already_exists()
{
    var users = Substitute.For<IUserRepository>();
    users.GetByEmailAsync("a@a.com", Arg.Any<CancellationToken>())
         .Returns(User.Create("a@a.com", "hash"));
    var hasher = Substitute.For<IPasswordHasher>();
    var sut = new RegisterCommandHandler(users, hasher);

    var result = await sut.Handle(new RegisterCommand("a@a.com", "123456"), default);

    result.Error.Type.Should().Be(ErrorType.Conflict);
}
```

- [ ] **Step 3: Implementar** handlers (Register: checa duplicado, `hasher.Hash`, `users.AddAsync`; Login: busca por email, `hasher.Verify`, se ok `jwt.Generate`, senão `Error.Unauthorized`).

- [ ] **Step 4: Infrastructure auth** — `JwtOptions` (binding de `Jwt:` section), `JwtTokenGenerator` (HS256, claims sub/email/exp), `PasswordHasher` (PBKDF2 via `Rfc2898DeriveBytes`). Registrar no `AddInfrastructure` + `AddAuthentication().AddJwtBearer(...)`.

- [ ] **Step 5: Rodar testes** → PASS.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(auth): User + JWT + password hasher + register/login use cases"
```

---

### Task 12: Api — Program, controllers, error handling, swagger

**Files:**
- Modify: `src/CleanArchTemplate.Api/Program.cs`
- Create: `src/CleanArchTemplate.Api/Controllers/BaseApiController.cs`
- Create: `src/CleanArchTemplate.Api/Controllers/ProductsController.cs`
- Create: `src/CleanArchTemplate.Api/Controllers/AuthController.cs`
- Create: `src/CleanArchTemplate.Api/Common/ResultExtensions.cs`
- Create: `src/CleanArchTemplate.Api/Common/GlobalExceptionHandler.cs`
- Create: `src/CleanArchTemplate.Api/appsettings.json` (provider, connection, Jwt)

**Interfaces:**
- Consumes: `AddApplication`, `AddInfrastructure`, `ISender`, comandos/queries, `Result<T>`.

- [ ] **Step 1: `ResultExtensions.cs`** (mapeia `Error.Type` → status)

```csharp
using CleanArchTemplate.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller, object? value = null)
    {
        if (result.IsSuccess)
            return value is null ? controller.NoContent() : controller.Ok(value);

        var status = result.Error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };
        return controller.Problem(detail: result.Error.Message, statusCode: status, title: result.Error.Code);
    }
}
```

- [ ] **Step 2: `BaseApiController` + `ProductsController` + `AuthController`** (finos; injetam `ISender`; `[ProducesResponseType]`). `ProductsController.Create` retorna `CreatedAtAction`; `GetById` usa `result.Value`.

- [ ] **Step 3: `GlobalExceptionHandler`** (`IExceptionHandler` → ProblemDetails), registrar `AddExceptionHandler` + `AddProblemDetails`.

- [ ] **Step 4: `Program.cs`**

```csharp
using CleanArchTemplate.Api.Common;
using CleanArchTemplate.Application;
using CleanArchTemplate.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(/* Bearer scheme */);
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program; // para WebApplicationFactory em testes futuros
```

- [ ] **Step 5: `appsettings.json`**

```json
{
  "Database": { "Provider": "Postgres" },
  "ConnectionStrings": { "Default": "Host=localhost;Database=cleanarch;Username=postgres;Password=postgres" },
  "Jwt": { "Issuer": "CleanArchTemplate", "Audience": "CleanArchTemplate", "Key": "troque-esta-chave-super-secreta-32+chars", "ExpiryMinutes": 60 },
  "Serilog": { "MinimumLevel": "Information" }
}
```

- [ ] **Step 6: Build + smoke run**

Run: `dotnet build` → SUCCEEDED.
Run (com Postgres up): `dotnet run --project src/CleanArchTemplate.Api` e `curl localhost:<port>/health` → 200.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat(api): controllers finos, Result->ActionResult, exception handler, swagger, health"
```

---

### Task 13: Architecture.Tests (NetArchTest)

**Files:**
- Create: `tests/CleanArchTemplate.Architecture.Tests/LayerDependencyTests.cs`
- Create: `tests/CleanArchTemplate.Architecture.Tests/NamingConventionTests.cs`

**Interfaces:**
- Consumes: assemblies de Domain/Application/Infrastructure/Api.

- [ ] **Step 1: Testes de dependência de camada**

```csharp
using CleanArchTemplate.Domain.Common;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace CleanArchTemplate.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string Domain = "CleanArchTemplate.Domain";
    private const string Application = "CleanArchTemplate.Application";
    private const string Infrastructure = "CleanArchTemplate.Infrastructure";

    private static System.Reflection.Assembly DomainAssembly => typeof(AggregateRoot<>).Assembly;

    [Fact]
    public void Domain_should_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().NotHaveDependencyOnAny(Application, Infrastructure, "MediatR", "Microsoft.EntityFrameworkCore")
            .GetResult();
        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure()
    {
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .Should().NotHaveDependencyOn(Infrastructure)
            .GetResult();
        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }
}
```

- [ ] **Step 2: Testes de convenção de nome** (handlers `sealed` + sufixo `Handler`; commands sufixo `Command`).

```csharp
[Fact]
public void Handlers_should_be_sealed_and_named_Handler()
{
    var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
        .That().ImplementInterface(typeof(MediatR.IRequestHandler<,>))
        .Should().BeSealed().And().HaveNameEndingWith("Handler")
        .GetResult();
    result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
}
```

- [ ] **Step 3: Rodar** → PASS. Se falhar, corrigir o código de produção (não o teste) até as regras valerem.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "test(architecture): NetArchTest trava regras de camada e naming"
```

---

### Task 14: CI (GitHub Actions) + husky.net + commitlint

**Files:**
- Create: `.github/workflows/ci.yml`
- Create: `.husky/pre-commit`, `.husky/commit-msg`, `.husky/task-runner.json`
- Create: `commitlint.config.js`
- Create: `package.json` (apenas para husky/commitlint)

- [ ] **Step 1: `ci.yml`**

```yaml
name: CI
on:
  push: { branches: [ main ] }
  pull_request: { branches: [ main ] }
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet format --verify-no-changes
      - run: dotnet test --no-build -c Release --collect:"XPlat Code Coverage"
```

> Testcontainers roda no runner ubuntu (Docker disponível). Postgres e SqlServer sobem via container nos testes de Infra.

- [ ] **Step 2: husky.net + commitlint**

```bash
dotnet new tool-manifest
dotnet tool install husky
dotnet husky install
dotnet husky add pre-commit -c "dotnet format --verify-no-changes"
# commit-msg -> commitlint (Node)
npm init -y
npm install --save-dev @commitlint/cli @commitlint/config-conventional
```

`commitlint.config.js`:
```js
module.exports = { extends: ['@commitlint/config-conventional'] };
```

`.husky/commit-msg`: roda `npx --no -- commitlint --edit "$1"`.

- [ ] **Step 3: Verificar** — `dotnet format --verify-no-changes` passa; um commit com mensagem inválida é rejeitado.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "build: CI no GitHub Actions + husky pre-commit + commitlint"
```

---

### Task 15: Empacotar como `dotnet new` template + README + ADRs

**Files:**
- Create: `.template.config/template.json`
- Create: `README.md`
- Create: `docs/adr/0001-clean-architecture.md`, `0002-cqrs-mediatr.md`, `0003-multi-provider-ef.md`

**Interfaces:**
- Produces: template instalável `cleanarch` com parâmetros `--provider`, `--no-sample`, `--auth`.

- [ ] **Step 1: `template.json`**

```json
{
  "$schema": "http://json.schemastore.org/template",
  "author": "Henrique",
  "classifications": [ "Web", "API", "Clean Architecture", "DDD", "CQRS" ],
  "identity": "CleanArchTemplate.CSharp",
  "name": "Clean Architecture Template (DDD + CQRS)",
  "shortName": "cleanarch",
  "sourceName": "CleanArchTemplate",
  "tags": { "language": "C#", "type": "project" },
  "preferNameDirectory": true,
  "symbols": {
    "provider": {
      "type": "parameter",
      "datatype": "choice",
      "choices": [ { "choice": "Postgres" }, { "choice": "SqlServer" } ],
      "defaultValue": "Postgres",
      "replaces": "Postgres",
      "description": "Provider EF Core padrão em appsettings."
    },
    "auth": {
      "type": "parameter", "datatype": "bool", "defaultValue": "true",
      "description": "Inclui autenticação JWT."
    },
    "no-sample": {
      "type": "parameter", "datatype": "bool", "defaultValue": "false",
      "description": "Remove a fatia de exemplo Product."
    }
  },
  "sources": [
    {
      "modifiers": [
        { "condition": "(no-sample)", "exclude": [ "src/**/Products/**", "tests/**/Products/**" ] },
        { "condition": "(!auth)", "exclude": [ "src/**/Users/**", "src/**/Authentication/**", "tests/**/Users/**" ] }
      ]
    }
  ]
}
```

> Cuidado: `"replaces": "Postgres"` é arriscado (a string "Postgres" aparece em código). Restringir a substituição ao `appsettings.json` via `fileRename`/`sources` específicos, OU usar um token dedicado (ex.: `DB_PROVIDER`) no appsettings e fazer o replace só nesse arquivo. Implementar com um `source` separado mirando `appsettings.json`.

- [ ] **Step 2: `README.md`** — visão geral, diagrama de camadas, como rodar (`docker compose`/connection), como gerar (`dotnet new install .` → `dotnet new cleanarch -n MinhaLoja --provider SqlServer`), link ao `ddd-cheatsheet.md` do MegaStore, padrões e DoD.

- [ ] **Step 3: ADRs** — 3 ADRs curtos (decisão, contexto, consequências).

- [ ] **Step 4: Verificar o template**

```bash
dotnet new install .
dotnet new cleanarch -n DemoSaida -o ../DemoSaida
cd ../DemoSaida && dotnet build
```
Expected: gera `DemoSaida` (namespaces renomeados), `dotnet build` SUCCEEDED.

- [ ] **Step 5: Cleanup + Commit**

```bash
dotnet new uninstall .   # opcional
rm -rf ../DemoSaida
git add -A
git commit -m "feat: empacota como dotnet new template (cleanarch) + README + ADRs"
```

---

## Self-Review (executado)

**1. Spec coverage:** Seções 1–10 do spec mapeadas → Tasks: estrutura (T1), Domain blocks (T2), Result (T3), CQRS/behaviors (T4), Product (T5), CreateProduct (T6), GetProductById (T7), Infra multi-provider (T8), migrations (T9), Testcontainers (T10), auth (T11), Api (T12), Architecture.Tests (T13), CI/husky (T14), template/README/ADR (T15). DoD do spec (§11) coberto por T1/T13/T12/T15. ✅

**2. Placeholder scan:** Tasks 11 e 12 usam descrições de passo + código parcial estratégico (handlers de auth e controllers seguem exatamente o padrão das Tasks 6/7/3 — código de referência presente). Sem "TBD/TODO". Pontos de risco sinalizados explicitamente (replace do provider no template; nomes de migration duplicados; Docker nos testes de Infra). ✅

**3. Type consistency:** `Result`/`Result<T>`/`Error` (T3) usados consistentemente; `ICommand<T>`/`IQuery<T>` (T4); `IHasDomainEvents` introduzido em T8 com retrofit explícito em T2; `IApplicationDbContext` definido em T7 e implementado em T8; `IProductRepository` (T5) consumido em T6/T8. ✅

**Retrofit registrado:** Task 2 ganha `IHasDomainEvents` quando chegar na Task 8 (sinalizado). Alternativa: já criar `IHasDomainEvents` na Task 2. **Recomendação:** criar já na Task 2 para evitar retorno — o implementador deve adicionar `IHasDomainEvents` ao `AggregateRoot<TId>` desde o início.
