using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма статистики системы
    /// </summary>
    public partial class StatisticsForm : Form
    {
        private Panel panelStats;
        private Button btnExport;
        private Button btnRefresh;
        private Label lblLastUpdate;

        public StatisticsForm()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Статистика";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Панель кнопок
            var panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(15, 12, 15, 12)
            };

            btnRefresh = CreateButton("🔄 Обновить", Color.FromArgb(52, 152, 219), 0);
            btnRefresh.Click += (s, e) => LoadStatistics();
            ToolTipHelper.SetToolTip(btnRefresh, "Обновить статистику");

            btnExport = CreateButton("📊 Экспорт", Color.FromArgb(46, 204, 113), 130);
            btnExport.Click += (s, e) => ExportStatistics();
            ToolTipHelper.SetToolTip(btnExport, "Экспортировать статистику в файл");

            panelButtons.Controls.AddRange(new Control[] { btnRefresh, btnExport });

            // Информация о последнем обновлении
            lblLastUpdate = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 5, 15, 0),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(127, 140, 141),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            // Панель статистики
            panelStats = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(25, 20, 25, 25),
                BackColor = Color.White
            };

            this.Controls.Add(panelStats);
            this.Controls.Add(lblLastUpdate);
            this.Controls.Add(panelButtons);
            this.ResumeLayout(false);
        }

        private Button CreateButton(string text, Color color, int x)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 0),
                Size = new Size(130, 32),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(
                    Math.Min(255, color.R + 15),
                    Math.Min(255, color.G + 15),
                    Math.Min(255, color.B + 15));
            };
            btn.MouseLeave += (s, e) => btn.BackColor = color;
            return btn;
        }

        private async void LoadStatistics()
        {
            try
            {
                panelStats.Controls.Clear();

                var devices = await Services.DataService.GetDevicesAsync();
                var requests = await Services.DataService.GetMaintenanceRequestsAsync();
                var users = await Services.DataService.GetUsersAsync();

                var total = devices.Count;
                var online = devices.Count(d => d.Status == DeviceStatus.Online);
                var offline = devices.Count(d => d.Status == DeviceStatus.Offline);
                var degraded = devices.Count(d => d.Status == DeviceStatus.Degraded);
                var maintenance = devices.Count(d => d.Status == DeviceStatus.Maintenance);

                int y = 0;

                // Заголовок
                var lblTitle = new Label
                {
                    Text = "СТАТИСТИКА СИСТЕМЫ",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(0, y),
                    AutoSize = true
                };
                panelStats.Controls.Add(lblTitle);
                y += 45;

                // Общая статистика устройств
                var lblDevicesTitle = new Label
                {
                    Text = "УСТРОЙСТВА",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(0, y),
                    AutoSize = true
                };
                panelStats.Controls.Add(lblDevicesTitle);
                y += 35;

                var deviceStats = new[]
                {
                    ("Всего устройств", total, Color.FromArgb(52, 73, 94)),
                    ("🟢 Online", online, Color.FromArgb(46, 204, 113)),
                    ("🔴 Offline", offline, Color.FromArgb(231, 76, 60)),
                    ("🟡 Degraded", degraded, Color.FromArgb(241, 196, 15)),
                    ("🔵 Maintenance", maintenance, Color.FromArgb(155, 89, 182))
                };

                foreach (var (name, count, color) in deviceStats)
                {
                    var statPanel = CreateStatCard(name, count, color, 0, y);
                    panelStats.Controls.Add(statPanel);
                    y += 65;
                }

                y += 20;

                // По типам устройств
                var lblTypesTitle = new Label
                {
                    Text = "РАСПРЕДЕЛЕНИЕ ПО ТИПАМ",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(0, y),
                    AutoSize = true
                };
                panelStats.Controls.Add(lblTypesTitle);
                y += 35;

                var typeNames = new Dictionary<DeviceType, string>
                {
                    { DeviceType.Router, "Роутер" },
                    { DeviceType.Switch, "Коммутатор" },
                    { DeviceType.Phone, "Телефон" },
                    { DeviceType.Modem, "Модем" },
                    { DeviceType.Computer, "Компьютер" },
                    { DeviceType.Server, "Сервер" },
                    { DeviceType.AccessPoint, "Точка доступа" },
                    { DeviceType.Printer, "Принтер" },
                    { DeviceType.Scanner, "Сканер" },
                    { DeviceType.Camera, "Камера" },
                    { DeviceType.Firewall, "МЭ" },
                    { DeviceType.Other, "Другое" }
                };

                foreach (DeviceType type in Enum.GetValues(typeof(DeviceType)))
                {
                    var count = devices.Count(d => d.DeviceType == type);
                    if (count > 0)
                    {
                        var typeName = typeNames.ContainsKey(type) ? typeNames[type] : type.ToString();
                        var statPanel = CreateStatCard(typeName, count, Color.FromArgb(127, 140, 141), 20, y);
                        panelStats.Controls.Add(statPanel);
                        y += 55;
                    }
                }

                y += 20;

                // Заявки на обслуживание
                var lblRequestsTitle = new Label
                {
                    Text = "ЗАЯВКИ НА ОБСЛУЖИВАНИЕ",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(0, y),
                    AutoSize = true
                };
                panelStats.Controls.Add(lblRequestsTitle);
                y += 35;

                var totalRequests = requests.Count;
                var pendingRequests = requests.Count(r => r.Status == RequestStatus.Pending);
                var inProgressRequests = requests.Count(r => r.Status == RequestStatus.InProgress);
                var completedRequests = requests.Count(r => r.Status == RequestStatus.Completed);

                var requestStats = new[]
                {
                    ("Всего заявок", totalRequests, Color.FromArgb(52, 73, 94)),
                    ("Ожидает", pendingRequests, Color.FromArgb(241, 196, 15)),
                    ("В работе", inProgressRequests, Color.FromArgb(52, 152, 219)),
                    ("Завершено", completedRequests, Color.FromArgb(46, 204, 113))
                };

                foreach (var (name, count, color) in requestStats)
                {
                    var statPanel = CreateStatCard(name, count, color, 0, y);
                    panelStats.Controls.Add(statPanel);
                    y += 65;
                }

                y += 20;

                // Пользователи
                var lblUsersTitle = new Label
                {
                    Text = "ПОЛЬЗОВАТЕЛИ",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(0, y),
                    AutoSize = true
                };
                panelStats.Controls.Add(lblUsersTitle);
                y += 35;

                var totalUsers = users.Count;
                var activeUsers = users.Count(u => u.IsActive);
                var adminUsers = users.Count(u => u.Role == UserRole.Admin);
                var regularUsers = users.Count(u => u.Role == UserRole.User);

                var userStats = new[]
                {
                    ("Всего пользователей", totalUsers, Color.FromArgb(52, 73, 94)),
                    ("Активных", activeUsers, Color.FromArgb(46, 204, 113)),
                    ("Администраторов", adminUsers, Color.FromArgb(155, 89, 182)),
                    ("Пользователей", regularUsers, Color.FromArgb(52, 152, 219))
                };

                foreach (var (name, count, color) in userStats)
                {
                    var statPanel = CreateStatCard(name, count, color, 0, y);
                    panelStats.Controls.Add(statPanel);
                    y += 65;
                }

                lblLastUpdate.Text = $"Последнее обновление: {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Загрузка статистики");
            }
        }

        private Panel CreateStatCard(string name, int count, Color color, int indent, int y)
        {
            var panel = new Panel
            {
                Location = new Point(indent, y),
                Size = new Size(450, 55),
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 10, 15, 10)
            };

            var lblName = new Label
            {
                Text = name,
                Location = new Point(0, 15),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true
            };

            var lblCount = new Label
            {
                Text = count.ToString(),
                Location = new Point(350, 12),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = color,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblCount);
            return panel;
        }

        private async void ExportStatistics()
        {
            try
            {
                UserActivityLogger.Log("Statistics Export", "Started");
                var exportService = new Services.ExportService(Program.DbContext);
                var stats = await exportService.ExportStatisticsAsync();

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    FileName = $"Статистика_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    Title = "Сохранить статистику"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, stats, Encoding.UTF8);
                    UserActivityLogger.Log("Statistics Export", $"File: {saveDialog.FileName}");
                    Helpers.ErrorHandler.ShowSuccess($"Статистика экспортирована в файл:\n{saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Экспорт статистики");
            }
        }
    }
}
