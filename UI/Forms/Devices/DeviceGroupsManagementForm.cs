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
    /// Группы устройств
    /// </summary>
    public partial class DeviceGroupsManagementForm : Form
    {
        private DataGridView dgvGroups;
        private ComboBox cmbGroupType;
        private Button btnRefresh;
        private Button btnExport;
        private GroupService? _groupService;

        public DeviceGroupsManagementForm()
        {
            InitializeComponent();
            _groupService = new GroupService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Группы устройств";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblType = new Label { Text = "Тип группы:", Location = new Point(15, 15), AutoSize = true };
            cmbGroupType = new ComboBox { Location = new Point(100, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGroupType.Items.AddRange(new[] { "По зданиям", "По местоположению", "По типу", "По статусу" });
            cmbGroupType.SelectedIndex = 0;
            cmbGroupType.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(310, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(420, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblType, cmbGroupType, btnRefresh, btnExport });

            dgvGroups = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvGroups.Columns.Add("GroupName", "Название группы");
            dgvGroups.Columns.Add("DeviceCount", "Количество устройств");
            dgvGroups.Columns.Add("OnlineCount", "Онлайн");
            dgvGroups.Columns.Add("OfflineCount", "Офлайн");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvGroups);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                dgvGroups.Rows.Clear();
                
                switch (cmbGroupType.SelectedIndex)
                {
                    case 0: // По зданиям
                        var byBuilding = await _groupService!.GetDevicesByBuildingAsync();
                        foreach (var group in byBuilding.OrderBy(g => g.Key))
                        {
                            var online = group.Value.Count(d => d.Status == DeviceStatus.Online);
                            var offline = group.Value.Count(d => d.Status == DeviceStatus.Offline);
                            dgvGroups.Rows.Add(group.Key, group.Value.Count, online, offline);
                        }
                        break;
                    case 1: // По местоположению
                        var byLocation = await _groupService!.GetDevicesByLocationAsync();
                        foreach (var group in byLocation.OrderBy(g => g.Key))
                        {
                            var online = group.Value.Count(d => d.Status == DeviceStatus.Online);
                            var offline = group.Value.Count(d => d.Status == DeviceStatus.Offline);
                            dgvGroups.Rows.Add(group.Key, group.Value.Count, online, offline);
                        }
                        break;
                    case 2: // По типу
                        var byType = await _groupService!.GetDevicesByTypeAsync();
                        foreach (var group in byType.OrderBy(g => g.Key))
                        {
                            var online = group.Value.Count(d => d.Status == DeviceStatus.Online);
                            var offline = group.Value.Count(d => d.Status == DeviceStatus.Offline);
                            dgvGroups.Rows.Add(group.Key.ToString(), group.Value.Count, online, offline);
                        }
                        break;
                    case 3: // По статусу
                        var byStatus = await _groupService!.GetDevicesByStatusAsync();
                        foreach (var group in byStatus.OrderBy(g => g.Key))
                            dgvGroups.Rows.Add(group.Key.ToString(), group.Value.Count, group.Key == DeviceStatus.Online ? group.Value.Count : 0, group.Key == DeviceStatus.Offline ? group.Value.Count : 0);
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка групп");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"groups_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Группа;Количество устройств;Онлайн;Офлайн");
                foreach (DataGridViewRow row in dgvGroups.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
