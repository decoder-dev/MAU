using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Services;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма создания/редактирования заявки на обслуживание
    /// </summary>
    public partial class MaintenanceRequestForm : Form
    {
        private MaintenanceRequest? request;
        private NetworkDevice? selectedDevice;
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel fieldsLayout;
        private ComboBox cmbDevice;
        private TextBox txtTitle;
        private TextBox txtDescription;
        private ComboBox cmbPriority;
        private ComboBox cmbStatus;
        private TextBox txtAssignedTo;
        private TextBox txtResolution;
        private Button btnSave;
        private Button btnCancel;

        public MaintenanceRequestForm(NetworkDevice? device = null, MaintenanceRequest? request = null)
        {
            this.selectedDevice = device;
            this.request = request ?? new MaintenanceRequest();
            InitializeComponent();
            LoadRequestData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = request?.Id > 0 ? "Редактирование заявки - МАУ" : "Создание заявки на обслуживание - МАУ";
            this.Size = new Size(650, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Icon = SystemIcons.Application;

            // Основной контейнер
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25),
                ColumnCount = 1,
                RowCount = 2
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));

            // Контейнер для полей с прокруткой
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 0, 5, 0)
            };

            // Таблица для полей формы
            fieldsLayout = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 7
            };
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int row = 0;

            // Устройство *
            var lblDevice = new Label
            {
                Text = "Устройство: *",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            fieldsLayout.Controls.Add(lblDevice, 0, row);
            cmbDevice = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5),
                FlatStyle = FlatStyle.Flat
            };
            LoadDevices();
            ToolTipHelper.SetToolTip(cmbDevice, "Выберите устройство для заявки");
            fieldsLayout.Controls.Add(cmbDevice, 1, row);
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;

            // Заголовок *
            AddFieldRow(fieldsLayout, "Заголовок:", "*", ref txtTitle, "Введите краткое описание проблемы", ref row);

            // Описание
            AddMultiLineRow(fieldsLayout, "Описание:", "", ref txtDescription, "Введите подробное описание проблемы (необязательно)", ref row);

            // Приоритет
            var lblPriority = new Label
            {
                Text = "Приоритет:",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            fieldsLayout.Controls.Add(lblPriority, 0, row);
            cmbPriority = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5),
                FlatStyle = FlatStyle.Flat
            };
            cmbPriority.Items.AddRange(new[] { "1 - Низкий", "2 - Средний", "3 - Высокий", "4 - Критический" });
            cmbPriority.SelectedIndex = 0;
            ToolTipHelper.SetToolTip(cmbPriority, "Выберите приоритет заявки");
            fieldsLayout.Controls.Add(cmbPriority, 1, row);
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;

            // Статус (только для админа или редактирования)
            if (Program.IsAdmin || request?.Id > 0)
            {
                var lblStatus = new Label
                {
                    Text = "Статус:",
                    Font = new Font("Segoe UI", 10),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 8, 10, 8),
                    ForeColor = Color.FromArgb(127, 140, 141)
                };
                fieldsLayout.Controls.Add(lblStatus, 0, row);
                cmbStatus = new ComboBox
                {
                    Font = new Font("Segoe UI", 10),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Dock = DockStyle.Fill,
                    Height = 32,
                    Margin = new Padding(0, 5, 0, 5),
                    FlatStyle = FlatStyle.Flat
                };
                cmbStatus.Items.AddRange(Enum.GetNames(typeof(RequestStatus)));
                ToolTipHelper.SetToolTip(cmbStatus, "Выберите статус заявки");
                fieldsLayout.Controls.Add(cmbStatus, 1, row);
                fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
                row++;

                AddFieldRow(fieldsLayout, "Исполнитель:", "", ref txtAssignedTo, "Введите имя исполнителя (необязательно)", ref row);
                AddMultiLineRow(fieldsLayout, "Решение:", "", ref txtResolution, "Введите решение проблемы (необязательно)", ref row);
            }

            scrollPanel.Controls.Add(fieldsLayout);
            fieldsLayout.Location = new Point(0, 0);

            mainLayout.Controls.Add(scrollPanel, 0, 0);

            // Панель кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 55
            };

            btnSave = new Button
            {
                Text = "Сохранить",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 38),
                Location = new Point(350, 10),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            btnSave.MouseEnter += (s, e) => btnSave.BackColor = Color.FromArgb(39, 174, 96);
            btnSave.MouseLeave += (s, e) => btnSave.BackColor = Color.FromArgb(46, 204, 113);

            btnCancel = new Button
            {
                Text = "Отмена",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(149, 165, 166),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 38),
                Location = new Point(490, 10),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.FromArgb(127, 140, 141);
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.FromArgb(149, 165, 166);

            // Подсказка
            var lblRequired = new Label
            {
                Text = "* - обязательные поля",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(25, 15),
                AutoSize = true
            };

            buttonPanel.Controls.Add(lblRequired);
            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);

            mainLayout.Controls.Add(buttonPanel, 0, 1);

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);
        }

        private void AddFieldRow(TableLayoutPanel panel, string labelText, string required, ref TextBox textBox, string tooltip, ref int row)
        {
            var lbl = new Label
            {
                Text = labelText + required,
                Font = new Font("Segoe UI", 10, required == "*" ? FontStyle.Bold : FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = required == "*" ? Color.FromArgb(52, 73, 94) : Color.FromArgb(127, 140, 141)
            };
            panel.Controls.Add(lbl, 0, row);

            textBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            ToolTipHelper.SetToolTip(textBox, tooltip);
            panel.Controls.Add(textBox, 1, row);
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;
        }

        private void AddMultiLineRow(TableLayoutPanel panel, string labelText, string required, ref TextBox textBox, string tooltip, ref int row)
        {
            var lbl = new Label
            {
                Text = labelText + required,
                Font = new Font("Segoe UI", 10, required == "*" ? FontStyle.Bold : FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 12, 10, 8),
                ForeColor = required == "*" ? Color.FromArgb(52, 73, 94) : Color.FromArgb(127, 140, 141)
            };
            panel.Controls.Add(lbl, 0, row);

            textBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                Height = 70,
                Multiline = true,
                Margin = new Padding(0, 5, 0, 5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ScrollBars = ScrollBars.Vertical
            };
            ToolTipHelper.SetToolTip(textBox, tooltip);
            panel.Controls.Add(textBox, 1, row);
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            row++;
        }

        private async void LoadDevices()
        {
            try
            {
                var devices = await Services.DataService.GetDevicesAsync();
                cmbDevice.DisplayMember = "Name";
                cmbDevice.ValueMember = "Id";
                cmbDevice.DataSource = devices.OrderBy(d => d.Name).ToList();

                if (selectedDevice != null)
                {
                    cmbDevice.SelectedValue = selectedDevice.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private void LoadRequestData()
        {
            if (request != null && request.Id > 0)
            {
                if (cmbDevice.Items.Count > 0)
                    cmbDevice.SelectedValue = request.DeviceId;
                txtTitle.Text = request.Title;
                txtDescription.Text = request.Description ?? "";
                cmbPriority.SelectedIndex = Math.Max(0, request.Priority - 1);
                if (cmbStatus != null)
                {
                    cmbStatus.SelectedItem = request.Status.ToString();
                    txtAssignedTo.Text = request.AssignedTo ?? "";
                    txtResolution.Text = request.Resolution ?? "";
                }
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            // Валидация
            if (cmbDevice.SelectedValue == null)
            {
                ErrorHandler.ShowWarning("Выберите устройство", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                cmbDevice.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ErrorHandler.ShowWarning("Введите заголовок заявки", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                txtTitle.Focus();
                return;
            }

            if (txtTitle.Text.Length < 3)
            {
                ErrorHandler.ShowWarning("Заголовок должен содержать минимум 3 символа", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                txtTitle.Focus();
                txtTitle.SelectAll();
                return;
            }

            try
            {
                if (request == null) request = new MaintenanceRequest();

                request.DeviceId = (int)cmbDevice.SelectedValue;
                request.UserId = Program.CurrentUser!.Id;
                request.Title = txtTitle.Text.Trim();
                request.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                request.Priority = cmbPriority.SelectedIndex + 1;

                if (request.Id == 0)
                {
                    request.CreatedAt = DateTime.UtcNow;
                    request.Status = RequestStatus.Pending;
                    var success = await Services.DataService.AddMaintenanceRequestAsync(request);
                    if (!success)
                    {
                        ErrorHandler.ShowError("Не удалось создать заявку");
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                }
                else
                {
                    if (cmbStatus != null && Enum.TryParse<RequestStatus>(cmbStatus.SelectedItem?.ToString(), out var status))
                    {
                        request.Status = status;
                        if (status == RequestStatus.Completed && request.CompletedAt == null)
                            request.CompletedAt = DateTime.UtcNow;
                    }
                    request.AssignedTo = string.IsNullOrWhiteSpace(txtAssignedTo?.Text) ? null : txtAssignedTo.Text.Trim();
                    request.Resolution = string.IsNullOrWhiteSpace(txtResolution?.Text) ? null : txtResolution.Text.Trim();
                    request.UpdatedAt = DateTime.UtcNow;
                    var success = await Services.DataService.UpdateMaintenanceRequestAsync(request);
                    if (!success)
                    {
                        ErrorHandler.ShowError("Не удалось обновить заявку");
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение заявки");
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
