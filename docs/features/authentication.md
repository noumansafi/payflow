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

Interactive docs (Development): `http://localhost:5079/swagger`

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
- Refresh tokens stored hashed only; password change/reset revokes active sessions
- Forgot-password response never returns the reset token (delivered via mock email/logs)
- Refresh-token reuse after rotation revokes the user's active sessions
- Validation via FluentValidation pipeline (not controllers)
- Register may return email verification token for local demo convenience

## Tests

Unit coverage for register/login/refresh/forgot-password rules, password hashing, and register validation.
