import { NavLink, Route, Routes } from 'react-router-dom'
import ForecastPage from './pages/ForecastPage'
import HomePage from './pages/HomePage'
import NotFoundPage from './pages/NotFoundPage'
import RecurringPage from './pages/RecurringPage'
import TransactionsPage from './pages/TransactionsPage'

function App() {
  return (
    <div className="min-h-screen">
      <header className="border-b">
        <nav className="mx-auto flex max-w-5xl items-center gap-4 p-4">
          <NavLink className="font-semibold" to="/">
            Expense Planner
          </NavLink>
          <NavLink className="underline" to="/forecast">
            Forecast
          </NavLink>
          <NavLink className="underline" to="/transactions">
            Transactions
          </NavLink>
          <NavLink className="underline" to="/recurring">
            Recurring
          </NavLink>
        </nav>
      </header>

      <main className="mx-auto max-w-5xl p-4">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/forecast" element={<ForecastPage />} />
          <Route path="/transactions" element={<TransactionsPage />} />
          <Route path="/recurring" element={<RecurringPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
    </div>
  )
}

export default App
