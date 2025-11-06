using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceHistoryForm : Form
    {
        private int deviceId;
        private DataGridView dgvHistory;

        public DeviceHistoryForm(int deviceId)
        {
            this.deviceId = deviceId;
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            this.Text = "История устройства";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(dgvHistory);
        }

        private void LoadHistory()
        {
            var history = Program.DbContext.DeviceStatusHistory
                .Where(h => h.DeviceId == deviceId)
                .OrderByDescending(h => h.Timestamp)
                .ToList();

            dgvHistory.DataSource = history;
        }
    }
}


