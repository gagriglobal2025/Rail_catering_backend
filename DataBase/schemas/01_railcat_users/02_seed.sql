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

-- Standard password hash: '123456'
INSERT INTO staff_credentials (user_id, password_hash)
VALUES
    ('11111111-1111-1111-1111-111111111111', '1zgifFGSf/3hD+maCaOmAvblMfW+F0JQbHw2PiS9Hx8='),
    ('22222222-2222-2222-2222-222222222222', '1zgifFGSf/3hD+maCaOmAvblMfW+F0JQbHw2PiS9Hx8='),
    ('33333333-3333-3333-3333-333333333333', '1zgifFGSf/3hD+maCaOmAvblMfW+F0JQbHw2PiS9Hx8='),
    ('44444444-4444-4444-4444-444444444444', '1zgifFGSf/3hD+maCaOmAvblMfW+F0JQbHw2PiS9Hx8='),
    ('55555555-5555-5555-5555-555555555555', '1zgifFGSf/3hD+maCaOmAvblMfW+F0JQbHw2PiS9Hx8=')
ON CONFLICT (user_id) DO UPDATE SET password_hash = EXCLUDED.password_hash;

