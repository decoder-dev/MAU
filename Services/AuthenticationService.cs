using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис аутентификации
    /// </summary>
    public class AuthenticationService : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Вход пользователя
        /// </summary>
        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !user.VerifyPassword(password))
            {
                UserActivityLogger.LogLogin(username, false);
                return null;
            }

            // Обновляем время последнего входа
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            UserActivityLogger.LogLogin(username, true);
            return user;
        }

        /// <summary>
        /// Создание пользователя по умолчанию (админ)
        /// </summary>
        public async Task CreateDefaultAdminAsync()
        {
            if (!await _context.Users.AnyAsync(u => u.Username == "admin"))
            {
                var admin = new User
                {
                    Username = "admin",
                    FullName = "Системный администратор",
                    Email = "admin@mau.ru",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedBy = "System"
                };
                admin.SetPassword("admin"); // Используем метод для установки пароля
                
                _context.Users.Add(admin);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public async Task<User> RegisterAsync(string username, string password, string fullName, string? email = null)
        {
            // Проверка существования пользователя
            if (await _context.Users.AnyAsync(u => u.Username == username))
                throw new InvalidOperationException($"Пользователь с именем '{username}' уже существует");

            // Проверка email (если указан)
            if (!string.IsNullOrEmpty(email) && await _context.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException($"Пользователь с email '{email}' уже существует");

            var user = new User
            {
                Username = username,
                FullName = fullName,
                Email = email,
                Role = UserRole.User, // По умолчанию обычный пользователь
                IsActive = true,
                CreatedBy = Program.CurrentUser?.FullName ?? "System"
            };

            user.SetPassword(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Логирование
            await UserActivityLogger.LogAsync("UserRegister", $"Зарегистрирован новый пользователь: {username}", "User", user.Id);

            return user;
        }
    }
}


