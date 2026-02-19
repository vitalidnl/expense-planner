# Expense Planner (Demo) — Requirements

## 1. Purpose
Build a small demo application that helps a user **plan and monitor personal cash flow** and **predict available funds at a future date**, based on:
- recorded past and planned future expenses (one-time and recurring)
- recorded past and planned future incomes (one-time and recurring)

The core outcome is: **“How much money will I have on date X?”**

## 2. Goals & Success Criteria
**Goals**
- Provide a clear, understandable forecast of balance on a selected future date.
- Make it quick to add past and future-dated transactions.
- Keep the app intentionally simple (demo scope).

**Success criteria (demo)**
- A new user can add a few transactions (including an optional “initial balance” income) and get a forecast in under 2 minutes.
- Forecast updates immediately after adding/editing a transaction.
- Forecast correctly accounts for one-time items and basic recurrence.

## 3. Target Users & Personas
- **P1: Individual planner** — wants to know if they can afford a purchase later this month.
- **P2: Budget-aware user** — wants visibility into upcoming bills and salary.

## 4. Assumptions (Demo Constraints)
- ASSUMP-001: Single user on a single device (no accounts, no sync).
- ASSUMP-002: Single currency; symbol/label is hardcoded in the UI (no FX).
- ASSUMP-003: “Available funds” = **cash balance** (not investments/credit).
- ASSUMP-004: Forecast precision is **calendar-day** granularity (not hourly).
- ASSUMP-005: Timezone is the device timezone.

## 5. In Scope / Out of Scope
### In scope
- Managing transactions (income/expense) across past and future dates
- Recurring transactions (basic frequencies)
- Forecasting balance for a chosen date and over a date range
- Lightweight monitoring view of upcoming items
- Local persistence

### Out of scope (explicitly)
- OOS-001: Multi-account support (checking/savings/cards) and transfers
- OOS-002: Bank integrations / automatic import
- OOS-003: Shared/family budgets
- OOS-004: Complex budgeting rules (envelopes, rollover budgets)
- OOS-005: Tax calculations
- OOS-006: Multi-currency

## 6. Glossary
- **Past transaction**: an income/expense with a date on or before “today”.
- **Future-dated transaction**: an income/expense with a date after “today”, one-time.
- **Recurring transaction**: a repeating plan that generates future occurrences on a schedule.
- **Balance / available funds**: the modeled cash amount the user has.
- **Forecast date**: the future date for which the balance is predicted.

## 7. Core Calculation Rules (Business Rules)
- BR-001: Balance is computed from transactions only; there is no separate “starting balance” setting.
- BR-002: If the user wants to represent an initial amount of cash, it is recorded as an **income transaction** (e.g., description “Initial balance”) on a chosen date.
- BR-003: Any transaction affects balance on its transaction date.
- BR-004: Expenses decrease balance; incomes increase balance.
- BR-005: Forecasted balance on date $D$ is:
  $$\text{Balance}(D)=\sum_{t \le D}(\text{Income}_t)-\sum_{t \le D}(\text{Expense}_t)$$
- BR-006 (recurrence): for recurring transactions, generate occurrences between “today” and the forecast range.
- BR-007: If multiple transactions share a date, their order does not matter (daily aggregation).

**Supported recurrence (demo):**
- Weekly: unit = week, interval = N, day index within week (1–7)
- Monthly: unit = month, interval = N, day index within month (1–28 to avoid edge cases)
- Yearly: unit = year, interval = N, day index within year (1–366)

## 8. Functional Requirements
### Transactions (one-time)
- FR-010: User can add an **expense transaction** with: amount, date, description (optional).
- FR-011: User can add an **income transaction** with: amount, date, description (optional).
- FR-012: User can edit or delete a transaction.
- FR-013: UI can present transactions as **Past** (date ≤ today) vs **Future** (date > today) without storing a separate timing flag.

### Recurring transactions
- FR-022: User can add a **recurring income/expense** plan with: amount, start date, recurrence unit, interval, day index, description (optional).
- FR-023: User can pause/resume a recurring transaction.
- FR-024: User can edit or delete a recurring transaction.

### Forecasting
- FR-030: User can select a **forecast date** and see predicted balance on that date.
- FR-031: User can view a forecast over a date range (e.g., next 30/90 days) as a daily series.
- FR-032: User can see the list of transactions contributing to the forecast within the selected range.
- FR-033: Forecast updates immediately after any transaction change.

### Monitoring (minimal)
- FR-040: User can view upcoming items within a selected horizon (default 30 days), including future-dated transactions and generated recurring occurrences.
- FR-041: User can mark a recurring occurrence as “occurred” by creating a matching one-time transaction for that date (simple flow).

### Persistence
- FR-050: App persists all data locally so it is available after restart.
- FR-051: User can reset all demo data (clear storage) with confirmation.

## 9. Acceptance Criteria (MVP)
- AC-001 (FR-010/FR-011): Given an income transaction “Initial balance” dated today exists, when the user opens the forecast view, then the current balance shown equals that amount plus/minus all transactions with date ≤ today.
- AC-002 (FR-013): Given a transaction dated after today exists, when the user views transactions, then it appears under “Future” (or equivalent upcoming scope).
- AC-003 (FR-022): Given a monthly recurring expense on day 15 starting this month, when forecasting 90 days ahead, then each month’s 15th occurrence is included in the forecast.
- AC-004 (FR-030): Given the user changes the forecast date, then the predicted balance recalculates without page refresh.
- AC-005 (FR-012/FR-024): Given a transaction is deleted, when returning to forecast, then the deleted item no longer affects any balances.
- AC-006 (FR-051): Given the user confirms reset, then all transactions and recurring rules are removed and the app returns to an empty state.

## 10. Non-Functional Requirements
- NFR-001: Usability — core flows (add transaction, add recurring, forecast) are achievable without reading documentation.
- NFR-002: Performance — forecast for 365 days renders in under 1 second for up to ~1,000 generated occurrences.
- NFR-003: Reliability — local data is not lost on refresh/restart.
- NFR-004: Privacy — all data stays on-device (no network calls required for core features).
- NFR-005: Accessibility — basic keyboard navigation and readable contrast using existing design system.

## 11. Primary Screens (Conceptual)
(Names are descriptive; exact UI can be simplified further during design.)
- **Dashboard / Forecast**: current balance, forecast date picker, predicted balance, short chart or table of daily balances (range), upcoming items.
- **Transactions**: list/filter by past vs future (minimal toggle), add/edit/delete.
- **Recurring**: list of recurring items with pause/resume.
- **Data (Reset)**: reset demo data.

## 12. Risks
- RISK-001: Recurrence edge cases (month-end dates, leap years). Mitigation: restrict monthly day-of-month to 1–28 for demo.
- RISK-002: Users may expect multi-account or category budgeting. Mitigation: clearly label demo scope and keep features minimal.

## 13. Open Questions
(If you answer these, we can tighten the spec for the architect/dev agents.)
- Q-001: Should “today” be included in the forecast range by default?
- Q-002: Do we need categories/tags at all, or keep only description?
- Q-003: Should recurring rules include “end date” or “number of occurrences” (or keep infinite + pause)?
