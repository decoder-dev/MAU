using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;

namespace NetworkMonitoringSystem.Services
{
    public class DeviceService
    {
        private readonly ApplicationDbContext _context;

        public DeviceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NetworkDevice>> GetAllDevicesAsync()
        {
            return await _context.NetworkDevices.ToListAsync();
        }

        public async Task<NetworkDevice?> GetDeviceByIdAsync(int id)
        {
            return await _context.NetworkDevices.FindAsync(id);
        }

        public async Task<bool> CreateDeviceAsync(NetworkDevice device)
        {
            try
            {
                _context.NetworkDevices.Add(device);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDeviceAsync(NetworkDevice device)
        {
            try
            {
                _context.NetworkDevices.Update(device);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDeviceAsync(int id)
        {
            try
            {
                var device = await _context.NetworkDevices.FindAsync(id);
                if (device != null)
                {
                    _context.NetworkDevices.Remove(device);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}


