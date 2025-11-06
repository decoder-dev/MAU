using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceConfigurationForm : Form
    {
        private NetworkDevice device;

        public DeviceConfigurationForm(NetworkDevice device)
        {
            this.device = device;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Конфигурация устройства";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}


