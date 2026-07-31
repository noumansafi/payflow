# Frontend (Angular)

**Milestone:** 9  
**Status:** Done

## Why a dedicated frontend milestone

Backend architecture is the primary hiring signal. The Angular app proves full-stack delivery: a non-technical reviewer can *feel* the product (wallet, send money, history) without reading C#.

## Stack choices

| Choice | Why |
|---|---|
| Angular 20 (standalone) | Matches README target; signals + modern templates |
| Tailwind CSS (v4) | Fast, consistent UI without Material default look; utility-first keeps styles generic |
| Shared component classes in `styles.css` | Avoids repeating long class strings / one-off hex in every template |
| `client/` at repo root | Keeps the .NET solution clean; frontend is its own app |
| Dev proxy → `localhost:5079` | Same-origin `/api/v1` via Angular proxy (`dotnet run` port). Docker API uses `8080` — update `proxy.conf.json` if needed |
| Dev CORS (`FrontendDev`) | Backup if the browser calls the API origin directly from `:4200` |
| Light theme only | Product direction: bright, approachable wallet — no dark mode |

We **do not** use Angular Material for this milestone (overridden from earlier roadmap notes) so the UI does not look like a stock admin kit.

## Visual language (SadaPay-inspired)

Inspired by SadaPay’s mint + peach consumer fintech feel — not a copy of their brand assets.

| Token | Role |
|---|---|
| Mint primary `#00C9A7` | CTAs, active nav, balance hero, focus rings |
| Peach accent `#FF7B66` | Sparse highlights (e.g. alerts, secondary emphasis) |
| Soft mint surface `#F4FBF9` | App background |
| Ink `#12323C` | Body text (readable contrast on light surfaces) |

**UX principles for hiring demos**

1. **Balance first** — Home opens on money, not a dashboard grid.
2. **One job per screen** — Transfer is a short guided flow, not a dense form.
3. **Confirm before commit** — Beneficiary lookup shows name before save/send.
4. **Human errors** — ProblemDetails mapped to plain language toasts/field errors.
5. **Mobile-friendly shell** — Bottom nav for primary destinations; works in a phone viewport.

## Generic CSS approach

Prefer:

- Semantic Tailwind theme colors (`bg-primary`, `text-ink-muted`)
- Shared classes: `.btn-primary`, `.input`, `.page`, `.balance-hero`, …

Avoid:

- Per-component large CSS files
- Ad-hoc hex colors in templates
- Duplicate button/input styling across features

Tokens live in `client/src/styles.css` (`@theme` + `@layer components`).

## App structure (target)

```text
client/src/app/
  core/           auth, interceptors, guards, API helpers
  shared/         pipes, tiny presentational helpers
  layout/         shell + auth layout
  features/       auth, home, transfer, transactions, …
```

## Local run

```bash
# terminal 1 — API (see root README)
dotnet run --project src/PayFlow.Api

# terminal 2 — UI (Node 20.19+ / 22+)
cd client
npm start
```

Open `http://localhost:4200`. API calls go to `/api/v1/*` via `proxy.conf.json` (defaults to `http://localhost:5079`).

**Not CORS:** a `500` means the server threw. CORS failures show as browser “blocked by CORS policy” and usually never reach a normal JSON body. If login fails after a proxy port mismatch, restart `npm start` after fixing `proxy.conf.json`.

## Delivery slices

| Slice | Focus | Status |
|---|---|---|
| 1–2 | Scaffold, Tailwind tokens, shell, home skeleton | Done |
| 3 | Auth (login/register/session, guards, interceptor) | Done |
| 4 | Home + wallet (balance, freeze, dev credit) | Done |
| 5 | Transfer flow (lookup → amount → confirm) | Done |
| 6 | Transactions + beneficiaries | Done |
| 7 | Notifications + profile + admin audit | Done |
| 8 | Polish + root README closeout | Done |

### Auth session notes

- Access token kept in memory (signal); refresh token in `localStorage`
- `authInterceptor` attaches Bearer and retries once after refresh on 401
- `authGuard` / `guestGuard` / `adminGuard` protect routes
- Dev-friendly verify/reset: tokens may appear in API logs or register response

## Out of scope (for now)

- Dark mode
- SignalR realtime
- Native mobile / PWA packaging
- i18n
- Pixel-perfect clone of any commercial wallet
