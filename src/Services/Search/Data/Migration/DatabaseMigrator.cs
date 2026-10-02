using DbUp;
using DbUp.Engine.Output;

namespace Search.Data.Migration;

// Runs the embedded SQL scripts in this folder, in name order, that haven't been run yet.
public static class DatabaseMigrator
{
    public static void Migrate(string connectionString, Serilog.ILogger logger)
    {
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
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
