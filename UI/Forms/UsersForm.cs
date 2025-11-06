using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма управления пользователями
    /// </summary>
    public partial class UsersForm : Form
    {
        private DataGridView dgvUsers;
        private Button btnRefresh;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Label lblStats;

        public UsersForm()
        {
            InitializeComponent();
            LoadUsers().Wait();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Пользователи";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Панель кнопок
            var panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(15, 12, 15, 12)
            };

            btnRefresh = CreateButton("🔄 Обновить", Color.FromArgb(52, 152, 219), 0);
            btnRefresh.Click += async (s, e) => await LoadUsers();
            ToolTipHelper.SetToolTip(btnRefresh, "Обновить список пользователей");

            btnAdd = CreateButton("➕ Добавить", Color.FromArgb(46, 204, 113), 130);
            btnAdd.Click += (s, e) => AddUser();
            ToolTipHelper.SetToolTip(btnAdd, "Добавить нового пользователя");

            btnEdit = CreateButton("✏️ Редактировать", Color.FromArgb(241, 196, 15), 260);
            btnEdit.Click += (s, e) => EditUser();
            ToolTipHelper.SetToolTip(btnEdit, "Редактировать выбранного пользователя");

            btnDelete = CreateButton("🗑️ Удалить", Color.FromArgb(231, 76, 60), 390);
            btnDelete.Click += (s, e) => DeleteUser();
            ToolTipHelper.SetToolTip(btnDelete, "Удалить выбранного пользователя");

            panelButtons.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });

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

            // Таблица пользователей
            dgvUsers = new DataGridView
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
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvUsers.ColumnHeadersHeight = 35;
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Стилизация строк
            dgvUsers.DefaultCellStyle.Padding = new Padding(5);
            dgvUsers.RowTemplate.Height = 30;
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            dgvUsers.SelectionChanged += (s, e) => UpdateButtonStates();

            this.Controls.Add(dgvUsers);
            this.Controls.Add(lblStats);
            this.Controls.Add(panelButtons);
            this.ResumeLayout(false);
        }

        private Button CreateButton(string text, Color color, int x)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 0),
                Size = new Size(130, 32),
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
            var hasSelection = dgvUsers.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private async Task LoadUsers()
        {
            try
            {
                var users = await Services.DataService.GetUsersAsync();

                dgvUsers.DataSource = users.Select(u => new
                {
                    u.Id,
                    Логин = u.Username,
                    ФИО = u.FullName,
                    Email = u.Email ?? "-",
                    Роль = u.Role == UserRole.Admin ? "Администратор" : "Пользователь",
                    Активен = u.IsActive ? "Да" : "Нет",
                    Последний_вход = u.LastLoginAt?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда",
                    Создан = u.CreatedAt.ToString("dd.MM.yyyy")
                }).ToList();

                // Настройка цветов строк по роли
                dgvUsers.RowPrePaint += (s, e) =>
                {
                    if (e.RowIndex >= 0 && e.RowIndex < dgvUsers.Rows.Count)
                    {
                        var row = dgvUsers.Rows[e.RowIndex];
                        var roleCell = row.Cells["Роль"];
                        var activeCell = row.Cells["Активен"];
                        if (roleCell?.Value != null)
                        {
                            if (roleCell.Value.ToString() == "Администратор")
                                row.DefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
                            if (activeCell?.Value?.ToString() == "Нет")
                                row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 150, 150);
                        }
                    }
                };

                // Обновление статистики
                var total = users.Count;
                var active = users.Count(u => u.IsActive);
                var admins = users.Count(u => u.Role == UserRole.Admin);
                var regularUsers = users.Count(u => u.Role == UserRole.User);
                lblStats.Text = $"Всего пользователей: {total} | Активных: {active} | Администраторов: {admins} | Пользователей: {regularUsers}";

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Загрузка пользователей");
            }
        }

        private async void AddUser()
        {
            var form = new UserForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                await LoadUsers();
                Helpers.ErrorHandler.ShowSuccess("Пользователь успешно создан!");
            }
        }

        private async void EditUser()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите пользователя для редактирования", "Информация");
                return;
            }

            var userId = (int)dgvUsers.SelectedRows[0].Cells["Id"].Value;
            var user = await Services.DataService.GetUserByIdAsync(userId);
            if (user != null)
            {
                var form = new UserForm(user);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await LoadUsers();
                    Helpers.ErrorHandler.ShowSuccess("Пользователь успешно обновлен!");
                }
            }
        }

        private async void DeleteUser()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                Helpers.ErrorHandler.ShowInfo("Выберите пользователя для удаления", "Информация");
                return;
            }

            var userId = (int)dgvUsers.SelectedRows[0].Cells["Id"].Value;
            var username = dgvUsers.SelectedRows[0].Cells["Логин"].Value?.ToString() ?? "пользователя";
            
            if (userId == Program.CurrentUser!.Id)
            {
                Helpers.ErrorHandler.ShowError("Нельзя удалить текущего пользователя");
                return;
            }

            if (Helpers.ErrorHandler.ShowConfirm(
                $"Вы уверены, что хотите удалить пользователя '{username}'?\n\n" +
                "Это действие нельзя отменить. Все связанные данные будут сохранены.",
                "Подтверждение удаления"))
            {
                try
                {
                    var success = await Services.DataService.DeleteUserAsync(userId);
                    if (success)
                    {
                        await LoadUsers();
                        Helpers.ErrorHandler.ShowSuccess("Пользователь успешно удален!");
                    }
                    else
                    {
                        Helpers.ErrorHandler.ShowError("Не удалось удалить пользователя. Возможно, у него есть связанные заявки или он является последним администратором.");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ErrorHandler.HandleException(ex, "Удаление пользователя");
                }
            }
        }
    }
}
