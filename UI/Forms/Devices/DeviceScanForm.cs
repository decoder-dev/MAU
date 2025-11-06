namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceScanForm : Form
    {
        public DeviceScanForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Сканирование сети";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

