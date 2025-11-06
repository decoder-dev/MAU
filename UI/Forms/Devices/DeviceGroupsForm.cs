namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceGroupsForm : Form
    {
        public DeviceGroupsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Группы устройств";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

