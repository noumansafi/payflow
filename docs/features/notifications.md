# Feature: Notifications

**Milestone:** 7  
**Status:** Planned

## Purpose

Record user-visible events (e.g. money received, security alerts) in a durable store.

## MVP scope

- Persist notifications in SQL Server
- List notifications for current user
- Mark as read (optional but recommended)

## Out of scope (explicit extension points)

- SignalR realtime push
- Mobile push
- Email delivery

Design the abstraction so those channels can subscribe later without changing transfer logic.

## Model (target)

| Field | Description |
|---|---|
| Id | Primary key |
| UserId | Recipient |
| Title | Short text |
| Body | Detail |
| Type | TransferReceived, Security, System, etc. |
| IsRead | Bool |
| CreatedAt | UTC |
| RelatedEntityId | Optional correlation (transaction id) |

## Queries / commands

**Queries:** `GetNotifications`  
**Commands:** `MarkNotificationRead`, `MarkAllNotificationsRead` (optional)

## API sketch

```http
GET  /api/v1/notifications
POST /api/v1/notifications/{id}/read
POST /api/v1/notifications/read-all
```

## Tradeoffs

| Choice | Rationale |
|---|---|
| DB-only first | Separates business event from delivery mechanism |
| Create inside transfer TX | User always has an in-app record if money moved |
| Channel workers later | Matches how notification platforms evolve |

## Acceptance criteria

- [ ] Successful transfer creates receiver notification
- [ ] Users only see their notifications
- [ ] List supports unread filtering
