namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceBackupForm : Form
    {
        public DeviceBackupForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Резервное копирование устройств";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

