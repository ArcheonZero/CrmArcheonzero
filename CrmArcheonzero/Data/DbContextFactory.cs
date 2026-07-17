using Microsoft.Extensions.Configuration;
using System;

namespace CrmArcheonzero.Data
{
    public static class DbContextFactory
    {
        private static IDbContext? _currentDbContext;
        private static readonly object _lock = new object();

        public static IDbContext GetDbContext()
        {
            if (_currentDbContext != null)
                return _currentDbContext;

            lock (_lock)
            {
                if (_currentDbContext != null)
                    return _currentDbContext;

                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                var provider = config["Database:Provider"] ?? "Sqlite";
                var connectionString = config["Database:ConnectionString"];

                _currentDbContext = provider.ToLower() switch
                {
                    "sqlserver" => new SqlServerDbContext(connectionString),
                    "sqlite" => new SqliteDbContext(connectionString ?? "Data Source=C:\\++Dev\\+WthDS\\crm.db;Mode=ReadWriteCreate;Cache=Shared;"),
                    "inmemory" => new InMemoryDbContext("CrmDb_InMemory"),
                    _ => new SqliteDbContext("Data Source=C:\\++Dev\\+WthDS\\crm.db;Mode=ReadWriteCreate;Cache=Shared;")
                };

                _currentDbContext.EnsureDatabaseCreated();
                _currentDbContext.EnsureSeedData();

                return _currentDbContext;
            }
        }

        public static void SetDbContext(IDbContext context)
        {
            lock (_lock)
            {
                _currentDbContext = context;
            }
        }

        public static void ResetDbContext()
        {
            lock (_lock)
            {
                _currentDbContext = null;
            }
        }
    }
}