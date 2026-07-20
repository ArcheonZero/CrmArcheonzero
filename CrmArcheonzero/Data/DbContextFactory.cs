using Microsoft.Extensions.Configuration;
using System;

namespace CrmArcheonzero.Data
{
    public static class DbContextFactory
    {
        private static IDbContext? _currentDbContext;
        private static readonly object _lock = new object();

        private static string _selectedProvider;
        private static string _selectedConnectionString;

        public static void SetProvider(string provider, string connectionString)
        {
            _selectedProvider = provider;
            _selectedConnectionString = connectionString;
            _currentDbContext = null; // сбросить текущий контекст
        }

        public static IDbContext GetDbContext()
        {
            if (_currentDbContext != null)
                return _currentDbContext;

            lock (_lock)
            {
                if (_currentDbContext != null)
                    return _currentDbContext;

                // Если провайдер ещё не выбран — берём из appsettings.json
                if (string.IsNullOrEmpty(_selectedProvider))
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();

                    _selectedProvider = config["Database:Provider"] ?? "Sqlite";
                    _selectedConnectionString = config["Database:ConnectionString"] ?? "Data Source=H:\\++MyDir++\\++Dev\\+WthDS\\crm.db;Mode=ReadWriteCreate;Cache=Shared;";
                }

                _currentDbContext = _selectedProvider.ToLower() switch
                {
                    "postgresql" or "postgres" or "npgsql" => new PostgreDbContext(_selectedConnectionString),
                    "sqlserver" => new SqlServerDbContext(_selectedConnectionString),
                    _ => new SqliteDbContext(_selectedConnectionString)
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