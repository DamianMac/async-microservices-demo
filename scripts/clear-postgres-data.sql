-- Deletes all the data from the Postgres databases, leaving the schemas in place.
-- Empties every table in the public schema except DbUp's journal (schemaversions),
-- so migrations don't re-run. Run through psql as postgres, e.g.:
--   docker compose -f infra/docker-compose.yml exec -T postgres \
--     psql -U postgres -v ON_ERROR_STOP=1 < scripts/clear-postgres-data.sql

\set clear_tables 'DO $$ DECLARE tables text; BEGIN SELECT string_agg(format(''%I.%I'', schemaname, tablename), '', '') INTO tables FROM pg_tables WHERE schemaname = ''public'' AND tablename <> ''schemaversions''; IF tables IS NOT NULL THEN EXECUTE ''TRUNCATE TABLE '' || tables || '' RESTART IDENTITY CASCADE''; RAISE NOTICE ''%: cleared %'', current_database(), tables; END IF; END $$;'

\connect catalog
:clear_tables

\connect search
:clear_tables

\connect warehouse
:clear_tables
