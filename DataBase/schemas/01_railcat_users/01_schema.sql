-- ============================================================================
-- DATABASE: railcat_users
-- SERVICE: Restaurant Staff Identity & Access Management
-- DIALECT: PostgreSQL 16+
-- ============================================================================

CREATE TABLE IF NOT EXISTS staff_users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id VARCHAR(30) NOT NULL UNIQUE,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    phone_number VARCHAR(20),
    station_id VARCHAR(30) NOT NULL DEFAULT 'SC',
    system_role VARCHAR(30) NOT NULL CHECK (system_role IN ('ADMIN', 'COUNTER_OPERATOR', 'KITCHEN_STAFF')),
    account_status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE' CHECK (account_status IN ('ACTIVE', 'INACTIVE')),
    last_active_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS staff_credentials (
    user_id UUID PRIMARY KEY REFERENCES staff_users(user_id) ON DELETE CASCADE,
    password_hash VARCHAR(255) NOT NULL,
    failed_logins INT NOT NULL DEFAULT 0,
    locked_until TIMESTAMPTZ,
    password_updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS staff_sessions (
    session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES staff_users(user_id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL,
    ip_address VARCHAR(45),
    user_agent VARCHAR(255),
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_staff_users_role ON staff_users(system_role);
CREATE INDEX IF NOT EXISTS idx_staff_users_status ON staff_users(account_status);
CREATE INDEX IF NOT EXISTS idx_staff_sessions_user_expires ON staff_sessions(user_id, expires_at);

