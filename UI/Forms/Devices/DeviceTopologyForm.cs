namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceTopologyForm : Form
    {
        public DeviceTopologyForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Топология сети";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

