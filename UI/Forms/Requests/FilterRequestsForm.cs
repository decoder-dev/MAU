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
    /// Фильтр заявок
    /// </summary>
    public partial class FilterRequestsForm : Form
    {
        private DataGridView dgvRequests;
        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private ComboBox cmbPriority;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private Button btnFilter;
        private Button btnExport;

        public FilterRequestsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Фильтр заявок";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(15) };
            var lblSearch = new Label { Text = "Поиск:", Location = new Point(15, 15), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(70, 12), Width = 200, PlaceholderText = "Поиск..." };
            var lblStatus = new Label { Text = "Статус:", Location = new Point(285, 15), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(335, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.Add("Все");
            cmbStatus.Items.AddRange(Enum.GetNames(typeof(RequestStatus)));
            cmbStatus.SelectedIndex = 0;
            var lblPriority = new Label { Text = "Приоритет:", Location = new Point(465, 15), AutoSize = true };
            cmbPriority = new ComboBox { Location = new Point(535, 12), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new[] { "Все", "1", "2", "3", "4" });
            cmbPriority.SelectedIndex = 0;
            
            var lblStart = new Label { Text = "С:", Location = new Point(15, 50), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(40, 47), Width = 130, Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-30);
            var lblEnd = new Label { Text = "По:", Location = new Point(180, 50), AutoSize = true };
            dtpEnd = new DateTimePicker { Location = new Point(210, 47), Width = 130, Format = DateTimePickerFormat.Short };
            
            btnFilter = new Button { Text = "Применить фильтр", Location = new Point(650, 12), Width = 130 };
            btnFilter.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(790, 12), Width = 100 };
            btnExport.Click += BtnExport_Click;
            
            panelFilters.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblStatus, cmbStatus, lblPriority, cmbPriority, lblStart, dtpStart, lblEnd, dtpEnd, btnFilter, btnExport });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("Priority", "Приоритет");
            dgvRequests.Columns.Add("CreatedAt", "Создано");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRequests);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    var search = txtSearch.Text.ToLower();
                    requests = requests.Where(r => r.Title.ToLower().Contains(search) || (r.Description ?? "").ToLower().Contains(search)).ToList();
                }
                
                if (cmbStatus.SelectedIndex > 0)
                {
                    var status = (RequestStatus)Enum.Parse(typeof(RequestStatus), cmbStatus.Text);
                    requests = requests.Where(r => r.Status == status).ToList();
                }
                
                if (cmbPriority.SelectedIndex > 0)
                {
                    var priority = cmbPriority.SelectedIndex;
                    requests = requests.Where(r => r.Priority == priority).ToList();
                }
                
                requests = requests.Where(r => r.CreatedAt >= dtpStart.Value && r.CreatedAt <= dtpEnd.Value.AddDays(1)).ToList();
                
                dgvRequests.Rows.Clear();
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status.ToString(), request.Priority, request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"filtered_requests_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID;Название;Устройство;Статус;Приоритет;Создано");
                foreach (DataGridViewRow row in dgvRequests.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value};{row.Cells[5].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
