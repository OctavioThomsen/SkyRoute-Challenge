# SkyRoute Challenge

SkyRoute is a flight aggregator platform. This repository implements the SkyRoute travel
module: **flight search** (search form + results with frontend sorting) and a **booking flow**
(flight selection → passenger details → confirmation with a booking reference).

The platform aggregates flights from two providers — **GlobalAir** and **BudgetWings** — each
with its own pricing rules, and persists bookings to a JSON store.

## Architecture

```
+-----------------------------+            +------------------------------+
|        Frontend             |  HTTP/REST |          Backend             |
|  Angular 22 (standalone)    | ---------> |  ASP.NET Core (.NET 10)      |
|  nginx :80  (Docker :4200)  |   /api     |  Kestrel :5000               |
|                             | <--------- |                              |
+-----------------------------+            +---------------+--------------+
                                                           |
                                                           v
                                            +------------------------------+
                                            |  JSON data (backend/Data)    |
                                            |   globalair.json (read)      |
                                            |   budgetwings.json (read)    |
                                            |   bookings.json (read/write) |
                                            +------------------------------+
```

- **Frontend → Backend:** HTTP REST. In Docker, nginx proxies `/api` to the backend service.
- **No authentication** is used in this challenge.
- **CORS:** the backend allows only the `http://localhost:4200` origin (configured in
  `appsettings.json` and applied in `Program.cs`).

## Tech stack

| Layer | Technology | Version |
|---|---|---|
| Backend | .NET / C# | 10 |
| Frontend | Angular | 22 |
| Runtime | Node.js | 24 |
| Containerization | Docker + docker-compose | latest |
| Flight data | Static JSON in backend | — |
| Booking persistence | JSON file (Docker volume) | — |

## Setup and run

### With Docker (recommended)

First time, or after modifying source code, dependencies, or Dockerfiles:
```bash
docker compose up --build
```

If the images have already been built and no source code changes were made, you can start the application directly with:
```bash
docker compose up
```

- Frontend: http://localhost:4200
- Backend: http://localhost:5000

The backend starts first; the frontend waits for the backend health check to pass.
Bookings persist across container restarts via the `bookings_data` Docker volume.

### Local development (without Docker)

**Backend**

```bash
cd backend/backend
dotnet run
```

The API listens on http://localhost:5000. OpenAPI is available in Development at
`http://localhost:5000/openapi/v1.json`.

**Frontend**

```bash
cd frontend
npm install
npm start
```

The app runs on http://localhost:4200 and calls the backend directly at
`http://localhost:5000/api` (this exercises the backend CORS policy).

## API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/flights/search` | Search priced flights for a route/date/cabin |
| `POST` | `/api/bookings` | Create a booking and receive a `SKY-XXXXXX` reference |
| `DELETE` | `/api/bookings/reset` | Clear all bookings (dev/test utility) |

### Pricing rules

- **GlobalAir:** `round(baseFare * 1.15, 2)` — 15% fuel surcharge.
- **BudgetWings:** `round(max(baseFare * 0.90, 29.99), 2)` — 10% discount with a $29.99 floor.

The displayed total is `pricePerPerson * passengers`.

### Document rules

- Domestic routes (same country) require a **National ID** (7–8 alphanumeric characters).
- International routes (different countries) require a **Passport** (6–9 alphanumeric characters).

The six supported airports are ATL, JFK, MIA, LAX (USA) and AEP, EZE (Argentina).

## Technical decisions

- **Strategy pattern for pricing** (`IPricingStrategy` with `GlobalAirPricingStrategy` and
  `BudgetWingsPricingStrategy`). New providers can be onboarded by adding a strategy and
  registering it — no existing pricing code changes.
- **JSON persistence with a Docker volume.** Flight data is read-only and cached in memory at
  startup. Bookings are read/written through a single thread-safe `BookingRepository`
  (`SemaphoreSlim`-guarded writes).
- **Frontend-only sorting.** Sort options (price low→high, high→low, shortest duration,
  departure time) are applied with an Angular computed signal — no extra API calls.
- **Dynamic document validation.** The passenger form switches the document label and
  validation rule between Passport and National ID based on whether the route is international.
- **Recurring daily schedule.** Provider flight data is modeled as a recurring daily schedule:
  a search projects each flight's stored time-of-day onto the requested departure date. This
  keeps the demo functional for any future date while still honoring the "no past dates" rule.

## Trade-offs and known limitations

- No authentication or authorization.
- No relational database — bookings live in a JSON file.
- Flight data is mocked (static JSON), not real-time availability.
- No payment processing; the confirmation is illustrative.
- Display fonts use open-source substitutes (Cormorant Garamond / Inter / JetBrains Mono),
  since the original Anthropic typefaces (Copernicus / StyreneB) are licensed.