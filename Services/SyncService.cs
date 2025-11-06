using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using System.Text.Json;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// ������ ������������� ������ ����� ������ � ������ ������
    /// </summary>
    public class SyncService
    {
        private readonly ApplicationDbContext _onlineContext;
        private readonly OfflineDbContext _offlineContext;

        public SyncService(ApplicationDbContext onlineContext, OfflineDbContext offlineContext)
        {
            _onlineContext = onlineContext;
            _offlineContext = offlineContext;
        }

        /// <summary>
        /// ������������� ���� �������� �� ������ ����
        /// </summary>
        public async Task<int> SyncPendingOperationsAsync()
        {
            if (!ConnectionService.IsMySqlOnline())
                return 0;

            var pendingOps = await _offlineContext.OfflineOperations
                .Where(o => o.Status == "Pending")
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            int syncedCount = 0;

            foreach (var op in pendingOps)
            {
                try
                {
                    switch (op.OperationType)
                    {
                        case "CreateDevice":
                            await SyncCreateDeviceAsync(op);
                            break;
                        case "UpdateDevice":
                            await SyncUpdateDeviceAsync(op);
                            break;
                        case "DeleteDevice":
                            await SyncDeleteDeviceAsync(op);
                            break;
                        case "CreateRequest":
                            await SyncCreateRequestAsync(op);
                            break;
                        case "UpdateRequest":
                            await SyncUpdateRequestAsync(op);
                            break;
                        case "CreateUser":
                            await SyncCreateUserAsync(op);
                            break;
                        case "UpdateUser":
                            await SyncUpdateUserAsync(op);
                            break;
                        case "DeleteUser":
                            await SyncDeleteUserAsync(op);
                            break;
                    }

                    op.Status = "Synced";
                    op.SyncedAt = DateTime.UtcNow;
                    op.ErrorMessage = null;
                    syncedCount++;
                }
                catch (Exception ex)
                {
                    op.Status = "Failed";
                    op.ErrorMessage = ex.Message;
                }
            }

            await _offlineContext.SaveChangesAsync();
            return syncedCount;
        }

        private async Task SyncCreateDeviceAsync(OfflineOperation op)
        {
            var device = JsonSerializer.Deserialize<NetworkDevice>(op.EntityData);
            if (device != null)
            {
                // ���������, �� ���������� �� ��� ����������
                var exists = await _onlineContext.NetworkDevices
                    .AnyAsync(d => d.IpAddress == device.IpAddress);
                
                if (!exists)
                {
                    device.Id = 0; // ����� ID ��� ������ ����������
                    _onlineContext.NetworkDevices.Add(device);
                    await _onlineContext.SaveChangesAsync();
                }
            }
        }

        private async Task SyncUpdateDeviceAsync(OfflineOperation op)
        {
            var device = JsonSerializer.Deserialize<NetworkDevice>(op.EntityData);
            if (device != null)
            {
                var existing = await _onlineContext.NetworkDevices
                    .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);
                
                if (existing != null)
                {
                    // ��������� ����
                    existing.Name = device.Name;
                    existing.DeviceType = device.DeviceType;
                    existing.Status = device.Status;
                    existing.Location = device.Location;
                    existing.Building = device.Building;
                    existing.Room = device.Room;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _onlineContext.SaveChangesAsync();
                }
            }
        }

        private async Task SyncDeleteDeviceAsync(OfflineOperation op)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(op.EntityData);
            if (data != null && data.ContainsKey("IpAddress"))
            {
                var device = await _onlineContext.NetworkDevices
                    .FirstOrDefaultAsync(d => d.IpAddress == data["IpAddress"]);
                
                if (device != null)
                {
                    _onlineContext.NetworkDevices.Remove(device);
                    await _onlineContext.SaveChangesAsync();
                }
            }
        }

        private async Task SyncCreateRequestAsync(OfflineOperation op)
        {
            var request = JsonSerializer.Deserialize<MaintenanceRequest>(op.EntityData);
            if (request != null)
            {
                request.Id = 0;
                _onlineContext.MaintenanceRequests.Add(request);
                await _onlineContext.SaveChangesAsync();
            }
        }

        private async Task SyncUpdateRequestAsync(OfflineOperation op)
        {
            var request = JsonSerializer.Deserialize<MaintenanceRequest>(op.EntityData);
            if (request != null && request.Id > 0)
            {
                var existing = await _onlineContext.MaintenanceRequests
                    .FirstOrDefaultAsync(r => r.Id == request.Id);
                
                if (existing != null)
                {
                    existing.Status = request.Status;
                    existing.Resolution = request.Resolution;
                    existing.AssignedTo = request.AssignedTo;
                    await _onlineContext.SaveChangesAsync();
                }
            }
        }

        private async Task SyncCreateUserAsync(OfflineOperation op)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(op.EntityData);
            if (data != null && data.ContainsKey("user") && data.ContainsKey("password"))
            {
                var userJson = data["user"].ToString();
                var password = data["password"].ToString();
                var user = JsonSerializer.Deserialize<User>(userJson!);
                
                if (user != null && !await _onlineContext.Users.AnyAsync(u => u.Username == user.Username))
                {
                    user.SetPassword(password!);
                    _onlineContext.Users.Add(user);
                    await _onlineContext.SaveChangesAsync();
                }
            }
        }

        private async Task SyncUpdateUserAsync(OfflineOperation op)
        {
            var user = JsonSerializer.Deserialize<User>(op.EntityData);
            if (user != null)
            {
                var existing = await _onlineContext.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username);
                
                if (existing != null)
                {
                    existing.FullName = user.FullName;
                    existing.Email = user.Email;
                    existing.Role = user.Role;
                    existing.IsActive = user.IsActive;
                    await _onlineContext.SaveChangesAsync();
                }
            }
        }

        private async Task SyncDeleteUserAsync(OfflineOperation op)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(op.EntityData);
            if (data != null && data.ContainsKey("UserId"))
            {
                var userId = Convert.ToInt32(data["UserId"]);
                var user = await _onlineContext.Users
                    .Include(u => u.MaintenanceRequests)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user != null && !user.MaintenanceRequests.Any())
                {
                    _onlineContext.Users.Remove(user);
                    await _onlineContext.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// ������������� ��������� �� ������ � ������ ����
        /// </summary>
        public async Task SyncDevicesFromOnlineAsync()
        {
            if (!ConnectionService.IsMySqlOnline())
                return;

            try
            {
                var onlineDevices = await _onlineContext.NetworkDevices.ToListAsync();
                
                foreach (var device in onlineDevices)
                {
                    var offlineDevice = await _offlineContext.NetworkDevices
                        .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);
                    
                    if (offlineDevice == null)
                    {
                        // ������� ����� ��� ������ ����
                        var deviceCopy = new NetworkDevice
                        {
                            Name = device.Name,
                            Description = device.Description,
                            DeviceType = device.DeviceType,
                            IpAddress = device.IpAddress,
                            MacAddress = device.MacAddress,
                            Model = device.Model,
                            Manufacturer = device.Manufacturer,
                            Location = device.Location,
                            Building = device.Building,
                            Room = device.Room,
                            Status = device.Status,
                            LastSeen = device.LastSeen,
                            CreatedAt = device.CreatedAt,
                            UpdatedAt = device.UpdatedAt
                        };
                        _offlineContext.NetworkDevices.Add(deviceCopy);
                    }
                    else
                    {
                        // ��������� ������������
                        offlineDevice.Name = device.Name;
                        offlineDevice.Status = device.Status;
                        offlineDevice.LastSeen = device.LastSeen;
                        offlineDevice.UpdatedAt = device.UpdatedAt;
                    }
                }

                await _offlineContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"������ ������������� ���������: {ex.Message}");
            }
        }
    }
}

