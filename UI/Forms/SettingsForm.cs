using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Helpers;
using System.IO;
using System.Text.Json;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма настроек приложения
    /// </summary>
    public partial class SettingsForm : Form
    {
        private TabControl tabControl;
        private TextBox txtDbServer;
        private TextBox txtDbPort;
        private TextBox txtDbDatabase;
        private TextBox txtDbUser;
        private TextBox txtDbPassword;
        private CheckBox chkDbShowPassword;
        private Button btnTestConnection;
        private Button btnSaveSettings;
        private Button btnCancel;
        private Label lblConnectionStatus;

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Настройки - МАУ";
            this.Size = new Size(650, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Icon = SystemIcons.Application;

            // Основной контейнер
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 2
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));

            // TabControl для вкладок настроек
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Padding = new Point(10, 5)
            };

            // Вкладка "База данных"
            var tabDatabase = new TabPage("База данных");
            tabDatabase.Padding = new Padding(20);
            var dbPanel = CreateDatabaseSettingsPanel();
            tabDatabase.Controls.Add(dbPanel);
            tabControl.TabPages.Add(tabDatabase);

            // Вкладка "Общие"
            var tabGeneral = new TabPage("Общие");
            tabGeneral.Padding = new Padding(20);
            var generalPanel = CreateGeneralSettingsPanel();
            tabGeneral.Controls.Add(generalPanel);
            tabControl.TabPages.Add(tabGeneral);

            // Вкладка "О программе"
            var tabAbout = new TabPage("О программе");
            tabAbout.Padding = new Padding(20);
            var aboutPanel = CreateAboutPanel();
            tabAbout.Controls.Add(aboutPanel);
            tabControl.TabPages.Add(tabAbout);

            mainPanel.Controls.Add(tabControl, 0, 0);

            // Панель кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 55
            };

            btnSaveSettings = new Button
            {
                Text = "Сохранить",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 38),
                Location = new Point(380, 10),
                Cursor = Cursors.Hand
            };
            btnSaveSettings.FlatAppearance.BorderSize = 0;
            btnSaveSettings.Click += BtnSaveSettings_Click;
            btnSaveSettings.MouseEnter += (s, e) => btnSaveSettings.BackColor = Color.FromArgb(39, 174, 96);
            btnSaveSettings.MouseLeave += (s, e) => btnSaveSettings.BackColor = Color.FromArgb(46, 204, 113);

            btnCancel = new Button
            {
                Text = "Отмена",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(149, 165, 166),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 38),
                Location = new Point(520, 10),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.FromArgb(127, 140, 141);
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.FromArgb(149, 165, 166);

            buttonPanel.Controls.Add(btnSaveSettings);
            buttonPanel.Controls.Add(btnCancel);

            mainPanel.Controls.Add(buttonPanel, 0, 1);

            this.Controls.Add(mainPanel);
            this.ResumeLayout(false);
        }

        private Panel CreateDatabaseSettingsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 0, 5, 0)
            };

            var tableLayout = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 8
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int row = 0;

            // Заголовок
            var lblTitle = new Label
            {
                Text = "Настройки подключения к базе данных MySQL",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 15)
            };
            tableLayout.SetColumnSpan(lblTitle, 2);
            tableLayout.Controls.Add(lblTitle, 0, row++);
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));

            // Сервер
            AddSettingRow(tableLayout, "Сервер:", ref txtDbServer, "Введите адрес MySQL сервера (например: localhost или 192.168.1.100)", ref row);
            
            // Порт
            AddSettingRow(tableLayout, "Порт:", ref txtDbPort, "Порт MySQL сервера (по умолчанию: 3306)", ref row);
            
            // База данных
            AddSettingRow(tableLayout, "База данных:", ref txtDbDatabase, "Название базы данных", ref row);
            
            // Пользователь
            AddSettingRow(tableLayout, "Пользователь:", ref txtDbUser, "Имя пользователя MySQL", ref row);
            
            // Пароль
            var lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            tableLayout.Controls.Add(lblPassword, 0, row);
            var passwordPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5)
            };
            txtDbPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Left,
                UseSystemPasswordChar = true,
                Height = 32,
                Width = passwordPanel.Width - 100,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            chkDbShowPassword = new CheckBox
            {
                Text = "Показать",
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Right,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            chkDbShowPassword.CheckedChanged += (s, e) => txtDbPassword.UseSystemPasswordChar = !chkDbShowPassword.Checked;
            passwordPanel.Controls.Add(txtDbPassword);
            passwordPanel.Controls.Add(chkDbShowPassword);
            tableLayout.Controls.Add(passwordPanel, 1, row++);
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            // Кнопка проверки подключения
            btnTestConnection = new Button
            {
                Text = "Проверить подключение",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Height = 38,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 0, 10)
            };
            btnTestConnection.FlatAppearance.BorderSize = 0;
            btnTestConnection.Click += BtnTestConnection_Click;
            btnTestConnection.MouseEnter += (s, e) => btnTestConnection.BackColor = Color.FromArgb(41, 128, 185);
            btnTestConnection.MouseLeave += (s, e) => btnTestConnection.BackColor = Color.FromArgb(52, 152, 219);
            tableLayout.SetColumnSpan(btnTestConnection, 2);
            tableLayout.Controls.Add(btnTestConnection, 0, row++);
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

            // Статус подключения
            lblConnectionStatus = new Label
            {
                Text = "Статус: проверка...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Height = 60
            };
            tableLayout.SetColumnSpan(lblConnectionStatus, 2);
            tableLayout.Controls.Add(lblConnectionStatus, 0, row++);
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            // Информация
            var lblInfo = new Label
            {
                Text = "Примечание: Если сервер недоступен, приложение автоматически\nпереключится на локальную базу данных (SQLite).\nВсе изменения будут синхронизированы при восстановлении подключения.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = false,
                Height = 70
            };
            tableLayout.SetColumnSpan(lblInfo, 2);
            tableLayout.Controls.Add(lblInfo, 0, row++);
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));

            panel.Controls.Add(tableLayout);
            tableLayout.Location = new Point(0, 0);
            return panel;
        }

        private void AddSettingRow(TableLayoutPanel panel, string labelText, ref TextBox textBox, string tooltip, ref int row)
        {
            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            panel.Controls.Add(label, 0, row);
            textBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            ToolTipHelper.SetToolTip(textBox, tooltip);
            panel.Controls.Add(textBox, 1, row);
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;
        }

        private Panel CreateGeneralSettingsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var lblInfo = new Label
            {
                Text = "Общие настройки приложения\n\nВ данной версии общие настройки находятся в разработке.\nВ будущих версиях здесь будут доступны:\n• Настройки интерфейса\n• Настройки уведомлений\n• Настройки экспорта\n• И другие параметры",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(127, 140, 141),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(lblInfo);
            return panel;
        }

        private Panel CreateAboutPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(20)
            };

            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblTitle = new Label
            {
                Text = Core.Models.UniversityInfo.ShortName,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableLayout.Controls.Add(lblTitle, 0, 0);

            var lblSubtitle = new Label
            {
                Text = "Система мониторинга сетевого оборудования",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(127, 140, 141),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableLayout.Controls.Add(lblSubtitle, 0, 1);

            var lblVersion = new Label
            {
                Text = "Версия: 1.0.0",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableLayout.Controls.Add(lblVersion, 0, 2);

            var lblOrganization = new Label
            {
                Text = Core.Models.UniversityInfo.FullName,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableLayout.Controls.Add(lblOrganization, 0, 3);

            var lblContacts = new Label
            {
                Text = $"Приёмная ректора: {Core.Models.UniversityInfo.RectorOfficePhone}\nEmail: {Core.Models.UniversityInfo.RectorOfficeEmail}\nСайт: {Core.Models.UniversityInfo.Website}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableLayout.Controls.Add(lblContacts, 0, 4);

            var lblCopyright = new Label
            {
                Text = $"© {DateTime.Now.Year} {Core.Models.UniversityInfo.ShortName}. Все права защищены.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableLayout.Controls.Add(lblCopyright, 0, 5);

            var lblDescription = new Label
            {
                Text = "Система для мониторинга и управления сетевым оборудованием университета.\nПоддерживает работу в онлайн и офлайн режимах.\nАвтоматическое переключение между серверной и локальной БД.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            };
            tableLayout.Controls.Add(lblDescription, 0, 6);

            panel.Controls.Add(tableLayout);
            return panel;
        }

        private void LoadSettings()
        {
            var settingsPath = GetSettingsPath();
            if (File.Exists(settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = JsonSerializer.Deserialize<DatabaseSettings>(json);
                    if (settings != null)
                    {
                        txtDbServer.Text = settings.Server ?? "localhost";
                        txtDbPort.Text = settings.Port?.ToString() ?? "3306";
                        txtDbDatabase.Text = settings.Database ?? "NetworkMonitoringDB";
                        txtDbUser.Text = settings.User ?? "root";
                        txtDbPassword.Text = settings.Password ?? "";
                    }
                }
                catch { }
            }
            else
            {
                // Значения по умолчанию
                txtDbServer.Text = "localhost";
                txtDbPort.Text = "3306";
                txtDbDatabase.Text = "NetworkMonitoringDB";
                txtDbUser.Text = "root";
                txtDbPassword.Text = "";
            }
            
            UpdateConnectionStatus();
        }

        private void UpdateConnectionStatus()
        {
            try
            {
                var connectionInfo = DatabaseConnectionService.GetConnectionInfo();
                lblConnectionStatus.Text = $"Тип подключения: {connectionInfo.ConnectionType}\n" +
                                          $"Сервер: {connectionInfo.Server}:{connectionInfo.Port}\n" +
                                          $"База данных: {connectionInfo.Database}\n" +
                                          $"Пользователь: {connectionInfo.User}\n" +
                                          $"Статус: {(connectionInfo.IsConnected ? "✅ Подключено" : "❌ Не подключено")}";
                lblConnectionStatus.ForeColor = connectionInfo.IsConnected ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
            }
            catch
            {
                lblConnectionStatus.Text = "Не удалось определить статус подключения";
                lblConnectionStatus.ForeColor = Color.FromArgb(127, 140, 141);
            }
        }

        private void BtnTestConnection_Click(object? sender, EventArgs e)
        {
            btnTestConnection.Enabled = false;
            btnTestConnection.Text = "Проверка...";
            lblConnectionStatus.Text = "Проверка подключения к серверу...";
            lblConnectionStatus.ForeColor = Color.FromArgb(241, 196, 15);
            Application.DoEvents();

            try
            {
                // Проверка заполненности полей
                if (string.IsNullOrWhiteSpace(txtDbServer.Text) || 
                    string.IsNullOrWhiteSpace(txtDbDatabase.Text) || 
                    string.IsNullOrWhiteSpace(txtDbUser.Text))
                {
                    lblConnectionStatus.Text = "Ошибка: Заполните все обязательные поля";
                    lblConnectionStatus.ForeColor = Color.FromArgb(231, 76, 60);
                    btnTestConnection.Enabled = true;
                    btnTestConnection.Text = "Проверить подключение";
                    return;
                }

                // Проверка доступности сервера
                var port = int.TryParse(txtDbPort.Text, out var p) ? p : 3306;
                var isReachable = DatabaseConnectionService.IsServerReachable(txtDbServer.Text, port);
                
                if (!isReachable)
                {
                    lblConnectionStatus.Text = $"Сервер {txtDbServer.Text}:{port} недоступен.\nПроверьте сеть или используйте локальную БД.";
                    lblConnectionStatus.ForeColor = Color.FromArgb(231, 76, 60);
                    btnTestConnection.Enabled = true;
                    btnTestConnection.Text = "Проверить подключение";
                    return;
                }

                // Проверка подключения к БД
                var connectionString = $"Server={txtDbServer.Text};Database={txtDbDatabase.Text};User={txtDbUser.Text};Password={txtDbPassword.Text};Port={port};";
                var isConnected = DatabaseConnectionService.TestServerConnection(connectionString);

                if (isConnected)
                {
                    lblConnectionStatus.Text = $"✅ Подключение успешно!\nСервер: {txtDbServer.Text}:{port} | База: {txtDbDatabase.Text}\nНастройки сохранены. Перезапустите приложение для применения.";
                    lblConnectionStatus.ForeColor = Color.FromArgb(46, 204, 113);
                }
                else
                {
                    lblConnectionStatus.Text = $"❌ Не удалось подключиться к БД.\nПроверьте данные для входа (пользователь, пароль, название БД).\nПриложение будет работать с локальной БД.";
                    lblConnectionStatus.ForeColor = Color.FromArgb(241, 196, 15);
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = $"Ошибка: {ex.Message}\nПриложение будет работать с локальной БД (SQLite).";
                lblConnectionStatus.ForeColor = Color.FromArgb(231, 76, 60);
            }
            finally
            {
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Проверить подключение";
            }
        }

        private void BtnSaveSettings_Click(object? sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtDbServer.Text))
            {
                Helpers.ErrorHandler.ShowWarning("Введите адрес сервера", "Ошибка");
                txtDbServer.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDbDatabase.Text))
            {
                Helpers.ErrorHandler.ShowWarning("Введите название базы данных", "Ошибка");
                txtDbDatabase.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDbUser.Text))
            {
                Helpers.ErrorHandler.ShowWarning("Введите имя пользователя", "Ошибка");
                txtDbUser.Focus();
                return;
            }

            if (!int.TryParse(txtDbPort.Text, out var port) || port < 1 || port > 65535)
            {
                Helpers.ErrorHandler.ShowWarning("Неверный порт (должен быть от 1 до 65535)", "Ошибка");
                txtDbPort.Focus();
                return;
            }

            try
            {
                var settings = new DatabaseSettings
                {
                    Server = txtDbServer.Text.Trim(),
                    Port = port,
                    Database = txtDbDatabase.Text.Trim(),
                    User = txtDbUser.Text.Trim(),
                    Password = txtDbPassword.Text
                };

                var settingsPath = GetSettingsPath();
                var settingsDir = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(settingsDir) && !Directory.Exists(settingsDir))
                    Directory.CreateDirectory(settingsDir);

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);

                // Сброс кеша подключения
                DatabaseConnectionService.ResetConnectionString();

                Helpers.ErrorHandler.ShowSuccess(
                    "Настройки сохранены!\n\n" +
                    "Для применения изменений подключения к базе данных перезапустите приложение.\n\n" +
                    "Примечание: Если сервер недоступен, приложение автоматически переключится на локальную БД (SQLite).",
                    "Настройки сохранены");
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }

        private string GetSettingsPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "settings.json");
        }

        private class DatabaseSettings
        {
            public string? Server { get; set; }
            public int? Port { get; set; }
            public string? Database { get; set; }
            public string? User { get; set; }
            public string? Password { get; set; }
        }
    }
}
