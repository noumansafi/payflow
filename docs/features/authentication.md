# Feature: Authentication

**Milestone:** 2  
**Status:** Planned

## Purpose

Securely identify users and authorize wallet operations. Auth is the gateway for every money-moving action.

## Capabilities

| Capability | Notes |
|---|---|
| Register | Creates user + wallet (same transaction) |
| Login | Issues JWT access token + refresh token |
| Refresh | Rotates/validates refresh token; issues new access token |
| Logout | Revokes refresh token; audit log |
| Roles | RBAC (e.g. `User`, `Admin`) |
| Email verification | Mock provider (no real SMTP required) |
| Password reset | Mock token flow |
| Password change | Authenticated; audit logged |

## Security design

- Passwords hashed with a modern algorithm (e.g. ASP.NET Core Identity hasher or equivalent)
- Short-lived access tokens
- Longer-lived refresh tokens stored server-side (hashed), revocable
- Claims include subject, email, roles — minimal necessary set
- No secrets in source control; config via environment / user secrets

## Commands / queries (target)

**Commands:** `RegisterUser`, `Login`, `RefreshToken`, `Logout`, `ChangePassword`, `RequestPasswordReset`, `ResetPassword`, `VerifyEmail`  
**Queries:** `GetCurrentUser` (profile)

## API sketch

```http
POST /api/v1/auth/register
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
POST /api/v1/auth/password/forgot
POST /api/v1/auth/password/reset
POST /api/v1/auth/email/verify
GET  /api/v1/auth/me
```

## Tradeoffs

| Choice | Rationale |
|---|---|
| Custom auth vs Identity | Clearer portfolio narrative; more code ownership |
| Refresh tokens in DB | Enables revocation; required for logout semantics |
| Mock email | Keeps local/demo frictionless; interface allows real SMTP later |

## Fintech note

Banks rarely own full IdP stacks in-app. They federate to enterprise IAM. PayFlow still implements token hygiene so the API security story is complete and interview-ready.

## Acceptance criteria

- [ ] Register creates user + active wallet atomically
- [ ] Invalid credentials return ProblemDetails (no user enumeration leaks where practical)
- [ ] Access token required for protected endpoints
- [ ] Refresh flow works; revoked tokens fail
- [ ] Unit tests for password/hash and token rules
- [ ] Integration tests for register/login/refresh
