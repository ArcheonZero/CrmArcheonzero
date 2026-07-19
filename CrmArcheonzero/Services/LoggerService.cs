using System;
using System.IO;

namespace CrmArcheonzero.Services
{
    public static class LoggerService
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static readonly object _lock = new object();

        public static void LogError(Exception ex, string context = "")
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                var logFile = Path.Combine(LogDirectory, $"crm_log_{DateTime.UtcNow:yyyy-MM-dd}.txt");
                var entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ERROR: {context} | {ex.Message}\n{ex.StackTrace}\n";

                lock (_lock)
                {
                    File.AppendAllText(logFile, entry + "\n");
                }
            }
            catch { }
        }

        public static void LogInfo(string message)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                var logFile = Path.Combine(LogDirectory, $"crm_log_{DateTime.UtcNow:yyyy-MM-dd}.txt");
                var entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n";

                lock (_lock)
                {
                    File.AppendAllText(logFile, entry);
                }
            }
            catch { }
        }

        // ===== НОВЫЙ МЕТОД — ЛОГИРОВАНИЕ ДЕЙСТВИЙ =====
        public static void LogAction(string action, string details)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                var logFile = Path.Combine(LogDirectory, $"crm_log_{DateTime.UtcNow:yyyy-MM-dd}.txt");
                var entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ACTION: {action} | {details}\n";

                lock (_lock)
                {
                    File.AppendAllText(logFile, entry);
                }
            }
            catch { }
        }

        public static void CleanOldLogs(int daysToKeep = 30)
        {
            try
            {
                if (!Directory.Exists(LogDirectory)) return;
                var files = Directory.GetFiles(LogDirectory, "crm_log_*.txt");
                var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
                foreach (var file in files)
                {
                    if (File.GetCreationTime(file) < cutoff)
                        File.Delete(file);
                }
            }
            catch { }
        }
    }
}