using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Аналитика устройств
    /// </summary>
    public partial class DeviceAnalyticsForm : Form
    {
        private DataGridView dgvAnalytics;
        private Button btnRefresh;
        private AnalyticsService? _analyticsService;

        public DeviceAnalyticsForm()
        {
            InitializeComponent();
            _analyticsService = new AnalyticsService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Аналитика устройств";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelButtons = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelButtons.Controls.Add(btnRefresh);

            dgvAnalytics = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvAnalytics.Columns.Add("Metric", "Метрика");
            dgvAnalytics.Columns.Add("Value", "Значение");
            dgvAnalytics.Columns.Add("Trend", "Тенденция");
            dgvAnalytics.Columns.Add("Period", "Период");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvAnalytics);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
        }

        private async void LoadData()
        {
            try
            {
                dgvAnalytics.Rows.Clear();
                var analytics = await _analyticsService!.GetDeviceAnalyticsAsync();
                
                dgvAnalytics.Rows.Add("Всего устройств", analytics.TotalDevices, "Стабильно", "Все время");
                dgvAnalytics.Rows.Add("Онлайн", analytics.OnlineDevices, "Стабильно", "Все время");
                dgvAnalytics.Rows.Add("Офлайн", analytics.OfflineDevices, analytics.OfflineDevices > 0 ? "Ухудшение" : "Стабильно", "Все время");
                dgvAnalytics.Rows.Add("Деградированы", analytics.DegradedDevices, analytics.DegradedDevices > 0 ? "Ухудшение" : "Стабильно", "Все время");
                dgvAnalytics.Rows.Add("Средний Uptime", $"{analytics.AverageUptime:F2}%", analytics.AverageUptime > 95 ? "Улучшение" : "Стабильно", "30 дней");
                
                if (analytics.DevicesByType != null)
                {
                    foreach (var type in analytics.DevicesByType)
                    {
                        dgvAnalytics.Rows.Add($"Тип: {type.Key}", type.Value, "Стабильно", "Все время");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка аналитики");
            }
        }
    }
}
