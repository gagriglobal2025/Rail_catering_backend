# 03. Entity-Relationship Diagrams (Per 'railcat_' Database)

---

## 1. `railcat_users` (Staff IAM)

```mermaid
erDiagram
    STAFF_USERS ||--|| STAFF_CREDENTIALS : "has"
    STAFF_USERS ||--o{ STAFF_SESSIONS : "opens"

    STAFF_USERS {
        uuid user_id PK
        varchar employee_id UK
        varchar full_name
        varchar email UK
        varchar phone_number
        varchar system_role
        varchar account_status
        timestamptz last_active_at
        timestamptz created_at
        timestamptz updated_at
    }

    STAFF_CREDENTIALS {
        uuid user_id PK, FK
        varchar password_hash
        integer failed_logins
        timestamptz locked_until
        timestamptz password_updated_at
    }

    STAFF_SESSIONS {
        uuid session_id PK
        uuid user_id FK
        varchar token_hash
        varchar ip_address
        varchar user_agent
        timestamptz expires_at
        timestamptz created_at
    }
```

---

## 2. `railcat_catalog` (Menu, Tariffs & Employee Concessions)

```mermaid
erDiagram
    MENU_CATEGORIES ||--o{ MENU_ITEMS : "groups"

    MENU_CATEGORIES {
        varchar category_id PK
        varchar category_name
        integer sort_order
        boolean is_active
        timestamptz created_at
    }

    MENU_ITEMS {
        varchar item_id PK
        varchar category_id FK
        varchar item_name
        text item_description
        numeric base_price
        varchar emoji_icon
        varchar image_url
        boolean is_vegetarian
        boolean is_available
        boolean is_sold_out
        boolean is_bestseller
        boolean is_spicy
        boolean is_kids_choice
        numeric rating
        integer rating_count
        varchar prep_time
        timestamptz created_at
        timestamptz updated_at
    }

    CONCESSION_POLICIES {
        varchar policy_id PK
        varchar policy_name
        varchar customer_type
        numeric discount_percentage
        boolean is_active
        timestamptz created_at
    }

    RAILWAY_CONCESSION_EMPLOYEES {
        varchar employee_id PK
        varchar full_name
        varchar email
        varchar phone
        varchar department
        varchar designation
        varchar shift_policy
        varchar verification_status
        varchar device_name
        varchar device_token
        boolean device_locked
        text id_card_url
        text rejection_reason
        timestamptz registered_at
        timestamptz verified_at
    }
```

---

## 3. `railcat_orders` (Orders, Daily Queue Tokens & Kitchen Fulfillment)

```mermaid
erDiagram
    DAILY_QUEUE_SEQUENCE ||--o{ ORDERS : "sequences"
    ORDERS ||--o{ ORDER_ITEMS : "contains"
    ORDERS ||--o{ PRINT_SPOOL : "spools"

    DAILY_QUEUE_SEQUENCE {
        date operational_date PK
        integer current_number
        varchar token_prefix
        timestamptz updated_at
    }

    ORDERS {
        uuid order_id PK
        varchar order_number UK
        varchar queue_token
        varchar customer_type
        varchar customer_ref
        varchar pickup_counter
        text special_instructions
        numeric subtotal_amount
        numeric concession_discount
        numeric tax_amount
        numeric grand_total
        varchar order_status
        varchar payment_status
        varchar payment_method
        uuid created_by_staff_id
        timestamptz placed_at
        timestamptz paid_at
        timestamptz preparing_at
        timestamptz ready_at
        timestamptz delivered_at
        timestamptz cancelled_at
        timestamptz created_at
        timestamptz updated_at
    }

    ORDER_ITEMS {
        bigserial order_item_id PK
        uuid order_id FK
        varchar item_id
        varchar item_name
        integer quantity
        numeric unit_price
        numeric line_total
    }

    PRINT_SPOOL {
        uuid job_id PK
        uuid order_id FK
        varchar job_type
        text raw_content
        varchar print_status
        integer retry_count
        timestamptz created_at
        timestamptz printed_at
    }
```

---

## 4. `railcat_settlement` (Payments, Cashier Shifts & Daily Revenue)

```mermaid
erDiagram
    CASHIER_SHIFTS ||--o{ PAYMENT_TRANSACTIONS : "reconciles"

    CASHIER_SHIFTS {
        uuid shift_id PK
        uuid cashier_user_id
        timestamptz opened_at
        timestamptz closed_at
        numeric starting_cash
        numeric cash_collected
        numeric upi_collected
        numeric card_collected
        numeric total_expected
        numeric actual_cash_counted
        varchar shift_status
    }

    PAYMENT_TRANSACTIONS {
        uuid payment_id PK
        uuid order_id
        uuid shift_id FK
        numeric amount
        varchar payment_method
        varchar gateway_reference
        varchar payment_status
        timestamptz created_at
    }

    DAILY_SALES_SUMMARY {
        date summary_date PK
        integer total_orders
        numeric gross_sales
        numeric concessions_given
        numeric tax_collected
        numeric net_revenue
        numeric upi_revenue
        numeric cash_revenue
        numeric card_revenue
        timestamptz updated_at
    }
```
