# Feature: Wallet

**Milestone:** 3  
**Status:** Done

## Purpose

Each user owns exactly one wallet that holds balance and lifecycle status. The wallet is the source of truth for spendable funds in this monolith.

## Model

| Field | Description |
|---|---|
| Id | Primary key |
| UserId | Owner (1:1 with User) |
| Balance | `decimal` balance |
| Currency | ISO-like code (e.g. `USD`) |
| Status | Active / Frozen (extensible) |
| CreatedAt | UTC timestamp |

## Operations

| Operation | Type | Rules | Status |
|---|---|---|---|
| View wallet | Query | Owner only | Done |
| View balance | Query | Owner only | Done |
| Wallet history | Query | Via transactions feature | Later (M5) |
| Change status | Command | Owner self-service transitions only (Active ↔ Frozen); audited; conflict if unchanged | Done |

## Invariants

- One wallet per user
- Balance never negative
- Transfers allowed only when status is Active (enforced in M4)
- Balance changes only through approved application commands (not arbitrary updates)
- Queries never mutate state

## Commands / queries

**Commands:** `ChangeWalletStatus`  
**Queries:** `GetWallet`, `GetBalance`

## API

```http
GET  /api/v1/wallets/me
GET  /api/v1/wallets/me/balance
POST /api/v1/wallets/me/status
```

Body example: `{ "status": "Frozen" }`

## Delivery slices

1. Application queries + port + unit tests ✅  
2. Infrastructure `WalletRepository` + DI ✅  
3. API read endpoints + Swagger ✅  
4. Change status command + transition policy + tests ✅  

## Tradeoffs

| Choice | Rationale |
|---|---|
| Single wallet per user | Matches product scope; avoids multi-account complexity |
| Status flag vs soft-delete | Explicit operational control for risk/support scenarios |
| Balance on wallet row | Simple for MVP; ledger projection can be added later |
| One status endpoint vs freeze/activate routes | Scales when statuses grow; domain allowlist keeps self-service vs system transitions separate |
| POST (not PATCH) for status change | CQRS command, consistent with other mutating endpoints |
| Conflict when status unchanged | Clear API semantics vs silent no-op |

## Fintech note

Mature payment systems often use an append-only **ledger** and treat wallet balance as a projection. PayFlow starts with a balance field + transactional updates, structured so a ledger can be introduced without rewriting the API.

## Acceptance criteria

- [x] Queries never mutate state
- [x] Unit tests for auth + not-found on get wallet/balance
- [x] Registration always yields one wallet (already true from M2; covered by register tests)
- [x] API endpoints expose wallet/balance
- [x] Status changes emit audit logs
- [x] Tests for allowed status transitions
- [x] Frozen wallet cannot send/receive transfers (M4)
