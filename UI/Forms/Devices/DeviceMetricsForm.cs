using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceMetricsForm : Form
    {
        private int deviceId;
        private DataGridView dgvMetrics;

        public DeviceMetricsForm(int deviceId)
        {
            this.deviceId = deviceId;
            InitializeComponent();
            LoadMetrics();
        }

        private void InitializeComponent()
        {
            this.Text = "Метрики устройства";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            dgvMetrics = new DataGridView
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(dgvMetrics);
        }

        private void LoadMetrics()
        {
            var metrics = Program.DbContext.DeviceMetrics
                .Where(m => m.DeviceId == deviceId)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            dgvMetrics.DataSource = metrics;
        }
    }
}


