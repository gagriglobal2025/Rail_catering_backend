-- ============================================================================
-- DATABASE: railcat_users
-- SEED DATA: Restaurant Staff Accounts (Admin, Counter, Kitchen)
-- ============================================================================

INSERT INTO staff_users (user_id, employee_id, full_name, email, phone_number, system_role, account_status)
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'STAFF-ADMIN', 'Restaurant Manager', 'manager@railcat.local', '+919876543210', 'ADMIN', 'ACTIVE'),
    ('22222222-2222-2222-2222-222222222222', 'STAFF-POS01', 'Counter Cashier 1', 'cashier1@railcat.local', '+919876543211', 'COUNTER_OPERATOR', 'ACTIVE'),
    ('33333333-3333-3333-3333-333333333333', 'STAFF-POS02', 'Counter Cashier 2', 'cashier2@railcat.local', '+919876543212', 'COUNTER_OPERATOR', 'ACTIVE'),
    ('44444444-4444-4444-4444-444444444444', 'STAFF-KIT01', 'Head Chef', 'chef@railcat.local', '+919876543213', 'KITCHEN_STAFF', 'ACTIVE'),
    ('55555555-5555-5555-5555-555555555555', 'STAFF-KIT02', 'Kitchen Assistant', 'kitchen@railcat.local', '+919876543214', 'KITCHEN_STAFF', 'ACTIVE')
ON CONFLICT (employee_id) DO NOTHING;

-- Standard development password hash: 'railcat123'
INSERT INTO staff_credentials (user_id, password_hash)
VALUES
    ('11111111-1111-1111-1111-111111111111', '$2b$12$e8Yk2uUqB5TzC5e6BwZ8ReXF.wF4dF8t9cE2Yd4lK6G3hB0fS7i7q'),
    ('22222222-2222-2222-2222-222222222222', '$2b$12$e8Yk2uUqB5TzC5e6BwZ8ReXF.wF4dF8t9cE2Yd4lK6G3hB0fS7i7q'),
    ('33333333-3333-3333-3333-333333333333', '$2b$12$e8Yk2uUqB5TzC5e6BwZ8ReXF.wF4dF8t9cE2Yd4lK6G3hB0fS7i7q'),
    ('44444444-4444-4444-4444-444444444444', '$2b$12$e8Yk2uUqB5TzC5e6BwZ8ReXF.wF4dF8t9cE2Yd4lK6G3hB0fS7i7q'),
    ('55555555-5555-5555-5555-555555555555', '$2b$12$e8Yk2uUqB5TzC5e6BwZ8ReXF.wF4dF8t9cE2Yd4lK6G3hB0fS7i7q')
ON CONFLICT (user_id) DO NOTHING;

