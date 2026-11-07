# RailCat Backend API Services

This directory will host the backend API services powering the RailCat private restaurant platform.

## Planned Architecture

The backend will expose REST and WebSocket endpoints adhering to the Domain-Driven Design (DDD) bounded contexts corresponding to the `railcat_*` databases:

1. **`users-service`**:
   - Staff authentication, JWT issuance, and RBAC (`ADMIN`, `COUNTER_OPERATOR`, `KITCHEN_STAFF`).
2. **`catalog-service`**:
   - Menu categories, dishes (veg/non-veg, culinary tags, ratings), and employee concession verification with device locking.
3. **`orders-service`**:
   - Cart checkout, daily atomic queue numbering (`A-001`), live kitchen KDS transitions, and thermal print spooling.
4. **`settlement-service`**:
   - Cashier shift drawer opening/closing, UPI soundbox integration, payment transaction logs, and daily revenue reports.
