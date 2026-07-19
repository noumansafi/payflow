# Feature: Beneficiaries

**Milestone:** 6  
**Status:** Planned

## Purpose

Let users save frequent transfer recipients to reduce friction and mistakes.

## Operations

| Operation | Rules |
|---|---|
| Add beneficiary | Target user must exist; not self; no duplicates |
| Remove beneficiary | Owner only |
| List beneficiaries | Owner only; support pagination if list grows |

## Commands / queries

**Commands:** `AddBeneficiary`, `RemoveBeneficiary`  
**Queries:** `GetBeneficiaries`

## API sketch

```http
GET    /api/v1/beneficiaries
POST   /api/v1/beneficiaries
DELETE /api/v1/beneficiaries/{id}
```

## Data integrity

Enforce uniqueness with a composite unique index, e.g. `(OwnerUserId, BeneficiaryUserId)`, not only application checks.

## Tradeoffs

| Choice | Rationale |
|---|---|
| Store user reference, not free-text account numbers | Matches closed P2P network model |
| DB unique constraint | Prevents race-condition duplicates |

## Acceptance criteria

- [ ] Duplicate add returns clear ProblemDetails
- [ ] Cannot add self
- [ ] Remove is idempotent or clearly 404 — pick one and test it
- [ ] Transfer UI can consume beneficiary list (Milestone 9)
