using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Сброс настроек
    /// </summary>
    public partial class ResetSettingsForm : Form
    {
        private Button btnResetAll;
        private Button btnResetInterface;
        private Button btnResetNotifications;
        private Label lblWarning;

        public ResetSettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Сброс настроек";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            lblWarning = new Label { Text = "Внимание! Сброс настроек необратим!", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.Red };
            btnResetInterface = new Button { Text = "Сбросить настройки интерфейса", Location = new Point(20, 60), Width = 400 };
            btnResetInterface.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Сбросить настройки интерфейса?", "Подтверждение"))
                    ErrorHandler.ShowSuccess("Настройки интерфейса сброшены");
            };
            btnResetNotifications = new Button { Text = "Сбросить настройки уведомлений", Location = new Point(20, 100), Width = 400 };
            btnResetNotifications.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Сбросить настройки уведомлений?", "Подтверждение"))
                    ErrorHandler.ShowSuccess("Настройки уведомлений сброшены");
            };
            btnResetAll = new Button { Text = "Сбросить ВСЕ настройки", Location = new Point(20, 140), Width = 400, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnResetAll.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Сбросить ВСЕ настройки? Это действие необратимо!", "ВНИМАНИЕ!"))
                    ErrorHandler.ShowSuccess("Все настройки сброшены");
            };
            panel.Controls.AddRange(new Control[] { lblWarning, btnResetInterface, btnResetNotifications, btnResetAll });
            this.Controls.Add(panel);
        }
    }
}
