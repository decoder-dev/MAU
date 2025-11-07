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
    /// Поиск заявок
    /// </summary>
    public partial class SearchRequestsForm : Form
    {
        private DataGridView dgvRequests;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnExport;

        public SearchRequestsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Поиск заявок";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelSearch = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblSearch = new Label { Text = "Поиск:", Location = new Point(15, 15), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(70, 12), Width = 400, PlaceholderText = "Введите текст для поиска..." };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnSearch.PerformClick(); };
            btnSearch = new Button { Text = "Найти", Location = new Point(480, 10), Width = 100 };
            btnSearch.Click += BtnSearch_Click;
            btnExport = new Button { Text = "Экспорт", Location = new Point(590, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelSearch.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch, btnExport });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("Description", "Описание");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRequests);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelSearch);
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    ErrorHandler.ShowWarning("Введите текст для поиска");
                    return;
                }

                var requests = await DataService.GetMaintenanceRequestsAsync();
                var search = txtSearch.Text.ToLower();
                var filtered = requests.Where(r => 
                    r.Title.ToLower().Contains(search) || 
                    (r.Description ?? "").ToLower().Contains(search) ||
                    (r.Device?.Name ?? "").ToLower().Contains(search) ||
                    (r.AssignedTo ?? "").ToLower().Contains(search)
                ).ToList();

                dgvRequests.Rows.Clear();
                foreach (var request in filtered.OrderByDescending(r => r.CreatedAt))
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status.ToString(), request.Description ?? "-");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Поиск заявок");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"search_requests_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID;Название;Устройство;Статус;Описание");
                foreach (DataGridViewRow row in dgvRequests.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
