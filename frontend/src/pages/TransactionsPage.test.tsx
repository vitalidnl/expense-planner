import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import TransactionsPage from './TransactionsPage'

function jsonResponse<T>(payload: T): Response {
  return new Response(JSON.stringify(payload), {
    status: 200,
    headers: { 'Content-Type': 'application/json' },
  })
}

describe('TransactionsPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse([])))
  })

  it('loads transactions with scope All on initial render', async () => {
    render(<TransactionsPage />)

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledTimes(1)
    })

    const firstCallUrl = (fetch as ReturnType<typeof vi.fn>).mock.calls[0][0]
    const requestUrl = new URL(String(firstCallUrl))

    expect(requestUrl.pathname).toBe('/transactions')
    expect(requestUrl.searchParams.get('scope')).toBe('All')
  })

  it('loads transactions with scope Future when filter changes', async () => {
    render(<TransactionsPage />)

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledTimes(1)
    })

    fireEvent.change(screen.getByLabelText('Scope'), {
      target: { value: 'future' },
    })

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledTimes(2)
    })

    const secondCallUrl = (fetch as ReturnType<typeof vi.fn>).mock.calls[1][0]
    const requestUrl = new URL(String(secondCallUrl))

    expect(requestUrl.pathname).toBe('/transactions')
    expect(requestUrl.searchParams.get('scope')).toBe('Future')
  })
})
