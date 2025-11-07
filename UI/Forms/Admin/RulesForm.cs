using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление правилами
    /// </summary>
    public partial class RulesForm : Form
    {
        private DataGridView dgvRules;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public RulesForm()
        {
            InitializeComponent();
            LoadRules();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление правилами";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvRules = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRules.Columns.Add("Rule", "Правило");
            dgvRules.Columns.Add("Condition", "Условие");
            dgvRules.Columns.Add("Action", "Действие");
            dgvRules.Columns.Add("Enabled", "Включено");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRules);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadRules()
        {
            try
            {
                dgvRules.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var rules = config.GetValue<List<object>>("Rules", new List<object>());
                if (rules != null)
                {
                    foreach (var rule in rules.Take(50))
                    {
                        if (rule is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var type = element.TryGetProperty("Type", out var t) ? t.GetString() ?? "-" : "-";
                            var enabled = element.TryGetProperty("Enabled", out var e) && e.GetBoolean();
                            dgvRules.Rows.Add(name, type, enabled ? "Включено" : "Выключено");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка правил");
            }
        }
    }
}
