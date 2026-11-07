# 06. Performance & Atomic Queue Management

---

## 1. High-Concurrency Queue Sequence Generation

During lunch and rush hours, concurrent orders placed at POS counters and Mobile Web kiosks require guaranteed, non-conflicting sequence numbers.

### Atomic Daily Token Generator Function
```sql
CREATE OR REPLACE FUNCTION generate_daily_queue_token(
    p_op_date DATE DEFAULT CURRENT_DATE
)
RETURNS TABLE (
    token_str VARCHAR(20),
    seq_number INT
) 
LANGUAGE plpgsql
AS $$
DECLARE
    v_seq INT;
    v_prefix VARCHAR(10);
BEGIN
    INSERT INTO daily_queue_sequence (operational_date, current_number, token_prefix, updated_at)
    VALUES (p_op_date, 1, 'A', CURRENT_TIMESTAMP)
    ON CONFLICT (operational_date)
    DO UPDATE SET 
        current_number = daily_queue_sequence.current_number + 1,
        updated_at = CURRENT_TIMESTAMP
    RETURNING daily_queue_sequence.current_number, daily_queue_sequence.token_prefix 
    INTO v_seq, v_prefix;

    token_str := v_prefix || '-' || LPAD(v_seq::text, 3, '0');
    seq_number := v_seq;
    RETURN NEXT;
END;
$$;
```

---

## 2. Indexing Strategy

- **`railcat_orders.orders`**:
  - `idx_orders_status (order_status)`: KDS dashboard filtering for active tickets.
  - `idx_orders_placed_at (placed_at DESC)`: Real-time chronological order feeds.
  - `idx_orders_queue_token (queue_token)`: Quick lookup when customer arrives at the counter.
- **`railcat_catalog.menu_items`**:
  - `idx_menu_items_category (category_id)`: Category tab rendering.
  - `idx_menu_items_available (is_available, is_sold_out)`: Filtering available items.
- **`railcat_settlement.payment_transactions`**:
  - `idx_payment_order_id (order_id)`: Quick order payment verification.
