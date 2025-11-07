using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для получения статистики по системе
    /// </summary>
    public class StatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить общую статистику системы
        /// </summary>
        public async Task<SystemStatistics> GetSystemStatisticsAsync()
        {
            var devices = await _context.NetworkDevices.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var requests = await _context.MaintenanceRequests.ToListAsync();
            var activities = await _context.UserActivities.ToListAsync();

            return new SystemStatistics
            {
                TotalDevices = devices.Count,
                OnlineDevices = devices.Count(d => d.Status == DeviceStatus.Online),
                OfflineDevices = devices.Count(d => d.Status == DeviceStatus.Offline),
                DegradedDevices = devices.Count(d => d.Status == DeviceStatus.Degraded),
                
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                AdminUsers = users.Count(u => u.Role == UserRole.Admin),
                
                TotalRequests = requests.Count,
                PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending),
                CompletedRequests = requests.Count(r => r.Status == RequestStatus.Completed),
                
                TotalActivities = activities.Count,
                ActivitiesLast24Hours = activities.Count(a => a.Timestamp >= DateTime.UtcNow.AddHours(-24)),
                ActivitiesLast7Days = activities.Count(a => a.Timestamp >= DateTime.UtcNow.AddDays(-7)),
                
                // Статистика по типам устройств
                DevicesByType = devices
                    .GroupBy(d => d.DeviceType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                // Статистика по зданиям
                DevicesByBuilding = devices
                    .Where(d => !string.IsNullOrEmpty(d.Building))
                    .GroupBy(d => d.Building!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Последние обновления
                LastDeviceUpdate = devices.Max(d => d.UpdatedAt),
                LastRequestUpdate = requests.Any() ? requests.Max(r => r.UpdatedAt ?? r.CreatedAt) : null
            };
        }

        /// <summary>
        /// Получить статистику по устройствам
        /// </summary>
        public async Task<DeviceStatistics> GetDeviceStatisticsAsync()
        {
            var devices = await _context.NetworkDevices.ToListAsync();
            var statusHistory = await _context.DeviceStatusHistory
                .Where(h => h.Timestamp >= DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            return new DeviceStatistics
            {
                Total = devices.Count,
                ByStatus = devices
                    .GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                ByType = devices
                    .GroupBy(d => d.DeviceType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                ByLocation = devices
                    .Where(d => !string.IsNullOrEmpty(d.Location))
                    .GroupBy(d => d.Location!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                ByBuilding = devices
                    .Where(d => !string.IsNullOrEmpty(d.Building))
                    .GroupBy(d => d.Building!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                OnlinePercentage = devices.Count > 0 
                    ? (double)devices.Count(d => d.Status == DeviceStatus.Online) / devices.Count * 100 
                    : 0,
                
                StatusChangesLast7Days = statusHistory.Count,
                RecentOfflineEvents = statusHistory.Count(h => h.Status == DeviceStatus.Offline)
            };
        }

        /// <summary>
        /// Получить статистику по заявкам
        /// </summary>
        public async Task<MaintenanceStatistics> GetMaintenanceStatisticsAsync()
        {
            var requests = await _context.MaintenanceRequests
                .Include(r => r.Device)
                .Include(r => r.User)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return new MaintenanceStatistics
            {
                Total = requests.Count,
                ByStatus = requests
                    .GroupBy(r => r.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                ByPriority = requests
                    .GroupBy(r => r.Priority)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                Pending = requests.Count(r => r.Status == RequestStatus.Pending),
                InProgress = requests.Count(r => r.Status == RequestStatus.InProgress),
                Completed = requests.Count(r => r.Status == RequestStatus.Completed),
                Cancelled = requests.Count(r => r.Status == RequestStatus.Cancelled),
                
                CreatedToday = requests.Count(r => r.CreatedAt.Date == now.Date),
                CreatedThisWeek = requests.Count(r => r.CreatedAt >= now.AddDays(-7)),
                CreatedThisMonth = requests.Count(r => r.CreatedAt >= now.AddDays(-30)),
                
                AverageResolutionTime = CalculateAverageResolutionTime(requests),
                LongestPendingRequest = requests
                    .Where(r => r.Status == RequestStatus.Pending)
                    .OrderBy(r => r.CreatedAt)
                    .FirstOrDefault()
            };
        }

        /// <summary>
        /// Получить статистику по пользователям
        /// </summary>
        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var activities = await _context.UserActivities
                .Where(a => a.Timestamp >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            return new UserStatistics
            {
                Total = users.Count,
                Active = users.Count(u => u.IsActive),
                Inactive = users.Count(u => !u.IsActive),
                Admins = users.Count(u => u.Role == UserRole.Admin),
                RegularUsers = users.Count(u => u.Role == UserRole.User),
                
                CreatedThisMonth = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                LastLoginToday = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value.Date == DateTime.UtcNow.Date),
                LastLoginThisWeek = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value >= DateTime.UtcNow.AddDays(-7)),
                
                TotalActivities = activities.Count,
                ActivitiesByUser = activities
                    .GroupBy(a => a.UserId)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                MostActiveUsers = activities
                    .GroupBy(a => a.UserId)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => (dynamic)new { UserId = g.Key, Count = g.Count() })
                    .ToList()
            };
        }

        /// <summary>
        /// Получить статистику по времени работы системы
        /// </summary>
        public async Task<UptimeStatistics> GetUptimeStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statusHistory = await _context.DeviceStatusHistory
                .Where(h => h.Timestamp >= start && h.Timestamp <= end)
                .ToListAsync();

            var devices = await _context.NetworkDevices.ToListAsync();

            var uptimeStats = new UptimeStatistics
            {
                PeriodStart = start,
                PeriodEnd = end,
                TotalDevices = devices.Count
            };

            foreach (var device in devices)
            {
                var deviceHistory = statusHistory
                    .Where(h => h.DeviceId == device.Id)
                    .OrderBy(h => h.Timestamp)
                    .ToList();

                if (deviceHistory.Any())
                {
                    var totalMinutes = (end - start).TotalMinutes;
                    var onlineMinutes = CalculateOnlineMinutes(deviceHistory, start, end);
                    var uptime = totalMinutes > 0 ? (onlineMinutes / totalMinutes) * 100 : 0;

                    uptimeStats.DeviceUptimes[device.Id] = uptime;
                    uptimeStats.TotalUptime += uptime;
                }
                else
                {
                    uptimeStats.DeviceUptimes[device.Id] = 0;
                }
            }

            uptimeStats.AverageUptime = devices.Count > 0 ? uptimeStats.TotalUptime / devices.Count : 0;

            return uptimeStats;
        }

        /// <summary>
        /// Рассчитать количество минут онлайн для устройства
        /// </summary>
        private double CalculateOnlineMinutes(List<DeviceStatusHistory> history, DateTime start, DateTime end)
        {
            if (!history.Any())
                return 0;

            double onlineMinutes = 0;
            var currentTime = start;
            var currentStatus = DeviceStatus.Unknown;

            foreach (var event_ in history.OrderBy(h => h.Timestamp))
            {
                if (currentStatus == DeviceStatus.Online)
                {
                    onlineMinutes += (event_.Timestamp - currentTime).TotalMinutes;
                }
                currentTime = event_.Timestamp;
                currentStatus = event_.Status;
            }

            // Если последний статус был Online, добавляем время до конца периода
            if (currentStatus == DeviceStatus.Online && currentTime < end)
            {
                onlineMinutes += (end - currentTime).TotalMinutes;
            }

            return onlineMinutes;
        }

        /// <summary>
        /// Рассчитать среднее время решения заявки
        /// </summary>
        private TimeSpan? CalculateAverageResolutionTime(List<MaintenanceRequest> requests)
        {
            var completed = requests
                .Where(r => r.Status == RequestStatus.Completed && r.CompletedAt.HasValue)
                .ToList();

            if (!completed.Any())
                return null;

            var totalMinutes = completed
                .Select(r => (r.CompletedAt!.Value - r.CreatedAt).TotalMinutes)
                .Average();

            return TimeSpan.FromMinutes(totalMinutes);
        }

        // Классы для результатов статистики
        public class SystemStatistics
        {
            public int TotalDevices { get; set; }
            public int OnlineDevices { get; set; }
            public int OfflineDevices { get; set; }
            public int DegradedDevices { get; set; }
            public int TotalUsers { get; set; }
            public int ActiveUsers { get; set; }
            public int AdminUsers { get; set; }
            public int TotalRequests { get; set; }
            public int PendingRequests { get; set; }
            public int CompletedRequests { get; set; }
            public int TotalActivities { get; set; }
            public int ActivitiesLast24Hours { get; set; }
            public int ActivitiesLast7Days { get; set; }
            public Dictionary<string, int> DevicesByType { get; set; } = new();
            public Dictionary<string, int> DevicesByBuilding { get; set; } = new();
            public DateTime LastDeviceUpdate { get; set; }
            public DateTime? LastRequestUpdate { get; set; }
        }

        public class DeviceStatistics
        {
            public int Total { get; set; }
            public Dictionary<string, int> ByStatus { get; set; } = new();
            public Dictionary<string, int> ByType { get; set; } = new();
            public Dictionary<string, int> ByLocation { get; set; } = new();
            public Dictionary<string, int> ByBuilding { get; set; } = new();
            public double OnlinePercentage { get; set; }
            public int StatusChangesLast7Days { get; set; }
            public int RecentOfflineEvents { get; set; }
        }

        public class MaintenanceStatistics
        {
            public int Total { get; set; }
            public Dictionary<string, int> ByStatus { get; set; } = new();
            public Dictionary<string, int> ByPriority { get; set; } = new();
            public int Pending { get; set; }
            public int InProgress { get; set; }
            public int Completed { get; set; }
            public int Cancelled { get; set; }
            public int CreatedToday { get; set; }
            public int CreatedThisWeek { get; set; }
            public int CreatedThisMonth { get; set; }
            public TimeSpan? AverageResolutionTime { get; set; }
            public MaintenanceRequest? LongestPendingRequest { get; set; }
        }

        public class UserStatistics
        {
            public int Total { get; set; }
            public int Active { get; set; }
            public int Inactive { get; set; }
            public int Admins { get; set; }
            public int RegularUsers { get; set; }
            public int CreatedThisMonth { get; set; }
            public int LastLoginToday { get; set; }
            public int LastLoginThisWeek { get; set; }
            public int TotalActivities { get; set; }
            public Dictionary<int, int> ActivitiesByUser { get; set; } = new();
            public List<dynamic> MostActiveUsers { get; set; } = new();
        }

        public class UptimeStatistics
        {
            public DateTime PeriodStart { get; set; }
            public DateTime PeriodEnd { get; set; }
            public int TotalDevices { get; set; }
            public Dictionary<int, double> DeviceUptimes { get; set; } = new();
            public double TotalUptime { get; set; }
            public double AverageUptime { get; set; }
        }
    }
}
