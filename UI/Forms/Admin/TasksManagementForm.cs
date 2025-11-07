using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление задачами
    /// </summary>
    public partial class TasksManagementForm : Form
    {
        private DataGridView dgvTasks;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRun;

        public TasksManagementForm()
        {
            InitializeComponent();
            LoadTasks();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление задачами";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            btnRun = new Button { Text = "Запустить", Location = new Point(345, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRun });

            dgvTasks = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvTasks.Columns.Add("Task", "Задача");
            dgvTasks.Columns.Add("Schedule", "Расписание");
            dgvTasks.Columns.Add("LastRun", "Последний запуск");
            dgvTasks.Columns.Add("NextRun", "Следующий запуск");
            dgvTasks.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvTasks);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadTasks()
        {
            try
            {
                dgvTasks.Rows.Clear();
                dgvTasks.Rows.Add("Синхронизация", "Каждую минуту", DateTime.Now.ToString("dd.MM.yyyy HH:mm"), DateTime.Now.AddMinutes(1).ToString("dd.MM.yyyy HH:mm"), "Активна");
                dgvTasks.Rows.Add("Резервное копирование", "Ежедневно в 00:00", "-", DateTime.Now.AddDays(1).Date.ToString("dd.MM.yyyy HH:mm"), "Активна");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка задач");
            }
        }
    }
}
