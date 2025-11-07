using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление экспортом
    /// </summary>
    public partial class ExportTasksForm : Form
    {
        private DataGridView dgvExports;
        private Button btnCreate;
        private Button btnDelete;

        public ExportTasksForm()
        {
            InitializeComponent();
            LoadExports();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление экспортом";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnCreate = new Button { Text = "Создать задачу", Location = new Point(15, 10), Width = 130 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(155, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnCreate, btnDelete });

            dgvExports = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvExports.Columns.Add("Task", "Задача");
            dgvExports.Columns.Add("Schedule", "Расписание");
            dgvExports.Columns.Add("LastRun", "Последний запуск");
            dgvExports.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvExports);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadExports()
        {
            try
            {
                dgvExports.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var exports = config.GetValue<List<object>>("ExportTasks", new List<object>());
                if (exports != null)
                {
                    foreach (var export in exports.Take(50))
                    {
                        if (export is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var status = element.TryGetProperty("Status", out var s) ? s.GetString() ?? "-" : "-";
                            var created = element.TryGetProperty("Created", out var c) ? c.GetString() ?? "-" : "-";
                            dgvExports.Rows.Add(name, status, created);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка экспорта");
            }
        }
    }
}
