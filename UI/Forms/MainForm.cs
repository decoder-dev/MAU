using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms
{
    public partial class MainForm : Form
    {
        private Panel panelMenu;
        private Panel panelContent;
        private Button btnDevices;
        private Button btnMap;
        private Button btnMaintenance;
        private Button btnStatistics;
        private Button btnUsers;
        private Button btnHelp;
        private Button btnLogout;
        private Label lblWelcome;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private Form? currentForm;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            LoadDevicesForm();
            
            // Обработчик закрытия формы
            this.FormClosing += MainForm_FormClosing;
        }
        
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Освобождаем текущую форму
            if (currentForm != null)
            {
                currentForm.Close();
                currentForm.Dispose();
                currentForm = null;
            }
            
            // Останавливаем все сервисы и освобождаем ресурсы
            CleanupResources();
            
            // Завершаем процесс приложения полностью
            Environment.Exit(0);
        }
        
        private void CleanupResources()
        {
            try
            {
                // Останавливаем мониторинг устройств
                if (Program.MonitoringService != null)
                {
                    Program.MonitoringService.Stop();
                }
                
                // Останавливаем таймер синхронизации
                if (Program.SyncTimer != null)
                {
                    Program.SyncTimer.Stop();
                    Program.SyncTimer.Dispose();
                }
                
                // Освобождаем контексты БД
                if (Program.Context != null)
                {
                    Program.Context.Dispose();
                }
                
                if (Program.OfflineContext != null)
                {
                    Program.OfflineContext.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при освобождении ресурсов: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы
            this.Text = $"{Core.Models.UniversityInfo.ShortName} - Система мониторинга сетевого оборудования";
            this.Icon = SystemIcons.Application;
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Меню слева
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(10)
            };

            // Приветствие
            lblWelcome = new Label
            {
                Text = $"Добро пожаловать,\n{Program.CurrentUser?.FullName ?? "Пользователь"}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 20),
                AutoSize = false,
                Size = new Size(230, 50),
                TextAlign = ContentAlignment.TopLeft
            };

            // Кнопки меню
            btnDevices = CreateMenuButton("📡 Устройства", 80);
            btnDevices.Click += (s, e) => LoadDevicesForm();

            btnMap = CreateMenuButton("🗺️ Карта", 130);
            btnMap.Click += (s, e) => LoadMapForm();

            btnMaintenance = CreateMenuButton("🔧 Заявки на обслуживание", 180);
            btnMaintenance.Click += (s, e) => LoadMaintenanceForm();

            btnStatistics = CreateMenuButton("📊 Статистика", 230);
            btnStatistics.Click += (s, e) => LoadStatisticsForm();

            if (Program.IsAdmin)
            {
                btnUsers = CreateMenuButton("👥 Пользователи", 280);
                btnUsers.Click += (s, e) => LoadUsersForm();
            }

            btnHelp = CreateMenuButton("❓ Справка", Program.IsAdmin ? 330 : 280);
            btnHelp.BackColor = Color.FromArgb(52, 152, 219);
            btnHelp.Click += (s, e) => ShowHelp();

            var btnSettings = CreateMenuButton("⚙️ Настройки", Program.IsAdmin ? 380 : 330);
            btnSettings.BackColor = Color.FromArgb(149, 165, 166);
            btnSettings.Click += (s, e) => ShowSettings();

            btnLogout = CreateMenuButton("🚪 Выход", Program.IsAdmin ? 430 : 380);
            btnLogout.BackColor = Color.FromArgb(231, 76, 60);
            btnLogout.Click += (s, e) => {
                if (ErrorHandler.ShowConfirm("Вы уверены, что хотите выйти из системы?", "Выход"))
                {
                    UserActivityLogger.Log("Logout", "User logged out");
                    Program.CurrentUser = null;
                    // Освобождаем ресурсы и закрываем приложение
                    CleanupResources();
                    Environment.Exit(0);
                }
            };

            panelMenu.Controls.Add(lblWelcome);
            panelMenu.Controls.Add(btnDevices);
            panelMenu.Controls.Add(btnMap);
            panelMenu.Controls.Add(btnMaintenance);
            panelMenu.Controls.Add(btnStatistics);
            if (Program.IsAdmin)
                panelMenu.Controls.Add(btnUsers);
            panelMenu.Controls.Add(btnHelp);
            panelMenu.Controls.Add(btnSettings);
            panelMenu.Controls.Add(btnLogout);

            // Панель контента
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Строка статуса
            statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom
            };
            lblStatus = new ToolStripStatusLabel
            {
                Text = "Готово к работе",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            var lblConnectionStatus = new ToolStripStatusLabel
            {
                Text = GetConnectionStatusText(),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 150
            };

            var lblUserInfo = new ToolStripStatusLabel
            {
                Text = $"Пользователь: {Program.CurrentUser?.FullName ?? "Неизвестно"}",
                TextAlign = ContentAlignment.MiddleRight
            };
            
            // Таймер для обновления статуса подключения
            var connectionTimer = new System.Windows.Forms.Timer();
            connectionTimer.Interval = 5000; // Каждые 5 секунд
            connectionTimer.Tick += (s, e) =>
            {
                lblConnectionStatus.Text = GetConnectionStatusText();
                var isOnline = Services.ConnectionService.IsMySqlOnline();
                lblConnectionStatus.ForeColor = isOnline ? Color.Green : Color.Red;
            };
            connectionTimer.Start();

            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblConnectionStatus, lblUserInfo });

            this.Controls.Add(panelMenu);
            this.Controls.Add(panelContent);
            this.Controls.Add(statusStrip);
            this.ResumeLayout(false);
        }

        private Button CreateMenuButton(string text, int y)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(44, 62, 80),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(230, 40),
                Location = new Point(10, y),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(52, 152, 219);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(44, 62, 80);
            return btn;
        }

        private void SetupUI()
        {
            // Дополнительные настройки
        }

        private void LoadDevicesForm()
        {
            LoadForm(new DevicesForm());
            UpdateStatus("Открыт раздел: Устройства");
        }

        private void LoadMapForm()
        {
            LoadForm(new MapForm());
            UpdateStatus("Открыт раздел: Карта устройств");
        }

        private void LoadMaintenanceForm()
        {
            LoadForm(new MaintenanceRequestsForm());
            UpdateStatus("Открыт раздел: Заявки на обслуживание");
        }

        private void LoadUsersForm()
        {
            if (Program.IsAdmin)
            {
                LoadForm(new UsersForm());
                UpdateStatus("Открыт раздел: Управление пользователями");
            }
        }

        private void LoadStatisticsForm()
        {
            LoadForm(new StatisticsForm());
            UpdateStatus("Открыт раздел: Статистика");
        }

        private void ShowHelp()
        {
            var helpForm = new HelpForm();
            helpForm.ShowDialog();
        }

        private void ShowSettings()
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
                lblStatus.Text = message;
        }

        private string GetConnectionStatusText()
        {
            var isOnline = Services.ConnectionService.IsMySqlOnline();
            if (isOnline)
            {
                var pendingCount = Services.DataService.GetPendingOperationsCountAsync().Result;
                if (pendingCount > 0)
                    return $"🟢 Онлайн ({pendingCount} на синхронизации)";
                return "🟢 Онлайн";
            }
            else
            {
                var pendingCount = Services.DataService.GetPendingOperationsCountAsync().Result;
                if (pendingCount > 0)
                    return $"🔴 Офлайн ({pendingCount} операций)";
                return "🔴 Офлайн";
            }
        }

        private void LoadForm(Form form)
        {
            if (currentForm != null)
            {
                try
                {
                    currentForm.FormClosing -= CurrentForm_FormClosing; // Убираем обработчик если был
                    currentForm.Close();
                    currentForm.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при закрытии формы: {ex.Message}");
                }
            }

            currentForm = form;
            currentForm.TopLevel = false;
            currentForm.FormBorderStyle = FormBorderStyle.None;
            currentForm.Dock = DockStyle.Fill;
            currentForm.FormClosing += CurrentForm_FormClosing; // Предотвращаем закрытие вложенных форм
            panelContent.Controls.Clear();
            panelContent.Controls.Add(currentForm);
            currentForm.Show();

            // Обновляем статус
            UpdateStatus($"Открыт раздел: {currentForm.Text}");
        }
        
        private void CurrentForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Предотвращаем закрытие вложенных форм через X
            // Они закрываются только при загрузке другой формы
            e.Cancel = true;
        }
    }
}
