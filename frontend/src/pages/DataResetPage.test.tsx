import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import DataResetPage from './DataResetPage'

describe('DataResetPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })))
  })

  it('requires confirmation before reset and calls reset endpoint after confirmation', async () => {
    render(<DataResetPage />)

    const resetButton = screen.getByRole('button', { name: 'Reset all data' })
    expect(resetButton).toBeDisabled()

    fireEvent.click(screen.getByRole('checkbox'))
    expect(resetButton).toBeEnabled()

    fireEvent.click(resetButton)

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledTimes(1)
    })

    const requestUrl = new URL(String((fetch as ReturnType<typeof vi.fn>).mock.calls[0][0]))
    expect(requestUrl.pathname).toBe('/reset')

    expect(
      screen.getByText('All demo data has been cleared.'),
    ).toBeInTheDocument()
  })
})
