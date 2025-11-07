using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление виджетами
    /// </summary>
    public partial class WidgetsForm : Form
    {
        private DataGridView dgvWidgets;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public WidgetsForm()
        {
            InitializeComponent();
            LoadWidgets();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление виджетами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvWidgets = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvWidgets.Columns.Add("Widget", "Виджет");
            dgvWidgets.Columns.Add("Type", "Тип");
            dgvWidgets.Columns.Add("DataSource", "Источник данных");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvWidgets);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadWidgets()
        {
            try
            {
                dgvWidgets.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var widgets = config.GetValue<List<object>>("Widgets", new List<object>());
                if (widgets != null)
                {
                    foreach (var widget in widgets.Take(50))
                    {
                        if (widget is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var type = element.TryGetProperty("Type", out var t) ? t.GetString() ?? "-" : "-";
                            var enabled = element.TryGetProperty("Enabled", out var e) && e.GetBoolean();
                            dgvWidgets.Rows.Add(name, type, enabled ? "Включен" : "Выключен");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка виджетов");
            }
        }
    }
}
