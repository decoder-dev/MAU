namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceCredentialsForm : Form
    {
        public DeviceCredentialsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Учетные данные устройств";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

