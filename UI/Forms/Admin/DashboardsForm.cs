using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление дашбордами
    /// </summary>
    public partial class DashboardsForm : Form
    {
        private DataGridView dgvDashboards;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public DashboardsForm()
        {
            InitializeComponent();
            LoadDashboards();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление дашбордами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvDashboards = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvDashboards.Columns.Add("Dashboard", "Дашборд");
            dgvDashboards.Columns.Add("Widgets", "Виджетов");
            dgvDashboards.Columns.Add("Owner", "Владелец");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvDashboards);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadDashboards()
        {
            try
            {
                dgvDashboards.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var dashboards = config.GetValue<List<object>>("Dashboards", new List<object>());
                if (dashboards != null)
                {
                    foreach (var dashboard in dashboards.Take(50))
                    {
                        if (dashboard is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var widgets = element.TryGetProperty("Widgets", out var w) ? w.EnumerateArray().Count() : 0;
                            var lastModified = element.TryGetProperty("LastModified", out var lm) ? lm.GetString() ?? "-" : "-";
                            dgvDashboards.Rows.Add(name, widgets, lastModified);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка дашбордов");
            }
        }
    }
}
