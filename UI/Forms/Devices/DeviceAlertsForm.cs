namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceAlertsForm : Form
    {
        public DeviceAlertsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Оповещения устройств";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

