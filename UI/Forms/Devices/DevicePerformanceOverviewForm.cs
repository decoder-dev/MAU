using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Производительность устройств
    /// </summary>
    public partial class DevicePerformanceOverviewForm : Form
    {
        private DataGridView dgvPerformance;
        private ComboBox cmbDevice;
        private Button btnRefresh;
        private Button btnExport;

        public DevicePerformanceOverviewForm()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.Text = "Производительность устройств";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblDevice = new Label { Text = "Устройство:", Location = new Point(15, 15), AutoSize = true };
            cmbDevice = new ComboBox { Location = new Point(85, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevice.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(400, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(510, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblDevice, cmbDevice, btnRefresh, btnExport });

            dgvPerformance = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvPerformance.Columns.Add("Device", "Устройство");
            dgvPerformance.Columns.Add("Status", "Статус");
            dgvPerformance.Columns.Add("Uptime", "Время работы");
            dgvPerformance.Columns.Add("ResponseTime", "Время отклика");
            dgvPerformance.Columns.Add("LastSeen", "Последний контакт");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvPerformance);

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
                var devices = await DataService.GetDevicesAsync();
                dgvPerformance.Rows.Clear();

                if (cmbDevice.SelectedIndex == 0)
                {
                    foreach (var device in devices.OrderBy(d => d.Name))
                    {
                        var uptime = device.LastSeen.HasValue ? (DateTime.UtcNow - device.LastSeen.Value).TotalHours : 0;
                        var responseTime = device.Status == DeviceStatus.Online ? "< 100ms" : "N/A";
                        dgvPerformance.Rows.Add(
                            device.Name,
                            device.Status.ToString(),
                            $"{uptime:F1} ч",
                            responseTime,
                            device.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда"
                        );
                    }
                }
                else
                {
                    var selected = cmbDevice.SelectedItem?.GetType().GetProperty("Device")?.GetValue(cmbDevice.SelectedItem) as NetworkDevice;
                    if (selected != null)
                    {
                        var uptime = selected.LastSeen.HasValue ? (DateTime.UtcNow - selected.LastSeen.Value).TotalHours : 0;
                        var responseTime = selected.Status == DeviceStatus.Online ? "< 100ms" : "N/A";
                        dgvPerformance.Rows.Add(
                            selected.Name,
                            selected.Status.ToString(),
                            $"{uptime:F1} ч",
                            responseTime,
                            selected.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных производительности");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"performance_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;Статус;Время работы;Время отклика;Последний контакт");
                foreach (DataGridViewRow row in dgvPerformance.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
