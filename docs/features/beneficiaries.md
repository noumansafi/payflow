# Feature: Beneficiaries

**Milestone:** 6  
**Status:** Done

## Purpose

Let users save frequent transfer recipients to reduce friction and mistakes. Each user owns a private beneficiaries list.

PayFlow is a **closed P2P network** (user ↔ user), so recipients are resolved by **PayFlow user id**, not IBAN/account number. Lookup confirms the person before save.

## Model

| Field | Description |
|---|---|
| Id | Beneficiary row id (used for delete) |
| OwnerUserId | Current user who saved the contact |
| BeneficiaryUserId | Target PayFlow user |
| DisplayName | Optional nickname |
| CreatedAtUtc | When saved |

Uniqueness: composite unique index `(OwnerUserId, BeneficiaryUserId)`.

## Recommended client flow

```text
1. User enters / receives a PayFlow user id
2. GET /api/v1/beneficiaries/lookup?userId={guid}
      → confirm name (+ email) with the user
3. POST /api/v1/beneficiaries
      → save after confirmation
```

Lookup is read-only. POST remains the authoritative write (still re-validates existence, self, and duplicates).

## Operations

| Operation | Rules |
|---|---|
| Lookup candidate | Target must exist; not self; returns id, name, email, `alreadySaved` |
| Add beneficiary | Target user must exist; not self; no duplicates → 409 |
| Remove beneficiary | Owner only; missing/foreign id → 404 |
| List beneficiaries | Owner only; paginated |

## Commands / queries

**Commands:** `AddBeneficiary`, `RemoveBeneficiary`  
**Queries:** `ResolveBeneficiaryCandidate`, `GetBeneficiaries`

## API

```http
GET    /api/v1/beneficiaries?page=1&pageSize=20
GET    /api/v1/beneficiaries/lookup?userId={guid}
POST   /api/v1/beneficiaries
DELETE /api/v1/beneficiaries/{id}
```

Lookup response:

```json
{
  "userId": "guid",
  "firstName": "Sara",
  "lastName": "Khan",
  "email": "sara@example.com",
  "alreadySaved": false
}
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
| Store user reference, not IBAN/account numbers | Closed P2P wallet model |
| Lookup-then-add two-step flow | Lets the user confirm identity before saving |
| Email included on lookup | Helps disambiguate same names in a demo network |
| DB unique constraint | Prevents race-condition duplicates |
| Remove returns 404 (not silent 204) | Clear client semantics; tested |
| Transfer still uses `receiverUserId` | Beneficiaries are a convenience list; wiring into transfer UX is M9 |
| Future: lookup by email/alias | Can extend resolve without changing the Beneficiary table |

## Acceptance criteria

- [x] Lookup returns candidate name/id (and email) or 404
- [x] Duplicate add returns clear ProblemDetails (409)
- [x] Cannot add self (lookup + add)
- [x] Remove missing/foreign id returns 404
- [ ] Transfer UI can consume beneficiary list (Milestone 9)
