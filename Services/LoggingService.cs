using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для логирования действий и событий системы
    /// </summary>
    public class LoggingService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _logDirectory;

        public LoggingService(ApplicationDbContext context)
        {
            _context = context;
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "Logs");

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        /// <summary>
        /// Записать действие пользователя
        /// </summary>
        public async Task LogUserActivityAsync(int userId, string action, string? entityType = null, int? entityId = null, string? details = null)
        {
            try
            {
                var activity = new UserActivity
                {
                    UserId = userId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Description = details,
                    Timestamp = DateTime.UtcNow
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка записи активности пользователя: {ex.Message}");
            }
        }

        /// <summary>
        /// Записать системное событие в файл
        /// </summary>
        public void LogSystemEvent(string level, string message, Exception? exception = null)
        {
            try
            {
                var logFile = Path.Combine(_logDirectory, $"system_{DateTime.Now:yyyyMMdd}.log");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

                if (exception != null)
                {
                    logEntry += $"\n{exception}";
                }

                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки логирования, чтобы не прерывать работу приложения
            }
        }

        /// <summary>
        /// Записать информацию в лог
        /// </summary>
        public void LogInfo(string message) => LogSystemEvent("INFO", message);

        /// <summary>
        /// Записать предупреждение в лог
        /// </summary>
        public void LogWarning(string message) => LogSystemEvent("WARNING", message);

        /// <summary>
        /// Записать ошибку в лог
        /// </summary>
        public void LogError(string message, Exception? exception = null) => LogSystemEvent("ERROR", message, exception);

        /// <summary>
        /// Получить логи за период
        /// </summary>
        public List<string> GetLogs(DateTime startDate, DateTime endDate)
        {
            var logs = new List<string>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var logFile = Path.Combine(_logDirectory, $"system_{currentDate:yyyyMMdd}.log");
                if (File.Exists(logFile))
                {
                    var lines = File.ReadAllLines(logFile);
                    foreach (var line in lines)
                    {
                        // Парсим дату из строки лога
                        if (line.Length > 20 && DateTime.TryParse(line.Substring(1, 19), out var logDate))
                        {
                            if (logDate >= startDate && logDate <= endDate)
                                logs.Add(line);
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return logs;
        }

        /// <summary>
        /// Очистить старые логи
        /// </summary>
        public void CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var files = Directory.GetFiles(_logDirectory, "*.log");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
    }
}
