namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DevicePerformanceForm : Form
    {
        public DevicePerformanceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Производительность устройств";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

