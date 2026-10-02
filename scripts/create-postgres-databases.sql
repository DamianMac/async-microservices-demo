-- Creates the Postgres databases and a role for each, which owns its database.
-- Safe to re-run. Uses psql's \gexec, so run it through psql as postgres, e.g.:
--   docker compose -f infra/docker-compose.yml exec -T postgres \
--     psql -U postgres -v ON_ERROR_STOP=1 < scripts/create-postgres-databases.sql

-- Catalog
SELECT 'CREATE ROLE catalog_user LOGIN PASSWORD ''catalog1!'''
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'catalog_user')\gexec
SELECT 'CREATE DATABASE catalog OWNER catalog_user'
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'catalog')\gexec
ALTER DATABASE catalog OWNER TO catalog_user;

-- Search
SELECT 'CREATE ROLE search_user LOGIN PASSWORD ''search1!'''
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'search_user')\gexec
SELECT 'CREATE DATABASE search OWNER search_user'
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'search')\gexec
ALTER DATABASE search OWNER TO search_user;

-- Warehouse
SELECT 'CREATE ROLE warehouse_user LOGIN PASSWORD ''warehouse1!'''
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'warehouse_user')\gexec
SELECT 'CREATE DATABASE warehouse OWNER warehouse_user'
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'warehouse')\gexec
ALTER DATABASE warehouse OWNER TO warehouse_user;
