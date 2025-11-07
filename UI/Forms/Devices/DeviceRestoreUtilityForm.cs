using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Восстановление устройств
    /// </summary>
    public partial class DeviceRestoreUtilityForm : Form
    {
        private Button btnRestore;
        private Label lblInfo;
        private RestoreService? _restoreService;

        public DeviceRestoreUtilityForm()
        {
            InitializeComponent();
            _restoreService = new RestoreService(Program.DbContext);
        }

        private void InitializeComponent()
        {
            this.Text = "Восстановление устройств";
            this.Size = new Size(600, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            btnRestore = new Button { Text = "Восстановить устройства из резервной копии", Location = new Point(20, 20), Width = 400 };
            btnRestore.Click += BtnRestore_Click;
            lblInfo = new Label { Location = new Point(20, 60), Width = 500 };
            panel.Controls.AddRange(new Control[] { btnRestore, lblInfo });
            this.Controls.Add(panel);
        }

        private async void BtnRestore_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "JSON файлы (*.json)|*.json", Title = "Выберите резервную копию" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    btnRestore.Enabled = false;
                    lblInfo.Text = "Восстановление...";
                    if (!ErrorHandler.ShowConfirm("Восстановить устройства из резервной копии?", "Подтверждение"))
                        return;
                    
                    var success = await _restoreService!.RestoreDevicesAsync(dialog.FileName, false);
                    if (success)
                    {
                        lblInfo.Text = "Устройства восстановлены";
                        ErrorHandler.ShowSuccess("Устройства успешно восстановлены");
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "Восстановление устройств");
                }
                finally
                {
                    btnRestore.Enabled = true;
                }
            }
        }
    }
}
