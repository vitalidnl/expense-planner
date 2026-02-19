import { apiRequest } from './client'
import type {
  RecurringTransactionRequest,
  RecurringTransactionResponse,
} from './types'

export function getRecurringTransactions() {
  return apiRequest<RecurringTransactionResponse[]>('recurring-transactions')
}

export function getRecurringTransactionById(id: string) {
  return apiRequest<RecurringTransactionResponse>(`recurring-transactions/${id}`)
}

export function createRecurringTransaction(request: RecurringTransactionRequest) {
  return apiRequest<RecurringTransactionResponse>('recurring-transactions', {
    method: 'POST',
    body: request,
  })
}

export function updateRecurringTransaction(
  id: string,
  request: RecurringTransactionRequest,
) {
  return apiRequest<void>(`recurring-transactions/${id}`, {
    method: 'PUT',
    body: request,
  })
}

export function pauseRecurringTransaction(id: string) {
  return apiRequest<void>(`recurring-transactions/${id}/pause`, {
    method: 'POST',
  })
}

export function resumeRecurringTransaction(id: string) {
  return apiRequest<void>(`recurring-transactions/${id}/resume`, {
    method: 'POST',
  })
}

export function deleteRecurringTransaction(id: string) {
  return apiRequest<void>(`recurring-transactions/${id}`, {
    method: 'DELETE',
  })
}
