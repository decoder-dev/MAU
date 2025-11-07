using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    public class DeviceImportForm : Form
    {
        private Button btnSelectFile = null!;
        private Button btnImport = null!;
        private Button btnCancel = null!;
        private TextBox txtFilePath = null!;
        private RadioButton rbCsv = null!;
        private RadioButton rbJson = null!;
        private Label lblStatus = null!;
        private ProgressBar progressBar = null!;
        private TextBox txtLog = null!;
        private ImportService? _importService;

        public DeviceImportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Импорт устройств";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // Панель выбора файла
            var panelFile = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(15)
            };

            var lblFile = new Label
            {
                Text = "Выберите файл для импорта:",
                Location = new Point(15, 15),
                AutoSize = true
            };

            txtFilePath = new TextBox
            {
                Location = new Point(15, 40),
                Width = 450,
                ReadOnly = true
            };

            btnSelectFile = new Button
            {
                Text = "Обзор...",
                Location = new Point(475, 38),
                Width = 100,
                Height = 25
            };
            btnSelectFile.Click += BtnSelectFile_Click;

            panelFile.Controls.AddRange(new Control[] { lblFile, txtFilePath, btnSelectFile });

            // Панель формата
            var panelFormat = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(15)
            };

            var lblFormat = new Label
            {
                Text = "Формат файла:",
                Location = new Point(15, 15),
                AutoSize = true
            };

            rbCsv = new RadioButton
            {
                Text = "CSV (разделитель - точка с запятой)",
                Location = new Point(15, 40),
                Checked = true,
                AutoSize = true
            };

            rbJson = new RadioButton
            {
                Text = "JSON",
                Location = new Point(250, 40),
                AutoSize = true
            };

            panelFormat.Controls.AddRange(new Control[] { lblFormat, rbCsv, rbJson });

            // Лог
            var panelLog = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            var lblLog = new Label
            {
                Text = "Лог импорта:",
                Dock = DockStyle.Top,
                Height = 20
            };

            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9)
            };

            panelLog.Controls.Add(txtLog);
            panelLog.Controls.Add(lblLog);

            // Статус и прогресс
            var panelStatus = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                Padding = new Padding(15)
            };

            lblStatus = new Label
            {
                Text = "Готов к импорту",
                Dock = DockStyle.Top,
                Height = 20
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 25,
                Style = ProgressBarStyle.Continuous
            };

            panelStatus.Controls.Add(progressBar);
            panelStatus.Controls.Add(lblStatus);

            // Кнопки
            var panelButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(15)
            };

            btnImport = new Button
            {
                Text = "Импортировать",
                Dock = DockStyle.Right,
                Width = 120,
                Height = 30,
                Enabled = false
            };
            btnImport.Click += BtnImport_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Dock = DockStyle.Right,
                Width = 100,
                Height = 30,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnCancel.Click += (s, e) => this.Close();

            panelButtons.Controls.AddRange(new Control[] { btnImport, btnCancel });

            this.Controls.Add(panelLog);
            this.Controls.Add(panelStatus);
            this.Controls.Add(panelFormat);
            this.Controls.Add(panelFile);
            this.Controls.Add(panelButtons);
        }

        private void BtnSelectFile_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = rbCsv.Checked 
                    ? "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*"
                    : "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                Title = "Выберите файл для импорта"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dialog.FileName;
                btnImport.Enabled = true;
                LogMessage($"Выбран файл: {dialog.FileName}");
            }
        }

        private async void BtnImport_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                ErrorHandler.ShowError("Выберите существующий файл для импорта");
                return;
            }

            btnImport.Enabled = false;
            btnSelectFile.Enabled = false;
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "Импорт в процессе...";
            LogMessage("Начало импорта...");

            try
            {
                _importService = new ImportService(Program.DbContext);

                var result = rbCsv.Checked
                    ? await _importService.ImportDevicesFromCsvAsync(txtFilePath.Text)
                    : await _importService.ImportDevicesFromJsonAsync(txtFilePath.Text);

                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 100;

                if (result.Success)
                {
                    lblStatus.Text = $"Импорт завершен: {result.Imported} устройств импортировано";
                    LogMessage($"✓ Импорт завершен успешно");
                    LogMessage($"  Импортировано: {result.Imported}");
                    LogMessage($"  Пропущено: {result.Skipped}");

                    if (result.Errors.Any())
                    {
                        LogMessage($"  Ошибок: {result.Errors.Count}");
                        foreach (var error in result.Errors)
                        {
                            LogMessage($"  - {error}");
                        }
                    }

                    ErrorHandler.ShowSuccess($"Импорт завершен!\nИмпортировано: {result.Imported}\nПропущено: {result.Skipped}");
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    lblStatus.Text = "Ошибка импорта";
                    LogMessage("✗ Ошибка импорта:");
                    foreach (var error in result.Errors)
                    {
                        LogMessage($"  - {error}");
                    }
                    ErrorHandler.ShowError($"Ошибка импорта:\n{string.Join("\n", result.Errors)}");
                }
            }
            catch (Exception ex)
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                lblStatus.Text = "Ошибка импорта";
                LogMessage($"✗ Критическая ошибка: {ex.Message}");
                ErrorHandler.HandleException(ex, "Импорт устройств");
            }
            finally
            {
                btnImport.Enabled = true;
                btnSelectFile.Enabled = true;
            }
        }

        private void LogMessage(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(LogMessage), message);
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}
