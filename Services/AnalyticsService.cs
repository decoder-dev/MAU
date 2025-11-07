using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для аналитики и анализа данных
    /// </summary>
    public class AnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить аналитику по устройствам
        /// </summary>
        public async Task<DeviceAnalytics> GetDeviceAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var devices = await _context.NetworkDevices.ToListAsync();
            var statusHistory = await _context.DeviceStatusHistory
                .Where(h => h.Timestamp >= start && h.Timestamp <= end)
                .ToListAsync();

            var analytics = new DeviceAnalytics
            {
                TotalDevices = devices.Count,
                OnlineDevices = devices.Count(d => d.Status == DeviceStatus.Online),
                OfflineDevices = devices.Count(d => d.Status == DeviceStatus.Offline),
                DegradedDevices = devices.Count(d => d.Status == DeviceStatus.Degraded),
                
                // Статистика по типам
                DevicesByType = devices
                    .GroupBy(d => d.DeviceType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                // Статистика по статусам
                DevicesByStatus = devices
                    .GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                // Uptime анализ
                AverageUptime = CalculateAverageUptime(statusHistory),
                
                // Топ проблемных устройств
                MostProblematicDevices = GetMostProblematicDevices(statusHistory, devices),
                
                // Тренды
                StatusTrends = GetStatusTrends(statusHistory, start, end),
                
                // Статистика по времени ответа (если есть метрики)
                ResponseTimeStats = await GetResponseTimeStatsAsync(start, end)
            };

            return analytics;
        }

        /// <summary>
        /// Получить аналитику по заявкам на обслуживание
        /// </summary>
        public async Task<MaintenanceAnalytics> GetMaintenanceAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var requests = await _context.MaintenanceRequests
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
                .ToListAsync();

            var analytics = new MaintenanceAnalytics
            {
                TotalRequests = requests.Count,
                PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending),
                InProgressRequests = requests.Count(r => r.Status == RequestStatus.InProgress),
                CompletedRequests = requests.Count(r => r.Status == RequestStatus.Completed),
                CancelledRequests = requests.Count(r => r.Status == RequestStatus.Cancelled),
                
                // Среднее время выполнения
                AverageResolutionTime = CalculateAverageResolutionTime(requests),
                
                // Распределение по приоритетам
                RequestsByPriority = requests
                    .GroupBy(r => r.Priority)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Топ исполнителей
                TopAssignees = requests
                    .Where(r => !string.IsNullOrEmpty(r.AssignedTo))
                    .GroupBy(r => r.AssignedTo!)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Статистика по устройствам
                RequestsByDevice = requests
                    .GroupBy(r => r.DeviceId)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return analytics;
        }

        /// <summary>
        /// Получить аналитику по активности пользователей
        /// </summary>
        public async Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var activities = await _context.UserActivities
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .Include(a => a.User)
                .ToListAsync();

            var analytics = new UserActivityAnalytics
            {
                TotalActivities = activities.Count,
                UniqueUsers = activities.Select(a => a.UserId).Distinct().Count(),
                
                // Активность по типам действий
                ActivitiesByAction = activities
                    .GroupBy(a => a.Action)
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Активность по пользователям
                ActivitiesByUser = activities
                    .GroupBy(a => a.User?.Username ?? "Unknown")
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Активность по времени суток
                ActivitiesByHour = activities
                    .GroupBy(a => a.Timestamp.Hour)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Активность по дням недели
                ActivitiesByDayOfWeek = activities
                    .GroupBy(a => a.Timestamp.DayOfWeek)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            return analytics;
        }

        /// <summary>
        /// Рассчитать средний uptime
        /// </summary>
        private double CalculateAverageUptime(List<DeviceStatusHistory> history)
        {
            if (!history.Any())
                return 0;

            var totalMinutes = history.Count;
            var onlineMinutes = history.Count(h => h.Status == DeviceStatus.Online);

            return totalMinutes > 0 ? (double)onlineMinutes / totalMinutes * 100 : 0;
        }

        /// <summary>
        /// Получить самые проблемные устройства
        /// </summary>
        private List<DeviceProblemInfo> GetMostProblematicDevices(List<DeviceStatusHistory> history, List<NetworkDevice> devices)
        {
            var deviceProblems = history
                .Where(h => h.Status == DeviceStatus.Offline || h.Status == DeviceStatus.Degraded)
                .GroupBy(h => h.DeviceId)
                .Select(g => new
                {
                    DeviceId = g.Key,
                    ProblemCount = g.Count(),
                    LastProblem = g.Max(h => h.Timestamp)
                })
                .OrderByDescending(x => x.ProblemCount)
                .Take(10)
                .ToList();

            return deviceProblems.Select(d => new DeviceProblemInfo
            {
                DeviceId = d.DeviceId,
                DeviceName = devices.FirstOrDefault(dev => dev.Id == d.DeviceId)?.Name ?? "Unknown",
                ProblemCount = d.ProblemCount,
                LastProblemTime = d.LastProblem
            }).ToList();
        }

        /// <summary>
        /// Получить тренды статусов
        /// </summary>
        private Dictionary<DateTime, Dictionary<DeviceStatus, int>> GetStatusTrends(List<DeviceStatusHistory> history, DateTime start, DateTime end)
        {
            var trends = new Dictionary<DateTime, Dictionary<DeviceStatus, int>>();
            var currentDate = start.Date;

            while (currentDate <= end.Date)
            {
                var dayHistory = history.Where(h => h.Timestamp.Date == currentDate).ToList();
                trends[currentDate] = new Dictionary<DeviceStatus, int>
                {
                    { DeviceStatus.Online, dayHistory.Count(h => h.Status == DeviceStatus.Online) },
                    { DeviceStatus.Offline, dayHistory.Count(h => h.Status == DeviceStatus.Offline) },
                    { DeviceStatus.Degraded, dayHistory.Count(h => h.Status == DeviceStatus.Degraded) }
                };
                currentDate = currentDate.AddDays(1);
            }

            return trends;
        }

        /// <summary>
        /// Получить статистику времени ответа
        /// </summary>
        private async Task<ResponseTimeStatistics> GetResponseTimeStatsAsync(DateTime start, DateTime end)
        {
            var metrics = await _context.DeviceMetrics
                .Where(m => m.Timestamp >= start && m.Timestamp <= end && 
                           m.MetricType == "ResponseTime")
                .ToListAsync();

            if (!metrics.Any())
                return new ResponseTimeStatistics();

            var values = metrics.Where(m => m.Value.HasValue).Select(m => (double)m.Value!.Value).ToList();

            if (!values.Any())
                return new ResponseTimeStatistics();

            return new ResponseTimeStatistics
            {
                Average = values.Average(),
                Min = values.Min(),
                Max = values.Max(),
                Median = values.OrderBy(v => v).Skip(values.Count / 2).FirstOrDefault()
            };
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

            var totalTime = completed
                .Select(r => (r.CompletedAt!.Value - r.CreatedAt).TotalMinutes)
                .Average();

            return TimeSpan.FromMinutes(totalTime);
        }

        // Классы для результатов аналитики
        public class DeviceAnalytics
        {
            public int TotalDevices { get; set; }
            public int OnlineDevices { get; set; }
            public int OfflineDevices { get; set; }
            public int DegradedDevices { get; set; }
            public Dictionary<string, int> DevicesByType { get; set; } = new();
            public Dictionary<string, int> DevicesByStatus { get; set; } = new();
            public double AverageUptime { get; set; }
            public List<DeviceProblemInfo> MostProblematicDevices { get; set; } = new();
            public Dictionary<DateTime, Dictionary<DeviceStatus, int>> StatusTrends { get; set; } = new();
            public ResponseTimeStatistics ResponseTimeStats { get; set; } = new();
        }

        public class MaintenanceAnalytics
        {
            public int TotalRequests { get; set; }
            public int PendingRequests { get; set; }
            public int InProgressRequests { get; set; }
            public int CompletedRequests { get; set; }
            public int CancelledRequests { get; set; }
            public TimeSpan? AverageResolutionTime { get; set; }
            public Dictionary<int, int> RequestsByPriority { get; set; } = new();
            public Dictionary<string, int> TopAssignees { get; set; } = new();
            public Dictionary<int, int> RequestsByDevice { get; set; } = new();
        }

        public class UserActivityAnalytics
        {
            public int TotalActivities { get; set; }
            public int UniqueUsers { get; set; }
            public Dictionary<string, int> ActivitiesByAction { get; set; } = new();
            public Dictionary<string, int> ActivitiesByUser { get; set; } = new();
            public Dictionary<int, int> ActivitiesByHour { get; set; } = new();
            public Dictionary<string, int> ActivitiesByDayOfWeek { get; set; } = new();
        }

        public class DeviceProblemInfo
        {
            public int DeviceId { get; set; }
            public string DeviceName { get; set; } = string.Empty;
            public int ProblemCount { get; set; }
            public DateTime LastProblemTime { get; set; }
        }

        public class ResponseTimeStatistics
        {
            public double Average { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public double Median { get; set; }
        }
    }
}
