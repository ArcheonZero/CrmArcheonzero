using CrmArcheonzero.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows;

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
            // Проверяем, существует ли файл БД для SQLite
            if (provider.ToLower() == "sqlite" && !string.IsNullOrEmpty(connectionString))
            {
                var dbPath = connectionString.Replace("Data Source=", "").Split(';')[0];
                if (!File.Exists(dbPath))
                {
                    var result = MessageBox.Show($"База данных по пути {dbPath} не найдена. Создать новую?",
                        "База данных не найдена", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                        return;
                }
            }

            _selectedProvider = provider;
            _selectedConnectionString = connectionString;
            _currentDbContext = null;
        }

        public static IDbContext GetDbContext()
        {
            if (_currentDbContext != null)
                return _currentDbContext;

            LoggerService.LogAction("DbContextFactory", $"Создаём новый контекст для: {_selectedProvider}");

            lock (_lock)
            {
                if (_currentDbContext != null)
                    return _currentDbContext;

                if (string.IsNullOrEmpty(_selectedProvider))
                {
                    // Если провайдер не выбран, читаем из appsettings.json
                    var config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

                    _selectedProvider = config["Database:DefaultProvider"] ?? "Sqlite";
                    _selectedConnectionString = config[$"Database:Providers:{_selectedProvider}:ConnectionString"];
                }

                // Создаём контекст
                _currentDbContext = _selectedProvider.ToLower() switch
                {
                    "postgresql" or "postgres" or "npgsql" or "postgre" => new PostgreDbContext(_selectedConnectionString),
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