namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceImportForm : Form
    {
        public DeviceImportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Импорт устройств";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

