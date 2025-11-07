using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для управления подключением к базе данных (сервер/локальная)
    /// </summary>
    public static class DatabaseConnectionService
    {
        private static string? _connectionString;
        private static DateTime _lastConnectionCheck = DateTime.MinValue;
        private const int ConnectionCheckIntervalSeconds = 30;

        /// <summary>
        /// Получить строку подключения к серверной БД
        /// </summary>
        public static string GetServerConnectionString()
        {
            if (_connectionString != null)
                return _connectionString;

            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "settings.json");

            if (File.Exists(settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (settings != null)
                    {
                        var server = settings.ContainsKey("Server") ? settings["Server"]?.ToString() ?? "localhost" : "localhost";
                        var port = settings.ContainsKey("Port") ? settings["Port"]?.ToString() ?? "3306" : "3306";
                        var database = settings.ContainsKey("Database") ? settings["Database"]?.ToString() ?? "NetworkMonitoringDB" : "NetworkMonitoringDB";
                        var user = settings.ContainsKey("User") ? settings["User"]?.ToString() ?? "root" : "root";
                        var password = settings.ContainsKey("Password") ? settings["Password"]?.ToString() ?? "" : "";
                        
                        _connectionString = $"Server={server};Database={database};User={user};Password={password};Port={port};";
                        return _connectionString;
                    }
                }
                catch { }
            }

            // Значения по умолчанию
            _connectionString = "Server=localhost;Database=NetworkMonitoringDB;User=root;Password=your_password;Port=3306;";
            return _connectionString;
        }


        /// <summary>
        /// Проверить подключение к серверной БД
        /// </summary>
        public static bool TestServerConnection(string? connectionString = null)
        {
            try
            {
                var connString = connectionString ?? GetServerConnectionString();
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseMySql(connString, new MySqlServerVersion(new Version(8, 0, 21)))
                    .Options;

                using var context = new ApplicationDbContext(options);
                var canConnect = context.Database.CanConnect();
                return canConnect;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получить информацию о текущем подключении
        /// </summary>
        public static ConnectionInfo GetConnectionInfo()
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "settings.json");

            if (File.Exists(settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (settings != null)
                    {
                        var server = settings.ContainsKey("Server") ? settings["Server"]?.ToString() ?? "localhost" : "localhost";
                        var port = settings.ContainsKey("Port") ? settings["Port"]?.ToString() ?? "3306" : "3306";
                        var database = settings.ContainsKey("Database") ? settings["Database"]?.ToString() ?? "NetworkMonitoringDB" : "NetworkMonitoringDB";
                        var user = settings.ContainsKey("User") ? settings["User"]?.ToString() ?? "root" : "root";

                        var isReachable = ConnectionService.IsMySqlServerReachable(server, int.Parse(port));
                        var isConnected = ConnectionService.IsMySqlOnline();

                        return new ConnectionInfo
                        {
                            Server = server,
                            Port = int.Parse(port),
                            Database = database,
                            User = user,
                            IsServerReachable = isReachable,
                            IsConnected = isConnected,
                            ConnectionType = isReachable && isConnected ? "Серверная БД" : "Локальная БД (SQLite)"
                        };
                    }
                }
                catch { }
            }

            return new ConnectionInfo
            {
                Server = "localhost",
                Port = 3306,
                Database = "NetworkMonitoringDB",
                User = "root",
                IsServerReachable = false,
                IsConnected = false,
                ConnectionType = "Локальная БД (SQLite)"
            };
        }

        /// <summary>
        /// Сбросить кеш строки подключения
        /// </summary>
        public static void ResetConnectionString()
        {
            _connectionString = null;
        }

        public class ConnectionInfo
        {
            public string Server { get; set; } = string.Empty;
            public int Port { get; set; }
            public string Database { get; set; } = string.Empty;
            public string User { get; set; } = string.Empty;
            public bool IsServerReachable { get; set; }
            public bool IsConnected { get; set; }
            public string ConnectionType { get; set; } = string.Empty;
        }
    }
}

