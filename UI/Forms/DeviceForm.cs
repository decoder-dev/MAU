using NetworkMonitoringSystem.Core.Models;
using System.Linq;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Services;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма добавления/редактирования устройства
    /// </summary>
    public partial class DeviceForm : Form
    {
        private NetworkDevice? device;
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel fieldsLayout;
        private TextBox txtName;
        private ComboBox cmbType;
        private TextBox txtIpAddress;
        private TextBox txtLocation;
        private TextBox txtBuilding;
        private TextBox txtRoom;
        private TextBox txtDescription;
        private TextBox txtMacAddress;
        private TextBox txtModel;
        private TextBox txtManufacturer;
        private ComboBox cmbStatus;
        private Button btnSave;
        private Button btnCancel;

        public DeviceForm(NetworkDevice? device = null)
        {
            this.device = device ?? new NetworkDevice();
            InitializeComponent();
            LoadDeviceData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = device?.Id > 0 ? "Редактирование устройства - МАУ" : "Добавление устройства - МАУ";
            this.Size = new Size(650, 680);
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
                RowCount = 11
            };
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int row = 0;

            // Название *
            AddFieldRow(fieldsLayout, "Название:", "*", ref txtName, "Введите название устройства", ref row);
            
            // Тип *
            AddComboRow(fieldsLayout, "Тип устройства:", "*", ref cmbType, Enum.GetNames(typeof(DeviceType)), "Выберите тип устройства", ref row);

            // IP адрес *
            AddFieldRow(fieldsLayout, "IP адрес:", "*", ref txtIpAddress, "Введите IP адрес (например: 192.168.1.1)", ref row);

            // MAC адрес
            AddFieldRow(fieldsLayout, "MAC адрес:", "", ref txtMacAddress, "Введите MAC адрес (необязательно)", ref row);

            // Производитель
            AddFieldRow(fieldsLayout, "Производитель:", "", ref txtManufacturer, "Введите производителя (необязательно)", ref row);

            // Модель
            AddFieldRow(fieldsLayout, "Модель:", "", ref txtModel, "Введите модель устройства (необязательно)", ref row);

            // Описание
            AddMultiLineRow(fieldsLayout, "Описание:", "", ref txtDescription, "Введите описание устройства (необязательно)", ref row);

            // Местоположение
            AddFieldRow(fieldsLayout, "Местоположение:", "", ref txtLocation, "Введите местоположение (необязательно)", ref row);

            // Здание
            AddFieldRow(fieldsLayout, "Здание:", "", ref txtBuilding, "Введите номер здания (необязательно)", ref row);

            // Помещение
            AddFieldRow(fieldsLayout, "Помещение:", "", ref txtRoom, "Введите номер помещения (необязательно)", ref row);

            // Статус
            AddComboRow(fieldsLayout, "Статус:", "", ref cmbStatus, Enum.GetNames(typeof(DeviceStatus)), "Выберите статус устройства", ref row);

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

            // Подсказка о обязательных полях
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
                Height = 60,
                Multiline = true,
                Margin = new Padding(0, 5, 0, 5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ScrollBars = ScrollBars.Vertical
            };
            ToolTipHelper.SetToolTip(textBox, tooltip);
            panel.Controls.Add(textBox, 1, row);
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            row++;
        }

        private void AddComboRow(TableLayoutPanel panel, string labelText, string required, ref ComboBox comboBox, string[] items, string tooltip, ref int row)
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

            comboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5),
                FlatStyle = FlatStyle.Flat
            };
            comboBox.Items.AddRange(items);
            ToolTipHelper.SetToolTip(comboBox, tooltip);
            panel.Controls.Add(comboBox, 1, row);
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;
        }

        private void LoadDeviceData()
        {
            if (device != null && device.Id > 0)
            {
                txtName.Text = device.Name;
                cmbType.SelectedItem = device.DeviceType.ToString();
                txtIpAddress.Text = device.IpAddress;
                txtMacAddress.Text = device.MacAddress ?? "";
                txtManufacturer.Text = device.Manufacturer ?? "";
                txtModel.Text = device.Model ?? "";
                txtDescription.Text = device.Description ?? "";
                txtLocation.Text = device.Location ?? "";
                txtBuilding.Text = device.Building ?? "";
                txtRoom.Text = device.Room ?? "";
                cmbStatus.SelectedItem = device.Status.ToString();
            }
            else
            {
                cmbType.SelectedIndex = 0;
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ErrorHandler.ShowWarning("Введите название устройства", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtIpAddress.Text))
            {
                ErrorHandler.ShowWarning("Введите IP адрес", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                txtIpAddress.Focus();
                return;
            }

            // Валидация IP адреса
            if (!System.Net.IPAddress.TryParse(txtIpAddress.Text.Trim(), out _))
            {
                ErrorHandler.ShowWarning("Неверный формат IP адреса. Используйте формат: xxx.xxx.xxx.xxx", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                txtIpAddress.Focus();
                txtIpAddress.SelectAll();
                return;
            }

            // Проверка уникальности IP
            var allDevices = Services.DataService.GetDevicesAsync().Result;
            var existingDevice = allDevices
                .FirstOrDefault(d => d.IpAddress == txtIpAddress.Text.Trim() && (device == null || d.Id != device.Id));
            if (existingDevice != null)
            {
                ErrorHandler.ShowError($"Устройство с IP адресом '{txtIpAddress.Text.Trim()}' уже существует.\nНазвание: {existingDevice.Name}");
                this.DialogResult = DialogResult.None;
                txtIpAddress.Focus();
                txtIpAddress.SelectAll();
                return;
            }

            // Валидация MAC адреса (если указан)
            if (!string.IsNullOrWhiteSpace(txtMacAddress.Text))
            {
                var macPattern = System.Text.RegularExpressions.Regex.IsMatch(
                    txtMacAddress.Text.Trim(),
                    @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
                if (!macPattern)
                {
                    ErrorHandler.ShowWarning("Неверный формат MAC адреса. Используйте формат: XX:XX:XX:XX:XX:XX или XX-XX-XX-XX-XX-XX", "Ошибка валидации");
                    this.DialogResult = DialogResult.None;
                    txtMacAddress.Focus();
                    return;
                }
            }

            if (device == null) device = new NetworkDevice();

            // Сохранение данных
            device.Name = txtName.Text.Trim();
            if (Enum.TryParse<DeviceType>(cmbType.SelectedItem?.ToString(), out var type))
                device.DeviceType = type;
            device.IpAddress = txtIpAddress.Text.Trim();
            device.MacAddress = string.IsNullOrWhiteSpace(txtMacAddress.Text) ? null : txtMacAddress.Text.Trim();
            device.Manufacturer = string.IsNullOrWhiteSpace(txtManufacturer.Text) ? null : txtManufacturer.Text.Trim();
            device.Model = string.IsNullOrWhiteSpace(txtModel.Text) ? null : txtModel.Text.Trim();
            device.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
            device.Location = string.IsNullOrWhiteSpace(txtLocation.Text) ? null : txtLocation.Text.Trim();
            device.Building = string.IsNullOrWhiteSpace(txtBuilding.Text) ? null : txtBuilding.Text.Trim();
            device.Room = string.IsNullOrWhiteSpace(txtRoom.Text) ? null : txtRoom.Text.Trim();
            if (Enum.TryParse<DeviceStatus>(cmbStatus.SelectedItem?.ToString(), out var status))
                device.Status = status;

            if (device.Id == 0)
            {
                device.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                device.UpdatedAt = DateTime.UtcNow;
            }
        }

        public NetworkDevice? GetDevice()
        {
            return device;
        }
    }
}
