import { useEffect, useMemo, useState } from 'react'
import {
  getForecast,
  getForecastBalance,
  type DailyBalancePointResponse,
} from '../api'

type ForecastRangePreset = '30' | '90' | 'custom'

function formatDate(date: Date): string {
  return date.toISOString().split('T')[0]
}

function addDays(date: Date, days: number): Date {
  const next = new Date(date)
  next.setDate(next.getDate() + days)
  return next
}

function getTodayDateString(): string {
  return formatDate(new Date())
}

function getDefaultForecastDateString(): string {
  return formatDate(addDays(new Date(), 30))
}

export default function ForecastPage() {
  const [forecastDate, setForecastDate] = useState(getDefaultForecastDateString)
  const [rangePreset, setRangePreset] = useState<ForecastRangePreset>('30')
  const [customRangeDays, setCustomRangeDays] = useState('30')
  const [dailyBalances, setDailyBalances] = useState<DailyBalancePointResponse[]>([])
  const [balanceAtDate, setBalanceAtDate] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const rangeDays = useMemo(() => {
    if (rangePreset === '30') {
      return 30
    }

    if (rangePreset === '90') {
      return 90
    }

    const parsedDays = Number(customRangeDays)
    if (!Number.isFinite(parsedDays) || parsedDays < 1) {
      return 30
    }

    return Math.floor(parsedDays)
  }, [customRangeDays, rangePreset])

  const horizonEndDate = useMemo(
    () => formatDate(addDays(new Date(), rangeDays)),
    [rangeDays],
  )

  useEffect(() => {
    async function loadForecast() {
      setIsLoading(true)
      setErrorMessage(null)

      try {
        const today = getTodayDateString()
        const [forecastResult, balanceResult] = await Promise.all([
          getForecast(today, horizonEndDate),
          getForecastBalance(forecastDate),
        ])

        setDailyBalances(forecastResult.dailyBalances)
        setBalanceAtDate(balanceResult.balance)
      } catch {
        setErrorMessage('Failed to load forecast data.')
      } finally {
        setIsLoading(false)
      }
    }

    void loadForecast()
  }, [forecastDate, horizonEndDate])

  const upcomingItems = useMemo(
    () => dailyBalances.filter((point) => point.dailyNet !== 0),
    [dailyBalances],
  )

  return (
    <section className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-xl font-semibold">Forecast</h1>
        <p className="text-sm text-gray-700">
          Select a forecast date and range to view projected balance and upcoming items.
        </p>
      </header>

      <section className="space-y-3 rounded border p-4">
        <h2 className="text-base font-semibold">Forecast controls</h2>

        <div className="grid gap-3 md:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Forecast date
            <input
              className="rounded border px-2 py-1"
              type="date"
              value={forecastDate}
              onChange={(event) => {
                setForecastDate(event.target.value)
              }}
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Range preset
            <select
              className="rounded border px-2 py-1"
              value={rangePreset}
              onChange={(event) => {
                setRangePreset(event.target.value as ForecastRangePreset)
              }}
            >
              <option value="30">Next 30 days</option>
              <option value="90">Next 90 days</option>
              <option value="custom">Custom</option>
            </select>
          </label>

          {rangePreset === 'custom' ? (
            <label className="flex flex-col gap-1 text-sm">
              Custom range (days)
              <input
                className="rounded border px-2 py-1"
                min="1"
                step="1"
                type="number"
                value={customRangeDays}
                onChange={(event) => {
                  setCustomRangeDays(event.target.value)
                }}
              />
            </label>
          ) : null}
        </div>

        <p className="text-sm text-gray-700">Horizon end date: {horizonEndDate}</p>
      </section>

      <section className="space-y-3 rounded border p-4">
        <h2 className="text-base font-semibold">Predicted balance</h2>

        {errorMessage ? <p className="text-sm text-red-700">{errorMessage}</p> : null}
        {isLoading ? <p className="text-sm">Loading forecast...</p> : null}

        {!isLoading && !errorMessage ? (
          <p className="text-lg font-semibold">
            {balanceAtDate !== null ? balanceAtDate.toFixed(2) : 'No data'}
          </p>
        ) : null}
      </section>

      <section className="space-y-3 rounded border p-4">
        <h2 className="text-base font-semibold">Daily balances (horizon)</h2>

        {!isLoading && dailyBalances.length === 0 ? (
          <p className="text-sm">No daily balances available for the selected horizon.</p>
        ) : null}

        {!isLoading && dailyBalances.length > 0 ? (
          <ul className="max-h-64 space-y-2 overflow-auto pr-1">
            {dailyBalances.map((point) => (
              <li
                key={point.date}
                className="flex items-center justify-between gap-4 rounded border p-2 text-sm"
              >
                <span>{point.date}</span>
                <span>Daily net: {point.dailyNet.toFixed(2)}</span>
                <span>Balance: {point.balance.toFixed(2)}</span>
              </li>
            ))}
          </ul>
        ) : null}
      </section>

      <section className="space-y-3 rounded border p-4">
        <h2 className="text-base font-semibold">Upcoming items in horizon</h2>

        {!isLoading && upcomingItems.length === 0 ? (
          <p className="text-sm">No upcoming items in the selected horizon.</p>
        ) : null}

        {!isLoading && upcomingItems.length > 0 ? (
          <ul className="space-y-2">
            {upcomingItems.map((point) => (
              <li key={`upcoming-${point.date}`} className="rounded border p-2 text-sm">
                {point.date} • Net change {point.dailyNet.toFixed(2)}
              </li>
            ))}
          </ul>
        ) : null}
      </section>
    </section>
  )
}
