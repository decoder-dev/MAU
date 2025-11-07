using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Логи устройств
    /// </summary>
    public partial class DeviceLogViewerForm : Form
    {
        private DataGridView dgvLogs;
        private ComboBox cmbDevice;
        private ComboBox cmbLogLevel;
        private Button btnRefresh;
        private Button btnExport;

        public DeviceLogViewerForm()
        {
            InitializeComponent();
            LoadDevices();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Логи устройств";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblDevice = new Label { Text = "Устройство:", Location = new Point(15, 15), AutoSize = true };
            cmbDevice = new ComboBox { Location = new Point(85, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevice.SelectedIndexChanged += (s, e) => LoadData();
            var lblLevel = new Label { Text = "Уровень:", Location = new Point(350, 15), AutoSize = true };
            cmbLogLevel = new ComboBox { Location = new Point(410, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLogLevel.Items.AddRange(new[] { "Все", "INFO", "WARNING", "ERROR" });
            cmbLogLevel.SelectedIndex = 0;
            cmbLogLevel.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(570, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(680, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblDevice, cmbDevice, lblLevel, cmbLogLevel, btnRefresh, btnExport });

            dgvLogs = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvLogs.Columns.Add("Device", "Устройство");
            dgvLogs.Columns.Add("Level", "Уровень");
            dgvLogs.Columns.Add("Message", "Сообщение");
            dgvLogs.Columns.Add("Timestamp", "Время");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvLogs);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadDevices()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                cmbDevice.Items.Clear();
                cmbDevice.Items.Add("Все устройства");
                foreach (var device in devices.OrderBy(d => d.Name))
                    cmbDevice.Items.Add(new { Device = device, Display = $"{device.Name} ({device.IpAddress})" });
                if (cmbDevice.Items.Count > 0)
                {
                    cmbDevice.DisplayMember = "Display";
                    cmbDevice.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private async void LoadData()
        {
            try
            {
                dgvLogs.Rows.Clear();
                var devices = await DataService.GetDevicesAsync();
                var statusHistory = await Program.DbContext.DeviceStatusHistory
                    .OrderByDescending(h => h.Timestamp)
                    .Take(500)
                    .ToListAsync();
                
                var selectedDevice = cmbDevice.SelectedItem;
                int? deviceId = null;
                if (selectedDevice != null && selectedDevice.GetType().GetProperty("Device")?.GetValue(selectedDevice) is NetworkDevice selectedDeviceObj)
                    deviceId = selectedDeviceObj.Id;
                
                var filterLevel = cmbLogLevel.SelectedItem?.ToString();
                
                foreach (var history in statusHistory)
                {
                    if (deviceId.HasValue && history.DeviceId != deviceId.Value) continue;
                    
                    var level = history.Status == DeviceStatus.Offline ? "ERROR" : 
                               history.Status == DeviceStatus.Degraded ? "WARNING" : "INFO";
                    
                    if (filterLevel != "Все" && filterLevel != level) continue;
                    
                    var logDevice = devices.FirstOrDefault(d => d.Id == history.DeviceId);
                    var message = $"Статус изменен на {history.Status}";
                    dgvLogs.Rows.Add(
                        logDevice?.Name ?? "Неизвестно",
                        level,
                        message,
                        history.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка логов");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"device_logs_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;Уровень;Сообщение;Время");
                foreach (DataGridViewRow row in dgvLogs.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
