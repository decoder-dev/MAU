using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Services;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма управления устройствами
    /// </summary>
    public partial class DevicesForm : Form
    {
        private DataGridView dgvDevices;
        private Button btnRefresh;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnCreateRequest;
        private Button btnViewDetails;
        private ComboBox cmbFilterType;
        private ComboBox cmbFilterStatus;
        private TextBox txtSearch;
        private Label lblSearchHint;
        private Label lblStats;
        private Button btnExport;

        public DevicesForm()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Устройства";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Панель фильтров и поиска
            var panelFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(15, 10, 15, 10)
            };

            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 2
            };
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            // Тип устройства
            var lblFilterType = new Label
            {
                Text = "Тип:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            filterLayout.Controls.Add(lblFilterType, 0, 0);

            cmbFilterType = new ComboBox
            {
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Height = 28,
                FlatStyle = FlatStyle.Flat
            };
            cmbFilterType.Items.AddRange(new[] { "Все типы", "Роутер", "Коммутатор", "Телефон", "Модем", "Компьютер", "Сервер", "Точка доступа", "Принтер", "Сканер", "Камера", "МЭ", "Другое" });
            cmbFilterType.SelectedIndex = 0;
            cmbFilterType.SelectedIndexChanged += (s, e) => LoadDevices();
            filterLayout.Controls.Add(cmbFilterType, 1, 0);

            // Статус
            var lblFilterStatus = new Label
            {
                Text = "Статус:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            filterLayout.Controls.Add(lblFilterStatus, 2, 0);

            cmbFilterStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Height = 28,
                FlatStyle = FlatStyle.Flat
            };
            cmbFilterStatus.Items.AddRange(new[] { "Все статусы", "Online", "Offline", "Degraded", "Maintenance" });
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => LoadDevices();
            filterLayout.Controls.Add(cmbFilterStatus, 3, 0);

            // Поиск
            var lblSearch = new Label
            {
                Text = "Поиск:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(52, 73, 94),
                Margin = new Padding(15, 0, 0, 0)
            };
            filterLayout.Controls.Add(lblSearch, 4, 0);

            var searchPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 28,
                Margin = new Padding(15, 0, 0, 0)
            };
            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                Height = 28,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Padding = new Padding(8, 5, 25, 5)
            };
            lblSearchHint = new Label
            {
                Text = "По названию, IP, местоположению...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(170, 170, 170),
                Location = new Point(8, 5),
                AutoSize = true,
                BackColor = Color.White,
                Cursor = Cursors.IBeam
            };
            txtSearch.Enter += (s, e) => lblSearchHint.Visible = false;
            txtSearch.Leave += (s, e) => { if (string.IsNullOrEmpty(txtSearch.Text)) lblSearchHint.Visible = true; };
            txtSearch.TextChanged += (s, e) => { lblSearchHint.Visible = string.IsNullOrEmpty(txtSearch.Text); LoadDevices(); };
            searchPanel.Controls.Add(lblSearchHint);
            searchPanel.Controls.Add(txtSearch);
            filterLayout.Controls.Add(searchPanel, 4, 0);

            panelFilters.Controls.Add(filterLayout);

            // Панель кнопок
            var panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(15, 12, 15, 12),
                BorderStyle = BorderStyle.None
            };

            btnRefresh = CreateButton("🔄 Обновить", Color.FromArgb(52, 152, 219), 0);
            btnRefresh.Click += (s, e) => LoadDevices();
            ToolTipHelper.SetToolTip(btnRefresh, "Обновить список устройств");

            if (Program.IsAdmin)
            {
                btnAdd = CreateButton("➕ Добавить", Color.FromArgb(46, 204, 113), 130);
                btnAdd.Click += (s, e) => AddDevice();
                ToolTipHelper.SetToolTip(btnAdd, "Добавить новое устройство");

                btnEdit = CreateButton("✏️ Редактировать", Color.FromArgb(241, 196, 15), 260);
                btnEdit.Click += (s, e) => EditDevice();
                ToolTipHelper.SetToolTip(btnEdit, "Редактировать выбранное устройство");

                btnDelete = CreateButton("🗑️ Удалить", Color.FromArgb(231, 76, 60), 390);
                btnDelete.Click += (s, e) => DeleteDevice();
                ToolTipHelper.SetToolTip(btnDelete, "Удалить выбранное устройство");
            }

            btnViewDetails = CreateButton("👁️ Подробности", Color.FromArgb(155, 89, 182), Program.IsAdmin ? 520 : 130);
            btnViewDetails.Click += (s, e) => ViewDeviceDetails();
            ToolTipHelper.SetToolTip(btnViewDetails, "Просмотреть детальную информацию об устройстве");

            btnCreateRequest = CreateButton("🔧 Создать заявку", Color.FromArgb(230, 126, 34), Program.IsAdmin ? 650 : 260);
            btnCreateRequest.Click += (s, e) => CreateMaintenanceRequest();
            ToolTipHelper.SetToolTip(btnCreateRequest, "Создать заявку на обслуживание");

            btnExport = CreateButton("📊 Экспорт", Color.FromArgb(52, 152, 219), Program.IsAdmin ? 780 : 390);
            btnExport.Click += (s, e) => ExportDevices();
            ToolTipHelper.SetToolTip(btnExport, "Экспортировать список устройств в CSV");

            panelButtons.Controls.Add(btnRefresh);
            if (Program.IsAdmin)
            {
                panelButtons.Controls.Add(btnAdd);
                panelButtons.Controls.Add(btnEdit);
                panelButtons.Controls.Add(btnDelete);
            }
            panelButtons.Controls.Add(btnViewDetails);
            panelButtons.Controls.Add(btnCreateRequest);
            panelButtons.Controls.Add(btnExport);

            // Статистика
            lblStats = new Label
            {
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 8, 0, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            // Таблица устройств
            dgvDevices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(230, 230, 230),
                Font = new Font("Segoe UI", 9),
                EnableHeadersVisualStyles = false
            };

            // Стилизация заголовков
            dgvDevices.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvDevices.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDevices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvDevices.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvDevices.ColumnHeadersHeight = 35;
            dgvDevices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Стилизация строк
            dgvDevices.DefaultCellStyle.Padding = new Padding(5);
            dgvDevices.RowTemplate.Height = 30;
            dgvDevices.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            dgvDevices.CellDoubleClick += (s, e) => ViewDeviceDetails();
            dgvDevices.SelectionChanged += (s, e) => UpdateButtonStates();

            this.Controls.Add(dgvDevices);
            this.Controls.Add(lblStats);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelFilters);
            this.ResumeLayout(false);
        }

        private Button CreateButton(string text, Color color, int x)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 0),
                Size = new Size(120, 32),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(
                    Math.Min(255, color.R + 15),
                    Math.Min(255, color.G + 15),
                    Math.Min(255, color.B + 15));
            };
            btn.MouseLeave += (s, e) => btn.BackColor = color;
            return btn;
        }

        private void UpdateButtonStates()
        {
            var hasSelection = dgvDevices.SelectedRows.Count > 0;
            if (Program.IsAdmin)
            {
                btnEdit.Enabled = hasSelection;
                btnDelete.Enabled = hasSelection;
            }
            btnViewDetails.Enabled = hasSelection;
            btnCreateRequest.Enabled = hasSelection;
        }

        private async void LoadDevices()
        {
            try
            {
                var devices = await Services.DataService.GetDevicesAsync();
                var query = devices.AsQueryable();

                // Фильтр по типу
                if (cmbFilterType.SelectedIndex > 0)
                {
                    var typeMap = new Dictionary<string, DeviceType>
                    {
                        { "Роутер", DeviceType.Router },
                        { "Коммутатор", DeviceType.Switch },
                        { "Телефон", DeviceType.Phone },
                        { "Модем", DeviceType.Modem },
                        { "Компьютер", DeviceType.Computer },
                        { "Сервер", DeviceType.Server },
                        { "Точка доступа", DeviceType.AccessPoint },
                        { "Принтер", DeviceType.Printer },
                        { "Сканер", DeviceType.Scanner },
                        { "Камера", DeviceType.Camera },
                        { "МЭ", DeviceType.Firewall },
                        { "Другое", DeviceType.Other }
                    };
                    var selectedType = cmbFilterType.SelectedItem.ToString();
                    if (typeMap.ContainsKey(selectedType!))
                        query = query.Where(d => d.DeviceType == typeMap[selectedType!]);
                }

                // Фильтр по статусу
                if (cmbFilterStatus.SelectedIndex > 0)
                {
                    var statusMap = new Dictionary<string, DeviceStatus>
                    {
                        { "Online", DeviceStatus.Online },
                        { "Offline", DeviceStatus.Offline },
                        { "Degraded", DeviceStatus.Degraded },
                        { "Maintenance", DeviceStatus.Maintenance }
                    };
                    var selectedStatus = cmbFilterStatus.SelectedItem.ToString();
                    if (statusMap.ContainsKey(selectedStatus!))
                        query = query.Where(d => d.Status == statusMap[selectedStatus!]);
                }

                // Поиск
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    var searchTerm = txtSearch.Text.ToLower();
                    query = query.Where(d =>
                        d.Name.ToLower().Contains(searchTerm) ||
                        d.IpAddress.ToLower().Contains(searchTerm) ||
                        (d.Location != null && d.Location.ToLower().Contains(searchTerm)) ||
                        (d.Building != null && d.Building.ToLower().Contains(searchTerm)) ||
                        (d.Room != null && d.Room.ToLower().Contains(searchTerm)) ||
                        (d.Description != null && d.Description.ToLower().Contains(searchTerm)));
                }

                devices = query.OrderBy(d => d.Name).ToList();

                dgvDevices.DataSource = devices.Select(d => new
                {
                    d.Id,
                    Название = d.Name,
                    Тип = GetDeviceTypeName(d.DeviceType),
                    IP_адрес = d.IpAddress,
                    Статус = GetStatusName(d.Status),
                    Местоположение = d.Location ?? "-",
                    Здание = d.Building ?? "-",
                    Помещение = d.Room ?? "-",
                    Последний_контакт = d.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда"
                }).ToList();

                // Настройка цветов строк по статусу
                dgvDevices.RowPrePaint += (s, e) =>
                {
                    if (e.RowIndex >= 0 && e.RowIndex < dgvDevices.Rows.Count)
                    {
                        var row = dgvDevices.Rows[e.RowIndex];
                        var statusCell = row.Cells["Статус"];
                        if (statusCell?.Value != null)
                        {
                            var status = statusCell.Value.ToString();
                            row.DefaultCellStyle.BackColor = status switch
                            {
                                "Online" => Color.FromArgb(240, 255, 240),
                                "Offline" => Color.FromArgb(255, 240, 240),
                                "Degraded" => Color.FromArgb(255, 255, 230),
                                "Maintenance" => Color.FromArgb(240, 240, 255),
                                _ => Color.White
                            };
                        }
                    }
                };

                // Обновление статистики
                var total = devices.Count;
                var online = devices.Count(d => d.Status == DeviceStatus.Online);
                var offline = devices.Count(d => d.Status == DeviceStatus.Offline);
                var degraded = devices.Count(d => d.Status == DeviceStatus.Degraded);
                var maintenance = devices.Count(d => d.Status == DeviceStatus.Maintenance);
                lblStats.Text = $"Всего устройств: {total} | 🟢 Online: {online} | 🔴 Offline: {offline} | 🟡 Degraded: {degraded} | 🔵 Maintenance: {maintenance}";

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private async void AddDevice()
        {
            UserActivityLogger.Log("Device Create", "Started");
            var form = new DeviceForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var device = form.GetDevice();
                if (device != null)
                {
                    var success = await Services.DataService.AddDeviceAsync(device);
                    if (success)
                    {
                        LoadDevices();
                        UserActivityLogger.Log("Device Create", "Success");
                        var mode = Services.ConnectionService.IsMySqlOnline() ? "" : " (офлайн режим)";
                        Helpers.ErrorHandler.ShowSuccess($"Устройство успешно добавлено!{mode}");
                    }
                    else
                    {
                        Helpers.ErrorHandler.ShowError("Ошибка при добавлении устройства");
                    }
                }
            }
        }

        private async void EditDevice()
        {
            if (dgvDevices.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите устройство для редактирования", "Информация");
                return;
            }

            var deviceId = (int)dgvDevices.SelectedRows[0].Cells["Id"].Value;
            var device = await Services.DataService.GetDeviceByIdAsync(deviceId);
            if (device != null)
            {
                var form = new DeviceForm(device);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var updatedDevice = form.GetDevice();
                    if (updatedDevice != null)
                    {
                        var success = await Services.DataService.UpdateDeviceAsync(updatedDevice);
                        if (success)
                        {
                            LoadDevices();
                            var mode = Services.ConnectionService.IsMySqlOnline() ? "" : " (офлайн режим)";
                            Helpers.ErrorHandler.ShowSuccess($"Устройство успешно обновлено!{mode}");
                        }
                    }
                }
            }
        }

        private async void DeleteDevice()
        {
            if (dgvDevices.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите устройство для удаления", "Информация");
                return;
            }

            var deviceName = dgvDevices.SelectedRows[0].Cells["Название"].Value?.ToString() ?? "устройство";
            if (Helpers.ErrorHandler.ShowConfirm($"Вы уверены, что хотите удалить устройство '{deviceName}'?\n\nЭто действие нельзя отменить.", "Подтверждение удаления"))
            {
                var deviceId = (int)dgvDevices.SelectedRows[0].Cells["Id"].Value;
                var device = await Services.DataService.GetDeviceByIdAsync(deviceId);
                if (device != null)
                {
                    UserActivityLogger.LogDeviceAction("Delete", device.Id, device.Name);
                    var success = await Services.DataService.DeleteDeviceAsync(device);
                    if (success)
                    {
                        LoadDevices();
                        var mode = Services.ConnectionService.IsMySqlOnline() ? "" : " (офлайн режим)";
                        Helpers.ErrorHandler.ShowSuccess($"Устройство успешно удалено!{mode}");
                    }
                    else
                    {
                        Helpers.ErrorHandler.ShowError("Не удалось удалить устройство. Возможно, у него есть связанные заявки.");
                    }
                }
            }
        }

        private async void ViewDeviceDetails()
        {
            if (dgvDevices.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите устройство для просмотра", "Информация");
                return;
            }

            var deviceId = (int)dgvDevices.SelectedRows[0].Cells["Id"].Value;
            var device = await Services.DataService.GetDeviceByIdAsync(deviceId);
            if (device != null)
            {
                var form = new DeviceDetailsForm(device);
                form.ShowDialog();
                LoadDevices(); // Обновляем после просмотра
            }
        }

        private async void CreateMaintenanceRequest()
        {
            if (dgvDevices.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите устройство для создания заявки", "Информация");
                return;
            }

            var deviceId = (int)dgvDevices.SelectedRows[0].Cells["Id"].Value;
            var device = await Services.DataService.GetDeviceByIdAsync(deviceId);
            if (device != null)
            {
                UserActivityLogger.Log("Request Create", $"Device: {device.Name}");
                var form = new MaintenanceRequestForm(device);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Helpers.ErrorHandler.ShowSuccess("Заявка создана успешно!");
                    UserActivityLogger.LogRequestAction("Created", 0);
                }
            }
        }

        private async void ExportDevices()
        {
            try
            {
                UserActivityLogger.Log("Device Export", "Started");
                var exportService = new Services.ExportService(Program.DbContext);
                var csv = await exportService.ExportDevicesToCsvAsync();

                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    FileName = $"Устройства_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    Title = "Сохранить список устройств"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, csv, Encoding.UTF8);
                    UserActivityLogger.Log("Device Export", $"File: {saveDialog.FileName}");
                    Helpers.ErrorHandler.ShowSuccess($"Данные экспортированы в файл:\n{saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Экспорт устройств");
            }
        }

        private string GetDeviceTypeName(DeviceType type)
        {
            return type switch
            {
                DeviceType.Router => "Роутер",
                DeviceType.Switch => "Коммутатор",
                DeviceType.Phone => "Телефон",
                DeviceType.Modem => "Модем",
                DeviceType.Computer => "Компьютер",
                DeviceType.Server => "Сервер",
                DeviceType.AccessPoint => "Точка доступа",
                DeviceType.Printer => "Принтер",
                DeviceType.Scanner => "Сканер",
                DeviceType.Camera => "Камера",
                DeviceType.Firewall => "МЭ",
                DeviceType.Other => "Другое",
                _ => "Неизвестно"
            };
        }

        private string GetStatusName(DeviceStatus status)
        {
            return status switch
            {
                DeviceStatus.Online => "Online",
                DeviceStatus.Offline => "Offline",
                DeviceStatus.Degraded => "Degraded",
                DeviceStatus.Maintenance => "Maintenance",
                _ => "Unknown"
            };
        }
    }
}
