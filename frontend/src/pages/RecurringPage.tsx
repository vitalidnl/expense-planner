import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import {
  createRecurrenceRule,
  createRecurringTransaction,
  deleteRecurringTransaction,
  getRecurrenceRules,
  getRecurringTransactions,
  pauseRecurringTransaction,
  RecurrenceUnit,
  resumeRecurringTransaction,
  TransactionType,
  updateRecurrenceRule,
  updateRecurringTransaction,
  type RecurrenceRuleResponse,
  type RecurringTransactionResponse,
} from '../api'

type FormState = {
  type: TransactionType
  amount: string
  startDate: string
  description: string
  unit: RecurrenceUnit
  interval: string
  dayIndex: string
}

const defaultFormState: FormState = {
  type: TransactionType.Expense,
  amount: '',
  startDate: '',
  description: '',
  unit: RecurrenceUnit.Month,
  interval: '1',
  dayIndex: '1',
}

function getTodayDateString(): string {
  return new Date().toISOString().split('T')[0]
}

function getUnitName(unit: RecurrenceUnit): string {
  if (unit === RecurrenceUnit.Week) {
    return 'Week'
  }

  if (unit === RecurrenceUnit.Year) {
    return 'Year'
  }

  return 'Month'
}

function getDayIndexRange(unit: RecurrenceUnit): { min: number; max: number } {
  if (unit === RecurrenceUnit.Week) {
    return { min: 1, max: 7 }
  }

  if (unit === RecurrenceUnit.Year) {
    return { min: 1, max: 366 }
  }

  return { min: 1, max: 28 }
}

export default function RecurringPage() {
  const [items, setItems] = useState<RecurringTransactionResponse[]>([])
  const [rulesById, setRulesById] = useState<Record<string, RecurrenceRuleResponse>>({})
  const [isLoading, setIsLoading] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formState, setFormState] = useState<FormState>({
    ...defaultFormState,
    startDate: getTodayDateString(),
  })

  async function loadData() {
    setIsLoading(true)
    setErrorMessage(null)

    try {
      const [recurringTransactions, recurrenceRules] = await Promise.all([
        getRecurringTransactions(),
        getRecurrenceRules(),
      ])

      const indexedRules: Record<string, RecurrenceRuleResponse> = {}
      for (const rule of recurrenceRules) {
        indexedRules[rule.id] = rule
      }

      setRulesById(indexedRules)
      setItems(recurringTransactions)
    } catch {
      setErrorMessage('Failed to load recurring transactions.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadData()
  }, [])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const amount = Number(formState.amount)
    const interval = Number(formState.interval)
    const dayIndex = Number(formState.dayIndex)
    const range = getDayIndexRange(formState.unit)

    if (!Number.isFinite(amount) || amount <= 0) {
      setErrorMessage('Amount must be greater than zero.')
      return
    }

    if (!Number.isInteger(interval) || interval < 1) {
      setErrorMessage('Interval must be a positive integer.')
      return
    }

    if (!Number.isInteger(dayIndex) || dayIndex < range.min || dayIndex > range.max) {
      setErrorMessage(
        `Day index must be between ${range.min} and ${range.max} for ${getUnitName(formState.unit).toLowerCase()} recurrence.`,
      )
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      if (editingId) {
        const existing = items.find((item) => item.id === editingId)
        if (!existing) {
          setErrorMessage('Recurring transaction to edit was not found.')
          return
        }

        await updateRecurrenceRule(existing.recurrenceRuleId, {
          unit: formState.unit,
          interval,
          dayIndex,
        })

        await updateRecurringTransaction(editingId, {
          type: formState.type,
          amount,
          startDate: formState.startDate,
          recurrenceRuleId: existing.recurrenceRuleId,
          description: formState.description.trim() || null,
          isPaused: existing.isPaused,
        })
      } else {
        const createdRule = await createRecurrenceRule({
          unit: formState.unit,
          interval,
          dayIndex,
        })

        await createRecurringTransaction({
          type: formState.type,
          amount,
          startDate: formState.startDate,
          recurrenceRuleId: createdRule.id,
          description: formState.description.trim() || null,
          isPaused: false,
        })
      }

      setEditingId(null)
      setFormState({
        ...defaultFormState,
        startDate: getTodayDateString(),
      })

      await loadData()
    } catch {
      setErrorMessage('Failed to save recurring transaction.')
    } finally {
      setIsSubmitting(false)
    }
  }

  function handleEdit(item: RecurringTransactionResponse) {
    const recurrenceRule = rulesById[item.recurrenceRuleId]
    if (!recurrenceRule) {
      setErrorMessage('Recurrence rule for selected item was not found.')
      return
    }

    setEditingId(item.id)
    setErrorMessage(null)
    setFormState({
      type: item.type,
      amount: String(item.amount),
      startDate: item.startDate,
      description: item.description ?? '',
      unit: recurrenceRule.unit,
      interval: String(recurrenceRule.interval),
      dayIndex: String(recurrenceRule.dayIndex),
    })
  }

  function handleCancelEdit() {
    setEditingId(null)
    setErrorMessage(null)
    setFormState({
      ...defaultFormState,
      startDate: getTodayDateString(),
    })
  }

  async function handleTogglePaused(item: RecurringTransactionResponse) {
    setErrorMessage(null)

    try {
      if (item.isPaused) {
        await resumeRecurringTransaction(item.id)
      } else {
        await pauseRecurringTransaction(item.id)
      }

      await loadData()
    } catch {
      setErrorMessage('Failed to update recurring transaction state.')
    }
  }

  async function handleDelete(item: RecurringTransactionResponse) {
    setErrorMessage(null)

    try {
      await deleteRecurringTransaction(item.id)
      await loadData()
    } catch {
      setErrorMessage('Failed to delete recurring transaction.')
    }
  }

  const dayIndexRange = getDayIndexRange(formState.unit)

  return (
    <section className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-xl font-semibold">Recurring</h1>
        <p className="text-sm text-gray-700">
          Create, edit, pause, and resume recurring incomes and expenses.
        </p>
      </header>

      <form className="space-y-3 rounded border p-4" onSubmit={handleSubmit}>
        <h2 className="text-base font-semibold">
          {editingId ? 'Edit recurring transaction' : 'Add recurring transaction'}
        </h2>

        <div className="grid gap-3 md:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Type
            <select
              className="rounded border px-2 py-1"
              value={formState.type}
              onChange={(event) => {
                setFormState((previous) => ({
                  ...previous,
                  type: Number(event.target.value) as TransactionType,
                }))
              }}
            >
              <option value={TransactionType.Expense}>Expense</option>
              <option value={TransactionType.Income}>Income</option>
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Amount
            <input
              className="rounded border px-2 py-1"
              inputMode="decimal"
              min="0"
              required
              step="0.01"
              type="number"
              value={formState.amount}
              onChange={(event) => {
                setFormState((previous) => ({
                  ...previous,
                  amount: event.target.value,
                }))
              }}
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Start date
            <input
              className="rounded border px-2 py-1"
              required
              type="date"
              value={formState.startDate}
              onChange={(event) => {
                setFormState((previous) => ({
                  ...previous,
                  startDate: event.target.value,
                }))
              }}
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Description
            <input
              className="rounded border px-2 py-1"
              placeholder="Optional"
              type="text"
              value={formState.description}
              onChange={(event) => {
                setFormState((previous) => ({
                  ...previous,
                  description: event.target.value,
                }))
              }}
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Unit
            <select
              className="rounded border px-2 py-1"
              value={formState.unit}
              onChange={(event) => {
                const nextUnit = Number(event.target.value) as RecurrenceUnit
                const nextRange = getDayIndexRange(nextUnit)
                setFormState((previous) => {
                  const existingDayIndex = Number(previous.dayIndex)
                  const boundedDayIndex =
                    Number.isInteger(existingDayIndex) &&
                    existingDayIndex >= nextRange.min &&
                    existingDayIndex <= nextRange.max
                      ? existingDayIndex
                      : nextRange.min

                  return {
                    ...previous,
                    unit: nextUnit,
                    dayIndex: String(boundedDayIndex),
                  }
                })
              }}
            >
              <option value={RecurrenceUnit.Week}>Week</option>
              <option value={RecurrenceUnit.Month}>Month</option>
              <option value={RecurrenceUnit.Year}>Year</option>
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Interval
            <input
              className="rounded border px-2 py-1"
              min="1"
              required
              step="1"
              type="number"
              value={formState.interval}
              onChange={(event) => {
                setFormState((previous) => ({
                  ...previous,
                  interval: event.target.value,
                }))
              }}
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Day index ({dayIndexRange.min}..{dayIndexRange.max})
            <input
              className="rounded border px-2 py-1"
              max={dayIndexRange.max}
              min={dayIndexRange.min}
              required
              step="1"
              type="number"
              value={formState.dayIndex}
              onChange={(event) => {
                setFormState((previous) => ({
                  ...previous,
                  dayIndex: event.target.value,
                }))
              }}
            />
          </label>
        </div>

        <div className="flex items-center gap-2">
          <button
            className="rounded border px-3 py-1 text-sm font-medium"
            disabled={isSubmitting}
            type="submit"
          >
            {isSubmitting
              ? 'Saving...'
              : editingId
                ? 'Update recurring transaction'
                : 'Add recurring transaction'}
          </button>

          {editingId ? (
            <button
              className="rounded border px-3 py-1 text-sm"
              type="button"
              onClick={handleCancelEdit}
            >
              Cancel
            </button>
          ) : null}
        </div>
      </form>

      <section className="space-y-3 rounded border p-4">
        <h2 className="text-base font-semibold">Recurring transactions</h2>

        {errorMessage ? <p className="text-sm text-red-700">{errorMessage}</p> : null}

        {isLoading ? <p className="text-sm">Loading recurring transactions...</p> : null}

        {!isLoading && items.length === 0 ? (
          <p className="text-sm">No recurring transactions yet.</p>
        ) : null}

        {!isLoading && items.length > 0 ? (
          <ul className="space-y-2">
            {items.map((item) => {
              const recurrenceRule = rulesById[item.recurrenceRuleId]

              return (
                <li
                  key={item.id}
                  className="flex flex-wrap items-center justify-between gap-3 rounded border p-3"
                >
                  <div className="space-y-1">
                    <p className="text-sm font-medium">
                      {item.type === TransactionType.Income ? 'Income' : 'Expense'} •{' '}
                      {item.amount.toFixed(2)}
                    </p>
                    <p className="text-sm">Start: {item.startDate}</p>
                    {recurrenceRule ? (
                      <p className="text-sm text-gray-700">
                        {getUnitName(recurrenceRule.unit)} every {recurrenceRule.interval}; day index{' '}
                        {recurrenceRule.dayIndex}
                      </p>
                    ) : (
                      <p className="text-sm text-red-700">Recurrence rule not found</p>
                    )}
                    {item.description ? (
                      <p className="text-sm text-gray-700">{item.description}</p>
                    ) : null}
                    <p className="text-sm">
                      Status: {item.isPaused ? 'Paused' : 'Active'}
                    </p>
                  </div>

                  <div className="flex items-center gap-2">
                    <button
                      className="rounded border px-3 py-1 text-sm"
                      type="button"
                      onClick={() => {
                        handleEdit(item)
                      }}
                    >
                      Edit
                    </button>
                    <button
                      className="rounded border px-3 py-1 text-sm"
                      type="button"
                      onClick={() => {
                        void handleTogglePaused(item)
                      }}
                    >
                      {item.isPaused ? 'Resume' : 'Pause'}
                    </button>
                    <button
                      className="rounded border px-3 py-1 text-sm"
                      type="button"
                      onClick={() => {
                        void handleDelete(item)
                      }}
                    >
                      Delete
                    </button>
                  </div>
                </li>
              )
            })}
          </ul>
        ) : null}
      </section>
    </section>
  )
}
