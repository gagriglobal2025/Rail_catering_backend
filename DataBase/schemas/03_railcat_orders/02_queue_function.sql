-- ============================================================================
-- DATABASE: railcat_orders
-- FUNCTION: Atomic Daily Queue Token Generator
-- Generates queue numbers like 'A-001', 'A-002' that reset at midnight
-- ============================================================================

CREATE OR REPLACE FUNCTION generate_daily_queue_token(
    p_op_date DATE DEFAULT CURRENT_DATE
)
RETURNS TABLE (
    token_str VARCHAR(20),
    seq_number INT
) 
LANGUAGE plpgsql
AS $$
DECLARE
    v_seq INT;
    v_prefix VARCHAR(10);
BEGIN
    INSERT INTO daily_queue_sequence (operational_date, current_number, token_prefix, updated_at)
    VALUES (p_op_date, 1, 'A', CURRENT_TIMESTAMP)
    ON CONFLICT (operational_date)
    DO UPDATE SET 
        current_number = daily_queue_sequence.current_number + 1,
        updated_at = CURRENT_TIMESTAMP
    RETURNING daily_queue_sequence.current_number, daily_queue_sequence.token_prefix 
    INTO v_seq, v_prefix;

    token_str := v_prefix || '-' || LPAD(v_seq::text, 3, '0');
    seq_number := v_seq;
    RETURN NEXT;
END;
$$;

