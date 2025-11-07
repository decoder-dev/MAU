using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Резервное копирование устройств
    /// </summary>
    public partial class DeviceBackupUtilityForm : Form
    {
        private Button btnBackup;
        private Label lblInfo;
        private BackupService? _backupService;

        public DeviceBackupUtilityForm()
        {
            InitializeComponent();
            _backupService = new BackupService(Program.DbContext);
        }

        private void InitializeComponent()
        {
            this.Text = "Резервное копирование устройств";
            this.Size = new Size(600, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            btnBackup = new Button { Text = "Создать резервную копию устройств", Location = new Point(20, 20), Width = 300 };
            btnBackup.Click += BtnBackup_Click;
            lblInfo = new Label { Location = new Point(20, 60), Width = 500 };
            panel.Controls.AddRange(new Control[] { btnBackup, lblInfo });
            this.Controls.Add(panel);
        }

        private async void BtnBackup_Click(object? sender, EventArgs e)
        {
            try
            {
                btnBackup.Enabled = false;
                lblInfo.Text = "Создание резервной копии...";
                var backupPath = await _backupService!.CreateDevicesBackupAsync();
                lblInfo.Text = $"Резервная копия создана: {Path.GetFileName(backupPath)}";
                ErrorHandler.ShowSuccess($"Резервная копия создана:\n{backupPath}");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Создание резервной копии");
            }
            finally
            {
                btnBackup.Enabled = true;
            }
        }
    }
}
