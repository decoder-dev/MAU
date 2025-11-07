using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление пользователями
    /// </summary>
    public partial class UsersManagementForm : Form
    {
        private DataGridView dgvUsers;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;

        public UsersManagementForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление пользователями";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnAdd.Click += BtnAdd_Click;
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnEdit.Click += BtnEdit_Click;
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            btnDelete.Click += BtnDelete_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(345, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

            dgvUsers = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvUsers.Columns.Add("Id", "ID");
            dgvUsers.Columns.Add("Username", "Имя пользователя");
            dgvUsers.Columns.Add("FullName", "ФИО");
            dgvUsers.Columns.Add("Role", "Роль");
            dgvUsers.Columns.Add("IsActive", "Активен");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvUsers);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                var users = await DataService.GetUsersAsync();
                dgvUsers.Rows.Clear();
                foreach (var user in users)
                {
                    dgvUsers.Rows.Add(user.Id, user.Username, user.FullName ?? "-", user.Role, user.IsActive ? "Да" : "Нет");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка пользователей");
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var form = new UserForm();
            if (form.ShowDialog() == DialogResult.OK)
                LoadData();
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                ErrorHandler.ShowWarning("Выберите пользователя");
                return;
            }
            var userId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells[0].Value);
            var user = await DataService.GetUserByIdAsync(userId);
            if (user != null)
            {
                var form = new UserForm(user);
                if (form.ShowDialog() == DialogResult.OK)
                    LoadData();
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                ErrorHandler.ShowWarning("Выберите пользователя");
                return;
            }
            if (!ErrorHandler.ShowConfirm("Удалить пользователя?", "Подтверждение"))
                return;

            var userId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells[0].Value);
            if (await DataService.DeleteUserAsync(userId))
            {
                ErrorHandler.ShowSuccess("Пользователь удален");
                LoadData();
            }
            else
                ErrorHandler.ShowError("Не удалось удалить пользователя");
        }
    }
}
