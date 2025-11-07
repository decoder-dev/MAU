using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки фильтров
    /// </summary>
    public partial class FiltersSettingsForm : Form
    {
        private DataGridView dgvFilters;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public FiltersSettingsForm()
        {
            InitializeComponent();
            LoadFilters();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки фильтров";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvFilters = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvFilters.Columns.Add("Name", "Название");
            dgvFilters.Columns.Add("Type", "Тип");
            dgvFilters.Columns.Add("Condition", "Условие");
            dgvFilters.Columns.Add("Enabled", "Включено");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvFilters);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadFilters()
        {
            try
            {
                dgvFilters.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var filters = config.GetValue<List<object>>("Filters", new List<object>());
                if (filters != null)
                {
                    foreach (var filter in filters.Take(50))
                    {
                        if (filter is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var type = element.TryGetProperty("Type", out var t) ? t.GetString() ?? "-" : "-";
                            dgvFilters.Rows.Add(name, type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка фильтров");
            }
        }
    }
}
