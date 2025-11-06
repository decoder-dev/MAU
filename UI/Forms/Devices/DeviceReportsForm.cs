namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceReportsForm : Form
    {
        public DeviceReportsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчеты по устройствам";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

