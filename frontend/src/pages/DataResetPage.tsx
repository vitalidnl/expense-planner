import { useState } from 'react'
import { resetData } from '../api'

export default function DataResetPage() {
  const [isConfirmed, setIsConfirmed] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  async function handleReset() {
    if (!isConfirmed) {
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)
    setSuccessMessage(null)

    try {
      await resetData()
      setSuccessMessage('All demo data has been cleared.')
      setIsConfirmed(false)
    } catch {
      setErrorMessage('Failed to reset data. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-xl font-semibold">Data Reset</h1>
        <p className="text-sm text-gray-700">
          This action removes all transactions, recurring transactions, and recurrence rules.
        </p>
      </header>

      <section className="space-y-4 rounded border p-4">
        <label className="flex items-start gap-2 text-sm">
          <input
            checked={isConfirmed}
            type="checkbox"
            onChange={(event) => {
              setIsConfirmed(event.target.checked)
            }}
          />
          <span>I understand this action cannot be undone.</span>
        </label>

        <button
          className="rounded border px-3 py-1 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-60"
          disabled={!isConfirmed || isSubmitting}
          type="button"
          onClick={() => {
            void handleReset()
          }}
        >
          {isSubmitting ? 'Resetting...' : 'Reset all data'}
        </button>

        {errorMessage ? <p className="text-sm text-red-700">{errorMessage}</p> : null}
        {successMessage ? <p className="text-sm text-green-700">{successMessage}</p> : null}
      </section>
    </section>
  )
}
