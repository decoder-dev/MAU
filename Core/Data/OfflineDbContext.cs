using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.Core.Data
{
    /// <summary>
    /// Контекст для офлайн режима (SQLite)
    /// </summary>
    public class OfflineDbContext : DbContext
    {
        private static readonly string DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NetworkMonitoringSystem",
            "offline.db");

        public DbSet<NetworkDevice> NetworkDevices { get; set; }
        public DbSet<DeviceStatusHistory> DeviceStatusHistory { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<OfflineOperation> OfflineOperations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Создаем директорию если не существует
            var directory = Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Индексы для офлайн базы
            modelBuilder.Entity<NetworkDevice>()
                .HasIndex(d => d.IpAddress);

            modelBuilder.Entity<OfflineOperation>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<OfflineOperation>()
                .HasIndex(o => o.OperationType);
        }
    }

    /// <summary>
    /// Операция, выполненная в офлайн режиме
    /// </summary>
    public class OfflineOperation
    {
        public int Id { get; set; }
        public string OperationType { get; set; } = string.Empty; // Create, Update, Delete
        public string EntityType { get; set; } = string.Empty; // NetworkDevice, MaintenanceRequest
        public string EntityData { get; set; } = string.Empty; // JSON данные
        public string Status { get; set; } = "Pending"; // Pending, Synced, Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SyncedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

