using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление сертификатами
    /// </summary>
    public partial class CertificatesForm : Form
    {
        private DataGridView dgvCertificates;
        private Button btnImport;
        private Button btnDelete;

        public CertificatesForm()
        {
            InitializeComponent();
            LoadCertificates();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление сертификатами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnImport = new Button { Text = "Импортировать", Location = new Point(15, 10), Width = 120 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(145, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnImport, btnDelete });

            dgvCertificates = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvCertificates.Columns.Add("Subject", "Субъект");
            dgvCertificates.Columns.Add("Issuer", "Издатель");
            dgvCertificates.Columns.Add("Expires", "Истекает");
            dgvCertificates.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvCertificates);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadCertificates()
        {
            try
            {
                dgvCertificates.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var certificates = config.GetValue<List<object>>("Certificates", new List<object>());
                if (certificates != null)
                {
                    foreach (var cert in certificates.Take(50))
                    {
                        if (cert is System.Text.Json.JsonElement element)
                        {
                            var name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "-" : "-";
                            var issuer = element.TryGetProperty("Issuer", out var i) ? i.GetString() ?? "-" : "-";
                            var expires = element.TryGetProperty("Expires", out var e) ? e.GetString() ?? "-" : "-";
                            dgvCertificates.Rows.Add(name, issuer, expires);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка сертификатов");
            }
        }
    }
}
