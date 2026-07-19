# Feature: Transfers (P2P Transfer Engine)

**Milestone:** 4  
**Status:** Planned

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

Suggested payload:

```json
{
  "receiverUserId": "guid",
  "amount": 25.50,
  "note": "optional"
}
```

## Processing pipeline (target)

```text
Validate request (FluentValidation)
  → Load sender/receiver wallets (consistent locking strategy)
    → Enforce domain rules
      → Begin transaction
        → Debit sender
        → Credit receiver
        → Insert Transaction (Completed)
        → Insert Notification(s)
        → Insert AuditLog
      → Commit
  → Return reference number
```

## Concurrency

Use a strategy that prevents lost updates under concurrent transfers, e.g.:

- Row version / concurrency token on Wallet, or
- Atomic SQL update with balance predicate (`WHERE Balance >= @amount`)

Document the chosen approach in code comments + tests.

## API sketch

```http
POST /api/v1/transfers
```

Responses:

- `201/200` with reference number on success
- `400` validation failures (ProblemDetails)
- `409` concurrency conflicts (if applicable)
- `404` receiver not found

## Tradeoffs

| Choice | Rationale |
|---|---|
| Synchronous transfer in one request | Clear demo; matches monolith MVP |
| Fee column now, engine later | Avoids schema churn |
| Notifications inside same TX | Consistent UX; outbox later for external channels |

## Fintech note

Card networks and banks use clearing, settlement, and often asynchronous states (`Pending` → `Completed`). PayFlow supports those statuses in the model; MVP P2P can complete inline while keeping the door open for pending flows (e.g. compliance holds).

## Acceptance criteria

- [ ] All business rules covered by unit tests
- [ ] Integration test proves atomicity (failure path leaves balances unchanged)
- [ ] Reference numbers unique
- [ ] Audit + notification created on success
- [ ] Self-transfer rejected
