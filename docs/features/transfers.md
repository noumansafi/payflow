# Feature: Transfers (P2P Transfer Engine)

**Milestone:** 4  
**Status:** Done

## Purpose

The core product capability: move value from one user's wallet to another **safely**, **atomically**, and **auditably**.

This is the highest-signal feature for interviews.

## Business rules

1. Cannot transfer to self
2. Receiver must exist
3. Sender and receiver wallets must be Active
4. Amount must be > 0
5. Sender must have sufficient balance
6. Operation must be atomic (all DB changes commit or none do)
7. Generate unique transaction reference
8. Record transaction history
9. Create notification(s)
10. Write audit log
11. Fee supported in schema (default `0` for now)

## Command

`TransferMoney`

Payload:

```json
{
  "receiverUserId": "guid",
  "amount": 25.50,
  "note": "optional"
}
```

## Processing pipeline

```text
Validate request (FluentValidation)
  → Load sender/receiver wallets
    → Enforce TransferRules (domain)
      → Debit sender / credit receiver
      → Insert Transaction (Completed)
      → Insert Notification(s)
      → Insert AuditLog
      → SaveChanges (single UnitOfWork; RowVersion concurrency → 409)
  → Return reference number
```

## Concurrency

`Wallet.RowVersion` optimistic concurrency token. Concurrent balance updates that collide map to `ConflictException` (HTTP 409) via `UnitOfWork`.

## API

```http
POST /api/v1/transfers
```

Development-only funding (local/Swagger demos):

```http
POST /api/v1/wallets/me/credit
{ "amount": 100 }
```

Responses:

- `201` with reference number on success
- `400` validation failures (ProblemDetails)
- `404` receiver / sender wallet not found
- `409` insufficient funds, inactive wallets, or concurrency conflict

## Tradeoffs

| Choice | Rationale |
|---|---|
| Synchronous transfer in one request | Clear demo; matches monolith MVP |
| Fee column now, engine later | Avoids schema churn |
| Notifications inside same TX | Consistent UX; outbox later for external channels |
| Dev-only credit endpoint | Enables Swagger demos without SQL; unavailable outside Development |
| Unit tests for rules; DB integration later (M10) | Fast feedback now; deeper atomicity proof when integration suite lands |

## Fintech note

Card networks and banks use clearing, settlement, and often asynchronous states (`Pending` → `Completed`). PayFlow supports those statuses in the model; MVP P2P completes inline while keeping the door open for pending flows (e.g. compliance holds).

## Acceptance criteria

- [x] All business rules covered by unit tests
- [ ] Integration test proves atomicity (failure path leaves balances unchanged) — deferred to M10
- [x] Reference numbers unique
- [x] Audit + notification created on success
- [x] Self-transfer rejected
- [x] Frozen wallets cannot send/receive
