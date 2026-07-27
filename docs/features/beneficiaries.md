# Feature: Beneficiaries

**Milestone:** 6  
**Status:** Done

## Purpose

Let users save frequent transfer recipients to reduce friction and mistakes. Each user owns a private beneficiaries list.

## Model

| Field | Description |
|---|---|
| Id | Beneficiary row id (used for delete) |
| OwnerUserId | Current user who saved the contact |
| BeneficiaryUserId | Target PayFlow user |
| DisplayName | Optional nickname |
| CreatedAtUtc | When saved |

Uniqueness: composite unique index `(OwnerUserId, BeneficiaryUserId)`.

## Operations

| Operation | Rules |
|---|---|
| Add beneficiary | Target user must exist; not self; no duplicates → 409 |
| Remove beneficiary | Owner only; missing/foreign id → 404 |
| List beneficiaries | Owner only; paginated |

## Commands / queries

**Commands:** `AddBeneficiary`, `RemoveBeneficiary`  
**Queries:** `GetBeneficiaries`

## API

```http
GET    /api/v1/beneficiaries?page=1&pageSize=20
POST   /api/v1/beneficiaries
DELETE /api/v1/beneficiaries/{id}
```

POST body:

```json
{
  "beneficiaryUserId": "guid",
  "displayName": "optional nickname"
}
```

## Data integrity

Enforce uniqueness with DB composite unique index `(OwnerUserId, BeneficiaryUserId)`, plus an application existence check for a clear Conflict response.

## Tradeoffs

| Choice | Rationale |
|---|---|
| Store user reference, not free-text account numbers | Matches closed P2P network model |
| DB unique constraint | Prevents race-condition duplicates |
| Remove returns 404 (not silent 204) | Clear client semantics; tested |
| Transfer still uses `receiverUserId` | Beneficiaries are a convenience list; wiring into transfer UX is M9 |

## Acceptance criteria

- [x] Duplicate add returns clear ProblemDetails (409)
- [x] Cannot add self
- [x] Remove missing/foreign id returns 404
- [ ] Transfer UI can consume beneficiary list (Milestone 9)
