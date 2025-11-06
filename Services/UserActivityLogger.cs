using System.IO;
using System.Text;
using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    public static class UserActivityLogger
    {
        private static readonly string LogDirectory = "Logs";
        private static readonly string LogFile = Path.Combine(LogDirectory, $"activity_{DateTime.Now:yyyyMMdd}.log");

        static UserActivityLogger()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }

        /// <summary>
        /// Логирование действия пользователя (в БД и файл)
        /// </summary>
        public static async Task LogAsync(string action, string? description = null, string? entityType = null, int? entityId = null, string? ipAddress = null)
        {
            try
            {
                var userId = Program.CurrentUser?.Id ?? 0;
                if (userId == 0) return;

                // Сохранение в БД
                if (ConnectionService.IsMySqlOnline())
                {
                    try
                    {
                        var activity = new UserActivity
                        {
                            UserId = userId,
                            Action = action,
                            Description = description,
                            EntityType = entityType,
                            EntityId = entityId,
                            IpAddress = ipAddress ?? GetLocalIpAddress(),
                            Timestamp = DateTime.UtcNow
                        };

                        Program.DbContext.UserActivities.Add(activity);
                        await Program.DbContext.SaveChangesAsync();
                    }
                    catch
                    {
                        // Если не удалось сохранить в БД, сохраняем в файл
                    }
                }

                // Сохранение в файл (для резервного копирования)
                var user = Program.CurrentUser?.FullName ?? "Unknown";
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] User: {user} | Action: {action}";
                if (!string.IsNullOrEmpty(description))
                    logEntry += $" | Details: {description}";
                if (!string.IsNullOrEmpty(entityType) && entityId.HasValue)
                    logEntry += $" | {entityType}: {entityId}";
                logEntry += Environment.NewLine;

                File.AppendAllText(LogFile, logEntry, Encoding.UTF8);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        /// <summary>
        /// Синхронное логирование (для обратной совместимости)
        /// </summary>
        public static void Log(string action, string details = "")
        {
            LogAsync(action, details).Wait();
        }

        public static void LogLogin(string username, bool success)
        {
            var action = success ? "Login" : "LoginFailed";
            LogAsync(action, $"Username: {username}, Success: {success}").Wait();
        }

        public static void LogDeviceAction(string action, int deviceId, string deviceName)
        {
            LogAsync($"Device{action}", $"Device: {deviceName}", "Device", deviceId).Wait();
        }

        public static void LogRequestAction(string action, int requestId)
        {
            LogAsync($"Request{action}", $"Request ID: {requestId}", "Request", requestId).Wait();
        }

        private static string? GetLocalIpAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
                }
            }
            catch { }
            return null;
        }
    }
}

