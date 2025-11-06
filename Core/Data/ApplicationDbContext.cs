using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.Core.Data
{
    /// <summary>
    /// Контекст базы данных приложения
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Основные таблицы
        public DbSet<User> Users { get; set; }
        public DbSet<NetworkDevice> NetworkDevices { get; set; }
        public DbSet<DeviceStatusHistory> DeviceStatusHistory { get; set; }
        public DbSet<DeviceMetric> DeviceMetrics { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== КОНФИГУРАЦИЯ ТАБЛИЦЫ USERS ==========
            modelBuilder.Entity<User>(entity =>
            {
                // Уникальный индекс на Username
                entity.HasIndex(u => u.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");

                // Индекс на Email для быстрого поиска
                entity.HasIndex(u => u.Email)
                    .HasDatabaseName("IX_Users_Email")
                    .HasFilter("[Email] IS NOT NULL");

                // Индекс на Role для фильтрации
                entity.HasIndex(u => u.Role)
                    .HasDatabaseName("IX_Users_Role");

                // Индекс на IsActive для активных пользователей
                entity.HasIndex(u => u.IsActive)
                    .HasDatabaseName("IX_Users_IsActive");

                // Индекс на LastLoginAt для сортировки
                entity.HasIndex(u => u.LastLoginAt)
                    .HasDatabaseName("IX_Users_LastLoginAt")
                    .HasFilter("[LastLoginAt] IS NOT NULL");

                // Значения по умолчанию
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                entity.Property(u => u.Role)
                    .HasDefaultValue(UserRole.User);
            });

            // ========== КОНФИГУРАЦИЯ ТАБЛИЦЫ NETWORKDEVICES ==========
            modelBuilder.Entity<NetworkDevice>(entity =>
            {
                // Уникальный индекс на IP адрес (критично!)
                entity.HasIndex(d => d.IpAddress)
                    .IsUnique()
                    .HasDatabaseName("IX_NetworkDevices_IpAddress");

                // Индекс на OneCId для интеграции
                entity.HasIndex(d => d.OneCId)
                    .HasDatabaseName("IX_NetworkDevices_OneCId")
                    .HasFilter("[OneCId] IS NOT NULL");

                // Индексы для фильтрации
                entity.HasIndex(d => d.Status)
                    .HasDatabaseName("IX_NetworkDevices_Status");

                entity.HasIndex(d => d.DeviceType)
                    .HasDatabaseName("IX_NetworkDevices_DeviceType");

                // Составной индекс для поиска по местоположению
                entity.HasIndex(d => new { d.Building, d.Room })
                    .HasDatabaseName("IX_NetworkDevices_Building_Room")
                    .HasFilter("[Building] IS NOT NULL AND [Room] IS NOT NULL");

                // Индекс на LastSeen для сортировки
                entity.HasIndex(d => d.LastSeen)
                    .HasDatabaseName("IX_NetworkDevices_LastSeen")
                    .HasFilter("[LastSeen] IS NOT NULL");

                // Индекс на CreatedAt для сортировки
                entity.HasIndex(d => d.CreatedAt)
                    .HasDatabaseName("IX_NetworkDevices_CreatedAt");

                // Значения по умолчанию
                entity.Property(d => d.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(d => d.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

                entity.Property(d => d.Status)
                    .HasDefaultValue(DeviceStatus.Unknown);
            });

            // ========== КОНФИГУРАЦИЯ ТАБЛИЦЫ DEVICESTATUSHISTORY ==========
            modelBuilder.Entity<DeviceStatusHistory>(entity =>
            {
                // Составной индекс для быстрого поиска истории устройства
                entity.HasIndex(d => new { d.DeviceId, d.Timestamp })
                    .HasDatabaseName("IX_DeviceStatusHistory_DeviceId_Timestamp");

                // Индекс на Status для фильтрации
                entity.HasIndex(d => d.Status)
                    .HasDatabaseName("IX_DeviceStatusHistory_Status");

                // Индекс на Timestamp для сортировки
                entity.HasIndex(d => d.Timestamp)
                    .HasDatabaseName("IX_DeviceStatusHistory_Timestamp");

                // Связь с устройством с каскадным удалением
                entity.HasOne(d => d.Device)
                    .WithMany(d => d.StatusHistory)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DeviceStatusHistory_NetworkDevices");

                // Значение по умолчанию
                entity.Property(d => d.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ========== КОНФИГУРАЦИЯ ТАБЛИЦЫ DEVICEMETRICS ==========
            modelBuilder.Entity<DeviceMetric>(entity =>
            {
                // Составной индекс для поиска метрик устройства
                entity.HasIndex(d => new { d.DeviceId, d.MetricType, d.Timestamp })
                    .HasDatabaseName("IX_DeviceMetrics_DeviceId_Type_Timestamp");

                // Индекс на Timestamp для сортировки
                entity.HasIndex(d => d.Timestamp)
                    .HasDatabaseName("IX_DeviceMetrics_Timestamp");

                // Индекс на MetricType для фильтрации
                entity.HasIndex(d => d.MetricType)
                    .HasDatabaseName("IX_DeviceMetrics_MetricType");

                // Связь с устройством с каскадным удалением
                entity.HasOne(d => d.Device)
                    .WithMany(d => d.Metrics)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DeviceMetrics_NetworkDevices");

                // Значение по умолчанию
                entity.Property(d => d.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ========== КОНФИГУРАЦИЯ ТАБЛИЦЫ MAINTENANCEREQUESTS ==========
            modelBuilder.Entity<MaintenanceRequest>(entity =>
            {
                // Составной индекс для поиска заявок по устройству и статусу
                entity.HasIndex(m => new { m.DeviceId, m.Status })
                    .HasDatabaseName("IX_MaintenanceRequests_DeviceId_Status");

                // Составной индекс для поиска заявок пользователя
                entity.HasIndex(m => new { m.UserId, m.Status })
                    .HasDatabaseName("IX_MaintenanceRequests_UserId_Status");

                // Индекс на Status для фильтрации
                entity.HasIndex(m => m.Status)
                    .HasDatabaseName("IX_MaintenanceRequests_Status");

                // Индекс на Priority для сортировки
                entity.HasIndex(m => m.Priority)
                    .HasDatabaseName("IX_MaintenanceRequests_Priority");

                // Индекс на CreatedAt для сортировки
                entity.HasIndex(m => m.CreatedAt)
                    .HasDatabaseName("IX_MaintenanceRequests_CreatedAt");

                // Связь с устройством (RESTRICT - нельзя удалить устройство с заявками)
                entity.HasOne(m => m.Device)
                    .WithMany(d => d.MaintenanceRequests)
                    .HasForeignKey(m => m.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MaintenanceRequests_NetworkDevices");

                // Связь с пользователем (RESTRICT - нельзя удалить пользователя с заявками)
                entity.HasOne(m => m.User)
                    .WithMany(u => u.MaintenanceRequests)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MaintenanceRequests_Users");

                // Значения по умолчанию
                entity.Property(m => m.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(m => m.Status)
                    .HasDefaultValue(RequestStatus.Pending);

                entity.Property(m => m.Priority)
                    .HasDefaultValue(1);

                // Ограничение: Priority от 1 до 4
                entity.HasCheckConstraint("CK_MaintenanceRequests_Priority", "[Priority] >= 1 AND [Priority] <= 4");
            });

            // ========== КОНФИГУРАЦИЯ ТАБЛИЦЫ USERACTIVITIES ==========
            modelBuilder.Entity<UserActivity>(entity =>
            {
                // Составной индекс для поиска активности пользователя
                entity.HasIndex(a => new { a.UserId, a.Timestamp })
                    .HasDatabaseName("IX_UserActivities_UserId_Timestamp");

                // Индекс на Action для фильтрации
                entity.HasIndex(a => a.Action)
                    .HasDatabaseName("IX_UserActivities_Action");

                // Индекс на Timestamp для сортировки
                entity.HasIndex(a => a.Timestamp)
                    .HasDatabaseName("IX_UserActivities_Timestamp");

                // Составной индекс для поиска по типу сущности
                entity.HasIndex(a => new { a.EntityType, a.EntityId })
                    .HasDatabaseName("IX_UserActivities_EntityType_EntityId")
                    .HasFilter("[EntityType] IS NOT NULL AND [EntityId] IS NOT NULL");

                // Связь с пользователем с каскадным удалением
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Activities)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_UserActivities_Users");

                // Значение по умолчанию
                entity.Property(a => a.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
