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
    /// Сравнение устройств
    /// </summary>
    public partial class DeviceComparisonInsightForm : Form
    {
        private DataGridView dgvComparison;
        private ComboBox cmbDevice1;
        private ComboBox cmbDevice2;
        private Button btnCompare;
        private ComparisonService? _comparisonService;

        public DeviceComparisonInsightForm()
        {
            InitializeComponent();
            _comparisonService = new ComparisonService(Program.DbContext);
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.Text = "Сравнение устройств";
            this.Size = new Size(1200, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblDevice1 = new Label { Text = "Устройство 1:", Location = new Point(15, 15), AutoSize = true };
            cmbDevice1 = new ComboBox { Location = new Point(90, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            var lblDevice2 = new Label { Text = "Устройство 2:", Location = new Point(360, 15), AutoSize = true };
            cmbDevice2 = new ComboBox { Location = new Point(440, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            btnCompare = new Button { Text = "Сравнить", Location = new Point(710, 10), Width = 100 };
            btnCompare.Click += BtnCompare_Click;
            panelFilters.Controls.AddRange(new Control[] { lblDevice1, cmbDevice1, lblDevice2, cmbDevice2, btnCompare });

            dgvComparison = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvComparison.Columns.Add("Parameter", "Параметр");
            dgvComparison.Columns.Add("Device1", "Устройство 1");
            dgvComparison.Columns.Add("Device2", "Устройство 2");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvComparison);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadDevices()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                foreach (var device in devices.OrderBy(d => d.Name))
                {
                    var item = new { Device = device, Display = $"{device.Name} ({device.IpAddress})" };
                    cmbDevice1.Items.Add(item);
                    cmbDevice2.Items.Add(item);
                }
                if (cmbDevice1.Items.Count > 0)
                {
                    cmbDevice1.DisplayMember = "Display";
                    cmbDevice2.DisplayMember = "Display";
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private void BtnCompare_Click(object? sender, EventArgs e)
        {
            if (cmbDevice1.SelectedItem == null || cmbDevice2.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите два устройства для сравнения");
                return;
            }

            var device1 = cmbDevice1.SelectedItem.GetType().GetProperty("Device")?.GetValue(cmbDevice1.SelectedItem) as NetworkDevice;
            var device2 = cmbDevice2.SelectedItem.GetType().GetProperty("Device")?.GetValue(cmbDevice2.SelectedItem) as NetworkDevice;

            if (device1 == null || device2 == null) return;

            dgvComparison.Rows.Clear();
            dgvComparison.Rows.Add("Название", device1.Name, device2.Name);
            dgvComparison.Rows.Add("IP адрес", device1.IpAddress, device2.IpAddress);
            dgvComparison.Rows.Add("Тип", device1.DeviceType.ToString(), device2.DeviceType.ToString());
            dgvComparison.Rows.Add("Статус", device1.Status.ToString(), device2.Status.ToString());
            dgvComparison.Rows.Add("Местоположение", device1.Location ?? "-", device2.Location ?? "-");
            dgvComparison.Rows.Add("Здание", device1.Building ?? "-", device2.Building ?? "-");
            dgvComparison.Rows.Add("Помещение", device1.Room ?? "-", device2.Room ?? "-");
            dgvComparison.Rows.Add("Модель", device1.Model ?? "-", device2.Model ?? "-");
            dgvComparison.Rows.Add("Производитель", device1.Manufacturer ?? "-", device2.Manufacturer ?? "-");
        }
    }
}
