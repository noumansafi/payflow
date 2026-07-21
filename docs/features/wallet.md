# Feature: Wallet

**Milestone:** 3  
**Status:** In progress (Step 2/4 — persistence wired)

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
| View wallet | Query | Owner only | Done (Application + Infra) |
| View balance | Query | Owner only | Done (Application + Infra) |
| Wallet history | Query | Via transactions feature | Later (M5) |
| Freeze wallet | Command | Authorized user; audited | Step 4 |
| Activate wallet | Command | Authorized; audited | Step 4 |

## Invariants

- One wallet per user
- Balance never negative
- Transfers allowed only when status is Active
- Balance changes only through approved application commands (not arbitrary updates)
- Queries never mutate state

## Commands / queries

**Commands (Step 4):** `FreezeWallet`, `ActivateWallet`  
**Queries (Step 1):** `GetWallet`, `GetBalance`

## API sketch (Steps 3–4)

```http
GET  /api/v1/wallets/me
GET  /api/v1/wallets/me/balance
POST /api/v1/wallets/me/freeze
POST /api/v1/wallets/me/activate
```

## Delivery slices

1. Application queries + port + unit tests ✅  
2. Infrastructure `WalletRepository` + DI ✅  
3. API endpoints + Swagger  
4. Freeze / Activate commands + tests  

## Tradeoffs

| Choice | Rationale |
|---|---|
| Single wallet per user | Matches product scope; avoids multi-account complexity |
| Status flag vs soft-delete | Explicit operational control for risk/support scenarios |
| Balance on wallet row | Simple for MVP; ledger projection can be added later |

## Fintech note

Mature payment systems often use an append-only **ledger** and treat wallet balance as a projection. PayFlow starts with a balance field + transactional updates, structured so a ledger can be introduced without rewriting the API.

## Acceptance criteria

- [x] Queries never mutate state
- [x] Unit tests for auth + not-found on get wallet/balance
- [ ] Registration always yields one wallet (already true from M2; covered by register tests)
- [ ] API endpoints expose wallet/balance
- [ ] Frozen wallet cannot send/receive transfers (M4)
- [ ] Freeze/activate emit audit logs
- [ ] Tests for status transitions
