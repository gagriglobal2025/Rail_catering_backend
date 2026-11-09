# RailCat — Microservices API Architecture (.NET 8)

This directory contains 4 independent, standalone **ASP.NET Core 8 Web API microservices** powering the RailCat private commercial restaurant / quick-service curry point platform.

---

## 1. Independent Microservice Solutions

Each microservice is completely decoupled with its own dedicated `.sln` file and `.csproj`:

```
Backend/API/
├── RailCat.Users.Api/
│   ├── RailCat.Users.sln                      # Dedicated solution for Users & IAM
│   ├── RailCat.Users.Api.csproj
│   └── ...
│
├── RailCat.Catalog.Api/
│   ├── RailCat.Catalog.sln                    # Dedicated solution for Menu & Concession
│   ├── RailCat.Catalog.Api.csproj
│   └── ...
│
├── RailCat.Orders.Api/
│   ├── RailCat.Orders.sln                     # Dedicated solution for Orders, Queue & KDS
│   ├── RailCat.Orders.Api.csproj
│   └── ...
│
└── RailCat.Settlement.Api/
    ├── RailCat.Settlement.sln                 # Dedicated solution for Payments & Shifts
    ├── RailCat.Settlement.Api.csproj
    └── ...
```

---

## 2. Microservices Directory Matrix

| Microservice | Solution File | Port (HTTP/HTTPS) | Logical Database | Key Responsibilities |
| :--- | :--- | :---: | :--- | :--- |
| **`RailCat.Users.Api`** | `RailCat.Users.sln` | `5001` / `7001` | `railcat_users` | Staff IAM, JWT issuance, Roles (`ADMIN`, `COUNTER_OPERATOR`, `KITCHEN_STAFF`) |
| **`RailCat.Catalog.Api`** | `RailCat.Catalog.sln` | `5002` / `7002` | `railcat_catalog` | Menu categories, items, availability toggles, 20% employee concession & device locking |
| **`RailCat.Orders.Api`** | `RailCat.Orders.sln` | `5003` / `7003` | `railcat_orders` | Cart intake, atomic daily queue numbering (`A-001`), Kitchen KDS status & ESC/POS print spool |
| **`RailCat.Settlement.Api`** | `RailCat.Settlement.sln` | `5004` / `7004` | `railcat_settlement`| Cashier shift drawer opening/closing, payments (UPI/Card/Cash) & daily sales summary reports |

---

## 3. How to Run Each Microservice

You can open and build each service independently:

```bash
# Users & Auth Service
cd RailCat.Users.Api
dotnet build RailCat.Users.sln
dotnet run

# Menu Catalog & Concession Service
cd ../RailCat.Catalog.Api
dotnet build RailCat.Catalog.sln
dotnet run

# Orders & Kitchen KDS Service
cd ../RailCat.Orders.Api
dotnet build RailCat.Orders.sln
dotnet run

# Payments & Settlement Service
cd ../RailCat.Settlement.Api
dotnet build RailCat.Settlement.sln
dotnet run
```

---

## 4. Swagger API Documentation

Once started, each service provides its own OpenAPI / Swagger documentation:
- Users API: `http://localhost:5001/swagger`
- Catalog API: `http://localhost:5002/swagger`
- Orders API: `http://localhost:5003/swagger`
- Settlement API: `http://localhost:5004/swagger`
