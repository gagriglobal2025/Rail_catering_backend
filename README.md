# RailCat — Backend Services & Database Architecture

Official backend repository for the **RailCat** private commercial restaurant / quick-service curry point platform.

---

## 1. Overview

RailCat is an independent commercial food outlet system featuring:
- **Counter POS & Admin Web Panel**: Multi-role staff access (`ADMIN`, `COUNTER_OPERATOR`, `KITCHEN_STAFF`).
- **Kitchen Display System (KDS)**: Real-time order progress tracking and thermal receipt/KOT print spooling.
- **Customer Mobile Web**: Self-ordering, category browsing, and differential pricing verification.
- **Differential Pricing Business Model**:
  - **Retail / Normal Customers**: Standard retail menu prices + 5% GST.
  - **Railway Employees**: Verified 20% concession discount upon UMID employee identification and hardware device lock.

---

## 2. Repository Structure

```
.
├── .gitignore
├── .env.example
├── README.md
├── API/                                      # Microservice API implementations
│   └── README.md
└── DataBase/                                 # Relational Database Architecture
    ├── README.md                             # Database master documentation
    ├── docker-compose.yml                    # Local PostgreSQL 16 container runner
    ├── deploy_to_server.py                  # Remote deployment automation script
    ├── docs/                                 # Specifications & ER Diagrams
    │   ├── 01_architecture_overview.md
    │   ├── 02_data_dictionary.md
    │   ├── 03_er_diagrams.md
    │   ├── 04_cross_service_relationships.md
    │   ├── 05_security_and_rls.md
    │   ├── 06_performance_and_scalability.md
    │   └── 07_local_print_agent_and_peripherals.md
    └── schemas/                              # PostgreSQL DDL & Seed Scripts
        ├── init-all-databases.sql
        ├── 01_railcat_users/
        ├── 02_railcat_catalog/
        ├── 03_railcat_orders/
        └── 04_railcat_settlement/
```

---

## 3. Database Ownership Matrix

| Database Name | Service Role | Purpose |
| :--- | :--- | :--- |
| **`railcat_users`** | `railcat_users_user` | Internal staff authentication, RBAC roles, and active sessions |
| **`railcat_catalog`** | `railcat_catalog_user` | Menu items, categories, 20% concession policies, and employee registry |
| **`railcat_orders`** | `railcat_orders_user` | Orders, atomic daily queue sequences (`A-001`), KDS stages, thermal print spool |
| **`railcat_settlement`** | `railcat_settlement_user` | Cashier shift reconciliations, payment transactions, and daily sales summaries |

---

## 4. Quick Start

### Local Database Cluster via Docker
```bash
cd DataBase
docker compose up -d
```

### Remote Server Deployment
```bash
cd DataBase
python deploy_to_server.py
```

