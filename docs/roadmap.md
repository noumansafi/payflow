# Roadmap

## Delivery philosophy

Ship vertical value in milestones. Each milestone should leave the solution **buildable**, **documented**, and **reviewable**.

## Milestones

### Milestone 1 — Foundation ✅
- Solution + Clean Architecture projects
- DI composition roots
- EF Core + SQL Server
- Docker Compose (API + SQL)
- Domain entities + initial migration

### Milestone 2 — Authentication ✅
- Register / login
- JWT access + refresh tokens
- Password hashing
- RBAC
- Mock email verification & password reset
- Audit: login / logout / password change

### Milestone 3 — Wallet ✅
- Auto-create wallet on registration
- Get wallet / balance
- Change wallet status (self-service transitions)
- Wallet status guards for transfers ✅

### Milestone 4 — Transfer engine ✅
- Atomic P2P transfer
- Business rule validation
- Reference number generation
- Notifications + audit on transfer
- Fee field reserved for future policy

### Milestone 5 — Transactions ✅
- History queries
- Pagination / filtering / sorting
- Status model: Pending / Completed / Failed / Cancelled

### Milestone 6 — Beneficiaries ✅
- Add / remove / list
- Duplicate prevention

### Milestone 7 — Notifications
- Persist notifications
- List / mark-read patterns
- Extension points for SignalR / email / push

### Milestone 8 — Audit logs
- Centralized audit recording
- Queryability for security review demos

### Milestone 9 — Angular frontend
- Auth flows, dashboard, wallet, transfer, history, beneficiaries, notifications, profile
- Material, signals, guards, interceptors
- Light / dark theme

### Milestone 10 — Testing
- Unit coverage for business rules
- Integration coverage for API + DB
- FluentAssertions conventions

### Milestone 11 — CI/CD
- GitHub Actions: restore → build → test → publish

### Milestone 12 — Documentation polish
- Screenshots
- ER + architecture diagrams
- API catalog completeness

## Post-MVP (explicitly out of current scope)

- Idempotency keys
- Double-entry ledger
- Fee engine
- Fraud/risk scoring
- Real email/SMS providers
- Multi-currency FX
- OpenTelemetry full observability stack
