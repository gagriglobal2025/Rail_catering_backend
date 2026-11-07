-- ============================================================================
-- DATABASE: railcat_orders
-- SERVICE: Order Processing, Daily Queue Token Generation & Kitchen KDS
-- DIALECT: PostgreSQL 16+
-- ============================================================================

-- Daily Token Generator (Resets sequence daily at 00:00:00 IST)
CREATE TABLE IF NOT EXISTS daily_queue_sequence (
    operational_date DATE PRIMARY KEY,
    current_number INT NOT NULL DEFAULT 0,
    token_prefix VARCHAR(10) NOT NULL DEFAULT 'A',
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS orders (
    order_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number VARCHAR(30) NOT NULL UNIQUE,
    queue_token VARCHAR(20) NOT NULL,
    customer_type VARCHAR(30) NOT NULL DEFAULT 'NORMAL' CHECK (customer_type IN ('NORMAL', 'RAILWAY_EMPLOYEE')),
    customer_ref VARCHAR(50),
    pickup_counter VARCHAR(50) DEFAULT 'Counter 1',
    special_instructions TEXT,
    subtotal_amount NUMERIC(10,2) NOT NULL CHECK (subtotal_amount >= 0),
    concession_discount NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (concession_discount >= 0),
    tax_amount NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (tax_amount >= 0),
    grand_total NUMERIC(10,2) NOT NULL CHECK (grand_total >= 0),
    order_status VARCHAR(30) NOT NULL DEFAULT 'NEW' CHECK (order_status IN ('NEW', 'PREPARING', 'READY', 'DELIVERED', 'CANCELLED')),
    payment_status VARCHAR(30) NOT NULL DEFAULT 'PENDING' CHECK (payment_status IN ('PENDING', 'PAID', 'FAILED')),
    payment_method VARCHAR(20) CHECK (payment_method IN ('UPI', 'CARD', 'CASH')),
    created_by_staff_id UUID,
    placed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid_at TIMESTAMPTZ,
    preparing_at TIMESTAMPTZ,
    ready_at TIMESTAMPTZ,
    delivered_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS order_items (
    order_item_id BIGSERIAL PRIMARY KEY,
    order_id UUID NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
    item_id VARCHAR(50) NOT NULL,
    item_name VARCHAR(100) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price NUMERIC(10,2) NOT NULL CHECK (unit_price >= 0),
    line_total NUMERIC(10,2) NOT NULL CHECK (line_total >= 0)
);

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

CREATE INDEX IF NOT EXISTS idx_orders_status ON orders(order_status);
CREATE INDEX IF NOT EXISTS idx_orders_payment_status ON orders(payment_status);
CREATE INDEX IF NOT EXISTS idx_orders_placed_at ON orders(placed_at DESC);
CREATE INDEX IF NOT EXISTS idx_orders_queue_token ON orders(queue_token);
CREATE INDEX IF NOT EXISTS idx_order_items_order_id ON order_items(order_id);
CREATE INDEX IF NOT EXISTS idx_print_spool_status ON print_spool(print_status);

