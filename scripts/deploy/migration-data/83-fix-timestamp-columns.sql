-- Fix timestamp columns - change from 'timestamp with time zone' to 'timestamp without time zone'
-- This avoids issues with DateTime.Kind in .NET

-- Identity schema
ALTER TABLE "Identity"."AspNetUsers" 
ALTER COLUMN "LockoutEnd" TYPE TIMESTAMP WITHOUT TIME ZONE;

-- Check if any other columns need fixing by looking for timestamptz columns
DO $$
DECLARE
    r RECORD;
    sql_cmd TEXT;
BEGIN
    FOR r IN 
        SELECT table_schema, table_name, column_name
        FROM information_schema.columns
        WHERE data_type = 'timestamp with time zone'
        AND table_schema IN ('Identity', 'Catalog', 'Customer', 'Order', 'Cart', 'Payment', 'Notification', 'Logging')
    LOOP
        sql_cmd := format('ALTER TABLE "%s"."%s" ALTER COLUMN "%s" TYPE TIMESTAMP WITHOUT TIME ZONE',
                          r.table_schema, r.table_name, r.column_name);
        EXECUTE sql_cmd;
        RAISE NOTICE 'Fixed: %.%.%', r.table_schema, r.table_name, r.column_name;
    END LOOP;
END $$;
