# Feature: Transactions

**Milestone:** 5  
**Status:** Done

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

| Query | Rules |
|---|---|
| `GetTransactions` | Owner wallet only; paginated; filterable; sortable |
| `GetTransactionById` | 404 if missing or not involving caller's wallet |
| `GetTransactionByReference` | Same ownership rule as by-id |

Filters on list:

- Date range (`fromUtc`, `toUtc`)
- Status
- Direction (`Sent` / `Received`)
- Reference number

Sort: `createdAt` or `-createdAt` (default), with `Id` tie-breaker.

## API

```http
GET /api/v1/transactions?page=1&pageSize=20&status=Completed&direction=Sent&sort=-createdAt
GET /api/v1/transactions/{id}
GET /api/v1/transactions/by-reference/{referenceNumber}
```

Response list envelope:

```json
{
  "items": [ /* TransactionDto */ ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3
}
```

`TransactionDto` includes `direction` and `counterpartyWalletId` relative to the current user.

## Design rules

- Queries **never** modify state
- Users only see transactions involving their wallet
- Foreign transactions return **404** (not 403) to avoid leaking existence
- DTOs hide internal implementation details
- Stable sorting for pagination (tie-breaker on Id)

## Tradeoffs

| Choice | Rationale |
|---|---|
| Single transactions table | Simple reporting for MVP |
| Status enum including Pending/Failed | Supports future async flows without redesign |
| Offset pagination first | Easy; keyset pagination can replace later for scale |
| 404 for unauthorized access to others' txs | Avoids existence oracle |

## Acceptance criteria

- [x] Sender and receiver both see the transaction
- [x] Pagination metadata returned
- [x] Unauthorized users cannot read others' transactions
- [ ] Integration tests for filter/sort/page — deferred to M10
