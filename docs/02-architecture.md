# Expense Planner (Demo) — High-Level Architecture

## 1. Context & Constraints
**Primary goal**: predict available funds at a future date based on past and future-dated (one-time/recurring) income/expense.

**Constraints from product/tech**
- Runs on a local machine; no deployment required right now.
- Persistence starts as CSV (prototype), but must be easy to switch to a real DB running in Docker later.
- Backend: .NET 10, ASP.NET Core Web API, Dapper, DbUp, FluentValidation, REST.
- Frontend: React + TypeScript + Vite + React Router + Tailwind CSS.
- Keep it demo-simple; avoid non-essential modules.

**Refinements (from requirements discussion)**
- Currency is hardcoded on the UI side (single currency).
- There is no separate “starting balance” setting; balance is computed as an aggregation of stored transactions.
- A transaction’s date determines whether it is in the past (already happened) or future (planned).

## 2. Architectural Goals (mapped to NFRs)
- AG-001 (NFR-001): Simple user flows; minimal screens and concepts.
- AG-002 (NFR-002): Forecast calculations fast for ~365 days and ~1k generated occurrences.
- AG-003 (NFR-003): Reliable local persistence.
- AG-004 (NFR-004): Privacy by design: local-only by default.
- AG-005 (Flexibility): Swap persistence from CSV to DB with minimal changes to higher layers.

## 3. System Overview
### 3.1 System Context (C4 L1)
- **User** interacts with a **Web UI (SPA)** running locally.
- UI communicates with a **local ASP.NET Core Web API**.
- API persists data via a **Storage Provider** (CSV initially; DB later).

### 3.2 Container View (C4 L2)
- **Frontend container**: React SPA
  - Serves UI, calls backend via REST.
- **Backend container**: ASP.NET Core Web API
  - Implements business use-cases, forecasting, validation.
- **Storage container** (mode-dependent)
  - Prototype mode: local CSV files on disk.
  - Later mode: Docker DB (e.g., PostgreSQL) accessed via Dapper.

## 4. Backend Architecture (.NET)
### 4.1 Layering / Projects
A minimal, clean layering to support demo scope without over-engineering:

- **ExpensePlanner.Domain**
  - Pure domain entities + value objects (no IO)
  - Domain rules that are stable (e.g., transaction type, recurrence definitions)

- **ExpensePlanner.Application**
  - Use cases / services (forecasting, transaction management)
  - Interfaces/ports for persistence (repositories) and time
  - DTOs for application boundaries (optional; API DTOs may map directly)

- **ExpensePlanner.DataAccess**
  - Persistence implementations
  - CSV implementation for prototype
  - Dapper implementation for DB mode

- **ExpensePlanner.Api**
  - REST controllers/endpoints
  - Request/response models
  - FluentValidation validators for request models
  - DI composition root + Swagger/OpenAPI

**Dependency direction**
- Api → Application → Domain
- DataAccess → Application + Domain
- Domain has no dependencies on other projects

### 4.2 Key Domain Model (high level)
**Entities**
- `Transaction`
  - `Id` (GUID)
  - `Type` (Income | Expense)
  - `Amount` (decimal)
  - `Date` (date-only)
  - `Description?`
  - `SourceRecurringTransactionId?` (nullable; links an “occurred” transaction to a recurring template)

- `RecurringTransaction`
  - `Id` (GUID)
  - `Type` (Income | Expense)
  - `Amount`
  - `StartDate`
  - `RecurrenceRuleId` (GUID)
  - `Description?`
  - `IsPaused`

- `RecurrenceRule`
  - `Id` (GUID)
  - `Unit` (Week | Month | Year)
  - `Interval` (int, e.g., every 1 week / every 2 months)
  - `DayIndex` (int)
    - Week: 1..7 (e.g., Monday=1)
    - Month: 1..28
    - Year: 1..366 (day-of-year)

**Notes (demo simplifications)**
- Monthly day-of-month restricted to 1–28 (matches RISK-001 mitigation).
- Date granularity is day-only (matches ASSUMP-004).
- “Past vs future-dated” is derived from `Date` relative to “today” (provided by `IClock`/`IDateProvider`).

### 4.3 Application Services
- `ForecastService`
  - Inputs: forecast date or range, transactions, recurring transactions + recurrence rules
  - Outputs:
    - Predicted balance at date D (FR-030)
    - Daily series for range (FR-031)
    - Contributing items list (FR-032)

- `TransactionService`
  - CRUD for transactions (past and future) (FR-010..013)
  - “Past vs future-dated” for UX is a query/view concern (based on `Date`).

- `RecurringTransactionService`
  - CRUD for recurring transactions (FR-022..024)
  - Pause/resume (FR-023)

**Cross-cutting ports (interfaces)**
- `IClock` / `IDateProvider` (for “today” semantics)
- `ITransactionRepository`
- `IRecurringTransactionRepository`
- `IRecurrenceRuleRepository`

### 4.4 Forecast Calculation Approach (BR-001..007)
1. Gather transactions within range:
  - stored transactions with `Date <= end` (includes future-dated planned items)
  - generated occurrences for recurring transactions between range start and end
2. Aggregate by date:
   - daily net = sum(incomes) - sum(expenses)
3. Compute prefix sums to produce the daily balance series.

**Starting point**
- Balance is computed from transactions only (no separate starting balance setting). If a user wants to represent an initial amount of money, it is recorded as an income transaction (e.g., “Initial balance”) on a chosen date.

**Complexities intentionally avoided in demo**
- No partial-day ordering (BR-007)
- No currency conversions

### 4.5 Validation Strategy (FluentValidation)
- Validate API request DTOs:
  - Amount > 0
  - Date required and within reasonable bounds
  - Recurrence rule:
    - `Unit` is supported (Week/Month/Year)
    - `Interval` within allowed bounds (demo: 1..52)
    - `DayIndex` range depends on `Unit` (Week 1..7; Month 1..28; Year 1..366)
- Return consistent 400 responses with validation errors.

### 4.6 Persistence Strategy (CSV now, DB later)
#### 4.6.1 Persistence Abstraction
Design to keep CSV vs DB as a replaceable implementation:
- Application depends only on repository interfaces.
- DataAccess provides two implementations:
  - `Csv*Repository`
  - `Db*Repository` (Dapper)

Choose storage mode via configuration:
- `Storage:Mode = Csv | Db`

#### 4.6.2 CSV Prototype Storage
**Files (suggested)**
- `data/transactions.csv`
- `data/recurring_transactions.csv`
- `data/recurrence_rules.csv`

**CSV schema approach**
- Fixed headers; unknown columns ignored for forward compatibility.
- Each record includes `Id` to support edits/deletes.

#### 4.6.3 Database (later, Docker)
**DB choice (suggestion)**: PostgreSQL in Docker (common), or SQLite for simplest local DB.

- Use Dapper for queries/commands.
- Use DbUp for migrations:
  - SQL scripts in `ExpensePlanner.DataAccess/Migrations`
  - Run migrations on API startup in DB mode.

**DB tables (minimal)**
- `transactions`
- `recurring_transactions`
- `recurrence_rules`

### 4.7 API Surface (REST, minimal)
Base path: `/api`

**Transactions**
- `GET /transactions?from=YYYY-MM-DD&to=YYYY-MM-DD&scope=past|future|all`
  - `scope` is evaluated relative to “today” (`IClock`), for UX toggles like Past vs Future.
- `POST /transactions`
- `PUT /transactions/{id}`
- `DELETE /transactions/{id}`

**Recurring transactions**
- `GET /recurring-transactions`
- `POST /recurring-transactions`
- `PUT /recurring-transactions/{id}`
- `POST /recurring-transactions/{id}/pause`
- `POST /recurring-transactions/{id}/resume`
- `DELETE /recurring-transactions/{id}`

**Forecast**
- `GET /forecast?from=YYYY-MM-DD&to=YYYY-MM-DD`
  - returns daily balances + contributing items summary
- `GET /forecast/balance?date=YYYY-MM-DD`
  - returns predicted balance at a single date

**Demo data management**
- `POST /reset` → clears local storage (CSV files or DB tables) after confirmation in UI

**Notes**
- Keep responses small; frontend can request ranges as needed.
- Swagger enabled in development.

### 4.8 Local-Only Execution Model
- Backend runs as `dotnet run` with a local HTTP port.
- Frontend runs as `vite` dev server.
- Configure CORS for local dev origin(s).

No authentication for demo (ASSUMP-001), but structure code so auth can be added later at API edge.

## 5. Frontend Architecture (React)
### 5.1 Structure
- React Router for modules/pages.
- A small API client layer:
  - `src/api/*` functions calling backend endpoints
- State management:
  - Keep simple: React Query or minimal custom hooks (choose later during implementation)

### 5.2 UX Modules (what the user interacts with)
Minimal set aligned to requirements:

1. **Forecast (Dashboard)**
  - Current balance summary (derived from transactions)
   - Forecast date picker (FR-030)
   - Range selection (30/90/custom) and daily series (FR-031)
   - Contributing items list (FR-032)
   - “Upcoming in 30 days” list (FR-040)

2. **Transactions**
  - Toggle: Past vs Future
   - Add/edit/delete forms (FR-010..012, FR-020..021, FR-024)

3. **Recurring**
   - List recurring schedules
   - Create/edit
   - Pause/resume (FR-023)

4. **Data (Reset)**
  - Reset demo data (FR-051)

## 6. Observability & Diagnostics (dev-friendly)
- Structured logs in API (request logging + key operations)
- Basic error handling:
  - consistent error response shape
  - validation errors (400) vs server errors (500)

## 7. Security & Privacy (demo posture)
- Local-only by default; no external services required.
- No authentication initially.
- Data stored locally (CSV or DB container) and should be treated as sensitive by the user.

## 8. ADRs (Architecture Decision Records)
### ADR-001: Layered architecture with Application ports
- **Decision**: Use Domain/Application/DataAccess/Api layering with repository interfaces in Application.
- **Why**: Enables CSV-first with a clean swap to DB later.
- **Alternatives**: Single project; EF Core; direct file IO in controllers.

### ADR-002: CSV as initial storage
- **Decision**: CSV storage for prototype with explicit schemas and IDs.
- **Why**: Fast to implement, easy to inspect.
- **Tradeoff**: Concurrency and schema evolution are limited.

### ADR-003: DbUp + Dapper for DB mode
- **Decision**: When moving to DB, use DbUp migrations + Dapper repositories.
- **Why**: Aligns with stack requirements and keeps data access explicit.

### ADR-004: Recurrence limited for demo
- **Decision**: Support weekly/monthly/yearly; monthly day-of-month 1–28.
- **Why**: Avoid month-end edge cases; reduces risk (RISK-001).

## 9. Traceability Matrix (Requirements → Architecture)
- FR-010..012 → `TransactionService` + `/transactions` endpoints
- FR-013 → past vs future-dated view in UI (derived from date; optional `scope` query)
- FR-022..023 → `RecurringTransactionService` + `/recurring-transactions` endpoints + recurrence generator using `RecurrenceRule`
- FR-024 → edit/delete recurring transactions
- FR-030..033 → `ForecastService` + forecast endpoints + daily aggregation algorithm
- FR-040..041 → Upcoming list derived from forecast range + transaction creation flow
- FR-050..051 → CSV/DB repositories + reset endpoint/operation (implementation decision)
- NFR-001..005 → UX modules, local-only model, performance approach, layering, accessibility in UI
