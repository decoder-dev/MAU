namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceInventoryForm : Form
    {
        public DeviceInventoryForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Инвентаризация устройств";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

