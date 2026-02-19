import { apiRequest } from './client'
import type {
  DateOnlyString,
  TransactionRequest,
  TransactionResponse,
  TransactionScope,
} from './types'

export interface GetTransactionsQuery {
  from?: DateOnlyString
  to?: DateOnlyString
  scope?: TransactionScope
}

function toApiScope(scope: TransactionScope | undefined): string | undefined {
  if (!scope) {
    return undefined
  }

  return scope.charAt(0).toUpperCase() + scope.slice(1)
}

export function getTransactions(query: GetTransactionsQuery = {}) {
  return apiRequest<TransactionResponse[]>('transactions', {
    query: {
      from: query.from,
      to: query.to,
      scope: toApiScope(query.scope),
    },
  })
}

export function getTransactionById(id: string) {
  return apiRequest<TransactionResponse>(`transactions/${id}`)
}

export function createTransaction(request: TransactionRequest) {
  return apiRequest<TransactionResponse>('transactions', {
    method: 'POST',
    body: request,
  })
}

export function updateTransaction(id: string, request: TransactionRequest) {
  return apiRequest<void>(`transactions/${id}`, {
    method: 'PUT',
    body: request,
  })
}

export function deleteTransaction(id: string) {
  return apiRequest<void>(`transactions/${id}`, {
    method: 'DELETE',
  })
}
