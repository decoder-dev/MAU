using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Резервное копирование
    /// </summary>
    public partial class BackupForm : Form
    {
        private Button btnFullBackup;
        private Button btnDevicesBackup;
        private ListBox lstBackups;
        private Button btnRefresh;
        private Button btnRestore;
        private Button btnDelete;
        private Label lblInfo;
        private BackupService? _backupService;

        public BackupForm()
        {
            InitializeComponent();
            _backupService = new BackupService(Program.DbContext);
            LoadBackups();
        }

        private void InitializeComponent()
        {
            this.Text = "Резервное копирование";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            btnFullBackup = new Button { Text = "Полная копия", Location = new Point(15, 15), Width = 120 };
            btnFullBackup.Click += BtnFullBackup_Click;
            btnDevicesBackup = new Button { Text = "Копия устройств", Location = new Point(145, 15), Width = 130 };
            btnDevicesBackup.Click += BtnDevicesBackup_Click;
            btnRefresh = new Button { Text = "Обновить список", Location = new Point(285, 15), Width = 130 };
            btnRefresh.Click += (s, e) => LoadBackups();
            panelTop.Controls.AddRange(new Control[] { btnFullBackup, btnDevicesBackup, btnRefresh });

            var panelList = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            var lblList = new Label { Text = "Доступные резервные копии:", Dock = DockStyle.Top, Height = 20 };
            lstBackups = new ListBox { Dock = DockStyle.Fill };
            panelList.Controls.Add(lstBackups);
            panelList.Controls.Add(lblList);

            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(15) };
            btnRestore = new Button { Text = "Восстановить", Location = new Point(15, 15), Width = 120, Enabled = false };
            btnRestore.Click += BtnRestore_Click;
            btnDelete = new Button { Text = "Удалить", Location = new Point(145, 15), Width = 100, Enabled = false, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;
            lstBackups.SelectedIndexChanged += (s, e) =>
            {
                btnRestore.Enabled = lstBackups.SelectedIndex >= 0;
                btnDelete.Enabled = lstBackups.SelectedIndex >= 0;
            };
            lblInfo = new Label { Dock = DockStyle.Top, Height = 30, Text = "Готово", Padding = new Padding(15, 5, 0, 0) };
            panelBottom.Controls.AddRange(new Control[] { btnRestore, btnDelete });

            this.Controls.Add(panelList);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
            this.Controls.Add(lblInfo);
        }

        private async void BtnFullBackup_Click(object? sender, EventArgs e)
        {
            try
            {
                btnFullBackup.Enabled = false;
                lblInfo.Text = "Создание резервной копии...";
                var backupPath = await _backupService!.CreateFullBackupAsync();
                lblInfo.Text = $"Резервная копия создана: {Path.GetFileName(backupPath)}";
                LoadBackups();
                ErrorHandler.ShowSuccess($"Резервная копия создана:\n{backupPath}");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Создание резервной копии");
            }
            finally
            {
                btnFullBackup.Enabled = true;
            }
        }

        private async void BtnDevicesBackup_Click(object? sender, EventArgs e)
        {
            try
            {
                btnDevicesBackup.Enabled = false;
                lblInfo.Text = "Создание копии устройств...";
                var backupPath = await _backupService!.CreateDevicesBackupAsync();
                lblInfo.Text = $"Копия устройств создана: {Path.GetFileName(backupPath)}";
                LoadBackups();
                ErrorHandler.ShowSuccess($"Копия устройств создана:\n{backupPath}");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Создание копии устройств");
            }
            finally
            {
                btnDevicesBackup.Enabled = true;
            }
        }

        private void LoadBackups()
        {
            try
            {
                var backups = _backupService!.GetAvailableBackups();
                lstBackups.Items.Clear();
                foreach (var backup in backups)
                {
                    lstBackups.Items.Add($"{backup.FileName} ({backup.CreatedAt:dd.MM.yyyy HH:mm}, {backup.Size / 1024} KB)");
                }
                lblInfo.Text = $"Найдено резервных копий: {backups.Count}";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка списка копий");
            }
        }

        private async void BtnRestore_Click(object? sender, EventArgs e)
        {
            if (lstBackups.SelectedIndex < 0) return;
            var backups = _backupService!.GetAvailableBackups();
            if (lstBackups.SelectedIndex >= backups.Count) return;

            var backup = backups[lstBackups.SelectedIndex];
            if (!ErrorHandler.ShowConfirm($"Восстановить данные из резервной копии?\n{backup.FileName}", "Подтверждение"))
                return;

            try
            {
                btnRestore.Enabled = false;
                lblInfo.Text = "Восстановление данных...";
                var restoreService = new RestoreService(Program.DbContext);
                var success = await restoreService.RestoreFromFullBackupAsync(backup.FilePath, false);
                if (success)
                {
                    lblInfo.Text = "Данные восстановлены";
                    ErrorHandler.ShowSuccess("Данные успешно восстановлены");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Восстановление данных");
            }
            finally
            {
                btnRestore.Enabled = true;
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lstBackups.SelectedIndex < 0) return;
            var backups = _backupService!.GetAvailableBackups();
            if (lstBackups.SelectedIndex >= backups.Count) return;

            var backup = backups[lstBackups.SelectedIndex];
            if (!ErrorHandler.ShowConfirm($"Удалить резервную копию?\n{backup.FileName}", "Подтверждение"))
                return;

            try
            {
                File.Delete(backup.FilePath);
                LoadBackups();
                ErrorHandler.ShowSuccess("Резервная копия удалена");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Удаление копии");
            }
        }
    }
}
