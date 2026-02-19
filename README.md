# Expense Planner (Demo)

A full-stack expense planning application with recurrence support, built with .NET and React.

## Project Structure

- `backend/` — .NET 10 API with layered architecture (Domain, Application, DataAccess, Api)
- `frontend/` — React + TypeScript + Tailwind CSS UI
- `docs/` — Architecture and task documentation

## Prerequisites

- **.NET 10 SDK** (or later)
- **Node.js 18+** and **npm** (or yarn)
- **Git**

## Running Locally

### Backend

1. Navigate to the backend directory:
   ```bash
   cd backend
   ```

2. Restore dependencies and build:
   ```bash
   dotnet build ExpensePlanner.sln
   ```

3. Run the API server:
   ```bash
   dotnet run --project src/ExpensePlanner.Api
   ```

   The API will be available at `https://localhost:5001` (or as configured).

### Frontend

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

   The UI will be available at `http://localhost:5173` (or as shown in the terminal).

## Development Workflow

### Formatting & Linting

**Backend:**
```bash
cd backend
dotnet format
```

**Frontend:**
```bash
cd frontend
npm run lint
npm run format
```

## Testing

Run tests from the backend directory:
```bash
cd backend
dotnet test
```

## Documentation

See the `docs/` folder for:
- `01-requirements.md` — Feature and functional requirements
- `02-architecture.md` — System design and layering
- `03-implementation-plan.md` — Overall approach
- `04-tasks.md` — Task breakdown by milestone

## Notes

- Data is persisted to CSV files in the `data/` directory by default.
- The API exposes Swagger documentation at `/swagger` in development mode.
