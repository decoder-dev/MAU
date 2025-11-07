using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки импорта
    /// </summary>
    public partial class ImportSettingsForm : Form
    {
        private ComboBox cmbDefaultFormat;
        private CheckBox chkAutoImport;
        private TextBox txtImportPath;
        private Button btnSave;

        public ImportSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки импорта";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblFormat = new Label { Text = "Формат по умолчанию:", Location = new Point(20, 20), AutoSize = true };
            cmbDefaultFormat = new ComboBox { Location = new Point(170, 17), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDefaultFormat.Items.AddRange(new[] { "CSV", "TXT", "JSON", "XML" });
            cmbDefaultFormat.SelectedIndex = 0;
            chkAutoImport = new CheckBox { Text = "Автоматический импорт из папки", Location = new Point(20, 60) };
            var lblPath = new Label { Text = "Папка для импорта:", Location = new Point(20, 90), AutoSize = true };
            txtImportPath = new TextBox { Location = new Point(20, 120), Width = 400, ReadOnly = true };
            var btnBrowse = new Button { Text = "Обзор...", Location = new Point(430, 118), Width = 80 };
            btnBrowse.Click += (s, e) =>
            {
                using var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                    txtImportPath.Text = dialog.SelectedPath;
            };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 160), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblFormat, cmbDefaultFormat, chkAutoImport, lblPath, txtImportPath, btnBrowse, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var format = config.GetValue<string>("Import_DefaultFormat", "CSV");
                cmbDefaultFormat.SelectedItem = format;
                chkAutoImport.Checked = config.GetValue<bool>("Import_AutoImport", false);
                txtImportPath.Text = config.GetValue<string>("Import_Path", "");
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
                config.SetValue("Import_DefaultFormat", cmbDefaultFormat.SelectedItem?.ToString() ?? "CSV");
                config.SetValue("Import_AutoImport", chkAutoImport.Checked);
                config.SetValue("Import_Path", txtImportPath.Text);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
