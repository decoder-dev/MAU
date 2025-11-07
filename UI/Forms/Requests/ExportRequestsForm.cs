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
    /// Экспорт заявок
    /// </summary>
    public partial class ExportRequestsForm : Form
    {
        private Button btnExport;
        private ProgressBar progressBar;
        private Label lblStatus;

        public ExportRequestsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Экспорт заявок";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            btnExport = new Button { Text = "Экспортировать в CSV", Dock = DockStyle.Top, Height = 50, Margin = new Padding(0, 0, 0, 20) };
            btnExport.Click += BtnExport_Click;
            progressBar = new ProgressBar { Dock = DockStyle.Top, Height = 30, Margin = new Padding(0, 0, 0, 10), Style = ProgressBarStyle.Marquee };
            progressBar.Visible = false;
            lblStatus = new Label { Dock = DockStyle.Top, Height = 30, Text = "Готово к экспорту", TextAlign = ContentAlignment.MiddleCenter };
            panel.Controls.AddRange(new Control[] { lblStatus, progressBar, btnExport });
            this.Controls.Add(panel);
        }

        private async void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"requests_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    btnExport.Enabled = false;
                    progressBar.Visible = true;
                    lblStatus.Text = "Экспорт...";
                    
                    var requests = await DataService.GetMaintenanceRequestsAsync();
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("ID;Название;Устройство;Статус;Приоритет;Создано;Исполнитель;Описание");
                    foreach (var request in requests)
                    {
                        sb.AppendLine($"{request.Id};{request.Title};{request.Device?.Name ?? "-"};{request.Status};{request.Priority};{request.CreatedAt:dd.MM.yyyy HH:mm};{request.AssignedTo ?? "-"};{request.Description ?? "-"}");
                    }
                    File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    
                    lblStatus.Text = $"Экспортировано {requests.Count} заявок";
                    ErrorHandler.ShowSuccess("Экспорт завершен");
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "Экспорт заявок");
                }
                finally
                {
                    btnExport.Enabled = true;
                    progressBar.Visible = false;
                }
            }
        }
    }
}
