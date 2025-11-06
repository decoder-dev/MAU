using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceListForm : Form
    {
        private DataGridView dgvDevices;
        private Button btnRefresh;

        public DeviceListForm()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.Text = "Список устройств";
            this.Dock = DockStyle.Fill;
            this.Size = new Size(800, 600);

            dgvDevices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnRefresh = new Button
            {
                Text = "Обновить",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnRefresh.Click += (s, e) => LoadDevices();

            this.Controls.Add(dgvDevices);
            this.Controls.Add(btnRefresh);
        }

        private void LoadDevices()
        {
            var devices = Program.DbContext.NetworkDevices.ToList();
            dgvDevices.DataSource = devices;
        }
    }
}


