# Expense Planner (Demo) — Task Breakdown

Tasks are sized for ~0.5–1.5 days each and intended to be assigned to small “dev agents”.

Conventions:
- `T-xxx` = task id
- Dependencies listed as task ids
- “Acceptance checks” are practical verification steps

## Milestone M0 — Repo & Dev Infrastructure

### T-001 [BACK] Initialize repository hygiene
- Summary: Add baseline repo files and ignores.
- Depends on: —
- Steps:
  - Add `README.md` (local run steps for backend + frontend).
  - Add `.gitignore` (ignore `data/`, `backend/**/bin/`, `backend/**/obj/`, `frontend/node_modules/`, `frontend/dist/`).
  - Add `.editorconfig` (basic whitespace rules).
- Tests: n/a
- Acceptance checks:
  - `git status` shows expected ignores.

### T-002 [BACK] Scaffold backend solution + projects
- Summary: Create .NET 10 solution with layered projects.
- Depends on: T-001
- Files/modules:
  - `backend/ExpensePlanner.sln`
  - `backend/src/*` projects
- Steps:
  - Create solution and projects: Domain, Application, DataAccess, Api.
  - Set references according to architecture (Api → Application → Domain; DataAccess → Application+Domain).
  - Add basic DI wiring in Api.
  - Add Swagger in development.
- Tests:
  - Add empty test projects: Domain.Tests, Application.Tests.
- Acceptance checks:
  - `dotnet build backend/ExpensePlanner.sln` passes.

### T-003 [BACK] Backend linting/formatting baseline
- Summary: Set up formatting + analyzers without over-complication.
- Depends on: T-002
- Steps:
  - Add `.editorconfig` rules specific to C# if needed.
  - Add `dotnet format` guidance in README (or CI later).
  - Ensure nullable reference types enabled.
- Tests: n/a
- Acceptance checks:
  - `dotnet format --verify-no-changes` (optional) is clean after formatting.

### T-004 [UI] Scaffold frontend (Vite React TS + Tailwind)
- Summary: Create the React UI project skeleton.
- Depends on: T-001
- Files/modules:
  - `frontend/` Vite project
- Steps:
  - Create Vite project (React + TS).
  - Add Tailwind CSS per standard Vite+Tailwind setup.
  - Add React Router and minimal routes.
- Tests:
  - Optional: keep Vite template tests out-of-scope unless already present.
- Acceptance checks:
  - `npm run dev` starts.

### T-005 [UI] Frontend linting/formatting baseline
- Summary: ESLint + Prettier + Tailwind class sorting (if desired) kept simple.
- Depends on: T-004
- Steps:
  - Ensure ESLint is configured for TS + React.
  - Add Prettier and `npm run format`.
  - Add `npm run lint`.
- Acceptance checks:
  - `npm run lint` passes.

## Milestone M1 — Domain + Application Core

### T-010 [BACK] Implement Domain models
- Summary: Add `Transaction`, `RecurringTransaction`, `RecurrenceRule`.
- Depends on: T-002
- Steps:
  - Implement entities and enums (`TransactionType`, `RecurrenceUnit`).
  - Document `DayIndex` meaning per unit (Week 1..7, Month 1..28, Year 1..366).
- Tests:
  - Domain unit tests for invariants (range checks if enforced in domain).
- Acceptance checks:
  - Domain compiles and tests pass.

### T-011 [BACK] Implement recurrence occurrence generator
- Summary: Generate dates for a `RecurringTransaction` using `RecurrenceRule`.
- Depends on: T-010
- Steps:
  - Implement generator producing occurrence dates in `[from,to]`.
  - Weekly: align by weekday index.
  - Monthly: day 1..28.
  - Yearly: day-of-year 1..366 (handle non-leap years by skipping 366).
- Tests:
  - Unit tests covering each unit and edge cases.
- Acceptance checks:
  - Tests validate expected occurrences for a fixed time range.

### T-012 [BACK] Implement forecast calculation (daily aggregation)
- Summary: Compute daily balances and balance-at-date.
- Depends on: T-010, T-011
- Steps:
  - Aggregate all one-time transactions with date <= end.
  - Add generated recurring occurrences (as virtual transactions).
  - Produce daily net and prefix-sum daily balances.
- Tests:
  - Unit tests: income/expense sums, multi-day ranges, delete effect scenario.
- Acceptance checks:
  - For a known fixture, balances match expected numbers.

### T-013 [BACK] Define Application ports + services
- Summary: Add repository interfaces and services.
- Depends on: T-010, T-012
- Steps:
  - Add `ITransactionRepository`, `IRecurringTransactionRepository`, `IRecurrenceRuleRepository`.
  - Add `IClock`.
  - Implement services: `TransactionService`, `RecurringTransactionService`, `ForecastService`.
- Tests:
  - Unit tests with in-memory repositories.
- Acceptance checks:
  - Application tests pass; no IO in Application.

## Milestone M2 — CSV Persistence

### T-020 [BACK] Define CSV schemas and file locations
- Summary: Decide headers and file format for each CSV.
- Depends on: T-002
- Steps:
  - Define CSV headers for `transactions.csv`, `recurring_transactions.csv`, `recurrence_rules.csv`.
  - Decide storage root path (e.g., `./data` relative to API working directory, configurable).
- Tests:
  - Small parsing/serialization tests.
- Acceptance checks:
  - Files can be created on first run.

### T-021 [BACK] Implement CSV repositories
- Summary: Implement repository interfaces using CSV.
- Depends on: T-020, T-013
- Steps:
  - Implement read/write with atomic replace.
  - Ensure stable IDs for update/delete.
- Tests:
  - Unit tests for CRUD behavior.
- Acceptance checks:
  - Restart API and data persists.

### T-022 [BACK] Implement reset operation
- Summary: Clear CSV files safely.
- Depends on: T-021
- Steps:
  - Implement a method that deletes or truncates CSV files.
  - Expose via Application service.
- Tests:
  - Unit test: reset removes stored records.
- Acceptance checks:
  - After reset, forecast shows empty state.

## Milestone M3 — REST API

### T-030 [BACK] Add API DTOs + FluentValidation
- Summary: Create request/response contracts with validation.
- Depends on: T-013
- Steps:
  - DTOs for transaction CRUD.
  - DTOs for recurring transactions + recurrence rules.
  - Validators for amount/date/rule ranges.
- Tests:
  - Validator unit tests (valid/invalid samples).
- Acceptance checks:
  - Invalid payloads return 400 with messages.

### T-031 [BACK] Implement Transactions endpoints
- Summary: `/transactions` CRUD and query.
- Depends on: T-030, T-021
- Steps:
  - `GET /transactions` supports `from`, `to`, and optional `scope=past|future|all` (based on `IClock`).
  - `POST/PUT/DELETE`.
- Tests:
  - API integration tests (in-memory test server) for CRUD.
- Acceptance checks:
  - Can add a future-dated transaction and retrieve it.

### T-032 [BACK] Implement Recurring endpoints
- Summary: `/recurring-transactions` CRUD + pause/resume.
- Depends on: T-030, T-021
- Tests:
  - Integration tests for pause/resume.
- Acceptance checks:
  - Paused recurring items produce no occurrences in forecast.

### T-033 [BACK] Implement Forecast endpoints
- Summary: `/forecast` and `/forecast/balance`.
- Depends on: T-012, T-021
- Tests:
  - Integration tests using known fixtures.
- Acceptance checks:
  - Forecast reflects both one-time and recurring occurrences.

### T-034 [BACK] Implement Reset endpoint
- Summary: `POST /reset` clears data.
- Depends on: T-022
- Tests:
  - Integration test verifying data cleared.
- Acceptance checks:
  - UI can call reset and see empty state.

## Milestone M4 — UI (MVP)

### T-040 [UI] Frontend routing + layout shell
- Summary: Add pages and navigation.
- Depends on: T-004
- Steps:
  - Routes: Forecast, Transactions, Recurring, DataReset.
- Acceptance checks:
  - Navigation works; empty states are visible.

### T-041 [UI] API client layer
- Summary: Implement typed API client functions.
- Depends on: T-040
- Steps:
  - Centralize base URL.
  - Functions for transactions, recurring, forecast, reset.
- Acceptance checks:
  - All calls compile with TS types.

### T-042 [UI] Transactions page
- Summary: List + add/edit/delete transactions.
- Depends on: T-041
- Steps:
  - Form for income/expense.
  - Date picker.
  - Optional toggle/filter: past vs future.
- Acceptance checks:
  - Editing updates list and forecast.

### T-043 [UI] Recurring page
- Summary: Create/edit/pause/resume recurring items.
- Depends on: T-041
- Steps:
  - Form for unit/interval/day index.
  - List with pause/resume controls.
- Acceptance checks:
  - Recurring items affect forecast.

### T-044 [UI] Forecast page
- Summary: Forecast date + range and display balances.
- Depends on: T-041
- Steps:
  - Date selector and preset ranges.
  - Display predicted balance for selected date.
  - Show upcoming items within horizon.
- Acceptance checks:
  - Changing date updates results.

### T-045 [UI] Data reset page
- Summary: Confirm and call reset.
- Depends on: T-041
- Acceptance checks:
  - Confirmation required; data cleared.

## Milestone M5 — Polish

### T-050 [UI] Improve empty states + error handling
- Summary: Provide friendly messages when no data exists.
- Depends on: T-042..T-045
- Acceptance checks:
  - No blank screens; errors surfaced.

### T-051 [BACK] Performance sanity + test coverage check
- Summary: Ensure 365-day forecast runs fast and is well-tested.
- Depends on: T-033
- Acceptance checks:
  - Forecast range request returns promptly with ~1k occurrences.

## Milestone M6 (Optional) — DB Mode

### T-060 [BACK] Add docker compose + DbUp migrations
- Summary: Introduce DB mode behind `Storage:Mode=Db`.
- Depends on: M3 complete

### T-061 [BACK] Implement Dapper repositories
- Summary: Replace CSV repositories with Dapper implementations.
- Depends on: T-060

## Definition of Done (global)
- `dotnet build` and tests pass for backend.
- `npm run lint` passes for frontend.
- Forecast logic has unit tests for recurrence units.
- API returns 400 for invalid inputs (validated).
- Docs updated if endpoints/contracts change.
