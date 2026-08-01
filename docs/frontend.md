# Frontend (Angular)

**Milestone:** 9  
**Status:** In progress — core screens wired; design system polished for light-mode P2P wallet UX

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

## Visual language (Wise / Revolut / Apple Cash inspired)

Cool slate surfaces, indigo brand actions, emerald inbound money — trust-forward consumer fintech.

| Token | Role |
|---|---|
| Indigo primary `#4F46E5` | CTAs, active nav, focus rings, brand accent |
| Surface `#F8FAFC` | App background (cool off-white, no harsh pure white) |
| Elevated white `#FFFFFF` | Cards with `border-slate-200/80` + soft shadow |
| Ink `#0F172A` / muted `#64748B` | Primary + secondary text |
| Emerald `#059669` | Received / success amounts and badges |

**Typography:** Inter with `tabular-nums` on financial amounts.  
**Spatial grid:** 8pt — card padding `p-5`/`p-6`, cards `rounded-2xl`, controls `rounded-xl`.

**UX principles for hiring demos**

1. **Balance first** — Home opens on money, not a dashboard grid.
2. **Privacy control** — Hide-balance toggle on the home hero.
3. **One job per screen** — Transfer is a short guided flow (recipient → keypad amount → confirm).
4. **Confirm before commit** — Beneficiary lookup shows name before save/send.
5. **Honest fee copy** — Transfers show “No fee” (no implied fee engine); success shows `result.fee` when non-zero.
6. **Human errors** — ProblemDetails mapped to plain language toasts/field errors.
7. **Mobile-friendly shell** — Bottom nav for primary destinations; works in a phone viewport.

### Known polish gaps

| Gap | Why | Follow-up |
|---|---|---|
| Activity rows lack counterparty names | API returns `counterpartyWalletId` only | Enrich DTO with display name, or map wallet→user once available |
| No request-money flow | Not in API scope | Keep Home CTA as **People**, not Request |

## Shared UI primitives

| Component | Path |
|---|---|
| `app-icon` | `shared/ui/icon.ts` |
| `app-avatar` | `shared/ui/avatar.ts` |
| `app-status-badge` | `shared/ui/status-badge.ts` |
| Date grouping helpers | `shared/utils/transaction-groups.ts` |

Prefer:

- Semantic Tailwind theme colors (`bg-primary`, `text-ink-muted`, `text-success`)
- Shared classes: `.btn-primary`, `.input`, `.page`, `.balance-hero`, `.filter-tab`, `.skeleton`, …

Avoid:

- Per-component large CSS files
- Ad-hoc hex colors in templates
- Duplicate button/input styling across features

Tokens live in `client/src/styles.css` (`@theme` + `@layer components`).

## App structure (target)

```text
client/src/app/
  core/           auth, interceptors, guards, API helpers
  shared/         pipes, icons, avatars, tiny helpers
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
| 8 | UI/UX refinement + polish closeout | In progress |

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
