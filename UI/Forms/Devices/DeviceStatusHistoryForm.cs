using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// История статусов устройств
    /// </summary>
    public class DeviceStatusHistoryForm : Form
    {
        private DataGridView dgvHistory;
        private ComboBox cmbDevice;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private Button btnRefresh;
        private Button btnExport;

        public DeviceStatusHistoryForm()
        {
            InitializeComponent();
            LoadDevices();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "История статусов устройств";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            var lblDevice = new Label { Text = "Устройство:", Location = new Point(15, 15), AutoSize = true };
            cmbDevice = new ComboBox { Location = new Point(85, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevice.SelectedIndexChanged += (s, e) => LoadData();
            
            var lblStart = new Label { Text = "С:", Location = new Point(350, 15), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(370, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-7);
            dtpStart.ValueChanged += (s, e) => LoadData();
            
            var lblEnd = new Label { Text = "По:", Location = new Point(530, 15), AutoSize = true };
            dtpEnd = new DateTimePicker { Location = new Point(550, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpEnd.ValueChanged += (s, e) => LoadData();
            
            btnRefresh = new Button { Text = "Обновить", Location = new Point(710, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            
            btnExport = new Button { Text = "Экспорт", Location = new Point(820, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;

            panelFilters.Controls.AddRange(new Control[] { lblDevice, cmbDevice, lblStart, dtpStart, lblEnd, dtpEnd, btnRefresh, btnExport });

            dgvHistory = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvHistory.Columns.Add("DeviceName", "Устройство");
            dgvHistory.Columns.Add("IpAddress", "IP адрес");
            dgvHistory.Columns.Add("Status", "Статус");
            dgvHistory.Columns.Add("Timestamp", "Время");
            dgvHistory.Columns.Add("Notes", "Примечания");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvHistory);

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
                var query = Program.DbContext.DeviceStatusHistory
                    .Include(h => h.Device)
                    .Where(h => h.Timestamp >= dtpStart.Value && h.Timestamp <= dtpEnd.Value.AddDays(1))
                    .AsQueryable();

                if (cmbDevice.SelectedIndex > 0)
                {
                    var selected = cmbDevice.SelectedItem?.GetType().GetProperty("Device")?.GetValue(cmbDevice.SelectedItem) as NetworkDevice;
                    if (selected != null)
                        query = query.Where(h => h.DeviceId == selected.Id);
                }

                var history = await query.OrderByDescending(h => h.Timestamp).Take(1000).ToListAsync();

                dgvHistory.Rows.Clear();
                foreach (var item in history)
                {
                    dgvHistory.Rows.Add(
                        item.Device?.Name ?? "-",
                        item.Device?.IpAddress ?? "-",
                        item.Status.ToString(),
                        item.Timestamp.ToString("dd.MM.yyyy HH:mm:ss"),
                        item.Message ?? "-"
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка истории");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"status_history_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;IP адрес;Статус;Время;Примечания");
                foreach (DataGridViewRow row in dgvHistory.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
