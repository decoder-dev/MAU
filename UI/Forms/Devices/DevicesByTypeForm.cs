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
    /// Устройства по типам
    /// </summary>
    public class DevicesByTypeForm : Form
    {
        private DataGridView dgvDevices;
        private ComboBox cmbType;
        private Button btnRefresh;
        private GroupService? _groupService;

        public DevicesByTypeForm()
        {
            InitializeComponent();
            _groupService = new GroupService(Program.DbContext);
            LoadTypes();
        }

        private void InitializeComponent()
        {
            this.Text = "Устройства по типам";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilter = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblType = new Label { Text = "Тип устройства:", Location = new Point(15, 15), AutoSize = true };
            cmbType = new ComboBox { Location = new Point(120, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(380, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelFilter.Controls.AddRange(new Control[] { lblType, cmbType, btnRefresh });

            dgvDevices = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvDevices.Columns.Add("Name", "Название");
            dgvDevices.Columns.Add("IpAddress", "IP адрес");
            dgvDevices.Columns.Add("Status", "Статус");
            dgvDevices.Columns.Add("Location", "Местоположение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvDevices);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilter);
        }

        private void LoadTypes()
        {
            cmbType.Items.Add("Все типы");
            cmbType.Items.AddRange(Enum.GetNames(typeof(DeviceType)));
            cmbType.SelectedIndex = 0;
        }

        private async void LoadData()
        {
            try
            {
                var devicesByType = await _groupService!.GetDevicesByTypeAsync();
                dgvDevices.Rows.Clear();
                
                if (cmbType.SelectedIndex == 0)
                {
                    foreach (var group in devicesByType.OrderBy(g => g.Key))
                    {
                        foreach (var device in group.Value.OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.Status.ToString(), device.Location ?? "-");
                    }
                }
                else
                {
                    var selectedType = (DeviceType)Enum.Parse(typeof(DeviceType), cmbType.Text);
                    if (devicesByType.ContainsKey(selectedType))
                    {
                        foreach (var device in devicesByType[selectedType].OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.Status.ToString(), device.Location ?? "-");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }
    }
}
