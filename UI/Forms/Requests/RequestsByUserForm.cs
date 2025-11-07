using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Заявки по пользователю
    /// </summary>
    public partial class RequestsByUserForm : Form
    {
        private DataGridView dgvRequests;
        private ComboBox cmbUser;
        private Button btnRefresh;
        private Button btnExport;

        public RequestsByUserForm()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "Заявки по пользователю";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblUser = new Label { Text = "Пользователь:", Location = new Point(15, 15), AutoSize = true };
            cmbUser = new ComboBox { Location = new Point(95, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUser.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(410, 10), Width = 100 };
            btnRefresh.Click += (s, e) => { LoadUsers(); LoadData(); };
            btnExport = new Button { Text = "Экспорт", Location = new Point(520, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblUser, cmbUser, btnRefresh, btnExport });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("CreatedAt", "Создано");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRequests);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadUsers()
        {
            try
            {
                var users = await DataService.GetUsersAsync();
                cmbUser.Items.Clear();
                foreach (var user in users.OrderBy(u => u.Username))
                    cmbUser.Items.Add(new { User = user, Display = $"{user.Username} ({user.FullName ?? "-"})" });
                if (cmbUser.Items.Count > 0)
                {
                    cmbUser.DisplayMember = "Display";
                    cmbUser.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка пользователей");
            }
        }

        private async void LoadData()
        {
            try
            {
                if (cmbUser.SelectedItem == null) return;
                
                var selected = cmbUser.SelectedItem.GetType().GetProperty("User")?.GetValue(cmbUser.SelectedItem) as User;
                if (selected == null) return;

                var requests = await DataService.GetMaintenanceRequestsAsync();
                var userRequests = requests.Where(r => r.UserId == selected.Id).OrderByDescending(r => r.CreatedAt).ToList();

                dgvRequests.Rows.Clear();
                foreach (var request in userRequests)
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status.ToString(), request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"user_requests_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID;Название;Устройство;Статус;Создано");
                foreach (DataGridViewRow row in dgvRequests.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
