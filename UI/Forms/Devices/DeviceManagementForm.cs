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
    /// Список всех устройств
    /// </summary>
    public partial class DeviceManagementForm : Form
    {
        private DataGridView dgvDevices;
        private Button btnRefresh;
        private Button btnExport;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private TextBox txtSearch;
        private ComboBox cmbFilterType;
        private ComboBox cmbFilterStatus;
        private Label lblInfo;

        public DeviceManagementForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Список устройств";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // Панель фильтров
            var panelFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(15)
            };

            var lblSearch = new Label { Text = "Поиск:", Location = new Point(15, 15), AutoSize = true };
            txtSearch = new TextBox
            {
                Location = new Point(70, 12),
                Width = 200,
                PlaceholderText = "Поиск по названию или IP..."
            };
            txtSearch.TextChanged += (s, e) => LoadData();

            var lblType = new Label { Text = "Тип:", Location = new Point(290, 15), AutoSize = true };
            cmbFilterType = new ComboBox
            {
                Location = new Point(325, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterType.Items.Add("Все");
            cmbFilterType.Items.AddRange(Enum.GetNames(typeof(DeviceType)));
            cmbFilterType.SelectedIndex = 0;
            cmbFilterType.SelectedIndexChanged += (s, e) => LoadData();

            var lblStatus = new Label { Text = "Статус:", Location = new Point(490, 15), AutoSize = true };
            cmbFilterStatus = new ComboBox
            {
                Location = new Point(545, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterStatus.Items.Add("Все");
            cmbFilterStatus.Items.AddRange(Enum.GetNames(typeof(DeviceStatus)));
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => LoadData();

            panelFilters.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblType, cmbFilterType, lblStatus, cmbFilterStatus });

            // Панель кнопок
            var panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(15)
            };

            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnAdd.Click += BtnAdd_Click;
            
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 120 };
            btnEdit.Click += BtnEdit_Click;
            
            btnDelete = new Button { Text = "Удалить", Location = new Point(255, 10), Width = 100, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;
            
            btnRefresh = new Button { Text = "Обновить", Location = new Point(365, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            
            btnExport = new Button { Text = "Экспорт", Location = new Point(475, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh, btnExport });

            // Таблица
            dgvDevices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            dgvDevices.Columns.Add("Id", "ID");
            dgvDevices.Columns.Add("Name", "Название");
            dgvDevices.Columns.Add("IpAddress", "IP адрес");
            dgvDevices.Columns.Add("DeviceType", "Тип");
            dgvDevices.Columns.Add("Status", "Статус");
            dgvDevices.Columns.Add("Location", "Местоположение");
            dgvDevices.Columns.Add("LastSeen", "Последний контакт");
            dgvDevices.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            lblInfo = new Label { Dock = DockStyle.Bottom, Height = 30, Text = "Готово", Padding = new Padding(15, 5, 0, 0) };

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvDevices);
            panelMain.Controls.Add(lblInfo);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                
                // Фильтрация
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    var search = txtSearch.Text.ToLower();
                    devices = devices.Where(d => 
                        d.Name.ToLower().Contains(search) || 
                        d.IpAddress.ToLower().Contains(search)).ToList();
                }

                if (cmbFilterType.SelectedIndex > 0)
                {
                    var selectedType = (DeviceType)Enum.Parse(typeof(DeviceType), cmbFilterType.Text);
                    devices = devices.Where(d => d.DeviceType == selectedType).ToList();
                }

                if (cmbFilterStatus.SelectedIndex > 0)
                {
                    var selectedStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), cmbFilterStatus.Text);
                    devices = devices.Where(d => d.Status == selectedStatus).ToList();
                }

                dgvDevices.Rows.Clear();
                foreach (var device in devices.OrderBy(d => d.Name))
                {
                    dgvDevices.Rows.Add(
                        device.Id,
                        device.Name,
                        device.IpAddress,
                        device.DeviceType.ToString(),
                        device.Status.ToString(),
                        device.Location ?? "-",
                        device.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "-"
                    );
                }

                lblInfo.Text = $"Всего устройств: {devices.Count}";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var form = new DeviceForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvDevices.SelectedRows.Count == 0)
            {
                ErrorHandler.ShowWarning("Выберите устройство для редактирования");
                return;
            }

            var deviceId = Convert.ToInt32(dgvDevices.SelectedRows[0].Cells[0].Value);
            var device = await DataService.GetDeviceByIdAsync(deviceId);
            
            if (device != null)
            {
                var form = new DeviceForm(device);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvDevices.SelectedRows.Count == 0)
            {
                ErrorHandler.ShowWarning("Выберите устройство для удаления");
                return;
            }

            if (!ErrorHandler.ShowConfirm("Удалить выбранное устройство?", "Подтверждение"))
                return;

            var deviceId = Convert.ToInt32(dgvDevices.SelectedRows[0].Cells[0].Value);
            var device = await DataService.GetDeviceByIdAsync(deviceId);
            
            if (device != null)
            {
                var success = await DataService.DeleteDeviceAsync(device);
                if (success)
                {
                    ErrorHandler.ShowSuccess("Устройство удалено");
                    LoadData();
                }
                else
                {
                    ErrorHandler.ShowError("Не удалось удалить устройство");
                }
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = $"devices_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID;Название;IP адрес;Тип;Статус;Местоположение;Последний контакт");
                
                foreach (DataGridViewRow row in dgvDevices.Rows)
                {
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value};{row.Cells[5].Value};{row.Cells[6].Value}");
                }
                
                System.IO.File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}