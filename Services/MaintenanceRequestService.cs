using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;

namespace NetworkMonitoringSystem.Services
{
    public class MaintenanceRequestService
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MaintenanceRequest>> GetAllRequestsAsync()
        {
            return await _context.MaintenanceRequests
                .Include(r => r.Device)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<bool> CreateRequestAsync(MaintenanceRequest request)
        {
            try
            {
                _context.MaintenanceRequests.Add(request);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


