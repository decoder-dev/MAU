using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для восстановления данных из резервных копий
    /// </summary>
    public class RestoreService
    {
        private readonly ApplicationDbContext _context;

        public RestoreService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Восстановить данные из полной резервной копии
        /// </summary>
        public async Task<bool> RestoreFromFullBackupAsync(string backupPath, bool overwriteExisting = false)
        {
            try
            {
                if (!File.Exists(backupPath))
                    throw new FileNotFoundException("Резервная копия не найдена", backupPath);

                var json = await File.ReadAllTextAsync(backupPath);
                var backup = JsonSerializer.Deserialize<BackupService.BackupData>(json);

                if (backup == null)
                    throw new Exception("Не удалось прочитать резервную копию");

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (overwriteExisting)
                    {
                        // Удаляем существующие данные
                        _context.DeviceStatusHistory.RemoveRange(await _context.DeviceStatusHistory.ToListAsync());
                        _context.MaintenanceRequests.RemoveRange(await _context.MaintenanceRequests.ToListAsync());
                        _context.NetworkDevices.RemoveRange(await _context.NetworkDevices.ToListAsync());
                        await _context.SaveChangesAsync();
                    }

                    // Восстанавливаем устройства
                    foreach (var device in backup.Devices)
                    {
                        var existing = await _context.NetworkDevices
                            .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);

                        if (existing == null)
                        {
                            device.Id = 0; // Сброс ID для новой записи
                            _context.NetworkDevices.Add(device);
                        }
                        else if (overwriteExisting)
                        {
                            existing.Name = device.Name;
                            existing.Status = device.Status;
                            existing.Description = device.Description;
                            existing.DeviceType = device.DeviceType;
                            existing.Location = device.Location;
                            existing.Building = device.Building;
                            existing.Room = device.Room;
                        }
                    }

                    // Восстанавливаем заявки
                    foreach (var request in backup.MaintenanceRequests)
                    {
                        var existing = await _context.MaintenanceRequests.FindAsync(request.Id);
                        if (existing == null)
                        {
                            request.Id = 0; // Сброс ID
                            _context.MaintenanceRequests.Add(request);
                        }
                    }

                    // Восстанавливаем историю статусов
                    foreach (var history in backup.StatusHistory)
                    {
                        history.Id = 0; // Сброс ID
                        _context.DeviceStatusHistory.Add(history);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка восстановления из резервной копии: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Восстановить только устройства из резервной копии
        /// </summary>
        public async Task<bool> RestoreDevicesAsync(string backupPath, bool overwriteExisting = false)
        {
            try
            {
                if (!File.Exists(backupPath))
                    throw new FileNotFoundException("Резервная копия не найдена", backupPath);

                var json = await File.ReadAllTextAsync(backupPath);
                var devices = JsonSerializer.Deserialize<List<NetworkDevice>>(json);

                if (devices == null)
                    throw new Exception("Не удалось прочитать резервную копию устройств");

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var device in devices)
                    {
                        var existing = await _context.NetworkDevices
                            .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);

                        if (existing == null)
                        {
                            device.Id = 0;
                            _context.NetworkDevices.Add(device);
                        }
                        else if (overwriteExisting)
                        {
                            existing.Name = device.Name;
                            existing.Status = device.Status;
                            existing.Description = device.Description;
                            existing.DeviceType = device.DeviceType;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка восстановления устройств: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Проверить валидность резервной копии
        /// </summary>
        public async Task<bool> ValidateBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                var json = await File.ReadAllTextAsync(backupPath);
                var backup = JsonSerializer.Deserialize<BackupService.BackupData>(json);

                return backup != null && 
                       backup.BackupDate != default &&
                       (backup.Devices?.Any() == true || backup.Users?.Any() == true);
            }
            catch
            {
                return false;
            }
        }
    }
}
