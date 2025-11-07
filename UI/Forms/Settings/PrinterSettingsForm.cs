using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки принтера
    /// </summary>
    public partial class PrinterSettingsForm : Form
    {
        private ComboBox cmbPrinter;
        private CheckBox chkUseDefault;
        private Button btnSave;

        public PrinterSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки принтера";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblPrinter = new Label { Text = "Принтер:", Location = new Point(20, 20), AutoSize = true };
            cmbPrinter = new ComboBox { Location = new Point(80, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                cmbPrinter.Items.Add(printer);
            chkUseDefault = new CheckBox { Text = "Использовать принтер по умолчанию", Location = new Point(20, 60), Checked = true };
            chkUseDefault.CheckedChanged += (s, e) => cmbPrinter.Enabled = !chkUseDefault.Checked;
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 100), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblPrinter, cmbPrinter, chkUseDefault, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var printer = config.GetValue<string>("Printer_Name", "");
                if (!string.IsNullOrEmpty(printer) && cmbPrinter.Items.Contains(printer))
                    cmbPrinter.SelectedItem = printer;
                chkUseDefault.Checked = config.GetValue<bool>("Printer_UseDefault", true);
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
                config.SetValue("Printer_Name", chkUseDefault.Checked ? "" : cmbPrinter.SelectedItem?.ToString() ?? "");
                config.SetValue("Printer_UseDefault", chkUseDefault.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
