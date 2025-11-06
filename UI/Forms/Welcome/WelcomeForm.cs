using System.Drawing;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.UI.Forms.Welcome
{
    /// <summary>
    /// Форма приветствия
    /// </summary>
    public partial class WelcomeForm : Form
    {
        private Panel panelMain;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblDescription;
        private Button btnStart;
        private Button btnSkip;

        public WelcomeForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Добро пожаловать - МАУ";
            this.Size = new Size(850, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Icon = SystemIcons.Application;

            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(50, 40, 50, 30)
            };

            // Заголовок
            lblTitle = new Label
            {
                Text = Core.Models.UniversityInfo.ShortName,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(750, 60),
                Location = new Point(50, 50),
                Dock = DockStyle.Top
            };

            // Подзаголовок
            lblSubtitle = new Label
            {
                Text = "Система мониторинга сетевого оборудования",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.FromArgb(127, 140, 141),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(750, 40),
                Location = new Point(50, 120),
                Dock = DockStyle.Top,
                Margin = new Padding(0, 10, 0, 0)
            };

            // Описание
            lblDescription = new Label
            {
                Text = "Добро пожаловать в систему мониторинга сетевого оборудования!\n\n" +
                       "С помощью этой системы вы можете:\n\n" +
                       "• Отслеживать статус всех сетевых устройств в реальном времени\n" +
                       "• Просматривать устройства на интерактивной карте\n" +
                       "• Создавать заявки на обслуживание оборудования\n" +
                       "• Просматривать статистику и формировать отчеты\n" +
                       "• Экспортировать данные в различных форматах\n" +
                       "• Управлять пользователями системы (для администраторов)\n\n" +
                       "Система поддерживает работу в онлайн и офлайн режимах с автоматической синхронизацией данных.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(750, 300),
                Location = new Point(50, 180),
                Dock = DockStyle.Fill
            };

            // Панель кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(20)
            };

            // Кнопка "Начать"
            btnStart = new Button
            {
                Text = "Начать работу",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 45),
                Location = new Point(250, 15),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            btnStart.MouseEnter += (s, e) => btnStart.BackColor = Color.FromArgb(41, 128, 185);
            btnStart.MouseLeave += (s, e) => btnStart.BackColor = Color.FromArgb(52, 152, 219);

            // Кнопка "Пропустить"
            btnSkip = new Button
            {
                Text = "Пропустить",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(127, 140, 141),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35),
                Location = new Point(500, 20),
                Cursor = Cursors.Hand
            };
            btnSkip.FlatAppearance.BorderSize = 0;
            btnSkip.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            btnSkip.MouseEnter += (s, e) => btnSkip.ForeColor = Color.FromArgb(52, 73, 94);
            btnSkip.MouseLeave += (s, e) => btnSkip.ForeColor = Color.FromArgb(127, 140, 141);

            buttonPanel.Controls.Add(btnStart);
            buttonPanel.Controls.Add(btnSkip);

            panelMain.Controls.Add(buttonPanel);
            panelMain.Controls.Add(lblDescription);
            panelMain.Controls.Add(lblSubtitle);
            panelMain.Controls.Add(lblTitle);

            this.Controls.Add(panelMain);
            this.ResumeLayout(false);
        }
    }
}
