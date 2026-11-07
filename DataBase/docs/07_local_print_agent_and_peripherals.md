# 07. Local Print Agent & Peripherals

---

## 1. ESC/POS Thermal Printing Flow

When an order payment is confirmed (`payment_status = 'PAID'`), a print job is spooled into `railcat_orders.print_spool`:

1. **Receipt Printing**:
   - Spooled with `job_type = 'RECEIPT'`.
   - Formatted with restaurant branding, order token (`A-014`), items, subtotal, 20% discount (if employee), 5% GST, and grand total.
2. **Kitchen Order Ticket (KOT)**:
   - Spooled with `job_type = 'KOT'`.
   - Sent to the kitchen thermal printer containing large token number, items, quantities, and special instructions.

---

## 2. Print Spool Schema

```sql
CREATE TABLE IF NOT EXISTS print_spool (
    job_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
    job_type VARCHAR(20) NOT NULL DEFAULT 'KOT' CHECK (job_type IN ('KOT', 'RECEIPT')),
    raw_content TEXT NOT NULL,
    print_status VARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK (print_status IN ('PENDING', 'PRINTED', 'FAILED')),
    retry_count INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    printed_at TIMESTAMPTZ
);
```
