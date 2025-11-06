using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceDetailsAdvancedForm : Form
    {
        private NetworkDevice device;

        public DeviceDetailsAdvancedForm(NetworkDevice device)
        {
            this.device = device;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"Детали устройства: {device.Name}";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var info = new Label
            {
                Text = $"Устройство: {device.Name}\nТип: {device.DeviceType}\nIP: {device.IpAddress}",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Segoe UI", 12)
            };

            panel.Controls.Add(info);
            this.Controls.Add(panel);
        }
    }
}


