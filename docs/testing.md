# Testing

PayFlow uses a backend test pyramid plus Angular Vitest unit tests:

| Project | Purpose |
|---|---|
| `tests/PayFlow.Tests.Unit` | Fast feedback: handlers, validators, domain rules (NSubstitute + FluentAssertions) |
| `tests/PayFlow.Tests.Integration` | Real HTTP + SQL Server via Testcontainers |
| `client/` (Vitest) | Auth, guards, interceptor, HTTP helpers, thin API facades |

## Unit tests

- Prefer testing Application handlers and Domain rules over controllers.
- Controllers stay thin; behavior lives in MediatR handlers.
- FluentAssertions for readable failures.

```bash
dotnet test tests/PayFlow.Tests.Unit
```

## Integration tests

Requires **Docker** (Testcontainers pulls SQL Server 2022).

Suite shares one container + one `WebApplicationFactory` host (`IntegrationFixture`):

1. Host runs in **Development** so EF migrations and the demo credit endpoint are available.
2. Connection string and JWT secret are overridden in-memory.
3. Each test registers fresh users so cases stay isolated on the shared database.

Covered flows:

- Register → credit → transfer → history + notification
- Insufficient funds → `409` and balances unchanged
- Frozen wallet → `409`
- Admin audit logs → `403` for users, `200` after role promote + re-login

```bash
dotnet test tests/PayFlow.Tests.Integration
```

First run may take a few minutes while the SQL Server image downloads.

## Frontend unit tests

Angular uses Vitest (jsdom) through `@angular/build:unit-test`. See [frontend.md](frontend.md#testing).

```bash
cd client
npm run test:ci
```

## Conventions

- Arrange / Act / Assert with clear names (`WhenX_ReturnsY`).
- Integration helpers live under `Infrastructure/` (`ApiClient`, `PayFlowApiFactory`).
- Do not call production-only paths that require real email providers; auth uses the mock sender already wired in Infrastructure.
- Frontend specs co-locate as `*.spec.ts` and prefer services/guards over page DOM suites.
