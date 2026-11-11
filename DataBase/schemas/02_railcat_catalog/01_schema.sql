-- ============================================================================
-- DATABASE: railcat_catalog
-- SERVICE: Menu Catalog, Pricing & Employee Concession Registry
-- DIALECT: PostgreSQL 16+
-- ============================================================================

CREATE TABLE IF NOT EXISTS menu_categories (
    category_id VARCHAR(50) PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL,
    sort_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS menu_items (
    item_id VARCHAR(50) PRIMARY KEY,
    category_id VARCHAR(50) NOT NULL REFERENCES menu_categories(category_id),
    item_name VARCHAR(100) NOT NULL,
    item_description TEXT NOT NULL,
    base_price NUMERIC(10,2) NOT NULL CHECK (base_price >= 0),
    emoji_icon VARCHAR(10) NOT NULL DEFAULT '🍲',
    image_url VARCHAR(500),
    is_vegetarian BOOLEAN NOT NULL DEFAULT true,
    is_available BOOLEAN NOT NULL DEFAULT true,
    is_sold_out BOOLEAN NOT NULL DEFAULT false,
    is_bestseller BOOLEAN NOT NULL DEFAULT false,
    is_spicy BOOLEAN NOT NULL DEFAULT false,
    is_kids_choice BOOLEAN NOT NULL DEFAULT false,
    rating NUMERIC(2,1) DEFAULT 4.5 CHECK (rating >= 1.0 AND rating <= 5.0),
    rating_count INT DEFAULT 0 CHECK (rating_count >= 0),
    prep_time VARCHAR(20) DEFAULT '10-12 mins',
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Differential Pricing Concession Policy (e.g., 20% discount for Railway Employees)
CREATE TABLE IF NOT EXISTS concession_policies (
    policy_id VARCHAR(50) PRIMARY KEY,
    policy_name VARCHAR(100) NOT NULL,
    customer_type VARCHAR(30) NOT NULL DEFAULT 'RAILWAY_EMPLOYEE',
    discount_percentage NUMERIC(5,2) NOT NULL DEFAULT 20.00 CHECK (discount_percentage >= 0 AND discount_percentage <= 100),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Verified Railway Employees Registry for Concession Discount
-- Used by Mobile Web to lock 1 device per employee, and by Admin /employees to verify UMID
CREATE TABLE IF NOT EXISTS railway_concession_employees (
    employee_id VARCHAR(30) PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    verification_status VARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK (verification_status IN ('ACTIVE', 'PENDING', 'REJECTED')),
    device_name VARCHAR(100),
    device_token VARCHAR(100),
    device_locked BOOLEAN NOT NULL DEFAULT false,
    id_card_url TEXT,
    rejection_reason TEXT,
    registered_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMPTZ
);

-- Railway Stations / Pickup Locations served by RailCat catering counters
CREATE TABLE IF NOT EXISTS stations (
    station_id VARCHAR(20) PRIMARY KEY,
    station_name VARCHAR(150) NOT NULL,
    station_code VARCHAR(20) NOT NULL,
    platform VARCHAR(50),
    address VARCHAR(255),
    geofence_radius_m INT NOT NULL DEFAULT 250,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Promotional / Partner Ad Banners shown on the Customer Mobile Web home screen
CREATE TABLE IF NOT EXISTS promotional_ads (
    ad_id VARCHAR(50) PRIMARY KEY,
    tag VARCHAR(50) NOT NULL DEFAULT 'SPONSORED',
    sponsor VARCHAR(100) NOT NULL,
    title VARCHAR(150) NOT NULL,
    subtitle VARCHAR(255),
    cta_text VARCHAR(50) NOT NULL DEFAULT 'Learn More',
    href VARCHAR(500),
    badge_color VARCHAR(20) NOT NULL DEFAULT '#ea580c',
    bg_gradient VARCHAR(255),
    image_url TEXT,
    sort_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_menu_items_category ON menu_items(category_id);
CREATE INDEX IF NOT EXISTS idx_menu_items_available ON menu_items(is_available, is_sold_out);
CREATE INDEX IF NOT EXISTS idx_concession_emp_status ON railway_concession_employees(verification_status);
CREATE INDEX IF NOT EXISTS idx_concession_emp_device ON railway_concession_employees(device_token);
CREATE INDEX IF NOT EXISTS idx_stations_active ON stations(is_active);
CREATE INDEX IF NOT EXISTS idx_ads_active_sort ON promotional_ads(is_active, sort_order);

