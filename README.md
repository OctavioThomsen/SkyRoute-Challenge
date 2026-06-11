# SkyRoute Challenge

SkyRoute is a flight aggregator platform. This repository implements the SkyRoute travel
module: **flight search** (search form + results with date slider and frontend sorting),
a **booking flow** (flight selection → per-passenger details → confirmation with a `SKY-XXXXXX`
reference), and a **My Bookings** page that lists all past reservations.

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

If the images have already been built and no source code changes were made:
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

## Application flow

```
/search  →  /results  →  /booking  →  /confirmation
                ↑                           |
         "Edit search"               "My Bookings" (navbar)
                                           ↓
                                      /my-bookings
```

1. **Search** — enter origin, destination, departure date, cabin class and number of
   passengers (1–9). The form values are restored if the user navigates back via "Edit
   search".

2. **Results** — shows a date chip slider across the top (price per person or "—" for dates
   with no flights). Exact matches for the selected date appear first; a divider separates
   them from suggestions for the same route on other dates. All lists support client-side
   sorting (price, duration, departure time).

3. **Booking** — collects full name, email and document number for **each passenger**. The
   document type and validation pattern switch automatically based on whether the route is
   domestic or international.

4. **Confirmation** — displays the `SKY-XXXXXX` booking reference, flight details and lead
   passenger name.

5. **My Bookings** — lists every past reservation (newest first). Each entry shows the same
   flight-card design used in Results. An inline panel reveals the full booking details for
   each entry.

## API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/flights/search` | Search priced flights — returns `matches` and `suggestions` |
| `POST` | `/api/bookings` | Create a booking for one or more passengers |
| `GET` | `/api/bookings` | List all bookings, newest first |
| `DELETE` | `/api/bookings/reset` | Clear all bookings (dev/test utility) |

### Search response shape

```json
{
  "matches": [ /* flights matching exact date + cabin */ ],
  "suggestions": [ /* same route + cabin, other dates */ ]
}
```

Each flight object includes `flightNumber`, `provider`, `origin`, `destination`,
`departureTime`, `arrivalTime`, `durationMinutes`, `cabinClass`, `pricePerPerson`,
`totalPrice` and `isInternational`.

### Booking request shape

```json
{
  "flightNumber": "GA-101",
  "provider": "GlobalAir",
  "origin": "JFK",
  "destination": "EZE",
  "departureTime": "2026-06-11T10:30:00",
  "arrivalTime": "2026-06-11T20:45:00",
  "durationMinutes": 615,
  "cabinClass": "Economy",
  "pricePerPerson": 552.00,
  "totalPrice": 1104.00,
  "passengers": 2,
  "passengerDetailsList": [
    {
      "fullName": "Juan Pérez García",
      "email": "juan.perez@example.com",
      "documentNumber": "A12345678",
      "documentType": "Passport"
    },
    {
      "fullName": "María López Martínez",
      "email": "maria.lopez@example.com",
      "documentNumber": "B9876543",
      "documentType": "Passport"
    }
  ]
}
```

`passengerDetailsList` must have exactly `passengers` entries. Each entry is validated
individually; document format errors are reported per-passenger.

### Pricing rules

- **GlobalAir:** `round(baseFare × 1.15, 2)` — 15% fuel surcharge.
- **BudgetWings:** `round(max(baseFare × 0.90, 29.99), 2)` — 10% discount with a $29.99 floor.

The displayed total is `pricePerPerson × passengers`.

### Document rules

- Domestic routes (same country) → **National ID** (7–8 alphanumeric characters).
- International routes (different countries) → **Passport** (6–9 alphanumeric characters).

The six supported airports are ATL, JFK, MIA, LAX (USA) and AEP, EZE (Argentina).

### Mock flight data

Flights span **2026-06-11 → 2026-07-08** across two providers:

| Provider | Routes |
|---|---|
| GlobalAir | JFK→EZE, MIA→EZE, ATL→AEP, JFK→LAX, ATL→MIA, LAX→ATL, AEP→EZE, EZE→JFK, MIA→LAX |
| BudgetWings | JFK→EZE, MIA→AEP, LAX→EZE, JFK→LAX, ATL→JFK, MIA→ATL, EZE→AEP, EZE→MIA, LAX→MIA |

Cabin classes available: `Economy`, `Business`, `FirstClass`.

## Technical decisions

- **Strategy pattern for pricing** (`IPricingStrategy` with `GlobalAirPricingStrategy` and
  `BudgetWingsPricingStrategy`). New providers can be onboarded by adding a strategy and
  registering it — no existing pricing code changes.
- **JSON persistence with a Docker volume.** Flight data is read-only and cached in memory at
  startup. Bookings are read/written through a single thread-safe `BookingRepository`
  (`SemaphoreSlim`-guarded writes).
- **Two-list search result.** `FlightService.Search()` produces `matches` (exact date + cabin)
  and `suggestions` (same route + cabin, nearest future dates). Past flights are excluded;
  arrival time is computed from `durationMinutes` rather than stored.
- **Per-passenger `FormArray`.** The booking form dynamically adds one form group per
  passenger at `ngOnInit`. Each group shares the same document-type rule (derived from route
  internationalness), and errors are reported individually.
- **Frontend-only sorting.** Sort options (price low→high, high→low, shortest duration,
  departure time) are applied via Angular computed signals — no extra API calls.
- **Date chip slider.** The results page builds a sorted list of dates from matches +
  suggestions. Each chip shows the cheapest per-person price for that date (or "—"). Clicking
  a chip fires a new search and scrolls the selected chip into view.
- **My Bookings page.** `GET /api/bookings` returns all persisted bookings (document numbers
  excluded). The frontend renders each entry using the same `FlightCardComponent` as Results,
  with an inline detail panel per booking.
- **Form state restoration.** When the user clicks "Edit search" from Results, the search
  form re-populates with the previous request values via Angular signals.

## Trade-offs and known limitations

- No authentication or authorization.
- No relational database — bookings live in a JSON file.
- Flight data is mocked (static JSON), not real-time availability.
- No payment processing; the confirmation is illustrative.
- The `My Bookings` view shows the lead passenger name only (index 0 of `passengerDetailsList`).
  Full per-passenger details are stored but not yet surfaced in the UI beyond the inline panel.
- Display fonts use open-source substitutes (Cormorant Garamond / Inter / JetBrains Mono),
  since the original Anthropic typefaces (Copernicus / StyreneB) are licensed.
