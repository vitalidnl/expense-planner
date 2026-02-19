import { apiRequest } from './client'
import type {
  RecurrenceRuleRequest,
  RecurrenceRuleResponse,
} from './types'

export function getRecurrenceRules() {
  return apiRequest<RecurrenceRuleResponse[]>('recurrence-rules')
}

export function getRecurrenceRuleById(id: string) {
  return apiRequest<RecurrenceRuleResponse>(`recurrence-rules/${id}`)
}

export function createRecurrenceRule(request: RecurrenceRuleRequest) {
  return apiRequest<RecurrenceRuleResponse>('recurrence-rules', {
    method: 'POST',
    body: request,
  })
}

export function updateRecurrenceRule(
  id: string,
  request: RecurrenceRuleRequest,
) {
  return apiRequest<void>(`recurrence-rules/${id}`, {
    method: 'PUT',
    body: request,
  })
}

export function deleteRecurrenceRule(id: string) {
  return apiRequest<void>(`recurrence-rules/${id}`, {
    method: 'DELETE',
  })
}
