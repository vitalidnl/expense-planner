import { apiRequest } from './client'
import type {
  DateOnlyString,
  ForecastBalanceResponse,
  ForecastResponse,
} from './types'

export function getForecast(from: DateOnlyString, to: DateOnlyString) {
  return apiRequest<ForecastResponse>('forecast', {
    query: { from, to },
  })
}

export function getForecastBalance(date: DateOnlyString) {
  return apiRequest<ForecastBalanceResponse>('forecast/balance', {
    query: { date },
  })
}
