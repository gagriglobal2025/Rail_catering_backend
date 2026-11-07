-- ============================================================================
-- DATABASE: railcat_settlement
-- SERVICE: Cashier Shifts, Payment Records & Daily Revenue Reporting
-- DIALECT: PostgreSQL 16+
-- ============================================================================

CREATE TABLE IF NOT EXISTS cashier_shifts (
    shift_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cashier_user_id UUID NOT NULL,
    opened_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    closed_at TIMESTAMPTZ,
    starting_cash NUMERIC(10,2) NOT NULL DEFAULT 500.00 CHECK (starting_cash >= 0),
    cash_collected NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (cash_collected >= 0),
    upi_collected NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (upi_collected >= 0),
    card_collected NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (card_collected >= 0),
    total_expected NUMERIC(10,2) NOT NULL DEFAULT 500.00,
    actual_cash_counted NUMERIC(10,2),
    shift_status VARCHAR(20) NOT NULL DEFAULT 'OPEN' CHECK (shift_status IN ('OPEN', 'CLOSED'))
);

CREATE TABLE IF NOT EXISTS payment_transactions (
    payment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    shift_id UUID REFERENCES cashier_shifts(shift_id),
    amount NUMERIC(10,2) NOT NULL CHECK (amount >= 0),
    payment_method VARCHAR(20) NOT NULL CHECK (payment_method IN ('UPI', 'CARD', 'CASH')),
    gateway_reference VARCHAR(100),
    payment_status VARCHAR(20) NOT NULL DEFAULT 'SUCCESS' CHECK (payment_status IN ('SUCCESS', 'FAILED', 'REFUNDED')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS daily_sales_summary (
    summary_date DATE PRIMARY KEY,
    total_orders INT NOT NULL DEFAULT 0,
    gross_sales NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    concessions_given NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    tax_collected NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    net_revenue NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    upi_revenue NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    cash_revenue NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    card_revenue NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_payment_order_id ON payment_transactions(order_id);
CREATE INDEX IF NOT EXISTS idx_payment_created ON payment_transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_cashier_shifts_status ON cashier_shifts(shift_status);

