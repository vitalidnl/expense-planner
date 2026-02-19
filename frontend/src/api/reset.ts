import { apiRequest } from './client'

export function resetData() {
  return apiRequest<void>('reset', {
    method: 'POST',
  })
}
