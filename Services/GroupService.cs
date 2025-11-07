using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для группировки и организации устройств
    /// </summary>
    public class GroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить устройства, сгруппированные по зданию
        /// </summary>
        public async Task<Dictionary<string, List<NetworkDevice>>> GetDevicesByBuildingAsync()
        {
            var devices = await _context.NetworkDevices
                .Where(d => !string.IsNullOrEmpty(d.Building))
                .ToListAsync();

            return devices
                .GroupBy(d => d.Building!)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Получить устройства, сгруппированные по типу
        /// </summary>
        public async Task<Dictionary<DeviceType, List<NetworkDevice>>> GetDevicesByTypeAsync()
        {
            var devices = await _context.NetworkDevices.ToListAsync();

            return devices
                .GroupBy(d => d.DeviceType)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Получить устройства, сгруппированные по статусу
        /// </summary>
        public async Task<Dictionary<DeviceStatus, List<NetworkDevice>>> GetDevicesByStatusAsync()
        {
            var devices = await _context.NetworkDevices.ToListAsync();

            return devices
                .GroupBy(d => d.Status)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Получить устройства, сгруппированные по местоположению
        /// </summary>
        public async Task<Dictionary<string, List<NetworkDevice>>> GetDevicesByLocationAsync()
        {
            var devices = await _context.NetworkDevices
                .Where(d => !string.IsNullOrEmpty(d.Location))
                .ToListAsync();

            return devices
                .GroupBy(d => d.Location!)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Получить статистику по группам
        /// </summary>
        public async Task<GroupStatistics> GetGroupStatisticsAsync()
        {
            var devices = await _context.NetworkDevices.ToListAsync();

            return new GroupStatistics
            {
                TotalGroups = devices.Where(d => !string.IsNullOrEmpty(d.Building)).Select(d => d.Building).Distinct().Count(),
                DevicesByBuilding = devices
                    .Where(d => !string.IsNullOrEmpty(d.Building))
                    .GroupBy(d => d.Building!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                DevicesByType = devices
                    .GroupBy(d => d.DeviceType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                DevicesByStatus = devices
                    .GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }

        /// <summary>
        /// Получить устройства в определенной группе
        /// </summary>
        public async Task<List<NetworkDevice>> GetDevicesInGroupAsync(string groupType, string groupValue)
        {
            return groupType.ToLower() switch
            {
                "building" => await _context.NetworkDevices
                    .Where(d => d.Building == groupValue)
                    .ToListAsync(),
                "location" => await _context.NetworkDevices
                    .Where(d => d.Location == groupValue)
                    .ToListAsync(),
                "room" => await _context.NetworkDevices
                    .Where(d => d.Room == groupValue)
                    .ToListAsync(),
                _ => new List<NetworkDevice>()
            };
        }

        public class GroupStatistics
        {
            public int TotalGroups { get; set; }
            public Dictionary<string, int> DevicesByBuilding { get; set; } = new();
            public Dictionary<string, int> DevicesByType { get; set; } = new();
            public Dictionary<string, int> DevicesByStatus { get; set; } = new();
        }
    }
}
