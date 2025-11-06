using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис управления пользователями
    /// </summary>
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Получить пользователя по имени
        /// </summary>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        public async Task<User> CreateUserAsync(User user, string password)
        {
            // Проверка уникальности Username
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                throw new InvalidOperationException($"Пользователь с именем '{user.Username}' уже существует");

            // Проверка уникальности Email (если указан)
            if (!string.IsNullOrEmpty(user.Email) && 
                await _context.Users.AnyAsync(u => u.Email == user.Email))
                throw new InvalidOperationException($"Пользователь с email '{user.Email}' уже существует");

            // Установка пароля
            user.SetPassword(password);
            user.CreatedBy = Program.CurrentUser?.FullName;
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Логирование
            await UserActivityLogger.LogAsync("UserCreate", $"Создан пользователь: {user.Username}", "User", user.Id);

            return user;
        }

        /// <summary>
        /// Обновить пользователя
        /// </summary>
        public async Task UpdateUserAsync(User user)
        {
            // Проверка уникальности Username (исключая текущего пользователя)
            if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != user.Id))
                throw new InvalidOperationException($"Пользователь с именем '{user.Username}' уже существует");

            // Проверка уникальности Email (если указан)
            if (!string.IsNullOrEmpty(user.Email) && 
                await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != user.Id))
                throw new InvalidOperationException($"Пользователь с email '{user.Email}' уже существует");

            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                throw new InvalidOperationException("Пользователь не найден");

            // Обновление полей (пароль обновляется отдельно)
            existingUser.Username = user.Username;
            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            await _context.SaveChangesAsync();

            // Логирование
            await UserActivityLogger.LogAsync("UserUpdate", $"Обновлен пользователь: {user.Username}", "User", user.Id);
        }

        /// <summary>
        /// Изменить пароль пользователя
        /// </summary>
        public async Task ChangePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Пользователь не найден");

            user.SetPassword(newPassword);
            await _context.SaveChangesAsync();

            // Логирование
            await UserActivityLogger.LogAsync("UserChangePassword", $"Изменен пароль для пользователя: {user.Username}", "User", userId);
        }

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.MaintenanceRequests)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("Пользователь не найден");

            // Проверка: нельзя удалить пользователя с заявками
            if (user.MaintenanceRequests.Any())
                throw new InvalidOperationException("Невозможно удалить пользователя, у которого есть заявки на обслуживание");

            // Проверка: нельзя удалить последнего администратора
            if (user.Role == UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin && u.IsActive);
                if (adminCount <= 1)
                    throw new InvalidOperationException("Невозможно удалить последнего администратора");
            }

            // Логирование перед удалением
            await UserActivityLogger.LogAsync("UserDelete", $"Удален пользователь: {user.Username}", "User", userId);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Проверить существование пользователя
        /// </summary>
        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        /// <summary>
        /// Получить статистику пользователей
        /// </summary>
        public async Task<Dictionary<string, int>> GetUsersStatisticsAsync()
        {
            var total = await _context.Users.CountAsync();
            var active = await _context.Users.CountAsync(u => u.IsActive);
            var admins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            var users = await _context.Users.CountAsync(u => u.Role == UserRole.User);

            return new Dictionary<string, int>
            {
                { "Total", total },
                { "Active", active },
                { "Admins", admins },
                { "Users", users }
            };
        }
    }
}
