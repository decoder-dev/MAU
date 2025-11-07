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
    /// Устройства по зданиям
    /// </summary>
    public class DevicesByBuildingForm : Form
    {
        private DataGridView dgvDevices;
        private ComboBox cmbBuilding;
        private Button btnRefresh;
        private GroupService? _groupService;

        public DevicesByBuildingForm()
        {
            InitializeComponent();
            _groupService = new GroupService(Program.DbContext);
            LoadBuildings();
        }

        private void InitializeComponent()
        {
            this.Text = "Устройства по зданиям";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilter = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblBuilding = new Label { Text = "Здание:", Location = new Point(15, 15), AutoSize = true };
            cmbBuilding = new ComboBox { Location = new Point(70, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBuilding.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(380, 10), Width = 100 };
            btnRefresh.Click += (s, e) => { LoadBuildings(); LoadData(); };
            panelFilter.Controls.AddRange(new Control[] { lblBuilding, cmbBuilding, btnRefresh });

            dgvDevices = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvDevices.Columns.Add("Name", "Название");
            dgvDevices.Columns.Add("IpAddress", "IP адрес");
            dgvDevices.Columns.Add("DeviceType", "Тип");
            dgvDevices.Columns.Add("Status", "Статус");
            dgvDevices.Columns.Add("Room", "Помещение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvDevices);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilter);
        }

        private async void LoadBuildings()
        {
            try
            {
                var devicesByBuilding = await _groupService!.GetDevicesByBuildingAsync();
                cmbBuilding.Items.Clear();
                cmbBuilding.Items.Add("Все здания");
                foreach (var building in devicesByBuilding.Keys.OrderBy(b => b))
                    cmbBuilding.Items.Add(building);
                if (cmbBuilding.Items.Count > 0)
                    cmbBuilding.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка зданий");
            }
        }

        private async void LoadData()
        {
            try
            {
                var devicesByBuilding = await _groupService!.GetDevicesByBuildingAsync();
                dgvDevices.Rows.Clear();
                
                if (cmbBuilding.SelectedIndex == 0)
                {
                    foreach (var group in devicesByBuilding.OrderBy(g => g.Key))
                    {
                        foreach (var device in group.Value.OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.DeviceType.ToString(), device.Status.ToString(), device.Room ?? "-");
                    }
                }
                else
                {
                    var selectedBuilding = cmbBuilding.Text;
                    if (devicesByBuilding.ContainsKey(selectedBuilding))
                    {
                        foreach (var device in devicesByBuilding[selectedBuilding].OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.DeviceType.ToString(), device.Status.ToString(), device.Room ?? "-");
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
