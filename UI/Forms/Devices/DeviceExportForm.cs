namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceExportForm : Form
    {
        public DeviceExportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Экспорт устройств";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

