using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;
using System.Net.NetworkInformation;
using System.Net;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для проверки подключения к базе данных
    /// </summary>
    public class ConnectionService
    {
        private static bool _isOnline = true;
        private static DateTime _lastCheck = DateTime.MinValue;
        private const int CheckIntervalSeconds = 10;

        /// <summary>
        /// Проверка подключения к MySQL базе данных
        /// </summary>
        public static bool IsMySqlOnline()
        {
            // Кешируем результат на 10 секунд
            if ((DateTime.Now - _lastCheck).TotalSeconds < CheckIntervalSeconds)
                return _isOnline;

            try
            {
                // Создаем новый контекст для проверки подключения
                var connectionString = DatabaseConnectionService.GetServerConnectionString();
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseMySql(connectionString, serverVersion)
                    .Options;
                
                using var context = new ApplicationDbContext(options);
                // Простой запрос для проверки подключения
                var canConnect = context.Database.CanConnect();
                _isOnline = canConnect;
                _lastCheck = DateTime.Now;
                return canConnect;
            }
            catch
            {
                _isOnline = false;
                _lastCheck = DateTime.Now;
                return false;
            }
        }

        /// <summary>
        /// Проверка доступности сервера MySQL по сети
        /// </summary>
        public static bool IsMySqlServerReachable(string host, int port = 3306)
        {
            try
            {
                using var tcpClient = new System.Net.Sockets.TcpClient();
                var result = tcpClient.BeginConnect(host, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                if (!success)
                    return false;
                tcpClient.EndConnect(result);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Принудительная проверка подключения
        /// </summary>
        public static bool CheckConnection()
        {
            _lastCheck = DateTime.MinValue;
            return IsMySqlOnline();
        }
    }
}


