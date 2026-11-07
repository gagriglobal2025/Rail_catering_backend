-- ============================================================================
-- DATABASE: railcat_orders
-- SEED DATA: Daily Queue Initialization & Sample Active Orders
-- ============================================================================

INSERT INTO daily_queue_sequence (operational_date, current_number, token_prefix)
VALUES (CURRENT_DATE, 3, 'A')
ON CONFLICT (operational_date) DO NOTHING;

-- Sample Order 1: Normal Retail Passenger order at Counter (Paid, in Kitchen)
INSERT INTO orders (
    order_id, order_number, queue_token, customer_type, customer_ref,
    pickup_counter, special_instructions, subtotal_amount, concession_discount,
    tax_amount, grand_total, order_status, payment_status, payment_method,
    placed_at, paid_at, preparing_at
) VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'ORD-20261107-001',
    'A-001',
    'NORMAL',
    '+919876500001',
    'Counter 1',
    'Extra sambar please',
    140.00,
    0.00,
    7.00,
    147.00,
    'PREPARING',
    'PAID',
    'UPI',
    CURRENT_TIMESTAMP - INTERVAL '15 minutes',
    CURRENT_TIMESTAMP - INTERVAL '14 minutes',
    CURRENT_TIMESTAMP - INTERVAL '10 minutes'
) ON CONFLICT (order_id) DO NOTHING;

INSERT INTO order_items (order_id, item_id, item_name, quantity, unit_price, line_total)
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'masala-dosa', 'Ghee Roast Masala Dosa', 1, 80.00, 80.00),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'idli-vada', 'Idli Vada Combo', 1, 60.00, 60.00)
ON CONFLICT DO NOTHING;

-- Sample Order 2: Railway Employee Order with 20% Concession applied (Paid, Ready for Pickup)
INSERT INTO orders (
    order_id, order_number, queue_token, customer_type, customer_ref,
    pickup_counter, special_instructions, subtotal_amount, concession_discount,
    tax_amount, grand_total, order_status, payment_status, payment_method,
    placed_at, paid_at, preparing_at, ready_at
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    'ORD-20261107-002',
    'A-002',
    'RAILWAY_EMPLOYEE',
    'G084',
    'Counter 1',
    'Pack separately',
    215.00,
    43.00,
    9.00,
    181.00,
    'READY',
    'PAID',
    'CASH',
    CURRENT_TIMESTAMP - INTERVAL '25 minutes',
    CURRENT_TIMESTAMP - INTERVAL '24 minutes',
    CURRENT_TIMESTAMP - INTERVAL '18 minutes',
    CURRENT_TIMESTAMP - INTERVAL '5 minutes'
) ON CONFLICT (order_id) DO NOTHING;

INSERT INTO order_items (order_id, item_id, item_name, quantity, unit_price, line_total)
VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'chicken-biryani', 'Dum Chicken Biryani', 1, 190.00, 190.00),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'filter-coffee', 'Degree Filter Coffee', 1, 25.00, 25.00)
ON CONFLICT DO NOTHING;

-- Sample Order 3: Normal Customer Mobile Web Order (Pending Payment at Counter)
INSERT INTO orders (
    order_id, order_number, queue_token, customer_type, customer_ref,
    pickup_counter, special_instructions, subtotal_amount, concession_discount,
    tax_amount, grand_total, order_status, payment_status, payment_method,
    placed_at
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccccccc',
    'ORD-20261107-003',
    'A-003',
    'NORMAL',
    '+919876500002',
    'Counter 2',
    'Less spicy',
    140.00,
    0.00,
    7.00,
    147.00,
    'NEW',
    'PENDING',
    NULL,
    CURRENT_TIMESTAMP - INTERVAL '2 minutes'
) ON CONFLICT (order_id) DO NOTHING;

INSERT INTO order_items (order_id, item_id, item_name, quantity, unit_price, line_total)
VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'veg-meals-thali', 'South Indian Special Thali', 1, 140.00, 140.00)
ON CONFLICT DO NOTHING;

