namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceNotificationsForm : Form
    {
        public DeviceNotificationsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки уведомлений";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

