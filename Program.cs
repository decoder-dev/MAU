using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.UI.Forms;
using NetworkMonitoringSystem.Core.Models;
using NetworkMonitoringSystem.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using NetworkMonitoringSystem.UI.Forms.Welcome;
using System.IO;
using System.Text.Json;

namespace NetworkMonitoringSystem
{
    internal static class Program
    {
        private static ApplicationDbContext? _context;
        private static OfflineDbContext? _offlineContext;
        private static User? _currentUser;
        private static DeviceMonitoringService? _monitoringService;
        private static SyncService? _syncService;
        private static System.Windows.Forms.Timer? _syncTimer;
        
        // Внутренние поля для доступа из MainForm
        internal static DeviceMonitoringService? MonitoringService => _monitoringService;
        internal static System.Windows.Forms.Timer? SyncTimer => _syncTimer;
        internal static ApplicationDbContext? Context => _context;
        internal static OfflineDbContext? OfflineContext => _offlineContext;

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public static User? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        /// <summary>
        /// Контекст базы данных (онлайн)
        /// </summary>
        public static ApplicationDbContext DbContext
        {
            get
            {
                if (_context == null)
                {
                    var connectionString = GetConnectionString();
                    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)))
                        .Options;
                    _context = new ApplicationDbContext(options);
                }
                return _context;
            }
        }

        /// <summary>
        /// Получить строку подключения из настроек
        /// </summary>
        private static string GetConnectionString()
        {
            return DatabaseConnectionService.GetServerConnectionString();
        }

        /// <summary>
        /// Контекст офлайн базы данных
        /// </summary>
        public static OfflineDbContext OfflineDbContext
        {
            get
            {
                if (_offlineContext == null)
                {
                    _offlineContext = new OfflineDbContext();
                    _offlineContext.Database.EnsureCreated();
                }
                return _offlineContext;
            }
        }

        /// <summary>
        /// Активный контекст (онлайн или офлайн)
        /// </summary>
        public static ApplicationDbContext ActiveDbContext
        {
            get
            {
                if (ConnectionService.IsMySqlOnline())
                    return DbContext;
                else
                    throw new InvalidOperationException("Используйте методы работы с БД через DataService");
            }
        }

        /// <summary>
        /// Проверка является ли текущий пользователь администратором
        /// </summary>
        public static bool IsAdmin => CurrentUser?.Role == Core.Models.UserRole.Admin;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Инициализация баз данных
            try
            {
                // Инициализация офлайн базы
                OfflineDbContext.Database.EnsureCreated();

                // Проверка подключения к MySQL
                if (ConnectionService.IsMySqlOnline())
                {
                    try
                    {
                        DbContext.Database.Migrate();
                        
                        // Создание администратора по умолчанию
                        var authService = new AuthenticationService(DbContext);
                        authService.CreateDefaultAdminAsync().Wait();

                        // Заполнение начальными данными
                        DatabaseSeeder.SeedAsync(DbContext).Wait();

                        // Синхронизация с офлайн базой
                        _syncService = new SyncService(DbContext, OfflineDbContext);
                        _syncService.SyncDevicesFromOnlineAsync().Wait();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка подключения к MySQL: {ex.Message}");
                        MessageBox.Show(
                            "Не удалось подключиться к базе данных MySQL.\n" +
                            "Приложение будет работать в офлайн режиме.\n\n" +
                            "Данные будут синхронизированы при восстановлении подключения.",
                            "Офлайн режим",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "База данных MySQL недоступна.\n" +
                        "Приложение работает в офлайн режиме.\n\n" +
                        "Данные будут синхронизированы при восстановлении подключения.",
                        "Офлайн режим",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                // Инициализация сервиса синхронизации
                if (_syncService == null && ConnectionService.IsMySqlOnline())
                {
                    _syncService = new SyncService(DbContext, OfflineDbContext);
                }

                // Запуск периодической синхронизации
                _syncTimer = new System.Windows.Forms.Timer();
                _syncTimer.Interval = 60000; // Каждую минуту
                _syncTimer.Tick += async (s, e) =>
                {
                    if (ConnectionService.IsMySqlOnline() && _syncService != null)
                    {
                        try
                        {
                            await _syncService.SyncPendingOperationsAsync();
                            await _syncService.SyncDevicesFromOnlineAsync();
                        }
                        catch { }
                    }
                };
                _syncTimer.Start();

                // Запуск мониторинга устройств (только если есть подключение)
                if (ConnectionService.IsMySqlOnline())
                {
                    _monitoringService = new DeviceMonitoringService(DbContext);
                    _monitoringService.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Показываем форму входа
            var loginForm = new LoginForm();
            loginForm.ShowDialog();

            // Если вход успешен, показываем welcome форму (только при первом запуске)
            if (Program.CurrentUser != null)
            {
                var welcomeFlagPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkMonitoringSystem", "welcome_shown.flag");
                var welcomeDir = Path.GetDirectoryName(welcomeFlagPath);
                if (!string.IsNullOrEmpty(welcomeDir) && !Directory.Exists(welcomeDir))
                    Directory.CreateDirectory(welcomeDir);

                var showWelcome = !File.Exists(welcomeFlagPath);
                if (showWelcome)
                {
                    var welcomeForm = new WelcomeForm();
                    if (welcomeForm.ShowDialog() == DialogResult.OK)
                    {
                        File.Create(welcomeFlagPath).Close();
                    }
                }

                // Показываем главную форму
                Application.Run(new MainForm());
            }

            // Остановка мониторинга и синхронизации при выходе
            try
            {
                _syncTimer?.Stop();
                _syncTimer?.Dispose();
                _monitoringService?.Stop();
                _context?.Dispose();
                _offlineContext?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при завершении приложения: {ex.Message}");
            }
        }
    }
}
