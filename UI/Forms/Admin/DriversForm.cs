using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление драйверами
    /// </summary>
    public partial class DriversForm : Form
    {
        private DataGridView dgvDrivers;
        private Button btnInstall;
        private Button btnUpdate;

        public DriversForm()
        {
            InitializeComponent();
            LoadDrivers();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление драйверами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnInstall = new Button { Text = "Установить", Location = new Point(15, 10), Width = 100 };
            btnUpdate = new Button { Text = "Обновить", Location = new Point(125, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnInstall, btnUpdate });

            dgvDrivers = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvDrivers.Columns.Add("Driver", "Драйвер");
            dgvDrivers.Columns.Add("Version", "Версия");
            dgvDrivers.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvDrivers);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadDrivers()
        {
            try
            {
                dgvDrivers.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var drivers = config.GetValue<List<object>>("Drivers", new List<object>());
                if (drivers != null)
                {
                    foreach (var driver in drivers.Take(50))
                    {
                        if (driver is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var version = element.TryGetProperty("Version", out var v) ? v.GetString() ?? "-" : "-";
                            var status = element.TryGetProperty("Status", out var s) ? s.GetString() ?? "-" : "-";
                            dgvDrivers.Rows.Add(name, version, status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка драйверов");
            }
        }
    }
}
