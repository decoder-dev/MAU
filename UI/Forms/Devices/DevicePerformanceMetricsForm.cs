using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Производительность устройств
    /// </summary>
    public partial class DevicePerformanceMetricsForm : Form
    {
        private DataGridView dgvPerformance;
        private Button btnRefresh;
        private PerformanceService? _performanceService;

        public DevicePerformanceMetricsForm()
        {
            InitializeComponent();
            _performanceService = new PerformanceService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Производительность устройств";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelButtons = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelButtons.Controls.Add(btnRefresh);

            dgvPerformance = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvPerformance.Columns.Add("Device", "Устройство");
            dgvPerformance.Columns.Add("Metric", "Метрика");
            dgvPerformance.Columns.Add("Value", "Значение");
            dgvPerformance.Columns.Add("Timestamp", "Время");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvPerformance);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
        }

        private async void LoadData()
        {
            try
            {
                dgvPerformance.Rows.Clear();
                var devices = await DataService.GetDevicesAsync();
                var metrics = await Program.DbContext.DeviceMetrics
                    .OrderByDescending(m => m.Timestamp)
                    .Take(200)
                    .ToListAsync();
                
                foreach (var metric in metrics)
                {
                    var device = devices.FirstOrDefault(d => d.Id == metric.DeviceId);
                    dgvPerformance.Rows.Add(
                        device?.Name ?? "Неизвестно",
                        metric.MetricType,
                        metric.Value?.ToString("F2") ?? "-",
                        metric.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка метрик производительности");
            }
        }
    }
}
