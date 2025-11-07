using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Шаблоны заявок
    /// </summary>
    public partial class RequestTemplatesForm : Form
    {
        private DataGridView dgvTemplates;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public RequestTemplatesForm()
        {
            InitializeComponent();
            LoadTemplates();
        }

        private void InitializeComponent()
        {
            this.Text = "Шаблоны заявок";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvTemplates = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvTemplates.Columns.Add("Name", "Название");
            dgvTemplates.Columns.Add("Description", "Описание");
            dgvTemplates.Columns.Add("Priority", "Приоритет");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvTemplates);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadTemplates()
        {
            try
            {
                dgvTemplates.Rows.Clear();
                var devices = await DataService.GetDevicesAsync();
                var templates = devices.GroupBy(d => new { d.DeviceType, d.Model }).ToList();
                foreach (var template in templates)
                {
                    dgvTemplates.Rows.Add($"{template.Key.DeviceType} - {template.Key.Model}", template.Count());
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка шаблонов");
            }
        }
    }
}
