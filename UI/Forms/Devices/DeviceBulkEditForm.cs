namespace NetworkMonitoringSystem.Forms.Devices
{
    public partial class DeviceBulkEditForm : Form
    {
        public DeviceBulkEditForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Массовое редактирование";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}

