using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки резервного копирования
    /// </summary>
    public partial class BackupSettingsForm : Form
    {
        private NumericUpDown numBackupDays;
        private CheckBox chkAutoBackup;
        private TextBox txtBackupPath;
        private Button btnBrowse;
        private Button btnSave;

        public BackupSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки резервного копирования";
            this.Size = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblDays = new Label { Text = "Хранить копии (дней):", Location = new Point(20, 20), AutoSize = true };
            numBackupDays = new NumericUpDown { Location = new Point(160, 17), Width = 100, Minimum = 1, Maximum = 365, Value = 30 };
            chkAutoBackup = new CheckBox { Text = "Автоматическое резервное копирование", Location = new Point(20, 60), Checked = true };
            var lblPath = new Label { Text = "Путь для копий:", Location = new Point(20, 100), AutoSize = true };
            txtBackupPath = new TextBox { Location = new Point(20, 130), Width = 400, ReadOnly = true };
            btnBrowse = new Button { Text = "Обзор...", Location = new Point(430, 128), Width = 100 };
            btnBrowse.Click += (s, e) =>
            {
                using var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                    txtBackupPath.Text = dialog.SelectedPath;
            };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 180), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblDays, numBackupDays, chkAutoBackup, lblPath, txtBackupPath, btnBrowse, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                numBackupDays.Value = config.GetValue<int>("Backup_RetentionDays", 30);
                chkAutoBackup.Checked = config.GetValue<bool>("Backup_AutoBackup", true);
                txtBackupPath.Text = config.GetValue<string>("Backup_Path", "");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка настроек");
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                config.SetValue("Backup_RetentionDays", (int)numBackupDays.Value);
                config.SetValue("Backup_AutoBackup", chkAutoBackup.Checked);
                config.SetValue("Backup_Path", txtBackupPath.Text);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
