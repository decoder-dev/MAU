namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceComparisonForm : Form
    {
        public DeviceComparisonForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Сравнение устройств";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

