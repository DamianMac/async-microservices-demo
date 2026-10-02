using DbUp;
using DbUp.Engine.Output;
using Microsoft.Data.SqlClient;

namespace Orders.Data.Migration;

// Runs the embedded SQL scripts in this folder, in name order, that haven't been run yet.
public static class DatabaseMigrator
{
    public static void Migrate(string connectionString, Serilog.ILogger logger)
    {
        // Orders runs more than one instance, and they start together. An application lock makes
        // them take turns, so only the first runs the scripts and the rest find nothing to do.
        using var lockConnection = new SqlConnection(connectionString);
        lockConnection.Open();
        using (var getLock = lockConnection.CreateCommand())
        {
            getLock.CommandText = "EXEC sp_getapplock @Resource = 'DbUp', @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 60000";
            // Longer than the lock timeout, so SQL Server gives up waiting before the client does.
            getLock.CommandTimeout = 90;
            getLock.ExecuteNonQuery();
        }

        try
        {
            Upgrade(connectionString, logger);
        }
        finally
        {
            // Released explicitly: closing a pooled connection doesn't end the session, so a
            // session lock would otherwise be held until the pool reuses or drops the connection.
            using var releaseLock = lockConnection.CreateCommand();
            releaseLock.CommandText = "EXEC sp_releaseapplock @Resource = 'DbUp', @LockOwner = 'Session'";
            releaseLock.ExecuteNonQuery();
        }
    }

    private static void Upgrade(string connectionString, Serilog.ILogger logger)
    {
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(
                typeof(DatabaseMigrator).Assembly,
                name => name.StartsWith(typeof(DatabaseMigrator).Namespace + "."))
            .LogTo(new SerilogUpgradeLog(logger))
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
            throw new InvalidOperationException($"Database migration failed on {result.ErrorScript?.Name}", result.Error);
    }

    private class SerilogUpgradeLog(Serilog.ILogger logger) : IUpgradeLog
    {
        public void LogTrace(string format, params object[] args) => logger.Verbose(format, args);
        public void LogDebug(string format, params object[] args) => logger.Debug(format, args);
        public void LogInformation(string format, params object[] args) => logger.Information(format, args);
        public void LogWarning(string format, params object[] args) => logger.Warning(format, args);
        public void LogError(string format, params object[] args) => logger.Error(format, args);
        public void LogError(Exception ex, string format, params object[] args) => logger.Error(ex, format, args);
    }
}
