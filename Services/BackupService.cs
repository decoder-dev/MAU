using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для создания резервных копий данных
    /// </summary>
    public class BackupService
    {
        private readonly ApplicationDbContext _context;

        public BackupService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создать полную резервную копию данных
        /// </summary>
        public async Task<string> CreateFullBackupAsync()
        {
            try
            {
                var backupDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NetworkMonitoringSystem",
                    "Backups");

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                var backupFileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var backupPath = Path.Combine(backupDir, backupFileName);

                var backup = new BackupData
                {
                    BackupDate = DateTime.UtcNow,
                    Devices = await _context.NetworkDevices.ToListAsync(),
                    Users = await _context.Users.ToListAsync(),
                    MaintenanceRequests = await _context.MaintenanceRequests
                        .Include(r => r.Device)
                        .Include(r => r.User)
                        .ToListAsync(),
                    StatusHistory = await _context.DeviceStatusHistory
                        .Take(10000) // Ограничиваем размер истории
                        .ToListAsync()
                };

                var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                await File.WriteAllTextAsync(backupPath, json);

                // Удаляем старые резервные копии (оставляем последние 10)
                CleanupOldBackups(backupDir, 10);

                return backupPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания резервной копии: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создать резервную копию только устройств
        /// </summary>
        public async Task<string> CreateDevicesBackupAsync()
        {
            try
            {
                var backupDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NetworkMonitoringSystem",
                    "Backups");

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                var backupFileName = $"devices_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var backupPath = Path.Combine(backupDir, backupFileName);

                var devices = await _context.NetworkDevices.ToListAsync();
                var json = JsonSerializer.Serialize(devices, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(backupPath, json);
                return backupPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания резервной копии устройств: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получить список доступных резервных копий
        /// </summary>
        public List<BackupInfo> GetAvailableBackups()
        {
            var backupDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "Backups");

            if (!Directory.Exists(backupDir))
                return new List<BackupInfo>();

            var backups = new List<BackupInfo>();
            var files = Directory.GetFiles(backupDir, "*.json");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupInfo
                {
                    FilePath = file,
                    FileName = fileInfo.Name,
                    CreatedAt = fileInfo.CreationTime,
                    Size = fileInfo.Length
                });
            }

            return backups.OrderByDescending(b => b.CreatedAt).ToList();
        }

        /// <summary>
        /// Удалить старые резервные копии
        /// </summary>
        private void CleanupOldBackups(string backupDir, int keepCount)
        {
            try
            {
                var files = Directory.GetFiles(backupDir, "*.json")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                if (files.Count > keepCount)
                {
                    foreach (var file in files.Skip(keepCount))
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        public class BackupData
        {
            public DateTime BackupDate { get; set; }
            public List<NetworkDevice> Devices { get; set; } = new();
            public List<User> Users { get; set; } = new();
            public List<MaintenanceRequest> MaintenanceRequests { get; set; } = new();
            public List<DeviceStatusHistory> StatusHistory { get; set; } = new();
        }

        public class BackupInfo
        {
            public string FilePath { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public long Size { get; set; }
        }
    }
}
