# Enterprise Manufacturing ERP

Microservices-based Manufacturing ERP built with ASP.NET Core 9, Clean Architecture, CQRS, PostgreSQL, RabbitMQ, Redis, and the shared **`erp-app`** React frontend.

## Quick Start

```bash
# Infrastructure
cd docker
docker compose up -d postgres redis rabbitmq

# Identity API
cd ..
dotnet ef migrations add InitialCreate --project src/Services/Identity.Infrastructure --startup-project src/Services/Identity.API
dotnet run --project src/Services/Identity.API

# Product, Inventory, Manufacturing (local)
dotnet run --project src/Services/Product/Product.API
dotnet run --project src/Services/Inventory/Inventory.API
dotnet run --project src/Services/Manufacturing/Manufacturing.API

# API Gateway
dotnet run --project src/Gateway/Erp.Gateway   # Ocelot :5000
```

### Frontend (`erp-app`)

```bash
cd ../../erp-app
npm install
cp .env.example .env
# For gateway mode:
# VITE_API_PROXY_TARGET=http://localhost:5000
# VITE_API_MODE=gateway
npm run dev
```

## Documentation

See [docs/architecture/README.md](docs/architecture/README.md) for solution structure, database design, events, sagas, security, and deployment.

## Services

| # | Service | Status |
|---|---------|--------|
| 1 | Identity | Reference implementation (CQRS + JWT + Refresh) |
| 2 | Organization | Domain models scaffolded |
| 3 | Product | Minimal API (`/api/v1/products`) |
| 4 | Inventory | Minimal API + MassTransit consumer |
| 5 | Procurement | Project shell |
| 6 | Sales | Project shell |
| 7 | Manufacturing | CQRS + RabbitMQ publish on production order |
| 8 | Quality | Project shell |
| 9 | Maintenance | Project shell |
| 10 | HR & Payroll | Project shell |
| 11 | Finance | Project shell |
| 12 | Reporting | Project shell |

## Legacy Monolith

The original monolith at `D:/ERP/ERP` remains the default backend for `erp-app` in `VITE_API_MODE=monolith`.
