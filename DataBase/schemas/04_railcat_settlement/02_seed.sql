-- ============================================================================
-- DATABASE: railcat_settlement
-- SEED DATA: Cashier Shift Session, Payment Records & Day Summary
-- ============================================================================

INSERT INTO cashier_shifts (
    shift_id, cashier_user_id, opened_at, starting_cash, cash_collected, upi_collected, total_expected, shift_status
) VALUES (
    '88888888-8888-8888-8888-888888888888',
    '22222222-2222-2222-2222-222222222222',
    CURRENT_DATE + TIME '07:00:00',
    500.00,
    181.00,
    147.00,
    828.00,
    'OPEN'
) ON CONFLICT (shift_id) DO NOTHING;

INSERT INTO payment_transactions (
    payment_id, order_id, shift_id, amount, payment_method, gateway_reference, payment_status, created_at
) VALUES 
    (
        '99999999-9999-9999-9999-999999999991',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        '88888888-8888-8888-8888-888888888888',
        147.00,
        'UPI',
        'UPI-REF-20261107-1001',
        'SUCCESS',
        CURRENT_TIMESTAMP - INTERVAL '14 minutes'
    ),
    (
        '99999999-9999-9999-9999-999999999992',
        'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
        '88888888-8888-8888-8888-888888888888',
        181.00,
        'CASH',
        'CASH-TENDERED',
        'SUCCESS',
        CURRENT_TIMESTAMP - INTERVAL '24 minutes'
    )
ON CONFLICT (payment_id) DO NOTHING;

INSERT INTO daily_sales_summary (
    summary_date, total_orders, gross_sales, concessions_given, tax_collected, net_revenue, upi_revenue, cash_revenue, card_revenue
) VALUES (
    CURRENT_DATE,
    2,
    355.00,
    43.00,
    16.00,
    328.00,
    147.00,
    181.00,
    0.00
) ON CONFLICT (summary_date) DO NOTHING;

