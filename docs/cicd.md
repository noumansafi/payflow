# CI/CD

PayFlow uses GitHub Actions for continuous integration. There is **no cloud deploy** in the MVP — the pipeline proves the solution builds, tests pass, and publishable artifacts are produced.

## Workflow

File: [`.github/workflows/ci.yml`](../.github/workflows/ci.yml)

| Trigger | Branches |
|---|---|
| `push`, `pull_request` | `main` |

### Pipeline steps

1. **Restore / build** — .NET 10, `PayFlow.sln` (Release)
2. **Unit tests** — `tests/PayFlow.Tests.Unit`
3. **Integration tests** — `tests/PayFlow.Tests.Integration` (Testcontainers + Docker on the runner)
4. **Publish API** — `dotnet publish` → artifact `payflow-api`
5. **Build Angular** — Node 22, `npm ci` + production build → artifact `payflow-client`

Concurrency cancels older runs on the same ref so a busy branch does not pile up jobs.

## Local parity

```bash
dotnet restore PayFlow.sln
dotnet build PayFlow.sln -c Release
dotnet test tests/PayFlow.Tests.Unit -c Release --no-build
dotnet test tests/PayFlow.Tests.Integration -c Release --no-build   # needs Docker
dotnet publish src/PayFlow.Api/PayFlow.Api.csproj -c Release -o artifacts/api --no-restore

cd client && npm ci && npm run build -- --configuration=production
```

See also [testing.md](testing.md).

## Out of scope (post-MVP)

- Deploy to Azure / AWS / Kubernetes
- Release tagging and environment promotion
- Multi-OS build matrices
