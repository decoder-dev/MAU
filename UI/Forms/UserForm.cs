using NetworkMonitoringSystem.Core.Models;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма добавления/редактирования пользователя
    /// </summary>
    public partial class UserForm : Form
    {
        private User? user;
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel fieldsLayout;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private TextBox txtFullName;
        private TextBox txtEmail;
        private ComboBox cmbRole;
        private CheckBox chkIsActive;
        private Button btnSave;
        private Button btnCancel;

        public UserForm(User? user = null)
        {
            this.user = user ?? new User();
            InitializeComponent();
            LoadUserData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = user?.Id > 0 ? "Редактирование пользователя - МАУ" : "Добавление пользователя - МАУ";
            this.Size = new Size(550, 500);
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

            // Контейнер для полей
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

            // Логин *
            var lblUsername = new Label
            {
                Text = "Логин: *",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            fieldsLayout.Controls.Add(lblUsername, 0, row);
            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Enabled = user?.Id == 0 // Нельзя менять логин при редактировании
            };
            ToolTipHelper.SetToolTip(txtUsername, "Введите уникальное имя пользователя (минимум 3 символа)");
            fieldsLayout.Controls.Add(txtUsername, 1, row);
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;

            // Пароль *
            var lblPassword = new Label
            {
                Text = user?.Id > 0 ? "Пароль:" : "Пароль: *",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            fieldsLayout.Controls.Add(lblPassword, 0, row);
            var passwordPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5)
            };
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Left,
                UseSystemPasswordChar = true,
                Height = 32,
                Width = passwordPanel.Width - 100,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            chkShowPassword = new CheckBox
            {
                Text = "Показать",
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Right,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            chkShowPassword.CheckedChanged += (s, e) => txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            passwordPanel.Controls.Add(txtPassword);
            passwordPanel.Controls.Add(chkShowPassword);
            ToolTipHelper.SetToolTip(txtPassword, user?.Id > 0 ? "Оставьте пустым, чтобы не менять пароль" : "Пароль пользователя (минимум 4 символа)");
            fieldsLayout.Controls.Add(passwordPanel, 1, row);
            if (user?.Id > 0)
            {
                var lblPasswordHint = new Label
                {
                    Text = "(Оставьте пустым, чтобы не менять)",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.FromArgb(127, 140, 141),
                    AutoSize = true,
                    Location = new Point(0, 35)
                };
                fieldsLayout.SetColumnSpan(lblPasswordHint, 2);
                fieldsLayout.Controls.Add(lblPasswordHint, 1, row);
            }
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;

            // ФИО *
            AddFieldRow(fieldsLayout, "ФИО:", "*", ref txtFullName, "Введите полное имя пользователя", ref row);

            // Email
            AddFieldRow(fieldsLayout, "Email:", "", ref txtEmail, "Введите email адрес (необязательно)", ref row);

            // Роль
            var lblRole = new Label
            {
                Text = "Роль:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            fieldsLayout.Controls.Add(lblRole, 0, row);
            cmbRole = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 5, 0, 5),
                FlatStyle = FlatStyle.Flat
            };
            cmbRole.Items.AddRange(new[] { "User", "Admin" });
            ToolTipHelper.SetToolTip(cmbRole, "Выберите роль пользователя");
            fieldsLayout.Controls.Add(cmbRole, 1, row);
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            row++;

            // Активен
            var lblActive = new Label
            {
                Text = "Статус:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            fieldsLayout.Controls.Add(lblActive, 0, row);
            chkIsActive = new CheckBox
            {
                Text = "Активен",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            ToolTipHelper.SetToolTip(chkIsActive, "Отметьте, если пользователь активен");
            fieldsLayout.Controls.Add(chkIsActive, 1, row);
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

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
                Location = new Point(290, 10),
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
                Location = new Point(430, 10),
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
            var label = new Label
            {
                Text = labelText + required,
                Font = new Font("Segoe UI", 10, required == "*" ? FontStyle.Bold : FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 8, 10, 8),
                ForeColor = required == "*" ? Color.FromArgb(52, 73, 94) : Color.FromArgb(127, 140, 141)
            };
            panel.Controls.Add(label, 0, row);
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

        private void LoadUserData()
        {
            if (user != null && user.Id > 0)
            {
                txtUsername.Text = user.Username;
                txtFullName.Text = user.FullName;
                txtEmail.Text = user.Email ?? "";
                cmbRole.SelectedItem = user.Role.ToString();
                chkIsActive.Checked = user.IsActive;
            }
            else
            {
                cmbRole.SelectedIndex = 0; // По умолчанию "User"
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ErrorHandler.ShowWarning("Заполните обязательные поля (Логин, ФИО)", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                return;
            }

            if (txtUsername.Text.Length < 3)
            {
                ErrorHandler.ShowWarning("Логин должен быть не менее 3 символов.", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                txtUsername.Focus();
                txtUsername.SelectAll();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(txtEmail.Text))
            {
                ErrorHandler.ShowWarning("Введите корректный Email адрес.", "Ошибка валидации");
                this.DialogResult = DialogResult.None;
                txtEmail.Focus();
                txtEmail.SelectAll();
                return;
            }

            if (user == null) user = new User();

            user.Username = txtUsername.Text.Trim();
            user.FullName = txtFullName.Text.Trim();
            user.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            if (Enum.TryParse<UserRole>(cmbRole.SelectedItem?.ToString(), out var role))
                user.Role = role;
            user.IsActive = chkIsActive.Checked;

            try
            {
                if (user.Id == 0) // Новый пользователь
                {
                    if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 4)
                    {
                        ErrorHandler.ShowWarning("Введите пароль (минимум 4 символа) для нового пользователя.", "Ошибка валидации");
                        this.DialogResult = DialogResult.None;
                        txtPassword.Focus();
                        return;
                    }
                    var success = await DataService.AddUserAsync(user, txtPassword.Text);
                    if (!success)
                    {
                        ErrorHandler.ShowError("Не удалось создать пользователя. Возможно, пользователь с таким логином уже существует.");
                        this.DialogResult = DialogResult.None;
                    }
                }
                else // Существующий пользователь
                {
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        if (txtPassword.Text.Length < 4)
                        {
                            ErrorHandler.ShowWarning("Пароль должен быть не менее 4 символов.", "Ошибка валидации");
                            this.DialogResult = DialogResult.None;
                            txtPassword.Focus();
                            return;
                        }
                        user.SetPassword(txtPassword.Text);
                    }
                    var success = await DataService.UpdateUserAsync(user);
                    if (!success)
                    {
                        ErrorHandler.ShowError("Не удалось обновить пользователя.");
                        this.DialogResult = DialogResult.None;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorHandler.ShowError(ex.Message, "Ошибка");
                this.DialogResult = DialogResult.None;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение пользователя");
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
