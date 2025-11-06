using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма управления заявками на обслуживание
    /// </summary>
    public partial class MaintenanceRequestsForm : Form
    {
        private DataGridView dgvRequests;
        private Button btnRefresh;
        private Button btnCreate;
        private Button btnEdit;
        private Button btnView;
        private ComboBox cmbFilterStatus;
        private Label lblStats;

        public MaintenanceRequestsForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Заявки на обслуживание";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Панель фильтров
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
                ColumnCount = 3,
                RowCount = 2
            };
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            var lblFilterStatus = new Label
            {
                Text = "Статус:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            filterLayout.Controls.Add(lblFilterStatus, 0, 0);

            cmbFilterStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Height = 28,
                FlatStyle = FlatStyle.Flat
            };
            cmbFilterStatus.Items.AddRange(new[] { "Все статусы", "Ожидает", "В работе", "Завершена", "Отменена" });
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => LoadRequests();
            filterLayout.Controls.Add(cmbFilterStatus, 1, 0);

            panelFilters.Controls.Add(filterLayout);

            // Панель кнопок
            var panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(15, 12, 15, 12)
            };

            btnRefresh = CreateButton("🔄 Обновить", Color.FromArgb(52, 152, 219), 0);
            btnRefresh.Click += (s, e) => LoadRequests();
            ToolTipHelper.SetToolTip(btnRefresh, "Обновить список заявок");

            btnCreate = CreateButton("➕ Создать заявку", Color.FromArgb(46, 204, 113), 130);
            btnCreate.Click += (s, e) => CreateRequest();
            ToolTipHelper.SetToolTip(btnCreate, "Создать новую заявку на обслуживание");

            btnEdit = CreateButton("✏️ Редактировать", Color.FromArgb(241, 196, 15), 270);
            btnEdit.Click += (s, e) => EditRequest();
            ToolTipHelper.SetToolTip(btnEdit, "Редактировать выбранную заявку");

            btnView = CreateButton("👁️ Просмотр", Color.FromArgb(155, 89, 182), 410);
            btnView.Click += (s, e) => ViewRequest();
            ToolTipHelper.SetToolTip(btnView, "Просмотреть детали заявки");

            panelButtons.Controls.AddRange(new Control[] { btnRefresh, btnCreate, btnEdit, btnView });

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

            // Таблица заявок
            dgvRequests = new DataGridView
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
            dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvRequests.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvRequests.ColumnHeadersHeight = 35;
            dgvRequests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Стилизация строк
            dgvRequests.DefaultCellStyle.Padding = new Padding(5);
            dgvRequests.RowTemplate.Height = 30;
            dgvRequests.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            dgvRequests.SelectionChanged += (s, e) => UpdateButtonStates();
            dgvRequests.CellDoubleClick += (s, e) => ViewRequest();

            this.Controls.Add(dgvRequests);
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
                Size = new Size(140, 32),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
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
            var hasSelection = dgvRequests.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection && Program.IsAdmin;
            btnView.Enabled = hasSelection;
        }

        private async void LoadRequests()
        {
            try
            {
                var requests = await Services.DataService.GetMaintenanceRequestsAsync();
                
                // Фильтрация по пользователю
                if (!Program.IsAdmin)
                {
                    requests = requests.Where(r => r.UserId == Program.CurrentUser!.Id).ToList();
                }

                // Фильтр по статусу
                if (cmbFilterStatus.SelectedIndex > 0)
                {
                    var statusMap = new Dictionary<string, RequestStatus>
                    {
                        { "Ожидает", RequestStatus.Pending },
                        { "В работе", RequestStatus.InProgress },
                        { "Завершена", RequestStatus.Completed },
                        { "Отменена", RequestStatus.Cancelled }
                    };
                    var selectedStatus = cmbFilterStatus.SelectedItem.ToString();
                    if (statusMap.ContainsKey(selectedStatus!))
                        requests = requests.Where(r => r.Status == statusMap[selectedStatus!]).ToList();
                }

                requests = requests.OrderByDescending(r => r.CreatedAt).ToList();

                dgvRequests.DataSource = requests.Select(r => new
                {
                    r.Id,
                    Устройство = r.Device?.Name ?? "Неизвестно",
                    Заголовок = r.Title,
                    Статус = GetStatusName(r.Status),
                    Приоритет = GetPriorityName(r.Priority),
                    Создано = r.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    Исполнитель = r.AssignedTo ?? "Не назначен",
                    Создатель = r.User?.FullName ?? "Неизвестно"
                }).ToList();

                // Настройка цветов строк по статусу
                dgvRequests.RowPrePaint += (s, e) =>
                {
                    if (e.RowIndex >= 0 && e.RowIndex < dgvRequests.Rows.Count)
                    {
                        var row = dgvRequests.Rows[e.RowIndex];
                        var statusCell = row.Cells["Статус"];
                        if (statusCell?.Value != null)
                        {
                            var status = statusCell.Value.ToString();
                            row.DefaultCellStyle.BackColor = status switch
                            {
                                "Ожидает" => Color.FromArgb(255, 255, 230),
                                "В работе" => Color.FromArgb(230, 240, 255),
                                "Завершена" => Color.FromArgb(240, 255, 240),
                                "Отменена" => Color.FromArgb(255, 240, 240),
                                _ => Color.White
                            };
                        }
                    }
                };

                // Обновление статистики
                var total = requests.Count;
                var pending = requests.Count(r => r.Status == RequestStatus.Pending);
                var inProgress = requests.Count(r => r.Status == RequestStatus.InProgress);
                var completed = requests.Count(r => r.Status == RequestStatus.Completed);
                lblStats.Text = $"Всего заявок: {total} | Ожидает: {pending} | В работе: {inProgress} | Завершено: {completed}";

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void CreateRequest()
        {
            var form = new MaintenanceRequestForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadRequests();
                Helpers.ErrorHandler.ShowSuccess("Заявка создана успешно!");
            }
        }

        private async void EditRequest()
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите заявку для редактирования", "Информация");
                return;
            }

            var requestId = (int)dgvRequests.SelectedRows[0].Cells["Id"].Value;
            var requests = await Services.DataService.GetMaintenanceRequestsAsync();
            var request = requests.FirstOrDefault(r => r.Id == requestId);

            if (request != null)
            {
                var device = await Services.DataService.GetDeviceByIdAsync(request.DeviceId);
                if (device != null)
                {
                    var form = new MaintenanceRequestForm(device, request);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadRequests();
                        Helpers.ErrorHandler.ShowSuccess("Заявка обновлена успешно!");
                    }
                }
            }
        }

        private async void ViewRequest()
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите заявку для просмотра", "Информация");
                return;
            }

            var requestId = (int)dgvRequests.SelectedRows[0].Cells["Id"].Value;
            var requests = await Services.DataService.GetMaintenanceRequestsAsync();
            var request = requests.FirstOrDefault(r => r.Id == requestId);

            if (request != null)
            {
                var device = await Services.DataService.GetDeviceByIdAsync(request.DeviceId);
                if (device != null)
                {
                    var form = new MaintenanceRequestForm(device, request);
                    form.ShowDialog();
                    LoadRequests();
                }
            }
        }

        private string GetStatusName(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Pending => "Ожидает",
                RequestStatus.InProgress => "В работе",
                RequestStatus.Completed => "Завершена",
                RequestStatus.Cancelled => "Отменена",
                _ => "Неизвестно"
            };
        }

        private string GetPriorityName(int priority)
        {
            return priority switch
            {
                1 => "Низкий",
                2 => "Средний",
                3 => "Высокий",
                4 => "Критический",
                _ => "Неизвестно"
            };
        }
    }
}
