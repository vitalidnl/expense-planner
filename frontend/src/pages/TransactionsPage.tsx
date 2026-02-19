import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import {
  createTransaction,
  deleteTransaction,
  getTransactions,
  TransactionType,
  updateTransaction,
  type TransactionResponse,
  type TransactionScope,
} from '../api'

type FormState = {
  type: TransactionType
  amount: string
  date: string
  description: string
}

const defaultFormState: FormState = {
  type: TransactionType.Expense,
  amount: '',
  date: '',
  description: '',
}

function getTodayDateString(): string {
  return new Date().toISOString().split('T')[0]
}

export default function TransactionsPage() {
  const [transactions, setTransactions] = useState<TransactionResponse[]>([])
  const [scope, setScope] = useState<TransactionScope>('all')
  const [isLoading, setIsLoading] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formState, setFormState] = useState<FormState>({
    ...defaultFormState,
    date: getTodayDateString(),
  })

  async function loadTransactions(currentScope: TransactionScope) {
    setIsLoading(true)
    setErrorMessage(null)

    try {
      const items = await getTransactions({ scope: currentScope })
      setTransactions(items)
    } catch {
      setErrorMessage('Failed to load transactions.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadTransactions(scope)
  }, [scope])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const amount = Number(formState.amount)
    if (!Number.isFinite(amount) || amount <= 0) {
      setErrorMessage('Amount must be greater than zero.')
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      if (editingId) {
        await updateTransaction(editingId, {
          type: formState.type,
          amount,
          date: formState.date,
          description: formState.description.trim() || null,
        })
      } else {
        await createTransaction({
          type: formState.type,
          amount,
          date: formState.date,
          description: formState.description.trim() || null,
        })
      }

      setEditingId(null)
      setFormState({
        ...defaultFormState,
        date: getTodayDateString(),
      })

      await loadTransactions(scope)
    } catch {
      setErrorMessage('Failed to save transaction.')
    } finally {
      setIsSubmitting(false)
    }
  }

  function handleEdit(transaction: TransactionResponse) {
    setEditingId(transaction.id)
    setErrorMessage(null)
    setFormState({
      type: transaction.type,
      amount: String(transaction.amount),
      date: transaction.date,
      description: transaction.description ?? '',
    })
  }

  function handleCancelEdit() {
    setEditingId(null)
    setErrorMessage(null)
    setFormState({
      ...defaultFormState,
      date: getTodayDateString(),
    })
  }

  async function handleDelete(id: string) {
    setErrorMessage(null)

    try {
      await deleteTransaction(id)
      await loadTransactions(scope)
    } catch {
      setErrorMessage('Failed to delete transaction.')
    }
  }

  return (
    <section className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-xl font-semibold">Transactions</h1>
        <p className="text-sm text-gray-700">
          Add, edit, and remove income or expense transactions.
        </p>
      </header>

      <form className="space-y-3 rounded border p-4" onSubmit={handleSubmit}>
        <h2 className="text-base font-semibold">
          {editingId ? 'Edit transaction' : 'Add transaction'}
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
              name="amount"
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
            Date
            <input
              className="rounded border px-2 py-1"
              name="date"
              required
              type="date"
              value={formState.date}
              onChange={(event) => {
                setFormState((previous) => ({
                  ...previous,
                  date: event.target.value,
                }))
              }}
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Description
            <input
              className="rounded border px-2 py-1"
              name="description"
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
                ? 'Update transaction'
                : 'Add transaction'}
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
        <div className="flex items-center justify-between gap-3">
          <h2 className="text-base font-semibold">Transaction list</h2>

          <label className="flex items-center gap-2 text-sm">
            Scope
            <select
              className="rounded border px-2 py-1"
              value={scope}
              onChange={(event) => {
                setScope(event.target.value as TransactionScope)
              }}
            >
              <option value="all">All</option>
              <option value="past">Past</option>
              <option value="future">Future</option>
            </select>
          </label>
        </div>

        {errorMessage ? <p className="text-sm text-red-700">{errorMessage}</p> : null}

        {isLoading ? <p className="text-sm">Loading transactions...</p> : null}

        {!isLoading && transactions.length === 0 ? (
          <p className="text-sm">No transactions found for this scope.</p>
        ) : null}

        {!isLoading && transactions.length > 0 ? (
          <ul className="space-y-2">
            {transactions.map((transaction) => (
              <li
                key={transaction.id}
                className="flex flex-wrap items-center justify-between gap-3 rounded border p-3"
              >
                <div className="space-y-1">
                  <p className="text-sm font-medium">
                    {transaction.type === TransactionType.Income ? 'Income' : 'Expense'} •{' '}
                    {transaction.amount.toFixed(2)}
                  </p>
                  <p className="text-sm">{transaction.date}</p>
                  {transaction.description ? (
                    <p className="text-sm text-gray-700">{transaction.description}</p>
                  ) : null}
                </div>

                <div className="flex items-center gap-2">
                  <button
                    className="rounded border px-3 py-1 text-sm"
                    type="button"
                    onClick={() => {
                      handleEdit(transaction)
                    }}
                  >
                    Edit
                  </button>
                  <button
                    className="rounded border px-3 py-1 text-sm"
                    type="button"
                    onClick={() => {
                      void handleDelete(transaction.id)
                    }}
                  >
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        ) : null}
      </section>
    </section>
  )
}
