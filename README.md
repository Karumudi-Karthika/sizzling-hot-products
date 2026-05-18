# 🔥 Sizzling Hot Products

A full-stack solution for the Bunnings take-home test.

Displays the **top-selling product per day** and **top product over the past 3 days**, applying all specified business rules.

---

## Table of Contents

1. [Stack & Rationale](#stack--rationale)
2. [Project Structure](#project-structure)
3. [Getting Started](#getting-started)
4. [Running Tests](#running-tests)
5. [API Reference](#api-reference)
6. [Business Rules](#business-rules)
7. [Assumptions & Design Decisions](#assumptions--design-decisions)
8. [Architecture Notes](#architecture-notes)
9. [Future Improvements](#future-improvements)

---

## Stack & Rationale

| Layer    | Technology                   | Why                                                                  |
|----------|------------------------------|----------------------------------------------------------------------|
| Backend  | .NET 10 / ASP.NET Core Web API | Matches the role's primary stack; strongly typed, fast, well-suited to REST APIs |
| Frontend | React 18 + TypeScript + Vite  | Matches the role's primary stack; Vite gives fast HMR in dev         |
| Data     | JSON files (from inputs/)     | Per brief requirements; loaded once and cached in memory             |
| Testing  | xUnit + FluentAssertions (.NET), Vitest + Testing Library (React) | Industry-standard pairing for each platform |

---

## Project Structure

```
sizzling-hot-products/
├── backend/
│   ├── SizzlingHotProducts.sln
│   ├── SizzlingHotProducts.Core/          # Domain models, interfaces, business logic
│   │   ├── Models/
│   │   │   ├── Order.cs
│   │   │   ├── Product.cs
│   │   │   └── SizzlingResults.cs         # Response DTOs
│   │   ├── Interfaces/
│   │   │   ├── ISizzlingProductService.cs
│   │   │   └── IDataLoader.cs
│   │   ├── Services/
│   │   │   └── SizzlingProductService.cs  # All business rule logic lives here
│   │   └── Exceptions/
│   │       └── DomainExceptions.cs
│   ├── SizzlingHotProducts.API/           # ASP.NET Core host
│   │   ├── Controllers/
│   │   │   └── ProductsController.cs
│   │   ├── Services/
│   │   │   └── JsonDataLoader.cs          # Reads + caches the JSON files
│   │   ├── Data/
│   │   │   ├── orders.json
│   │   │   └── products.json
│   │   └── Program.cs
│   └── SizzlingHotProducts.Tests/         # xUnit unit tests
│       ├── SizzlingProductServiceTests.cs
│       └── Helpers/TestHelpers.cs
└── frontend/                              # React + TypeScript app
    └── src/
        ├── api/productsApi.ts             # Axios API client
        ├── hooks/useSizzlingHotProducts.ts # React Query hook
        ├── components/
        │   ├── SizzlingDashboard.tsx      # Page-level component
        │   ├── PeriodWinnerBanner.tsx     # 3-day winner hero section
        │   ├── DailyResultCard.tsx        # Per-day result card
        │   ├── LoadingSkeleton.tsx        # Loading state
        │   └── ErrorMessage.tsx           # Error state
        ├── types/index.ts                 # TypeScript types mirroring API DTOs
        └── test/
            ├── setup.ts
            ├── components.test.tsx        # Component tests
            └── businessLogic.test.ts      # Pure logic tests (all 4 BRs + edge cases)
```

---

## Getting Started

### Prerequisites

- [.NET 8+ SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)

### 1. Clone your repo

```bash
git clone <your-repo-url>
cd sizzling-hot-products
```

### 2. Run the Backend

```bash
cd backend/SizzlingHotProducts.API
dotnet restore
dotnet run
```

The API starts on **http://localhost:5000** by default.  
Swagger UI is available at **http://localhost:5000/swagger**.

### 3. Run the Frontend

```bash
cd frontend
npm install
npm run dev
```

Open **http://localhost:5173** in your browser.

> The frontend expects the API on `http://localhost:5000`. To change this, copy
> `.env.example` to `.env` and update `VITE_API_URL`.

---

## Running Tests

### Backend (.NET)

```bash
cd backend
dotnet test
```

### Frontend (Vitest)

```bash
cd frontend
npm run test:run      # single run with summary
npm test              # watch mode
```

---

## API Reference

### `GET /api/products/sizzling-hot`

Returns the top sizzling product for each of the past 3 days, plus the overall 3-day winner.

**Query parameters**

| Param   | Type   | Default      | Description                              |
|---------|--------|--------------|------------------------------------------|
| `today` | string | 23/04/2026   | Override "today" (format: `dd/MM/yyyy`)  |

**Example response**

```json
{
  "dailyResults": [
    {
      "date": "21/04/2026",
      "productId": "P1",
      "productName": "Ezy Storage 37L Flexi Laundry Basket - White",
      "saleCount": 3
    },
    ...
  ],
  "threeDayResult": {
    "periodStart": "21/04/2026",
    "periodEnd": "23/04/2026",
    "productId": "P1",
    "productName": "Ezy Storage 37L Flexi Laundry Basket - White",
    "saleCount": 5
  }
}
```

### `GET /api/products`

Returns all products in the catalogue.

---

## Business Rules

All four rules are implemented in `SizzlingProductService.cs`:

| Rule | Description | Implementation |
|------|-------------|----------------|
| BR1  | Quantity is irrelevant — count a product once per order | `Distinct()` over entry product IDs within each order |
| BR2  | Same customer + same product + same day across multiple orders = 1 sale | Deduplicate via `HashSet<(customerId, date, productId)>` |
| BR3  | Cancelled orders remove the original order's sales | Collect cancelled `orderId`s and exclude those orders from scoring |
| BR4  | On a sales tie, pick the alphabetically first product name | `.OrderByDescending(score).ThenBy(name)` |

---

## Assumptions & Design Decisions

1. **"Today" is 23/04/2026** as specified in the brief. This is the default when no `today` query param is provided. The API also accepts a `today` override so the UI (and tests) can query for any date.

2. **BR3 — Cancellation applies to the original order's date**, not the cancellation date. A cancelled `orderId` removes that entire order from the completed set, regardless of when the cancellation occurred.

3. **Duplicate `orderId`s in the input** (e.g. O30 appears as both a completed 21/04 order and a cancelled 22/04 record) are handled by treating any `orderId` appearing with `status: "cancelled"` as cancelled for the purposes of scoring — this matches the brief's sample data and expected outputs.

4. **Unknown product IDs** referenced in orders but absent from the catalogue are silently skipped. This is a defensive choice to avoid crashing on partial/dirty data.

5. **In-memory caching** of the JSON files: since the files don't change at runtime, a singleton `JsonDataLoader` reads and caches them on first request. In production this would be replaced by a proper database or a distributed cache.

6. **Date format** is `dd/MM/yyyy` throughout (as used in the input files). The API validates this format and returns a 400 if malformed.

7. **CORS** is configured to allow `localhost:5173` (Vite) and `localhost:3000` in development. This would be locked down to specific origins in production.

8. **ISizzlingProductService** is designed to be stateless and accepts data as parameters rather than having dependencies on the data loader. This makes unit testing trivial — no mocks required, just pass in lists of test data.

---

## Architecture Notes

### Separation of concerns

- **`SizzlingHotProducts.Core`** — pure domain logic with no framework dependencies. Can be tested, reused, or ported independently.
- **`SizzlingHotProducts.API`** — thin HTTP host layer. Controllers are deliberately thin; they delegate immediately to the service.
- **`SizzlingHotProducts.Tests`** — tests against the Core layer directly. No HTTP calls needed for business logic tests.

### SOLID principles applied

- **S** — `SizzlingProductService` does one thing (calculate sizzling scores). `JsonDataLoader` does one thing (load data).
- **O** — New data sources (e.g. a database) can be added by implementing `IDataLoader` without changing any business logic.
- **L** — All interfaces are satisfied by concrete implementations without unexpected behaviour.
- **I** — `ISizzlingProductService` exposes granular methods (per-day, per-period, combined) rather than one monolithic method.
- **D** — The controller depends on `ISizzlingProductService` and `IDataLoader` interfaces, not concrete classes.

---

## Future Improvements

- **Persist data to a database** (e.g. SQL Server/EF Core) with a migration pipeline, removing the JSON file dependency.
- **IDateTimeProvider abstraction** — inject a clock interface into the controller so "today" can be mocked cleanly in integration tests without query-param hacks.
- **Integration tests** — add `WebApplicationFactory<Program>` tests to test the full HTTP stack end-to-end.
- **Pagination** — if the daily history grew beyond 3 days, the `GET /sizzling-hot` endpoint should support `from`/`to` range parameters.
- **CI/CD** — add a GitHub Actions workflow to run `dotnet test` and `npm run test:run` on every push.
- **Docker** — add a `Dockerfile` for each project and a `docker-compose.yml` for one-command local startup.
