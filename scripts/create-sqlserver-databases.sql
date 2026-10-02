-- Creates the SQL Server databases and a login for each, which owns its database.
-- Safe to re-run. Run as sa, e.g.:
--   docker compose -f infra/docker-compose.yml exec -T sqlserver \
--     /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'asyncdemo1!' -C -b < scripts/create-sqlserver-databases.sql

-- CHECK_POLICY = OFF allows the simple demo passwords.

-- CatalogManager
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'catalogmanager_user')
    CREATE LOGIN catalogmanager_user WITH PASSWORD = 'catalogmanager1!', CHECK_POLICY = OFF;
IF DB_ID('CatalogManager') IS NULL
    CREATE DATABASE CatalogManager;
ALTER AUTHORIZATION ON DATABASE::CatalogManager TO catalogmanager_user;
GO

-- Orders
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'orders_user')
    CREATE LOGIN orders_user WITH PASSWORD = 'orders1!', CHECK_POLICY = OFF;
IF DB_ID('Orders') IS NULL
    CREATE DATABASE Orders;
ALTER AUTHORIZATION ON DATABASE::Orders TO orders_user;
GO

-- Profile
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'profile_user')
    CREATE LOGIN profile_user WITH PASSWORD = 'profile1!', CHECK_POLICY = OFF;
IF DB_ID('Profile') IS NULL
    CREATE DATABASE Profile;
ALTER AUTHORIZATION ON DATABASE::Profile TO profile_user;
GO
