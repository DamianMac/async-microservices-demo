-- Deletes all the data from the SQL Server databases, leaving the schemas in place.
-- Empties every user table except DbUp's journal (dbo.SchemaVersions), so
-- migrations don't re-run, and reseeds identity columns. Run as sa, e.g.:
--   docker compose -f infra/docker-compose.yml exec -T sqlserver \
--     /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'asyncdemo1!' -C -b < scripts/clear-sqlserver-data.sql

:setvar ClearTables "SET NOCOUNT ON; DECLARE @sql nvarchar(max) = N''; SELECT @sql += N'ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' NOCHECK CONSTRAINT ALL; ' FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE t.is_ms_shipped = 0 AND t.name <> N'SchemaVersions'; SELECT @sql += N'DELETE FROM ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N'; ' FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE t.is_ms_shipped = 0 AND t.name <> N'SchemaVersions'; SELECT @sql += N'DBCC CHECKIDENT (''' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N''', RESEED, 0) WITH NO_INFOMSGS; ' FROM sys.identity_columns ic JOIN sys.tables t ON t.object_id = ic.object_id JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE t.is_ms_shipped = 0 AND t.name <> N'SchemaVersions' AND ic.last_value IS NOT NULL; SELECT @sql += N'ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' WITH CHECK CHECK CONSTRAINT ALL; ' FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE t.is_ms_shipped = 0 AND t.name <> N'SchemaVersions'; EXEC sp_executesql @sql; PRINT DB_NAME() + N': cleared';"

USE CatalogManager;
$(ClearTables)
GO

USE Orders;
$(ClearTables)
GO

USE Profile;
$(ClearTables)
GO
