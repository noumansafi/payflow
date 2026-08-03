# API catalog

REST API under `/api/v1`. Interactive schemas: **Swagger UI** at `/swagger` when the API runs in Development.

Feature behavior, status codes, and tradeoffs live in the linked docs — this page is the route index.

## Health

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/v1/health` | Anonymous |

## Auth

See [authentication.md](features/authentication.md).

| Method | Path | Auth |
|---|---|---|
| `POST` | `/api/v1/auth/register` | Anonymous |
| `POST` | `/api/v1/auth/login` | Anonymous |
| `POST` | `/api/v1/auth/refresh` | Anonymous |
| `POST` | `/api/v1/auth/logout` | Bearer |
| `GET` | `/api/v1/auth/me` | Bearer |
| `POST` | `/api/v1/auth/password/forgot` | Anonymous |
| `POST` | `/api/v1/auth/password/reset` | Anonymous |
| `POST` | `/api/v1/auth/password/change` | Bearer |
| `POST` | `/api/v1/auth/email/verify` | Anonymous |

## Wallet

See [wallet.md](features/wallet.md).

| Method | Path | Auth | Notes |
|---|---|---|---|
| `GET` | `/api/v1/wallets/me` | Bearer | |
| `GET` | `/api/v1/wallets/me/balance` | Bearer | |
| `POST` | `/api/v1/wallets/me/status` | Bearer | Active ↔ Frozen (self-service) |
| `POST` | `/api/v1/wallets/me/credit` | Bearer | **Development only** |

## Transfers

See [transfers.md](features/transfers.md).

| Method | Path | Auth |
|---|---|---|
| `POST` | `/api/v1/transfers` | Bearer |

## Transactions

See [transactions.md](features/transactions.md).

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/v1/transactions` | Bearer |
| `GET` | `/api/v1/transactions/{id}` | Bearer |
| `GET` | `/api/v1/transactions/by-reference/{referenceNumber}` | Bearer |

## Beneficiaries

See [beneficiaries.md](features/beneficiaries.md).

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/v1/beneficiaries` | Bearer |
| `GET` | `/api/v1/beneficiaries/lookup?userId={guid}` | Bearer |
| `POST` | `/api/v1/beneficiaries` | Bearer |
| `DELETE` | `/api/v1/beneficiaries/{id}` | Bearer |

## Notifications

See [notifications.md](features/notifications.md).

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/v1/notifications` | Bearer |
| `POST` | `/api/v1/notifications/read-all` | Bearer |
| `POST` | `/api/v1/notifications/{id}/read` | Bearer |

## Admin — audit logs

See [audit-logs.md](features/audit-logs.md).

| Method | Path | Auth |
|---|---|---|
| `GET` | `/api/v1/admin/audit-logs` | Bearer + Admin role |
