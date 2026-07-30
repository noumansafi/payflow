# Feature: Audit Logs

**Milestone:** 8  
**Status:** Done

## Purpose

Queryable security/compliance trail for sensitive actions. Distinct from Serilog ops logs: audit rows are business events you can filter in SQL/API demos.

## Audited actions

| Action | When |
|---|---|
| `Login` | Successful login |
| `Logout` | Logout / refresh revoke |
| `PasswordChange` | Change password / reset password |
| `Register` | New user registration |
| `Transfer` | Successful P2P transfer (includes reference number in metadata) |
| `WalletFreeze` / `WalletActivation` | Self-service wallet status change |

## Model

| Field | Description |
|---|---|
| Id | Primary key |
| ActorUserId | Who performed the action (nullable for system) |
| Action | `AuditAction` enum (stored as string) |
| EntityType | e.g. Wallet, Transaction, User |
| EntityId | Target entity |
| Metadata | Structured JSON (no secrets/passwords/tokens) |
| IpAddress | Optional; explicit on auth commands, else from `IClientInfo` |
| CreatedAtUtc | UTC |

Indexes: `CreatedAtUtc`, `(ActorUserId, CreatedAtUtc)`, `(Action, CreatedAtUtc)`.

## Design rules

- Append-oriented (no update/delete APIs)
- Never store passwords or raw tokens in metadata
- Application decides *when* to audit; Infrastructure persists (`IAuditLogger`)
- Read path is a separate port (`IAuditLogRepository`) — CQRS-style write vs read
- Admin-only list: handler enforces role + controller `[Authorize(Roles = Admin)]`

## Commands / queries

**Write:** existing handlers call `IAuditLogger.WriteAsync`  
**Query:** `GetAuditLogs` (admin only)

## API

```http
GET /api/v1/admin/audit-logs?page=1&pageSize=20&action=Transfer&actorUserId={guid}&fromUtc=&toUtc=
```

Non-admin → 403. Unauthenticated → 401.

## Tradeoffs

| Choice | Rationale |
|---|---|
| First-class `AuditLogs` table | Easy to demo and query |
| vs Serilog only | Ops logs ≠ durable business audit trail |
| Admin read API | Avoids exposing global audit to all users |
| `IClientInfo` fallback for IP | Transfer/wallet audits get IP without widening every command DTO |
| Dual admin check (JWT role + handler) | Early reject at edge; rule still testable in Application |

## Fintech note

Regulated environments often require immutable storage, retention, and SIEM. PayFlow demonstrates the application-level audit model those systems build on.

## Acceptance criteria

- [x] Listed sensitive actions create an audit row
- [x] Transfer audit includes reference number (not secrets)
- [x] Non-admin cannot list global audit logs
- [x] Unit tests cover admin list + forbid non-admin
