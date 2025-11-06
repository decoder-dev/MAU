namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceFirmwareForm : Form
    {
        public DeviceFirmwareForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление прошивками";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

