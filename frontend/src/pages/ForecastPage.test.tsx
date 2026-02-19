import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import ForecastPage from './ForecastPage'

function jsonResponse<T>(payload: T): Response {
  return new Response(JSON.stringify(payload), {
    status: 200,
    headers: { 'Content-Type': 'application/json' },
  })
}

describe('ForecastPage', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValueOnce(jsonResponse({ dailyBalances: [] }))
        .mockResolvedValueOnce(jsonResponse({ date: '2026-02-20', balance: 100 }))
        .mockResolvedValueOnce(jsonResponse({ dailyBalances: [] }))
        .mockResolvedValueOnce(jsonResponse({ date: '2026-03-20', balance: 120 })),
    )
  })

  it('reloads forecast balance when forecast date changes', async () => {
    render(<ForecastPage />)

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledTimes(2)
    })

    fireEvent.change(screen.getByLabelText('Forecast date'), {
      target: { value: '2026-03-20' },
    })

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledTimes(4)
    })

    const fourthCallUrl = (fetch as ReturnType<typeof vi.fn>).mock.calls[3][0]
    const requestUrl = new URL(String(fourthCallUrl))

    expect(requestUrl.pathname).toBe('/forecast/balance')
    expect(requestUrl.searchParams.get('date')).toBe('2026-03-20')
  })
})
