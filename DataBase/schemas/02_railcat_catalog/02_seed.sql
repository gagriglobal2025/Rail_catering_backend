-- ============================================================================
-- DATABASE: railcat_catalog
-- SEED DATA: Menu Categories, Dishes, Concession Policy & Employees
-- ============================================================================

INSERT INTO menu_categories (category_id, category_name, sort_order)
VALUES
    ('breakfast', 'Breakfast', 1),
    ('lunch', 'Lunch & Meals', 2),
    ('snacks', 'Evening Snacks', 3),
    ('beverages', 'Hot & Cold Beverages', 4)
ON CONFLICT (category_id) DO NOTHING;

INSERT INTO menu_items (item_id, category_id, item_name, item_description, base_price, emoji_icon, image_url, is_vegetarian, is_bestseller, is_spicy, is_kids_choice, rating, rating_count, prep_time)
VALUES
    ('idli-vada', 'breakfast', 'Idli Vada Combo', '2 Steamed rice cakes & 1 crispy medu vada served with fresh coconut chutney and piping hot sambar.', 60.00, '🥟', 'https://images.unsplash.com/photo-1589301760014-d929f3979dbc?w=500', true, true, false, true, 4.8, 120, '5-7 mins'),
    ('masala-dosa', 'breakfast', 'Ghee Roast Masala Dosa', 'Crispy fermented crepe roasted in desi ghee, filled with spiced potato masala.', 80.00, '🥞', 'https://images.unsplash.com/photo-1668236543090-82eba5ee5976?w=500', true, true, false, false, 4.9, 210, '8-10 mins'),
    ('puri-bhaji', 'breakfast', 'Puri Bhaji (3 Pcs)', 'Three puffed whole-wheat puris served with mildly spiced potato masala curry.', 70.00, '🍘', 'https://images.unsplash.com/photo-1601050690597-df0568f70950?w=500', true, false, false, true, 4.4, 65, '8-10 mins'),
    ('veg-meals-thali', 'lunch', 'South Indian Special Thali', 'Rice, sambar, rasam, curd, 2 seasonal vegetable curries, appalam, sweet, and pickle.', 140.00, '🍱', 'https://images.unsplash.com/photo-1610057099443-fde8c4d50f91?w=500', true, true, false, false, 4.7, 185, '10-12 mins'),
    ('sambar-rice', 'lunch', 'Hot Ghee Sambar Rice', 'Comforting aromatic sambar rice tempered with mustard, curry leaves, and generous desi ghee.', 90.00, '🍛', 'https://images.unsplash.com/photo-1546833999-b9f581a1996d?w=500', true, false, false, true, 4.6, 92, '5-7 mins'),
    ('curd-rice', 'lunch', 'Tadka Curd Rice', 'Creamy curd rice tempered with ginger, green chillies, mustard seeds, and pomegranate.', 75.00, '🍚', 'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=500', true, false, false, true, 4.5, 78, '5 mins'),
    ('chicken-biryani', 'lunch', 'Dum Chicken Biryani', 'Hyderabadi dum cooked chicken biryani with long grain basmati rice, served with mirchi ka salan and raita.', 190.00, '🍗', 'https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=500', false, true, true, false, 4.8, 340, '12-15 mins'),
    ('egg-curry', 'lunch', 'Andhra Style Egg Curry with Rice', 'Two boiled eggs simmered in spicy onion-tomato gravy, served with steamed rice.', 130.00, '🥚', 'https://images.unsplash.com/photo-1589302168068-964664d93dc0?w=500', false, false, true, false, 4.4, 52, '10-12 mins'),
    ('mutton-biryani', 'lunch', 'Special Mutton Biryani', 'Tender mutton cooked in authentic spices and layered with fragrant saffron rice.', 260.00, '🍖', 'https://images.unsplash.com/photo-1544025162-d76694265947?w=500', false, true, true, false, 4.9, 115, '15 mins'),
    ('samosa-plate', 'snacks', 'Crispy Punjabi Samosa (2 Pcs)', 'Crisp golden pastry stuffed with spiced potatoes and green peas, with mint and tamarind chutneys.', 40.00, '🥟', 'https://images.unsplash.com/photo-1601050690597-df0568f70950?w=500', true, true, true, false, 4.5, 140, '5 mins'),
    ('mirchi-bajji', 'snacks', 'Cut Mirchi Bajji (4 Pcs)', 'Double fried battered banana peppers dusted with spicy chaat masala and onions.', 50.00, '🌶️', 'https://images.unsplash.com/photo-1606491956689-2ea866880c84?w=500', true, false, true, false, 4.6, 88, '6-8 mins'),
    ('filter-coffee', 'beverages', 'Degree Filter Coffee', 'Authentic South Indian chicory-blend coffee brewed in brass dabarah and tumbler.', 25.00, '☕', 'https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=500', true, true, false, false, 4.9, 410, '3 mins'),
    ('masala-chai', 'beverages', 'Adrak Elaichi Chai', 'Slow-boiled whole milk tea infused with crushed ginger and aromatic green cardamom.', 20.00, '🍵', 'https://images.unsplash.com/photo-1576092768241-dec231879fc3?w=500', true, true, false, false, 4.8, 290, '3 mins')
ON CONFLICT (item_id) DO NOTHING;

-- Concession Policy: 20% Discount for verified Railway Staff
INSERT INTO concession_policies (policy_id, policy_name, customer_type, discount_percentage, is_active)
VALUES
    ('POL-RLY-20', 'Railway Staff 20% Concession', 'RAILWAY_EMPLOYEE', 20.00, true)
ON CONFLICT (policy_id) DO NOTHING;

-- Pre-registered Railway Employees with Device Binding (for differential pricing verification)
INSERT INTO railway_concession_employees (employee_id, full_name, email, phone, department, designation, shift_policy, verification_status, device_name, device_token, device_locked, id_card_url)
VALUES
    ('G084', 'Challa Narayana Rao', 'challa.n@railcat.local', '+919440123456', 'Running Staff', 'Senior Loco Pilot (Mail/Express)', 'Night Shift Roster', 'ACTIVE', 'Redmi 24094RAD4I', 'dvt_redmi_24094rad4i_88f9c2', true, 'https://images.unsplash.com/photo-1633332755192-727a05c4013d?w=400'),
    ('SCR-1042', 'Venkata Ramanamurthy', 'v.ramana@railcat.local', '+919848011223', 'Commercial', 'Senior Ticket Examiner (TTE)', 'General Shift', 'ACTIVE', 'Samsung Galaxy M34', 'dvt_samsung_m34_a1b2c3', true, 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=400'),
    ('SCR-2091', 'P. Satyanarayana', 'p.satya@railcat.local', '+919848099887', 'Operating', 'Station Master Grade II', 'Rotational', 'ACTIVE', 'OnePlus Nord CE 3', 'dvt_oneplus_nord_99d4e1', true, 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=400'),
    ('SCR-3388', 'B. Koteswara Rao', 'kotesh.b@railcat.local', '+919701234890', 'Mechanical', 'Senior Section Engineer', 'General Shift', 'PENDING', 'Vivo T2 Pro', 'dvt_vivo_t2p_77a4b1', false, 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400'),
    ('SCR-4512', 'M. Srinivasa Reddy', 'ms.reddy@railcat.local', '+919618055443', 'Electrical', 'Loco Pilot (Goods)', 'Running Duty', 'PENDING', NULL, NULL, false, 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=400')
ON CONFLICT (employee_id) DO NOTHING;

