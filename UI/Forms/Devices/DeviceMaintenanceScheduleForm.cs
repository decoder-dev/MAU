namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceMaintenanceScheduleForm : Form
    {
        public DeviceMaintenanceScheduleForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Расписание обслуживания";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

