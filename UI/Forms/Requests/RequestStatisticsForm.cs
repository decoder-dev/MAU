using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Статистика заявок
    /// </summary>
    public partial class RequestStatisticsForm : Form
    {
        private DataGridView dgvStats;
        private Button btnRefresh;
        private StatisticsService? _statisticsService;

        public RequestStatisticsForm()
        {
            InitializeComponent();
            _statisticsService = new StatisticsService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Статистика заявок";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelTop.Controls.Add(btnRefresh);

            dgvStats = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvStats.Columns.Add("Metric", "Метрика");
            dgvStats.Columns.Add("Value", "Значение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvStats);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                var stats = await _statisticsService!.GetMaintenanceStatisticsAsync();
                dgvStats.Rows.Clear();
                dgvStats.Rows.Add("Всего заявок", stats.Total);
                dgvStats.Rows.Add("Ожидают", stats.Pending);
                dgvStats.Rows.Add("В работе", stats.InProgress);
                dgvStats.Rows.Add("Завершены", stats.Completed);
                dgvStats.Rows.Add("Отменены", stats.Cancelled);
                dgvStats.Rows.Add("Создано сегодня", stats.CreatedToday);
                dgvStats.Rows.Add("Создано на этой неделе", stats.CreatedThisWeek);
                dgvStats.Rows.Add("Создано в этом месяце", stats.CreatedThisMonth);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка статистики");
            }
        }
    }
}
