using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Экспорт устройств
    /// </summary>
    public partial class DeviceExportWizardForm : Form
    {
        private Button btnExport;
        private ComboBox cmbFormat;
        private Label lblInfo;

        public DeviceExportWizardForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Экспорт устройств";
            this.Size = new Size(600, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblFormat = new Label { Text = "Формат:", Location = new Point(20, 20), AutoSize = true };
            cmbFormat = new ComboBox { Location = new Point(80, 17), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFormat.Items.AddRange(new[] { "CSV", "JSON", "Excel" });
            cmbFormat.SelectedIndex = 0;
            btnExport = new Button { Text = "Экспортировать", Location = new Point(240, 15), Width = 120 };
            btnExport.Click += BtnExport_Click;
            lblInfo = new Label { Location = new Point(20, 60), Width = 500, Text = "Выберите формат и нажмите 'Экспортировать'" };
            panel.Controls.AddRange(new Control[] { lblFormat, cmbFormat, btnExport, lblInfo });
            this.Controls.Add(panel);
        }

        private async void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                btnExport.Enabled = false;
                lblInfo.Text = "Экспорт...";
                
                var exportService = new ExportService(Program.DbContext);
                string content;
                string extension;
                string filter;
                
                switch (cmbFormat.SelectedIndex)
                {
                    case 0: // CSV
                        content = await exportService.ExportDevicesToCsvAsync();
                        extension = "csv";
                        filter = "CSV файлы (*.csv)|*.csv";
                        break;
                    case 1: // JSON
                        var devices = await DataService.GetDevicesAsync();
                        content = System.Text.Json.JsonSerializer.Serialize(devices, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        extension = "json";
                        filter = "JSON файлы (*.json)|*.json";
                        break;
                    default:
                        ErrorHandler.ShowError("Формат Excel пока не поддерживается");
                        btnExport.Enabled = true;
                        return;
                }
                
                using var dialog = new SaveFileDialog { Filter = filter, FileName = $"devices_{DateTime.Now:yyyyMMdd}.{extension}" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    await File.WriteAllTextAsync(dialog.FileName, content);
                    lblInfo.Text = "Экспорт завершен";
                    ErrorHandler.ShowSuccess("Устройства успешно экспортированы");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Экспорт устройств");
            }
            finally
            {
                btnExport.Enabled = true;
            }
        }
    }
}
