# ADR 0001 — Clean Architecture com quatro camadas

- **Status:** Aceito
- **Data:** 2026-06-25

## Contexto

Precisamos de uma base que isole regra de negócio de detalhes de infraestrutura (banco, web, frameworks), com testabilidade alta e fronteiras explícitas.

## Decisão

Adotar Clean Architecture com as camadas `Domain`, `Application`, `Infrastructure` e `Api`, com a regra de dependência `Domain ← Application ← Infrastructure → Api`. O `Domain` não referencia nenhuma outra camada nem pacote de infraestrutura. A regra é verificada por testes de arquitetura (NetArchTest).

## Consequências

- (+) Núcleo de negócio testável sem banco/web.
- (+) Troca de detalhes (provider, framework web) sem tocar no domínio.
- (−) Mais projetos e indireções (ports/adapters) — custo aceitável pela clareza.
