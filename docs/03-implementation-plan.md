# Expense Planner (Demo) — Implementation Plan (Lead Dev)

This plan turns [docs/01-requirements.md](docs/01-requirements.md) and [docs/02-architecture.md](docs/02-architecture.md) into an executable delivery sequence.

## 1. Delivery Principles (demo)
- Ship **vertical slices** early: Forecast + basic transaction CRUD first.
- Keep persistence **CSV-first** but behind interfaces, so DB mode can be added later.
- Avoid over-engineering: one API, one SPA, one local data folder.
- Use deterministic “today” via `IClock` so forecasting is testable.

## 2. Milestones
### M0 — Repo & Dev Infrastructure (foundation)
Outcome: repo has backend + frontend scaffolding, consistent formatting/linting, and a single-command developer workflow.

Includes:
- Git repo hygiene (README, .gitignore)
- Backend solution/projects scaffolding
- Frontend scaffolding (Vite React TS + Tailwind)
- Lint/format scripts (C# + TS)

### M1 — Domain + Application Core
Outcome: core types and forecasting algorithm implemented and unit-tested, independent of storage.

Includes:
- Domain entities + recurrence model
- Forecast calculation (daily aggregation + recurrence occurrence generation)
- Application services and interfaces

### M2 — CSV Persistence (prototype mode)
Outcome: data is stored/reloaded from disk using CSV repositories.

Includes:
- CSV schemas + repositories
- Reset operation
- Minimal error handling around file IO

### M3 — REST API
Outcome: working API with endpoints for transactions, recurring transactions, forecast, reset; input validation; Swagger.

Includes:
- Controllers + DTOs + FluentValidation
- CORS for local dev
- Integration tests (happy path)

### M4 — UI (MVP)
Outcome: user can add/edit/delete transactions, manage recurrence, view forecast and upcoming items.

Includes:
- Routing and modules/pages
- Forms and lists
- API client

### M5 — End-to-End Polish
Outcome: stable demo experience.

Includes:
- Seed/demo data helper (optional)
- UX edge cases: empty states, validation messages
- Performance sanity (365-day forecast)

### M6 (Optional) — DB Mode in Docker
Outcome: optional switch from CSV to DB with DbUp + Dapper.

Includes:
- Docker compose
- DbUp migrations
- Dapper repositories

## 3. Proposed Folder Layout
### Backend
- `backend/ExpensePlanner.sln`
- `backend/src/ExpensePlanner.Domain`
- `backend/src/ExpensePlanner.Application`
- `backend/src/ExpensePlanner.DataAccess`
- `backend/src/ExpensePlanner.Api`
- `backend/tests/ExpensePlanner.Domain.Tests`
- `backend/tests/ExpensePlanner.Application.Tests`
- `backend/tests/ExpensePlanner.Api.Tests` (integration)

### Frontend
- `frontend/` (Vite project)
  - `src/api/` (REST client)
  - `src/pages/` (Forecast, Transactions, Recurring, DataReset)
  - `src/components/` (shared UI)
  - `src/routes/`

### Data (prototype)
- `data/` (ignored by git)
  - `transactions.csv`
  - `recurring_transactions.csv`
  - `recurrence_rules.csv`

## 4. Build/Run UX (local)
Target developer commands:
- Backend: `dotnet run --project backend/src/ExpensePlanner.Api`
- Frontend: `npm install` then `npm run dev` in `frontend/`

Optional: a top-level `README.md` with copy/paste steps.

## 5. Task Breakdown
See [docs/04-tasks.md](docs/04-tasks.md).

## 6. Risks & Spikes
- Recurrence correctness (especially yearly day-of-year): add unit tests and pick a clear definition (1..366).
- CSV concurrency and partial writes: keep single-user assumption; write full-file atomically (write temp then replace).

## 7. Definition of Done (per task)
- Code compiles and all tests pass locally.
- Lint/format checks pass.
- APIs validated (requests + 400 error cases) where applicable.
- No new terminology drift (use “past” / “future-dated”; avoid deprecated wording).
- Updated docs if public behavior changed.
