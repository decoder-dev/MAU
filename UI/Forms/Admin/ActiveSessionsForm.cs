using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление сессиями
    /// </summary>
    public partial class ActiveSessionsForm : Form
    {
        private DataGridView dgvSessions;
        private Button btnKill;
        private Button btnRefresh;

        public ActiveSessionsForm()
        {
            InitializeComponent();
            LoadSessions();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление сессиями";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnKill = new Button { Text = "Завершить сессию", Location = new Point(15, 10), Width = 130 };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(155, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadSessions();
            panelTop.Controls.AddRange(new Control[] { btnKill, btnRefresh });

            dgvSessions = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvSessions.Columns.Add("User", "Пользователь");
            dgvSessions.Columns.Add("LoginTime", "Время входа");
            dgvSessions.Columns.Add("LastActivity", "Последняя активность");
            dgvSessions.Columns.Add("IP", "IP адрес");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvSessions);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadSessions()
        {
            try
            {
                dgvSessions.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var sessions = config.GetValue<List<object>>("ActiveSessions", new List<object>());
                if (sessions != null)
                {
                    foreach (var session in sessions.Take(50))
                    {
                        if (session is System.Text.Json.JsonElement element)
                        {
                            var user = element.TryGetProperty("User", out var u) ? u.GetString() ?? "-" : "-";
                            var loginTime = element.TryGetProperty("LoginTime", out var lt) ? lt.GetString() ?? "-" : "-";
                            var lastActivity = element.TryGetProperty("LastActivity", out var la) ? la.GetString() ?? "-" : "-";
                            dgvSessions.Rows.Add(user, loginTime, lastActivity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка сессий");
            }
        }
    }
}
