using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки отчетов
    /// </summary>
    public partial class ReportSettingsForm : Form
    {
        private ComboBox cmbReportFormat;
        private CheckBox chkAutoReport;
        private Button btnSave;

        public ReportSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки отчетов";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblFormat = new Label { Text = "Формат отчета:", Location = new Point(20, 20), AutoSize = true };
            cmbReportFormat = new ComboBox { Location = new Point(120, 17), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportFormat.Items.AddRange(new[] { "CSV", "TXT", "PDF" });
            cmbReportFormat.SelectedIndex = 0;
            chkAutoReport = new CheckBox { Text = "Автоматическая генерация отчетов", Location = new Point(20, 60) };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 100), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblFormat, cmbReportFormat, chkAutoReport, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var format = config.GetValue<string>("Reports_Format", "CSV");
                cmbReportFormat.SelectedItem = format;
                chkAutoReport.Checked = config.GetValue<bool>("Reports_AutoGenerate", false);
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
                config.SetValue("Reports_Format", cmbReportFormat.SelectedItem?.ToString() ?? "CSV");
                config.SetValue("Reports_AutoGenerate", chkAutoReport.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
