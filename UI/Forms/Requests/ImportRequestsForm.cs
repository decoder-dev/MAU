using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Импорт заявок
    /// </summary>
    public partial class ImportRequestsForm : Form
    {
        private Button btnImport;
        private Label lblStatus;

        public ImportRequestsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Импорт заявок";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            btnImport = new Button { Text = "Импортировать из CSV", Dock = DockStyle.Top, Height = 50, Margin = new Padding(0, 0, 0, 20) };
            btnImport.Click += BtnImport_Click;
            lblStatus = new Label { Dock = DockStyle.Top, Height = 30, Text = "Готово к импорту", TextAlign = ContentAlignment.MiddleCenter };
            panel.Controls.AddRange(new Control[] { lblStatus, btnImport });
            this.Controls.Add(panel);
        }

        private async void BtnImport_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "CSV файлы (*.csv)|*.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    btnImport.Enabled = false;
                    lblStatus.Text = "Импорт...";
                    
                    var lines = File.ReadAllLines(dialog.FileName, System.Text.Encoding.UTF8);
                    int imported = 0;
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var parts = lines[i].Split(';');
                        if (parts.Length >= 3)
                        {
                            var request = new MaintenanceRequest
                            {
                                Title = parts[1],
                                Description = parts.Length > 7 ? parts[7] : "",
                                Status = RequestStatus.Pending,
                                Priority = parts.Length > 4 && int.TryParse(parts[4], out var p) ? p : 1,
                                CreatedAt = DateTime.UtcNow
                            };
                            await DataService.AddMaintenanceRequestAsync(request);
                            imported++;
                        }
                    }
                    
                    lblStatus.Text = $"Импортировано {imported} заявок";
                    ErrorHandler.ShowSuccess($"Импортировано {imported} заявок");
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "Импорт заявок");
                }
                finally
                {
                    btnImport.Enabled = true;
                }
            }
        }
    }
}
