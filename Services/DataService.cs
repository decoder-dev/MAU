using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using System.Text.Json;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Единый сервис для работы с данными (онлайн/офлайн)
    /// </summary>
    public static class DataService
    {
        /// <summary>
        /// Получить все устройства
        /// </summary>
        public static async Task<List<NetworkDevice>> GetDevicesAsync()
        {
            if (ConnectionService.IsMySqlOnline())
            {
                return await Program.DbContext.NetworkDevices.OrderBy(d => d.Name).ToListAsync();
            }
            else
            {
                return await Program.OfflineDbContext.NetworkDevices.OrderBy(d => d.Name).ToListAsync();
            }
        }

        /// <summary>
        /// Добавить устройство
        /// </summary>
        public static async Task<bool> AddDeviceAsync(NetworkDevice device)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    Program.DbContext.NetworkDevices.Add(device);
                    await Program.DbContext.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Сохраняем в офлайн базу
                Program.OfflineDbContext.NetworkDevices.Add(device);
                await Program.OfflineDbContext.SaveChangesAsync();

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "CreateDevice",
                    EntityType = "NetworkDevice",
                    EntityData = JsonSerializer.Serialize(device),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }

        /// <summary>
        /// Обновить устройство
        /// </summary>
        public static async Task<bool> UpdateDeviceAsync(NetworkDevice device)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    device.UpdatedAt = DateTime.UtcNow;
                    Program.DbContext.NetworkDevices.Update(device);
                    await Program.DbContext.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Обновляем в офлайн базе
                var offlineDevice = await Program.OfflineDbContext.NetworkDevices
                    .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);
                
                if (offlineDevice != null)
                {
                    offlineDevice.Name = device.Name;
                    offlineDevice.DeviceType = device.DeviceType;
                    offlineDevice.Status = device.Status;
                    offlineDevice.Location = device.Location;
                    offlineDevice.Building = device.Building;
                    offlineDevice.Room = device.Room;
                    offlineDevice.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    Program.OfflineDbContext.NetworkDevices.Add(device);
                }
                
                await Program.OfflineDbContext.SaveChangesAsync();

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "UpdateDevice",
                    EntityType = "NetworkDevice",
                    EntityData = JsonSerializer.Serialize(device),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }

        /// <summary>
        /// Удалить устройство
        /// </summary>
        public static async Task<bool> DeleteDeviceAsync(NetworkDevice device)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    Program.DbContext.NetworkDevices.Remove(device);
                    await Program.DbContext.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Удаляем из офлайн базы
                var offlineDevice = await Program.OfflineDbContext.NetworkDevices
                    .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);
                
                if (offlineDevice != null)
                {
                    Program.OfflineDbContext.NetworkDevices.Remove(offlineDevice);
                    await Program.OfflineDbContext.SaveChangesAsync();
                }

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "DeleteDevice",
                    EntityType = "NetworkDevice",
                    EntityData = JsonSerializer.Serialize(new { IpAddress = device.IpAddress }),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }

        /// <summary>
        /// Получить устройство по ID
        /// </summary>
        public static async Task<NetworkDevice?> GetDeviceByIdAsync(int id)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                return await Program.DbContext.NetworkDevices.FindAsync(id);
            }
            else
            {
                return await Program.OfflineDbContext.NetworkDevices
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
        }

        /// <summary>
        /// Получить устройство по IP
        /// </summary>
        public static async Task<NetworkDevice?> GetDeviceByIpAsync(string ipAddress)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                return await Program.DbContext.NetworkDevices
                    .FirstOrDefaultAsync(d => d.IpAddress == ipAddress);
            }
            else
            {
                return await Program.OfflineDbContext.NetworkDevices
                    .FirstOrDefaultAsync(d => d.IpAddress == ipAddress);
            }
        }

        /// <summary>
        /// Получить заявки на обслуживание
        /// </summary>
        public static async Task<List<MaintenanceRequest>> GetMaintenanceRequestsAsync()
        {
            if (ConnectionService.IsMySqlOnline())
            {
                return await Program.DbContext.MaintenanceRequests
                    .Include(r => r.Device)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                return await Program.OfflineDbContext.MaintenanceRequests
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Добавить заявку на обслуживание
        /// </summary>
        public static async Task<bool> AddMaintenanceRequestAsync(MaintenanceRequest request)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    Program.DbContext.MaintenanceRequests.Add(request);
                    await Program.DbContext.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                Program.OfflineDbContext.MaintenanceRequests.Add(request);
                await Program.OfflineDbContext.SaveChangesAsync();

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "CreateRequest",
                    EntityType = "MaintenanceRequest",
                    EntityData = JsonSerializer.Serialize(request),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }

        /// <summary>
        /// Обновить заявку на обслуживание
        /// </summary>
        public static async Task<bool> UpdateMaintenanceRequestAsync(MaintenanceRequest request)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    Program.DbContext.MaintenanceRequests.Update(request);
                    await Program.DbContext.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Обновляем в офлайн базе
                var offlineRequest = await Program.OfflineDbContext.MaintenanceRequests
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (offlineRequest != null)
                {
                    offlineRequest.Title = request.Title;
                    offlineRequest.Description = request.Description;
                    offlineRequest.Status = request.Status;
                    offlineRequest.Priority = request.Priority;
                    offlineRequest.AssignedTo = request.AssignedTo;
                    offlineRequest.Resolution = request.Resolution;
                    offlineRequest.CompletedAt = request.CompletedAt;
                    Program.OfflineDbContext.MaintenanceRequests.Update(offlineRequest);
                    await Program.OfflineDbContext.SaveChangesAsync();
                }
                else
                {
                    Program.OfflineDbContext.MaintenanceRequests.Add(request);
                    await Program.OfflineDbContext.SaveChangesAsync();
                }

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "UpdateRequest",
                    EntityType = "MaintenanceRequest",
                    EntityData = JsonSerializer.Serialize(request),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }

        /// <summary>
        /// Получить количество ожидающих синхронизации операций
        /// </summary>
        public static async Task<int> GetPendingOperationsCountAsync()
        {
            if (ConnectionService.IsMySqlOnline())
            {
                return await Program.OfflineDbContext.OfflineOperations
                    .CountAsync(o => o.Status == "Pending");
            }
            return 0;
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        public static async Task<List<User>> GetUsersAsync()
        {
            if (ConnectionService.IsMySqlOnline())
            {
                return await Program.DbContext.Users
                    .OrderBy(u => u.Username)
                    .ToListAsync();
            }
            else
            {
                return await Program.OfflineDbContext.Users
                    .OrderBy(u => u.Username)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        public static async Task<User?> GetUserByIdAsync(int id)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                return await Program.DbContext.Users.FindAsync(id);
            }
            else
            {
                return await Program.OfflineDbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
        }

        /// <summary>
        /// Добавить пользователя
        /// </summary>
        public static async Task<bool> AddUserAsync(User user, string password)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    var userService = new UserService(Program.DbContext);
                    await userService.CreateUserAsync(user, password);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Сохраняем в офлайн базу
                user.SetPassword(password);
                Program.OfflineDbContext.Users.Add(user);
                await Program.OfflineDbContext.SaveChangesAsync();

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "CreateUser",
                    EntityType = "User",
                    EntityData = JsonSerializer.Serialize(new { user, password }),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }

        /// <summary>
        /// Обновить пользователя
        /// </summary>
        public static async Task<bool> UpdateUserAsync(User user)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    var userService = new UserService(Program.DbContext);
                    await userService.UpdateUserAsync(user);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Обновляем в офлайн базе
                var offlineUser = await Program.OfflineDbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == user.Id);
                
                if (offlineUser != null)
                {
                    offlineUser.Username = user.Username;
                    offlineUser.FullName = user.FullName;
                    offlineUser.Email = user.Email;
                    offlineUser.Role = user.Role;
                    offlineUser.IsActive = user.IsActive;
                }
                else
                {
                    Program.OfflineDbContext.Users.Add(user);
                }
                
                await Program.OfflineDbContext.SaveChangesAsync();

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "UpdateUser",
                    EntityType = "User",
                    EntityData = JsonSerializer.Serialize(user),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        public static async Task<bool> DeleteUserAsync(int userId)
        {
            if (ConnectionService.IsMySqlOnline())
            {
                try
                {
                    var userService = new UserService(Program.DbContext);
                    await userService.DeleteUserAsync(userId);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Удаляем из офлайн базы
                var offlineUser = await Program.OfflineDbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (offlineUser != null)
                {
                    Program.OfflineDbContext.Users.Remove(offlineUser);
                    await Program.OfflineDbContext.SaveChangesAsync();
                }

                // Регистрируем операцию для синхронизации
                var operation = new OfflineOperation
                {
                    OperationType = "DeleteUser",
                    EntityType = "User",
                    EntityData = JsonSerializer.Serialize(new { UserId = userId }),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                Program.OfflineDbContext.OfflineOperations.Add(operation);
                await Program.OfflineDbContext.SaveChangesAsync();

                return true;
            }
        }
    }
}


