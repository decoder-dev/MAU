namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceScheduleForm : Form
    {
        public DeviceScheduleForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Расписание проверок";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

