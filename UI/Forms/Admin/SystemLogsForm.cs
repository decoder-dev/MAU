using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// ���������� ���������� ������
    /// </summary>
    public partial class SystemLogsForm : Form
    {
        private DataGridView dgvLogs = null!;
        private Button btnRefresh = null!;
        private Button btnClear = null!;
        private Button btnExport = null!;
        private ComboBox cmbLogLevel = null!;
        private DateTimePicker dtpStart = null!;
        private DateTimePicker dtpEnd = null!;

        public SystemLogsForm()
        {
            InitializeComponent();
            LoadLogs();
        }

        private void InitializeComponent()
        {
            this.Text = "��������� ����";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(15)
            };

            var lblLevel = new Label { Text = "�������:", Location = new Point(15, 15), AutoSize = true };
            cmbLogLevel = new ComboBox
            {
                Location = new Point(80, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLogLevel.Items.AddRange(new[] { "���", "INFO", "WARNING", "ERROR" });
            cmbLogLevel.SelectedIndex = 0;
            cmbLogLevel.SelectedIndexChanged += (s, e) => LoadLogs();

            var lblStart = new Label { Text = "�:", Location = new Point(250, 15), AutoSize = true };
            dtpStart = new DateTimePicker
            {
                Location = new Point(275, 12),
                Width = 150,
                Format = DateTimePickerFormat.Short
            };
            dtpStart.Value = DateTime.Now.AddDays(-7);
            dtpStart.ValueChanged += (s, e) => LoadLogs();

            var lblEnd = new Label { Text = "��:", Location = new Point(435, 15), AutoSize = true };
            dtpEnd = new DateTimePicker
            {
                Location = new Point(460, 12),
                Width = 150,
                Format = DateTimePickerFormat.Short
            };
            dtpEnd.ValueChanged += (s, e) => LoadLogs();

            btnRefresh = new Button { Text = "��������", Location = new Point(630, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadLogs();

            btnClear = new Button { Text = "��������", Location = new Point(740, 10), Width = 100, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnClear.Click += BtnClear_Click;

            btnExport = new Button { Text = "�������", Location = new Point(850, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;

            panelFilters.Controls.AddRange(new Control[] { lblLevel, cmbLogLevel, lblStart, dtpStart, lblEnd, dtpEnd, btnRefresh, btnClear, btnExport });

            dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvLogs.Columns.Add("Timestamp", "�����");
            dgvLogs.Columns.Add("Level", "�������");
            dgvLogs.Columns.Add("Message", "���������");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvLogs);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private void LoadLogs()
        {
            try
            {
                var loggingService = new LoggingService(Program.DbContext);
                var logs = loggingService.GetLogs(dtpStart.Value, dtpEnd.Value);

                dgvLogs.Rows.Clear();

                foreach (var log in logs)
                {
                    var level = cmbLogLevel.SelectedIndex == 0 ? "" : cmbLogLevel.Text;
                    if (string.IsNullOrEmpty(level) || log.Contains($"[{level}]"))
                    {
                        var parts = log.Split(new[] { "] [" }, StringSplitOptions.None);
                        if (parts.Length >= 3)
                        {
                            var time = parts[0].TrimStart('[');
                            var logLevel = parts[1].TrimEnd(']');
                            var message = parts[2].Trim();
                            dgvLogs.Rows.Add(time, logLevel, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "�������� �����");
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (ErrorHandler.ShowConfirm("�������� ��� ����?", "�������������"))
            {
                var loggingService = new LoggingService(Program.DbContext);
                loggingService.CleanupOldLogs(0);
                LoadLogs();
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "��������� ����� (*.txt)|*.txt",
                FileName = $"logs_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                foreach (DataGridViewRow row in dgvLogs.Rows)
                {
                    sb.AppendLine($"{row.Cells[0].Value}\t{row.Cells[1].Value}\t{row.Cells[2].Value}");
                }
                File.WriteAllText(dialog.FileName, sb.ToString());
                ErrorHandler.ShowSuccess("���� ��������������");
            }
        }
    }
}