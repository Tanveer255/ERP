# ERP

Manufacturing ERP with an ASP.NET Core 8 monolith API, an optional Enterprise microservices stack, and a shared React frontend.

## Stack

- **Monolith backend:** ASP.NET Core 8, EF Core, SQL Server, JWT + HttpOnly cookie auth
- **Enterprise backend:** ASP.NET Core 9 microservices, PostgreSQL, RabbitMQ, Ocelot gateway (see `Enterprise/`)
- **Frontend:** `erp-app` — React 19, Vite, Tailwind v4, Redux Toolkit, React Router
- **Infrastructure:** Docker Compose, GitHub Actions CI

## Local development (monolith + erp-app)

### Prerequisites

- .NET 8 SDK
- Node.js 22+
- SQL Server (LocalDB or Docker)

### Backend

```bash
dotnet run --project ERP/ERP.csproj --launch-profile https
```

Swagger: `https://localhost:7013/swagger`

### Frontend

```bash
cd erp-app
npm install
cp .env.example .env
npm run dev
```

App: `http://localhost:61104` — API requests to `/api` are proxied to the monolith.

## Enterprise microservices (optional)

See [Enterprise/README.md](Enterprise/README.md). Point `erp-app` at the gateway:

```env
VITE_API_PROXY_TARGET=http://localhost:5000
VITE_API_MODE=gateway
```

## Docker

Monolith stack:

```bash
docker compose up --build
```

Enterprise infrastructure:

```bash
cd Enterprise/docker && docker compose up -d
```

## Authentication

- **Monolith:** HttpOnly `AuthToken` cookie (JWT) via `/api/Users/*`
- **Gateway:** JWT Bearer from Identity service at `/api/v1/auth/*`

## Configuration

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection (monolith) |
| `JwtSettings` | JWT signing and expiry |
| `VITE_API_MODE` | `monolith` (default) or `gateway` |
| `VITE_API_PROXY_TARGET` | Backend URL for Vite dev proxy |

See `ERP/appsettings.example.json` and `erp-app/.env.example` for templates.
