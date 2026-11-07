# RailCat — Private Restaurant & Curry Point Platform Database Architecture

This repository contains the complete enterprise microservices database architecture, Third Normal Form (3NF) relational schemas, PostgreSQL DDL scripts, and seed data for **RailCat** — a private commercial quick-service restaurant (QSR) and curry point.

---

## 1. Project Theme & Core Domain

- **Private Restaurant Model**: Operates as an independent commercial food outlet (Counter POS, Kitchen KDS, Cashier Settlement, and Customer Mobile Web).
- **Zero Railway Travel Entities**: Completely free of railway-specific entities (no stations, trains, coaches, berths, PNRs, or passenger manifests).
- **Differential Pricing Business Logic**:
  - **Normal / Retail Users**: Standard retail menu prices + 5% GST.
  - **Railway Employees**: Receive a verified 20% concession discount upon UMID identification and single-device lock verification.
- **Naming Convention**: All databases and service roles consistently use the **`railcat_`** prefix.

---

## 2. Directory Structure

```
Backend/DataBase/
├── README.md                                 # Master architecture guide
├── docker-compose.yml                         # Containerized local PostgreSQL 16 runner
├── deploy_to_server.py                       # Automated safe deployment script for remote server
├── docs/                                     # Comprehensive architectural specifications
│   ├── 01_architecture_overview.md          # Domain boundaries & DDD microservices overview
│   ├── 02_data_dictionary.md                # Complete field dictionary, types & constraints
│   ├── 03_er_diagrams.md                    # Mermaid ER diagrams for all 4 databases
│   ├── 04_cross_service_relationships.md    # Distributed entity mapping & saga workflows
│   ├── 05_security_and_rls.md               # Staff RBAC & UMID device lock anti-fraud security
│   ├── 06_performance_and_scalability.md    # High-concurrency atomic daily queue sequence generator
│   └── 07_local_print_agent_and_peripherals.md # ESC/POS KOT & receipt print spooling protocol
└── schemas/                                  # Production SQL DDL & Seed scripts per database
    ├── init-all-databases.sql                # Master cluster initializer (roles, databases, pgcrypto)
    ├── 01_railcat_users/                     # Staff IAM & Access Service
    │   ├── 01_schema.sql
    │   └── 02_seed.sql
    ├── 02_railcat_catalog/                   # Menu, Pricing & Concession Registry
    │   ├── 01_schema.sql
    │   └── 02_seed.sql
    ├── 03_railcat_orders/                    # Orders, Daily Queue Tokens & Kitchen Fulfillment
    │   ├── 01_schema.sql
    │   ├── 02_queue_function.sql
    │   └── 03_seed.sql
    └── 04_railcat_settlement/                # Payments, Cashier Shifts & Daily Revenue
        ├── 01_schema.sql
        └── 02_seed.sql
```

---

## 3. Microservice Database Ownership Matrix

| Database Name | Service Owner Role | Schema Files | Responsibility |
| :--- | :--- | :--- | :--- |
| **`railcat_users`** | `railcat_users_user` | `schemas/01_railcat_users/` | Restaurant staff IAM, roles (`ADMIN`, `COUNTER_OPERATOR`, `KITCHEN_STAFF`), passwords & sessions |
| **`railcat_catalog`** | `railcat_catalog_user` | `schemas/02_railcat_catalog/` | Menu categories, dishes (Veg/Non-Veg, badges), 20% concession policy, employee registry |
| **`railcat_orders`** | `railcat_orders_user` | `schemas/03_railcat_orders/` | Orders, atomic daily queue numbering (`A-001`), kitchen KDS stages, thermal print spool |
| **`railcat_settlement`** | `railcat_settlement_user` | `schemas/04_railcat_settlement/` | Cashier shift drawer reconciliation, UPI/Card/Cash transactions, daily sales ledger |

---

## 4. Quick Start

### Deploy to Remote PostgreSQL Server
```bash
python deploy_to_server.py
```

### Run Locally via Docker
```bash
docker compose up -d
```
