# Feature: Audit Logs

**Milestone:** 8  
**Status:** Planned

## Purpose

Provide a security and compliance trail for sensitive actions. In interviews, this shows you think beyond happy-path CRUD.

## Audited actions (minimum)

- Login
- Logout
- Password change
- Transfer
- Wallet freeze
- Wallet activation

## Model (target)

| Field | Description |
|---|---|
| Id | Primary key |
| ActorUserId | Who performed the action (nullable for system) |
| Action | Enum/string action name |
| EntityType | e.g. Wallet, Transaction, User |
| EntityId | Target entity |
| Metadata | JSON details (no secrets/passwords) |
| IpAddress | Optional, from HTTP context |
| CreatedAt | UTC |

## Design rules

- Append-oriented (no update/delete APIs for normal users)
- Never store passwords or tokens in metadata
- Prefer structured metadata over free text
- Application layer decides *when* to audit; Infrastructure persists

## API sketch

```http
GET /api/v1/admin/audit-logs   # admin-only in MVP demos
```

End-user self-service audit views are optional later.

## Tradeoffs

| Choice | Rationale |
|---|---|
| First-class AuditLog table | Easy to demo and query |
| vs relying only on Serilog sinks | Logs ≠ queryable business audit trail |
| Admin read API | Avoids exposing global audit to all users |

## Fintech note

Regulated environments often require immutable audit storage, retention policies, and SIEM integration. PayFlow demonstrates the application-level audit model those systems build on.

## Acceptance criteria

- [ ] Each listed action creates an audit row
- [ ] Transfer audit includes reference number (not secrets)
- [ ] Non-admin cannot list global audit logs
- [ ] Unit tests ensure handlers call audit port
