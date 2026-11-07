# 02. Data Dictionary — RailCat Platform

---

## 1. Database: `railcat_users`

### Table: `staff_users`
Internal staff roster backing `/users` management.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `user_id` | UUID | No | `gen_random_uuid()` | PK | — | Unique staff user ID |
| `employee_id` | VARCHAR(30) | No | — | Unique | — | Internal staff code (e.g. `STAFF-ADMIN`, `STAFF-POS01`) |
| `full_name` | VARCHAR(100) | No | — | — | — | Staff member full name |
| `email` | VARCHAR(120) | No | — | Unique | — | Work email address |
| `phone_number` | VARCHAR(20) | Yes | — | — | — | Contact mobile number |
| `system_role` | VARCHAR(30) | No | — | — | `IN ('ADMIN', 'COUNTER_OPERATOR', 'KITCHEN_STAFF')` | RBAC role |
| `account_status` | VARCHAR(20) | No | `'ACTIVE'` | — | `IN ('ACTIVE', 'INACTIVE')` | Active status |
| `last_active_at` | TIMESTAMPTZ | Yes | — | — | — | Last activity timestamp |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Record creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Record update timestamp |

### Table: `staff_credentials`
Isolated authentication table for Argon2id/bcrypt passwords.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `user_id` | UUID | No | — | PK, FK | `REFERENCES staff_users(user_id)` | Parent user ID |
| `password_hash` | VARCHAR(255) | No | — | — | — | Secure password hash |
| `failed_logins` | INT | No | `0` | — | `>= 0` | Counter for brute-force lock |
| `locked_until` | TIMESTAMPTZ | Yes | — | — | — | Lockout expiry time |
| `password_updated_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Last password change |

### Table: `staff_sessions`
Active JWT refresh tokens and device sessions.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `session_id` | UUID | No | `gen_random_uuid()` | PK | — | Session identifier |
| `user_id` | UUID | No | — | FK | `REFERENCES staff_users(user_id)` | Authenticated user |
| `token_hash` | VARCHAR(255) | No | — | — | — | SHA-256 hash of refresh token |
| `ip_address` | VARCHAR(45) | Yes | — | — | — | Client IPv4/IPv6 |
| `user_agent` | VARCHAR(255) | Yes | — | — | — | Browser / client identifier |
| `expires_at` | TIMESTAMPTZ | No | — | — | — | Expiration timestamp |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Session start time |

---

## 2. Database: `railcat_catalog`

### Table: `menu_categories`
Classification tabs on POS and Mobile Web (`Breakfast`, `Lunch`, `Snacks`, `Beverages`).

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `category_id` | VARCHAR(50) | No | — | PK | — | Category key (`breakfast`, `lunch`, `snacks`, `beverages`) |
| `category_name` | VARCHAR(100) | No | — | — | — | Display label |
| `sort_order` | INT | No | `0` | — | — | Display order |
| `is_active` | BOOLEAN | No | `true` | — | — | Active flag |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Created timestamp |

### Table: `menu_items`
Culinary dishes and beverages with customer-facing tags and retail prices.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `item_id` | VARCHAR(50) | No | — | PK | — | Item slug (`masala-dosa`, `chicken-biryani`) |
| `category_id` | VARCHAR(50) | No | — | FK | `REFERENCES menu_categories(category_id)` | Category foreign key |
| `item_name` | VARCHAR(100) | No | — | — | — | Dish name |
| `item_description` | TEXT | No | — | — | — | Description / ingredients |
| `base_price` | NUMERIC(10,2) | No | — | — | `>= 0` | Standard retail price (INR) |
| `emoji_icon` | VARCHAR(10) | No | `'🍲'` | — | — | Display emoji icon |
| `image_url` | VARCHAR(500) | Yes | — | — | — | Photo URL |
| `is_vegetarian` | BOOLEAN | No | `true` | — | — | Veg / Non-veg indicator |
| `is_available` | BOOLEAN | No | `true` | — | — | In-menu availability |
| `is_sold_out` | BOOLEAN | No | `false` | — | — | Real-time sold out toggle |
| `is_bestseller` | BOOLEAN | No | `false` | — | — | Bestseller badge |
| `is_spicy` | BOOLEAN | No | `false` | — | — | Spicy indicator |
| `is_kids_choice` | BOOLEAN | No | `false` | — | — | Kids choice badge |
| `rating` | NUMERIC(2,1) | Yes | `4.5` | — | `>= 1.0 AND <= 5.0` | Average customer rating |
| `rating_count` | INT | No | `0` | — | `>= 0` | Total review count |
| `prep_time` | VARCHAR(20) | Yes | `'10-12 mins'` | — | — | Preparation time estimate |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Last update timestamp |

### Table: `concession_policies`
Discount governance rules for differential pricing.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `policy_id` | VARCHAR(50) | No | — | PK | — | Policy code (e.g. `POL-RLY-20`) |
| `policy_name` | VARCHAR(100) | No | — | — | — | Rule title |
| `customer_type` | VARCHAR(30) | No | `'RAILWAY_EMPLOYEE'` | — | — | Target customer group |
| `discount_percentage` | NUMERIC(5,2) | No | `20.00` | — | `>= 0 AND <= 100` | Discount percent (e.g. 20%) |
| `is_active` | BOOLEAN | No | `true` | — | — | Policy status |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Policy creation timestamp |

### Table: `railway_concession_employees`
Pre-registered railway employees eligible for discounted pricing, with device-level lock.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `employee_id` | VARCHAR(30) | No | — | PK | — | Railway Employee ID (`G084`, `SCR-1042`) |
| `full_name` | VARCHAR(100) | No | — | — | — | Employee name |
| `email` | VARCHAR(120) | Yes | — | — | — | Email address |
| `phone` | VARCHAR(20) | Yes | — | — | — | Mobile phone number |
| `department` | VARCHAR(100) | No | — | — | — | Department (`Running Staff`, `Commercial`) |
| `designation` | VARCHAR(100) | No | — | — | — | Official designation |
| `shift_policy` | VARCHAR(50) | Yes | `'Standard'` | — | — | Shift / duty roster |
| `verification_status`| VARCHAR(20) | No | `'PENDING'` | — | `IN ('ACTIVE', 'PENDING', 'REJECTED')` | UMID verification state |
| `device_name` | VARCHAR(100) | Yes | — | — | — | Bound phone model |
| `device_token` | VARCHAR(100) | Yes | — | — | — | Unique device fingerprint |
| `device_locked` | BOOLEAN | No | `false` | — | — | Prevents sharing discount |
| `id_card_url` | TEXT | Yes | — | — | — | UMID card image URL |
| `rejection_reason` | TEXT | Yes | — | — | — | Reason if rejected |
| `registered_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Registration timestamp |
| `verified_at` | TIMESTAMPTZ | Yes | — | — | — | Admin verification timestamp |

---

## 3. Database: `railcat_orders`

### Table: `daily_queue_sequence`
Atomic daily sequence counter resetting to `1` at midnight (`A-001`, `A-002`, ...).

| Column | Type | Nullable | Default | PK/FK | Description |
| :--- | :--- | :---: | :--- | :---: | :--- |
| `operational_date` | DATE | No | — | PK | Date of service |
| `current_number` | INT | No | `0` | — | Current running sequence counter |
| `token_prefix` | VARCHAR(10) | No | `'A'` | — | Token letter prefix |
| `updated_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | Last sequence update |

### Table: `orders`
Master order record from POS counter or customer Mobile Web.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `order_id` | UUID | No | `gen_random_uuid()` | PK | — | Internal UUID |
| `order_number` | VARCHAR(30) | No | — | Unique | — | Display order number (`ORD-20261107-001`) |
| `queue_token` | VARCHAR(20) | No | — | — | — | Daily token (`A-001`) |
| `customer_type` | VARCHAR(30) | No | `'NORMAL'` | — | `IN ('NORMAL', 'RAILWAY_EMPLOYEE')` | Pricing tier |
| `customer_ref` | VARCHAR(50) | Yes | — | — | — | Customer phone or Railway Emp ID |
| `pickup_counter` | VARCHAR(50) | Yes | `'Counter 1'` | — | — | Assigned pickup counter |
| `special_instructions`| TEXT | Yes | — | — | — | Kitchen preparation notes |
| `subtotal_amount` | NUMERIC(10,2) | No | — | — | `>= 0` | Gross item sum before discount |
| `concession_discount`| NUMERIC(10,2) | No | `0.00` | — | `>= 0` | 20% discount (if eligible) |
| `tax_amount` | NUMERIC(10,2) | No | `0.00` | — | `>= 0` | 5% GST on (subtotal - discount) |
| `grand_total` | NUMERIC(10,2) | No | — | — | `>= 0` | Net payable amount |
| `order_status` | VARCHAR(30) | No | `'NEW'` | — | `IN ('NEW', 'PREPARING', 'READY', 'DELIVERED', 'CANCELLED')` | KDS stage |
| `payment_status` | VARCHAR(30) | No | `'PENDING'` | — | `IN ('PENDING', 'PAID', 'FAILED')` | Payment status |
| `payment_method` | VARCHAR(20) | Yes | — | — | `IN ('UPI', 'CARD', 'CASH')` | Method used |
| `created_by_staff_id`| UUID | Yes | — | — | — | Null if mobile self-order |
| `placed_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Order placement time |
| `paid_at` | TIMESTAMPTZ | Yes | — | — | — | Payment confirmation time |
| `preparing_at` | TIMESTAMPTZ | Yes | — | — | — | Kitchen start time |
| `ready_at` | TIMESTAMPTZ | Yes | — | — | — | Marked ready for pickup |
| `delivered_at` | TIMESTAMPTZ | Yes | — | — | — | Handed over to customer |
| `cancelled_at` | TIMESTAMPTZ | Yes | — | — | — | Cancellation time |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Record created |
| `updated_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Record updated |

### Table: `order_items`
Individual line items in an order.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `order_item_id` | BIGSERIAL | No | — | PK | — | Item row ID |
| `order_id` | UUID | No | — | FK | `REFERENCES orders(order_id)` | Parent order |
| `item_id` | VARCHAR(50) | No | — | — | — | Catalog item key |
| `item_name` | VARCHAR(100) | No | — | — | — | Item title at order time |
| `quantity` | INT | No | — | — | `> 0` | Quantity ordered |
| `unit_price` | NUMERIC(10,2) | No | — | — | `>= 0` | Unit price at order time |
| `line_total` | NUMERIC(10,2) | No | — | — | `>= 0` | Total for this item line |

### Table: `print_spool`
Thermal receipt and Kitchen Order Ticket (KOT) printing queue.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `job_id` | UUID | No | `gen_random_uuid()` | PK | — | Spool job ID |
| `order_id` | UUID | No | — | FK | `REFERENCES orders(order_id)` | Target order |
| `job_type` | VARCHAR(20) | No | `'KOT'` | — | `IN ('KOT', 'RECEIPT')` | Ticket type |
| `raw_content` | TEXT | No | — | — | — | ESC/POS or formatted string |
| `print_status` | VARCHAR(20) | No | `'PENDING'` | — | `IN ('PENDING', 'PRINTED', 'FAILED')` | Spool status |
| `retry_count` | INT | No | `0` | — | `>= 0` | Retry attempts |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Queued timestamp |
| `printed_at` | TIMESTAMPTZ | Yes | — | — | — | Printed timestamp |

---

## 4. Database: `railcat_settlement`

### Table: `cashier_shifts`
Cashier drawer session reconciliation.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `shift_id` | UUID | No | `gen_random_uuid()` | PK | — | Shift session ID |
| `cashier_user_id`| UUID | No | — | — | — | Cashier staff user ID |
| `opened_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Shift start time |
| `closed_at` | TIMESTAMPTZ | Yes | — | — | — | Shift close time |
| `starting_cash` | NUMERIC(10,2) | No | `500.00` | — | `>= 0` | Float cash in drawer |
| `cash_collected` | NUMERIC(10,2) | No | `0.00` | — | `>= 0` | Cash collected |
| `upi_collected` | NUMERIC(10,2) | No | `0.00` | — | `>= 0` | UPI payments collected |
| `card_collected`| NUMERIC(10,2) | No | `0.00` | — | `>= 0` | Card payments collected |
| `total_expected`| NUMERIC(10,2) | No | `500.00` | — | — | System calculated drawer sum |
| `actual_cash_counted` | NUMERIC(10,2) | Yes | — | — | — | Physical count at shift close |
| `shift_status` | VARCHAR(20) | No | `'OPEN'` | — | `IN ('OPEN', 'CLOSED')` | Drawer status |

### Table: `payment_transactions`
Individual transaction ledger for all payment methods.

| Column | Type | Nullable | Default | PK/FK | Constraints | Description |
| :--- | :--- | :---: | :--- | :---: | :--- | :--- |
| `payment_id` | UUID | No | `gen_random_uuid()` | PK | — | Transaction ID |
| `order_id` | UUID | No | — | — | — | Reference to `railcat_orders.orders` |
| `shift_id` | UUID | Yes | — | FK | `REFERENCES cashier_shifts(shift_id)` | Associated shift |
| `amount` | NUMERIC(10,2) | No | — | — | `>= 0` | Paid amount (INR) |
| `payment_method`| VARCHAR(20) | No | — | — | `IN ('UPI', 'CARD', 'CASH')` | Payment mode |
| `gateway_reference` | VARCHAR(100) | Yes | — | — | — | UPI UTR / Card Auth code |
| `payment_status`| VARCHAR(20) | No | `'SUCCESS'` | — | `IN ('SUCCESS', 'FAILED', 'REFUNDED')` | Status |
| `created_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | — | Transaction timestamp |

### Table: `daily_sales_summary`
Pre-aggregated commercial sales ledger backing Admin `/reports`.

| Column | Type | Nullable | Default | PK/FK | Description |
| :--- | :--- | :---: | :--- | :---: | :--- |
| `summary_date` | DATE | No | — | PK | Date of summary |
| `total_orders` | INT | No | `0` | — | Total count of completed orders |
| `gross_sales` | NUMERIC(10,2) | No | `0.00` | — | Gross item sales before discounts |
| `concessions_given` | NUMERIC(10,2) | No | `0.00` | — | Total employee concession discounts |
| `tax_collected` | NUMERIC(10,2) | No | `0.00` | — | 5% GST collected |
| `net_revenue` | NUMERIC(10,2) | No | `0.00` | — | Net revenue realized |
| `upi_revenue` | NUMERIC(10,2) | No | `0.00` | — | Revenue from UPI |
| `cash_revenue` | NUMERIC(10,2) | No | `0.00` | — | Revenue from Cash |
| `card_revenue` | NUMERIC(10,2) | No | `0.00` | — | Revenue from Cards |
| `updated_at` | TIMESTAMPTZ | No | `CURRENT_TIMESTAMP` | — | Last aggregation timestamp |
