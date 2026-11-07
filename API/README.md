# RailCat — Microservices API Architecture (.NET 8)

This directory contains the production-grade **ASP.NET Core 8 Web API microservices** powering the RailCat private commercial restaurant / quick-service curry point platform.

---

## 1. Solution Overview

Master Solution: **`RailCat.Backend.sln`**

Every microservice targets **`.NET 8`** (`net8.0`) and follows the Clean Architecture / Repository-Service Pattern:
- `Controllers/` — Thin HTTP controller endpoints
- `Models/Entities/` — EF Core domain entity models
- `Models/DTOs/` — Request and response contracts
- `Data/` — EF Core DbContext with Fluent API configurations
- `Repositories/` — Data persistence abstraction (Interface + Implementation)
- `Services/` — Pure business domain logic (Interface + Implementation)
- `Helpers/` — Cross-cutting utilities (JWT, Passwords, ESC/POS)
- `Middleware/` — Centralized `GlobalExceptionMiddleware`
- `Validators/` — FluentValidation request rules
- `Extensions/` — Clean dependency injection registration
- `Properties/launchSettings.json` — Pre-configured local ports and Swagger

---

## 2. Microservices Matrix

| Microservice | Port (HTTP/HTTPS) | Logical Database | Key Responsibilities |
| :--- | :---: | :--- | :--- |
| **`RailCat.Users.Api`** | `5001` / `7001` | `railcat_users` | Staff IAM, JWT issuance, Roles (`ADMIN`, `COUNTER_OPERATOR`, `KITCHEN_STAFF`) |
| **`RailCat.Catalog.Api`** | `5002` / `7002` | `railcat_catalog` | Menu categories, items, availability toggles, 20% employee concession & device locking |
| **`RailCat.Orders.Api`** | `5003` / `7003` | `railcat_orders` | Cart intake, atomic daily queue numbering (`A-001`), Kitchen KDS status & ESC/POS print spool |
| **`RailCat.Settlement.Api`** | `5004` / `7004` | `railcat_settlement`| Cashier shift drawer opening/closing, payments (UPI/Card/Cash) & daily sales summary reports |

---

## 3. How to Run

### Run Entire Solution via .NET CLI
```bash
# From Backend/API directory:
dotnet build RailCat.Backend.sln
```

### Run Individual Microservice
```bash
# In separate terminal windows:
dotnet run --project RailCat.Users.Api
dotnet run --project RailCat.Catalog.Api
dotnet run --project RailCat.Orders.Api
dotnet run --project RailCat.Settlement.Api
```

### Swagger API Documentation
Once running, interactive OpenAPI / Swagger documentation is available at:
- Users API: `http://localhost:5001/swagger`
- Catalog API: `http://localhost:5002/swagger`
- Orders API: `http://localhost:5003/swagger`
- Settlement API: `http://localhost:5004/swagger`
