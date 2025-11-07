using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление метриками
    /// </summary>
    public partial class AdminMetricsForm : Form
    {
        private DataGridView dgvMetrics;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public AdminMetricsForm()
        {
            InitializeComponent();
            LoadMetrics();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление метриками";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvMetrics = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvMetrics.Columns.Add("Metric", "Метрика");
            dgvMetrics.Columns.Add("Unit", "Единица");
            dgvMetrics.Columns.Add("Interval", "Интервал");
            dgvMetrics.Columns.Add("Enabled", "Включено");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvMetrics);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadMetrics()
        {
            try
            {
                dgvMetrics.Rows.Clear();
                var metrics = await Program.DbContext.DeviceMetrics
                    .GroupBy(m => m.MetricType)
                    .Select(g => new { Type = g.Key, Count = g.Count(), Avg = g.Average(m => (double?)m.Value) })
                    .ToListAsync();
                foreach (var metric in metrics)
                    dgvMetrics.Rows.Add(metric.Type, metric.Count, metric.Avg?.ToString("F2") ?? "-", true);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка метрик");
            }
        }
    }
}
