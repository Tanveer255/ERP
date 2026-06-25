# Enterprise Manufacturing ERP

Microservices-based Manufacturing ERP built with ASP.NET Core 9, Clean Architecture, CQRS, PostgreSQL, RabbitMQ, Redis, and React.

## Quick Start

```bash
# Infrastructure
cd docker
docker compose up -d postgres redis rabbitmq

# Identity API
cd ..
dotnet ef migrations add InitialCreate --project src/Services/Identity.Infrastructure --startup-project src/Services/Identity.API
dotnet run --project src/Services/Identity.API

# API Gateway
dotnet run --project src/Gateway/Erp.Gateway
```

## Documentation

See [docs/architecture/README.md](docs/architecture/README.md) for:

1. Solution structure
2. Database design
3. ER diagram
4. Microservice communication
5. API contracts
6. RabbitMQ events
7. Saga workflows
8. Security design
9. Deployment architecture
10. CI/CD pipeline

## Services

| # | Service | Status |
|---|---------|--------|
| 1 | Identity | Reference implementation (CQRS + JWT + Refresh) |
| 2 | Organization | Domain models scaffolded |
| 3 | Product | Project shell |
| 4 | Inventory | Project shell |
| 5 | Procurement | Project shell |
| 6 | Sales | Project shell |
| 7 | Manufacturing | Domain models scaffolded |
| 8 | Quality | Project shell |
| 9 | Maintenance | Project shell |
| 10 | HR & Payroll | Project shell |
| 11 | Finance | Project shell |
| 12 | Reporting | Project shell |

## Legacy Monolith

The original monolith remains at `D:/ERP` and can be migrated incrementally.
