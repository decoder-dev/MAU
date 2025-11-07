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
    /// Сводка по заявкам
    /// </summary>
    public partial class RequestsSummaryForm : Form
    {
        private DataGridView dgvSummary;
        private Button btnRefresh;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;

        public RequestsSummaryForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Сводка по заявкам";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            var lblStart = new Label { Text = "С:", Location = new Point(15, 15), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(40, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-30);
            dtpStart.ValueChanged += (s, e) => LoadData();
            var lblEnd = new Label { Text = "По:", Location = new Point(200, 15), AutoSize = true };
            dtpEnd = new DateTimePicker { Location = new Point(230, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpEnd.ValueChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(400, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelFilters.Controls.AddRange(new Control[] { lblStart, dtpStart, lblEnd, dtpEnd, btnRefresh });

            dgvSummary = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvSummary.Columns.Add("Metric", "Метрика");
            dgvSummary.Columns.Add("Value", "Значение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvSummary);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var filtered = requests.Where(r => r.CreatedAt >= dtpStart.Value && r.CreatedAt <= dtpEnd.Value.AddDays(1)).ToList();

                dgvSummary.Rows.Clear();
                dgvSummary.Rows.Add("Всего заявок", filtered.Count);
                dgvSummary.Rows.Add("Ожидают", filtered.Count(r => r.Status == RequestStatus.Pending));
                dgvSummary.Rows.Add("В работе", filtered.Count(r => r.Status == RequestStatus.InProgress));
                dgvSummary.Rows.Add("Завершены", filtered.Count(r => r.Status == RequestStatus.Completed));
                dgvSummary.Rows.Add("Отменены", filtered.Count(r => r.Status == RequestStatus.Cancelled));
                dgvSummary.Rows.Add("Средний приоритет", filtered.Any() ? filtered.Average(r => r.Priority).ToString("F1") : "0");
                dgvSummary.Rows.Add("Без исполнителя", filtered.Count(r => string.IsNullOrEmpty(r.AssignedTo)));
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка сводки");
            }
        }
    }
}
