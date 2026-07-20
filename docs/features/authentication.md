# Feature: Authentication

**Milestone:** 2  
**Status:** Done

## Purpose

Secure identity for wallet operations: register, login, refresh, logout, and basic account recovery.

## Implemented

| Capability | Notes |
|---|---|
| Register | Creates user + wallet in one save; issues email verification token (mock) |
| Login | JWT access token + hashed refresh token |
| Refresh | Rotates refresh token |
| Logout | Revokes refresh token; audits |
| Roles | JWT role claim (`User` / `Admin`) |
| Email verification | Mock token flow |
| Password forgot/reset | Mock token flow; generic response avoids enumeration |
| Password change | Authenticated; audited |
| Current user | `GET /api/v1/auth/me` |

## API

```http
POST /api/v1/auth/register
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
POST /api/v1/auth/password/forgot
POST /api/v1/auth/password/reset
POST /api/v1/auth/password/change
POST /api/v1/auth/email/verify
GET  /api/v1/auth/me
```

## Design notes

- Application uses ports (`IUserRepository`, `ITokenService`, …) — no EF in handlers
- Passwords hashed via ASP.NET Identity hasher
- Refresh tokens stored hashed only
- Validation via FluentValidation pipeline (not controllers)
- Demo responses may return mock tokens for local testing

## Tests

Unit coverage for register/login/refresh/forgot-password rules, password hashing, and register validation.
