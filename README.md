# ERP

Manufacturing ERP with an ASP.NET Core 8 API and a React frontend.

## Stack

- **Backend:** ASP.NET Core 8, EF Core, SQL Server, JWT + HttpOnly cookie auth
- **Frontend:** React 19, Vite
- **Infrastructure:** Docker Compose, GitHub Actions CI

## Local development

### Prerequisites

- .NET 8 SDK
- Node.js 22+
- SQL Server (local or Docker)

### Backend

1. Copy `ERP/appsettings.example.json` to `ERP/appsettings.json` and update secrets.
2. Or use the defaults in `ERP/appsettings.Development.json` with Docker SQL Server.
3. Run the API:

```bash
dotnet run --project ERP/ERP.csproj
```

Swagger: `http://localhost:5254/swagger`

### Frontend

```bash
cd erp-app
npm install
cp .env.example .env
npm run dev
```

App: `http://localhost:61104` — API requests to `/api` are proxied to the backend.

## Docker

Start SQL Server, API, and frontend together:

```bash
docker compose up --build
```

| Service   | URL                      |
|-----------|--------------------------|
| Frontend  | http://localhost:61104   |
| API       | http://localhost:5254    |
| SQL Server| localhost:1433           |

## Authentication

- Login sets an HttpOnly `AuthToken` cookie (JWT).
- Protected endpoints require authentication by default.
- Public auth routes (`Login`, `Signup`, etc.) are marked `[AllowAnonymous]`.
- JWT is also accepted via `Authorization: Bearer` header.

## Configuration

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection |
| `JwtSettings` | JWT signing and expiry |
| `FrontendUrl` | CORS origin for the React app |
| `SendGridSettings` | Email delivery |
| `ReCaptchaSettings` | Login/signup CAPTCHA |

See `ERP/appsettings.example.json` and `erp-app/.env.example` for templates.
