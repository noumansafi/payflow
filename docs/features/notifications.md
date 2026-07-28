# Feature: Notifications

**Milestone:** 7  
**Status:** Done

## Purpose

Durable in-app inbox for user-visible events (transfer sent/received today; security/system later). Creation stays inside the business transaction; delivery channels can subscribe later without changing transfer logic.

## MVP scope

- Persist notifications in SQL Server (already created on transfer)
- List notifications for the current user (paginated, optional `isRead` filter)
- Mark one as read (idempotent)
- Mark all unread as read

## Out of scope (extension points)

- SignalR realtime push
- Mobile push
- Email delivery

## Model

| Field | Description |
|---|---|
| Id | Primary key |
| UserId | Recipient (owner filter on every read/write) |
| Title | Short text |
| Body | Detail |
| Type | `TransferSent`, `TransferReceived`, … |
| IsRead | Bool |
| RelatedEntityId | Optional correlation (transaction id) |
| CreatedAtUtc | UTC |

Index: `(UserId, IsRead, CreatedAtUtc)` for inbox + unread filters.

## Commands / queries

**Queries:** `GetNotifications`  
**Commands:** `MarkNotificationRead`, `MarkAllNotificationsRead`

Create path: `TransferMoney` adds `TransferSent` + `TransferReceived` via `INotificationRepository.Add` in the same unit of work.

## API

```http
GET  /api/v1/notifications?page=1&pageSize=20&isRead=false
POST /api/v1/notifications/read-all
POST /api/v1/notifications/{id}/read
```

| Endpoint | Behavior |
|---|---|
| `GET` | Owner only; newest first; stable sort `CreatedAtUtc`, `Id` |
| `POST .../read` | Owner only; missing/foreign → 404; already read → 204 (idempotent) |
| `POST .../read-all` | Owner only; returns `{ "markedCount": n }` |

## Architecture notes

| Layer | Responsibility |
|---|---|
| Application | CQRS handlers, ports, DTOs, FluentValidation |
| Infrastructure | EF list/get; bulk `ExecuteUpdate` for mark-all |
| Api | Thin `NotificationsController` → MediatR |

Owner scoping is enforced in the repository (`UserId` predicates), so foreign ids surface as 404 rather than 403.

## Tradeoffs

| Choice | Rationale |
|---|---|
| DB-only inbox first | Separates business event from delivery channel |
| Create inside transfer TX | User always has an in-app record if money moved |
| Idempotent mark-read | Safe client retries |
| Bulk `ExecuteUpdate` for mark-all | Avoids loading every unread row |
| Channels later | Workers/outbox can subscribe without changing transfer |

## Acceptance criteria

- [x] Successful transfer creates sender + receiver notifications
- [x] Users only see / mutate their notifications
- [x] List supports unread filtering
- [x] Mark-read is idempotent; foreign id → 404
- [ ] Realtime / email / push delivery (post-MVP)
