# Architecture tradeoffs

This document captures decisions a hiring manager can discuss in an interview.

## 1. Clean Architecture vs vertical slices

| | Clean Architecture | Vertical slices |
|---|---|---|
| Pros | Clear dependency rules; great for demonstrating SOLID | Fast feature delivery; less ceremony |
| Cons | More projects/folders early | Easy to blur boundaries without discipline |

**Choice:** Clean Architecture. For a portfolio proving senior backend judgment, explicit boundaries beat speed.

## 2. Monolith first vs microservices

PayFlow is a **modular monolith**.

Microservices would require network boundaries, distributed transactions, and ops overhead that obscure the learning goals (wallet correctness, auth, CQRS).

**Evolution path:** extract Transfer/Ledger later behind interfaces + outbox — without rewriting domain rules.

## 3. Custom auth vs ASP.NET Identity vs external IdP

| Approach | When it wins |
|---|---|
| Custom JWT + refresh | Portfolio clarity; full control of claims/token lifecycle |
| ASP.NET Identity | Faster standards-compliant user store |
| Auth0 / Cognito / Entra | Real enterprise default |

**Choice:** Custom auth with secure defaults, while documenting that production fintech typically offloads identity.

## 4. Decimal money vs minor units

- **`decimal(18,2)`:** readable, EF-friendly, good for demos.
- **Minor units (`long` cents):** preferred in many payment cores to avoid rounding ambiguity.

**Choice:** decimal + currency for MVP; document minor-units as a hardening step.

## 5. Same-transaction side effects vs outbox

On transfer completion we will:

1. Debit/credit wallets
2. Insert transaction row
3. Write audit log
4. Insert notification

Initially in **one DB transaction**.

**Tradeoff:** simpler consistency now; weaker if we later add email/SignalR that must not roll back money movement. Outbox pattern is the upgrade path used by serious payment platforms.

## 6. EF Core in Infrastructure only

Application talks through abstractions (repositories / unit of work / query services).

**Why:** keeps business rules testable without a database, and prevents “accidentally using SQL Server types in handlers.”

## 7. Shared kernel discipline

`PayFlow.Shared` stays thin (results, pagination primitives, constants).

If Shared grows domain entities or EF types, the architecture has already failed.

## 8. Frontend after API solidity

UI is Milestone 9 on purpose. Hiring signal for this repo is backend architecture; Angular then proves full-stack delivery without driving domain design from screens.
