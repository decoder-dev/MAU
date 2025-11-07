using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление токенами
    /// </summary>
    public partial class ApiTokensForm : Form
    {
        private DataGridView dgvTokens;
        private Button btnGenerate;
        private Button btnRevoke;

        public ApiTokensForm()
        {
            InitializeComponent();
            LoadTokens();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление токенами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnGenerate = new Button { Text = "Сгенерировать", Location = new Point(15, 10), Width = 120 };
            btnRevoke = new Button { Text = "Отозвать", Location = new Point(145, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnGenerate, btnRevoke });

            dgvTokens = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvTokens.Columns.Add("Token", "Токен");
            dgvTokens.Columns.Add("User", "Пользователь");
            dgvTokens.Columns.Add("Created", "Создан");
            dgvTokens.Columns.Add("Expires", "Истекает");
            dgvTokens.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvTokens);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadTokens()
        {
            try
            {
                dgvTokens.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var tokens = config.GetValue<List<object>>("Tokens", new List<object>());
                if (tokens != null)
                {
                    foreach (var token in tokens.Take(50))
                    {
                        if (token is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var created = element.TryGetProperty("Created", out var c) ? c.GetString() ?? "-" : "-";
                            var expires = element.TryGetProperty("Expires", out var e) ? e.GetString() ?? "-" : "-";
                            dgvTokens.Rows.Add(name, created, expires);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка токенов");
            }
        }
    }
}
