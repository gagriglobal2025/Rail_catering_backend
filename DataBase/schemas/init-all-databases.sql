-- ============================================================================
-- MASTER DATABASE INITIALIZATION SCRIPT
-- Project: Private Restaurant / Curry Point (RailCat)
-- Consistent 'railcat_' naming convention across all databases and roles
-- ============================================================================

-- 1. Create Dedicated Service Roles with Secure Passwords
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'railcat_users_user') THEN
        CREATE ROLE railcat_users_user WITH LOGIN PASSWORD 'railcat_users_secret_2026';
    END IF;
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'railcat_catalog_user') THEN
        CREATE ROLE railcat_catalog_user WITH LOGIN PASSWORD 'railcat_catalog_secret_2026';
    END IF;
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'railcat_orders_user') THEN
        CREATE ROLE railcat_orders_user WITH LOGIN PASSWORD 'railcat_orders_secret_2026';
    END IF;
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'railcat_settlement_user') THEN
        CREATE ROLE railcat_settlement_user WITH LOGIN PASSWORD 'railcat_settlement_secret_2026';
    END IF;
END $$;

-- 2. Create Isolated Service Logical Databases
CREATE DATABASE railcat_users OWNER railcat_users_user;
CREATE DATABASE railcat_catalog OWNER railcat_catalog_user;
CREATE DATABASE railcat_orders OWNER railcat_orders_user;
CREATE DATABASE railcat_settlement OWNER railcat_settlement_user;

-- 3. Enable Extensions (psql meta-commands)
\c railcat_users
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\c railcat_catalog
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\c railcat_orders
CREATE EXTENSION IF NOT EXISTS pgcrypto;

\c railcat_settlement
CREATE EXTENSION IF NOT EXISTS pgcrypto;

