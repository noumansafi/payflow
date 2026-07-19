# Feature: Wallet

**Milestone:** 3  
**Status:** Planned

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

| Operation | Type | Rules |
|---|---|---|
| View wallet | Query | Owner only |
| View balance | Query | Owner only |
| Wallet history | Query | Via transactions feature |
| Freeze wallet | Command | Authorized user/admin; audited |
| Activate wallet | Command | Authorized; audited |

## Invariants

- One wallet per user
- Balance never negative
- Transfers allowed only when status is Active
- Balance changes only through approved application commands (not arbitrary updates)

## Commands / queries (target)

**Commands:** `FreezeWallet`, `ActivateWallet`  
**Queries:** `GetWallet`, `GetBalance`

## API sketch

```http
GET  /api/v1/wallets/me
GET  /api/v1/wallets/me/balance
POST /api/v1/wallets/me/freeze
POST /api/v1/wallets/me/activate
```

## Tradeoffs

| Choice | Rationale |
|---|---|
| Single wallet per user | Matches product scope; avoids multi-account complexity |
| Status flag vs soft-delete | Explicit operational control for risk/support scenarios |
| Balance on wallet row | Simple for MVP; ledger projection can be added later |

## Fintech note

Mature payment systems often use an append-only **ledger** and treat wallet balance as a projection. PayFlow starts with a balance field + transactional updates, structured so a ledger can be introduced without rewriting the API.

## Acceptance criteria

- [ ] Registration always yields one wallet
- [ ] Frozen wallet cannot send/receive transfers
- [ ] Freeze/activate emit audit logs
- [ ] Queries never mutate state
- [ ] Tests for status transitions and authorization
