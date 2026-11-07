# 04. Cross-Service Relationships & Data Flow

---

## 1. Cross-Database Entity References

In compliance with the **Database-per-Service** pattern, services never use physical SQL Foreign Keys across databases. Instead, distributed entities reference each other through immutable keys.

```mermaid
flowchart LR
    subgraph railcat_users
        STAFF[staff_users.user_id]
    end

    subgraph railcat_catalog
        ITEM[menu_items.item_id]
        EMP[railway_concession_employees.employee_id]
    end

    subgraph railcat_orders
        ORDER[orders.order_id]
        O_ITEM[order_items.item_id]
        O_CUST[orders.customer_ref]
        O_STAFF[orders.created_by_staff_id]
    end

    subgraph railcat_settlement
        PAY[payment_transactions.order_id]
        SHIFT[cashier_shifts.cashier_user_id]
    end

    STAFF -.->|Referenced by| O_STAFF
    STAFF -.->|Referenced by| SHIFT
    ITEM -.->|Copied snapshot| O_ITEM
    EMP -.->|Validated against| O_CUST
    ORDER -.->|Settled by| PAY
```

---

## 2. Order Lifecycle & Differential Pricing Saga

```mermaid
sequenceDiagram
    autonumber
    actor C as Customer / Cashier
    participant POS as Frontend / API Gateway
    participant Catalog as railcat_catalog
    participant Orders as railcat_orders
    participant Settlement as railcat_settlement
    participant Kitchen as Kitchen KDS & Printer

    C->>POS: Select Items & Choose Customer Type
    alt If Customer Type == 'RAILWAY_EMPLOYEE'
        POS->>Catalog: Validate employee_id & device_token
        Catalog-->>POS: Concession Verified (20% discount applied)
    else Normal Retail Customer
        POS->>POS: Standard Retail Pricing
    end

    POS->>Orders: Submit Cart & Allocate Daily Queue Token (e.g. A-014)
    Orders-->>POS: Order Created (Status: NEW, Payment: PENDING)

    C->>POS: Pay via UPI / Card / Cash
    POS->>Settlement: Record Payment Transaction
    Settlement-->>Orders: Mark Order as PAID

    Orders->>Kitchen: Order appears on Kitchen KDS
    Orders->>Kitchen: Spool Kitchen Order Ticket (KOT)
    Kitchen-->>Kitchen: Start Prep -> Mark Ready -> Hand Over
```
