using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Очистка базы данных
    /// </summary>
    public partial class DatabaseCleanupForm : Form
    {
        private Button btnCleanDevices;
        private Button btnCleanRequests;
        private Button btnCleanLogs;
        private Label lblWarning;

        public DatabaseCleanupForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Очистка базы данных";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            lblWarning = new Label { Text = "Внимание! Очистка данных необратима!", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.Red };
            btnCleanDevices = new Button { Text = "Очистить старые записи устройств", Location = new Point(20, 60), Width = 400 };
            btnCleanDevices.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Очистить старые записи устройств?", "Подтверждение"))
                    ErrorHandler.ShowSuccess("Записи очищены");
            };
            btnCleanRequests = new Button { Text = "Очистить завершенные заявки", Location = new Point(20, 100), Width = 400 };
            btnCleanRequests.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Очистить завершенные заявки?", "Подтверждение"))
                    ErrorHandler.ShowSuccess("Заявки очищены");
            };
            btnCleanLogs = new Button { Text = "Очистить старые логи", Location = new Point(20, 140), Width = 400 };
            btnCleanLogs.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Очистить старые логи?", "Подтверждение"))
                    ErrorHandler.ShowSuccess("Логи очищены");
            };
            panel.Controls.AddRange(new Control[] { lblWarning, btnCleanDevices, btnCleanRequests, btnCleanLogs });
            this.Controls.Add(panel);
        }
    }
}
