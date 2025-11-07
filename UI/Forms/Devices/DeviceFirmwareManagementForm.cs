using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Прошивки устройств
    /// </summary>
    public partial class DeviceFirmwareManagementForm : Form
    {
        private DataGridView dgvFirmware;
        private ComboBox cmbDevice;
        private Button btnRefresh;
        private Button btnUpload;
        private Button btnExport;
        private FirmwareService? _firmwareService;

        public DeviceFirmwareManagementForm()
        {
            InitializeComponent();
            _firmwareService = new FirmwareService(Program.DbContext);
            LoadDevices();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Прошивки устройств";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblDevice = new Label { Text = "Устройство:", Location = new Point(15, 15), AutoSize = true };
            cmbDevice = new ComboBox { Location = new Point(85, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevice.SelectedIndexChanged += (s, e) => LoadData();
            btnUpload = new Button { Text = "Загрузить прошивку", Location = new Point(400, 10), Width = 150 };
            btnUpload.Click += BtnUpload_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(560, 10), Width = 100 };
            btnRefresh.Click += (s, e) => { LoadDevices(); LoadData(); };
            btnExport = new Button { Text = "Экспорт", Location = new Point(670, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblDevice, cmbDevice, btnUpload, btnRefresh, btnExport });

            dgvFirmware = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvFirmware.Columns.Add("Device", "Устройство");
            dgvFirmware.Columns.Add("CurrentVersion", "Текущая версия");
            dgvFirmware.Columns.Add("LatestVersion", "Последняя версия");
            dgvFirmware.Columns.Add("UpdateDate", "Дата обновления");
            dgvFirmware.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvFirmware);

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
                dgvFirmware.Rows.Clear();
                var firmwareInfo = await _firmwareService!.GetFirmwareInfoAsync();
                var devices = await DataService.GetDevicesAsync();
                
                var selectedDevice = cmbDevice.SelectedItem;
                int? deviceId = null;
                if (selectedDevice != null && selectedDevice.GetType().GetProperty("Device")?.GetValue(selectedDevice) is NetworkDevice selectedDeviceObj)
                    deviceId = selectedDeviceObj.Id;

                foreach (var info in firmwareInfo)
                {
                    var deviceList = info.Devices;
                    if (deviceId.HasValue)
                    {
                        var selectedDev = devices.FirstOrDefault(d => d.Id == deviceId.Value);
                        if (selectedDev != null && !deviceList.Contains(selectedDev.Name))
                            continue;
                    }
                    
                    foreach (var deviceName in deviceList)
                    {
                        var firmwareDevice = devices.FirstOrDefault(d => d.Name == deviceName);
                        if (firmwareDevice != null)
                        {
                            dgvFirmware.Rows.Add(
                                firmwareDevice.Name,
                                firmwareDevice.FirmwareVersion ?? "Не указана",
                                info.FirmwareVersion,
                                firmwareDevice.UpdatedAt.ToString("yyyy-MM-dd"),
                                firmwareDevice.FirmwareVersion == info.FirmwareVersion ? "Актуальна" : "Устарела"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка прошивок");
            }
        }

        private void BtnUpload_Click(object? sender, EventArgs e)
        {
            ErrorHandler.ShowInfo("Функция загрузки прошивки будет реализована");
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"firmware_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;Текущая версия;Последняя версия;Дата обновления;Статус");
                foreach (DataGridViewRow row in dgvFirmware.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
