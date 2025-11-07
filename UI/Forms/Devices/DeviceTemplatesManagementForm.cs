using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Шаблоны устройств
    /// </summary>
    public partial class DeviceTemplatesManagementForm : Form
    {
        private DataGridView dgvTemplates;
        private Button btnRefresh;
        private Button btnCreate;
        private Button btnDelete;
        private Label lblInfo;
        private TemplateService? _templateService;

        public DeviceTemplatesManagementForm()
        {
            InitializeComponent();
            _templateService = new TemplateService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Шаблоны устройств";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelButtons = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnCreate = new Button { Text = "Создать шаблон", Location = new Point(15, 10), Width = 130 };
            btnCreate.Click += BtnCreate_Click;
            btnDelete = new Button { Text = "Удалить", Location = new Point(155, 10), Width = 100, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(265, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelButtons.Controls.AddRange(new Control[] { btnCreate, btnDelete, btnRefresh });

            dgvTemplates = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvTemplates.Columns.Add("Name", "Название");
            dgvTemplates.Columns.Add("Type", "Тип");
            dgvTemplates.Columns.Add("Description", "Описание");
            dgvTemplates.Columns.Add("CreatedAt", "Создано");

            lblInfo = new Label { Dock = DockStyle.Bottom, Height = 30, Text = "Готово", Padding = new Padding(15, 5, 0, 0) };

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvTemplates);
            panelMain.Controls.Add(lblInfo);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
        }

        private async void LoadData()
        {
            try
            {
                dgvTemplates.Rows.Clear();
                var devices = await DataService.GetDevicesAsync();
                var templates = devices
                    .GroupBy(d => new { d.DeviceType, d.Model, d.Manufacturer })
                    .Select(g => new { Name = $"{g.Key.DeviceType} - {g.Key.Model}", Type = g.Key.DeviceType.ToString(), Description = $"Производитель: {g.Key.Manufacturer}, Устройств: {g.Count()}", CreatedAt = g.Min(d => d.CreatedAt) });
                
                foreach (var template in templates)
                {
                    dgvTemplates.Rows.Add(template.Name, template.Type, template.Description, template.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                }
                lblInfo.Text = $"Загружено шаблонов: {templates.Count()}";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка шаблонов");
            }
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            ErrorHandler.ShowInfo("Функция создания шаблона будет реализована");
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvTemplates.SelectedRows.Count == 0)
            {
                ErrorHandler.ShowWarning("Выберите шаблон для удаления");
                return;
            }
            if (ErrorHandler.ShowConfirm("Удалить выбранный шаблон?", "Подтверждение"))
            {
                ErrorHandler.ShowInfo("Функция удаления шаблона будет реализована");
            }
        }
    }
}
