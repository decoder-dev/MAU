using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки экспорта
    /// </summary>
    public partial class ExportSettingsForm : Form
    {
        private ComboBox cmbDefaultFormat;
        private TextBox txtExportPath;
        private CheckBox chkIncludeHeaders;
        private Button btnSave;

        public ExportSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки экспорта";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblFormat = new Label { Text = "Формат по умолчанию:", Location = new Point(20, 20), AutoSize = true };
            cmbDefaultFormat = new ComboBox { Location = new Point(170, 17), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDefaultFormat.Items.AddRange(new[] { "CSV", "TXT", "JSON", "XML" });
            cmbDefaultFormat.SelectedIndex = 0;
            var lblPath = new Label { Text = "Путь экспорта:", Location = new Point(20, 60), AutoSize = true };
            txtExportPath = new TextBox { Location = new Point(20, 90), Width = 400, ReadOnly = true };
            var btnBrowse = new Button { Text = "Обзор...", Location = new Point(430, 88), Width = 80 };
            btnBrowse.Click += (s, e) =>
            {
                using var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                    txtExportPath.Text = dialog.SelectedPath;
            };
            chkIncludeHeaders = new CheckBox { Text = "Включать заголовки", Location = new Point(20, 130), Checked = true };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 170), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblFormat, cmbDefaultFormat, lblPath, txtExportPath, btnBrowse, chkIncludeHeaders, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var format = config.GetValue<string>("Export_DefaultFormat", "CSV");
                cmbDefaultFormat.SelectedItem = format;
                txtExportPath.Text = config.GetValue<string>("Export_Path", "");
                chkIncludeHeaders.Checked = config.GetValue<bool>("Export_IncludeHeaders", true);
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
                config.SetValue("Export_DefaultFormat", cmbDefaultFormat.SelectedItem?.ToString() ?? "CSV");
                config.SetValue("Export_Path", txtExportPath.Text);
                config.SetValue("Export_IncludeHeaders", chkIncludeHeaders.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
