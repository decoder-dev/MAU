using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки расписания
    /// </summary>
    public partial class ScheduleSettingsForm : Form
    {
        private DataGridView dgvSchedule;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public ScheduleSettingsForm()
        {
            InitializeComponent();
            LoadSchedule();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки расписания";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvSchedule = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvSchedule.Columns.Add("Task", "Задание");
            dgvSchedule.Columns.Add("Frequency", "Частота");
            dgvSchedule.Columns.Add("LastRun", "Последний запуск");
            dgvSchedule.Columns.Add("NextRun", "Следующий запуск");
            dgvSchedule.Columns.Add("Enabled", "Включено");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvSchedule);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadSchedule()
        {
            try
            {
                dgvSchedule.Rows.Clear();
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var scheduled = requests.Where(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.InProgress).ToList();
                foreach (var request in scheduled)
                {
                    var nextRun = request.CreatedAt.AddDays(7);
                    dgvSchedule.Rows.Add(request.Title, "Еженедельно", nextRun.ToString("yyyy-MM-dd HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка расписания");
            }
        }
    }
}
