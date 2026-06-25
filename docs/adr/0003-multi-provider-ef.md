# ADR 0003 — EF Core multi-provider (Postgres + SQL Server)

- **Status:** Aceito
- **Data:** 2026-06-25

## Contexto

O template precisa rodar tanto em PostgreSQL quanto em SQL Server, com a troca sendo simples e sem reescrever o modelo.

## Decisão

Um único `AppDbContext`. O provider é selecionado em runtime pela configuração `Database:Provider` (`Postgres` | `SqlServer`), aplicando `UseNpgsql`/`UseSqlServer`. As migrations vivem em pastas separadas por provider (`Persistence/Migrations/{Postgres,SqlServer}`), com tabela de histórico distinta por provider. Um `IDesignTimeDbContextFactory` lê `-- --provider` para o `dotnet ef`.

Consideramos projetos de Infrastructure separados por provider, mas seria cerimônia excessiva para um template.

## Consequências

- (+) Troca de banco = uma linha de configuração.
- (+) SQL gerado correto para cada banco (migrations dedicadas).
- (−) Dois conjuntos de migrations para manter.
