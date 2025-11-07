using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление импортом
    /// </summary>
    public partial class ImportTasksForm : Form
    {
        private DataGridView dgvImports;
        private Button btnCreate;
        private Button btnDelete;

        public ImportTasksForm()
        {
            InitializeComponent();
            LoadImports();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление импортом";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnCreate = new Button { Text = "Создать задачу", Location = new Point(15, 10), Width = 130 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(155, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnCreate, btnDelete });

            dgvImports = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvImports.Columns.Add("Task", "Задача");
            dgvImports.Columns.Add("Schedule", "Расписание");
            dgvImports.Columns.Add("LastRun", "Последний запуск");
            dgvImports.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvImports);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadImports()
        {
            try
            {
                dgvImports.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var imports = config.GetValue<List<object>>("ImportTasks", new List<object>());
                if (imports != null)
                {
                    foreach (var import in imports.Take(50))
                    {
                        if (import is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var status = element.TryGetProperty("Status", out var s) ? s.GetString() ?? "-" : "-";
                            var created = element.TryGetProperty("Created", out var c) ? c.GetString() ?? "-" : "-";
                            dgvImports.Rows.Add(name, status, created);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка импорта");
            }
        }
    }
}
