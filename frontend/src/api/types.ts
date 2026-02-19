export const TransactionType = {
  Income: 0,
  Expense: 1,
} as const

export type TransactionType =
  (typeof TransactionType)[keyof typeof TransactionType]

export const RecurrenceUnit = {
  Week: 0,
  Month: 1,
  Year: 2,
} as const

export type RecurrenceUnit =
  (typeof RecurrenceUnit)[keyof typeof RecurrenceUnit]

export type DateOnlyString = string
export type TransactionScope = 'all' | 'past' | 'future'

export interface TransactionRequest {
  type: TransactionType
  amount: number
  date: DateOnlyString
  description?: string | null
}

export interface TransactionResponse {
  id: string
  type: TransactionType
  amount: number
  date: DateOnlyString
  description?: string | null
  sourceRecurringTransactionId?: string | null
}

export interface RecurrenceRuleRequest {
  unit: RecurrenceUnit
  interval: number
  dayIndex: number
}

export interface RecurrenceRuleResponse {
  id: string
  unit: RecurrenceUnit
  interval: number
  dayIndex: number
}

export interface RecurringTransactionRequest {
  type: TransactionType
  amount: number
  startDate: DateOnlyString
  recurrenceRuleId: string
  description?: string | null
  isPaused: boolean
}

export interface RecurringTransactionResponse {
  id: string
  type: TransactionType
  amount: number
  startDate: DateOnlyString
  recurrenceRuleId: string
  description?: string | null
  isPaused: boolean
}

export interface DailyBalancePointResponse {
  date: DateOnlyString
  dailyNet: number
  balance: number
}

export interface ForecastResponse {
  dailyBalances: DailyBalancePointResponse[]
}

export interface ForecastBalanceResponse {
  date: DateOnlyString
  balance: number
}
