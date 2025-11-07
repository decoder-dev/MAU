using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление ролями
    /// </summary>
    public partial class RolesManagementForm : Form
    {
        private DataGridView dgvRoles;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public RolesManagementForm()
        {
            InitializeComponent();
            LoadRoles();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление ролями";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvRoles = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRoles.Columns.Add("Role", "Роль");
            dgvRoles.Columns.Add("Users", "Пользователей");
            dgvRoles.Columns.Add("Permissions", "Права");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRoles);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadRoles()
        {
            try
            {
                var users = await DataService.GetUsersAsync();
                dgvRoles.Rows.Clear();
                foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
                {
                    var count = users.Count(u => u.Role == role);
                    dgvRoles.Rows.Add(role.ToString(), count, role == UserRole.Admin ? "Все права" : "Базовые права");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка ролей");
            }
        }
    }
}
