namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceLogsForm : Form
    {
        public DeviceLogsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Логи устройств";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

