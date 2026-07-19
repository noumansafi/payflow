# Feature: Transactions

**Milestone:** 5  
**Status:** Planned

## Purpose

Provide a durable history of money movement for users and for system diagnostics.

## Model

| Field | Description |
|---|---|
| Id | Primary key |
| ReferenceNumber | Public correlation id |
| SenderWalletId | Source wallet |
| ReceiverWalletId | Destination wallet |
| Amount | Transfer amount |
| Fee | Reserved for fee engine |
| Status | Pending / Completed / Failed / Cancelled |
| TransactionType | e.g. P2P Transfer (extensible) |
| CreatedAt | UTC |
| CompletedAt | UTC nullable |

## Queries

`GetTransactions` — paginated, filterable, sortable.

Typical filters:

- Date range
- Status
- Direction (sent / received)
- Reference number

## API sketch

```http
GET /api/v1/transactions?page=1&pageSize=20&status=Completed&sort=-createdAt
GET /api/v1/transactions/{id}
GET /api/v1/transactions/by-reference/{referenceNumber}
```

## Design rules

- Queries **never** modify state
- Users only see transactions involving their wallet
- DTOs hide internal implementation details
- Stable sorting for pagination (tie-breaker on Id)

## Tradeoffs

| Choice | Rationale |
|---|---|
| Single transactions table | Simple reporting for MVP |
| Status enum including Pending/Failed | Supports future async flows without redesign |
| Offset pagination first | Easy; keyset pagination can replace later for scale |

## Acceptance criteria

- [ ] Sender and receiver both see the transaction
- [ ] Pagination metadata returned
- [ ] Unauthorized users cannot read others' transactions
- [ ] Integration tests for filter/sort/page
