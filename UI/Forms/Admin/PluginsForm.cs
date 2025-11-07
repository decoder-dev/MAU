using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление плагинами
    /// </summary>
    public partial class PluginsForm : Form
    {
        private DataGridView dgvPlugins;
        private Button btnInstall;
        private Button btnUninstall;
        private Button btnRefresh;

        public PluginsForm()
        {
            InitializeComponent();
            LoadPlugins();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление плагинами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnInstall = new Button { Text = "Установить", Location = new Point(15, 10), Width = 100 };
            btnUninstall = new Button { Text = "Удалить", Location = new Point(125, 10), Width = 100 };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnInstall, btnUninstall, btnRefresh });

            dgvPlugins = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvPlugins.Columns.Add("Plugin", "Плагин");
            dgvPlugins.Columns.Add("Version", "Версия");
            dgvPlugins.Columns.Add("Author", "Автор");
            dgvPlugins.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvPlugins);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadPlugins()
        {
            try
            {
                dgvPlugins.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var plugins = config.GetValue<List<object>>("Plugins", new List<object>());
                if (plugins != null)
                {
                    foreach (var plugin in plugins.Take(50))
                    {
                        if (plugin is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var version = element.TryGetProperty("Version", out var v) ? v.GetString() ?? "-" : "-";
                            var status = element.TryGetProperty("Enabled", out var e) && e.GetBoolean() ? "Включен" : "Выключен";
                            dgvPlugins.Rows.Add(name, version, status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка плагинов");
            }
        }
    }
}
