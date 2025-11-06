using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Core.Models;
using NetworkMonitoringSystem.Helpers;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма входа в систему
    /// </summary>
    public partial class LoginForm : Form
    {
        private TableLayoutPanel mainLayout;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private Button btnLogin;
        private Label? lblError;

        public LoginForm()
        {
            InitializeComponent();
            SetupUI();
        }

        /// <summary>
        /// Создать новый контекст для аутентификации
        /// </summary>
        private AuthenticationService CreateAuthService()
        {
            // Создаем новый контекст для каждой операции аутентификации
            // Сначала пытаемся использовать онлайн БД, если не доступна - офлайн
            try
            {
                if (ConnectionService.IsMySqlOnline())
                {
                    var connectionString = DatabaseConnectionService.GetServerConnectionString();
                    var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
                    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseMySql(connectionString, serverVersion)
                        .Options;
                    var context = new ApplicationDbContext(options);
                    return new AuthenticationService(context);
                }
            }
            catch
            {
                // Если не удалось создать онлайн контекст, пробуем офлайн
            }

            // В офлайн режиме пытаемся создать контекст для аутентификации
            // (пользователи должны быть синхронизированы в офлайн БД)
            var connectionString2 = DatabaseConnectionService.GetServerConnectionString();
            var serverVersion2 = new MySqlServerVersion(new Version(8, 0, 21));
            var options2 = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseMySql(connectionString2, serverVersion2)
                .Options;
            var context2 = new ApplicationDbContext(options2);
            return new AuthenticationService(context2);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы
            this.Text = "Вход в систему - МАУ";
            this.Size = new Size(480, 560); // Увеличена высота для кнопки
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Icon = SystemIcons.Application;

            // Основной контейнер
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40, 35, 40, 35),
                ColumnCount = 1,
                RowCount = 8,
                AutoSize = false
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int row = 0;

            // Заголовок
            lblTitle = new Label
            {
                Text = Core.Models.UniversityInfo.ShortName,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Height = 40,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainLayout.Controls.Add(lblTitle, 0, row++);

            // Подзаголовок
            lblSubtitle = new Label
            {
                Text = "Система мониторинга сетевого оборудования",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(127, 140, 141),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 25)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.Controls.Add(lblSubtitle, 0, row++);

            // Поле логина - метка
            lblUsername = new Label
            {
                Text = "Логин:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 25,
                Margin = new Padding(0, 5, 0, 5)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            mainLayout.Controls.Add(lblUsername, 0, row++);

            // Поле логина - ввод
            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Height = 38,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(10, 8, 8, 8)
            };
            ToolTipHelper.SetToolTip(txtUsername, "Введите ваш логин для входа в систему");
            txtUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.Controls.Add(txtUsername, 0, row++);

            // Поле пароля - метка
            lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 25,
                Margin = new Padding(0, 5, 0, 5)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            mainLayout.Controls.Add(lblPassword, 0, row++);

            // Контейнер для пароля и чекбокса
            var passwordContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 38,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Поле пароля - ввод
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Dock = DockStyle.Left,
                UseSystemPasswordChar = true,
                Height = 38,
                Width = passwordContainer.Width - 120,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Padding = new Padding(10, 8, 8, 8)
            };
            ToolTipHelper.SetToolTip(txtPassword, "Введите ваш пароль для входа в систему");
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };

            // Чекбокс "Показать пароль"
            chkShowPassword = new CheckBox
            {
                Text = "Показать",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                AutoSize = true,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            passwordContainer.Controls.Add(txtPassword);
            passwordContainer.Controls.Add(chkShowPassword);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.Controls.Add(passwordContainer, 0, row++);

            // Кнопка входа
            btnLogin = new Button
            {
                Text = "Войти",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false,
                Height = 50,
                Margin = new Padding(0, 18, 0, 18) // Увеличенные отступы сверху и снизу
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(34, 102, 148);
            btnLogin.Click += BtnLogin_Click;
            ToolTipHelper.SetToolTip(btnLogin, "Войти в систему с указанными учетными данными");
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(41, 128, 185);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(52, 152, 219);
            // Высота строки: 50 (кнопка) + 18*2 (margin) = 86px
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            mainLayout.Controls.Add(btnLogin, 0, row++);

            // Сообщение об ошибке (многострочное, с авторазмером)
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(231, 76, 60),
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopCenter,
                Visible = false,
                MaximumSize = new Size(0, 80), // Максимальная высота для ошибки
                Padding = new Padding(5)
            };
            
            // Панель для сообщения об ошибке с фиксированной высотой
            var errorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Height = 0,
                Padding = new Padding(0, 5, 0, 5)
            };
            errorPanel.Controls.Add(lblError);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.Controls.Add(errorPanel, 0, row++);

            // Заполнение оставшегося пространства
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // Установка фокуса на поле логина
            this.Load += (s, e) => txtUsername.Focus();
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Вход...";
            if (lblError != null)
            {
                lblError.Visible = false;
                if (lblError.Parent != null)
                    lblError.Parent.Visible = false;
            }

            try
            {
                User? user = null;
                
                // Пытаемся аутентифицироваться через онлайн БД
                if (ConnectionService.IsMySqlOnline())
                {
                    try
                    {
                        using var authService = CreateAuthService();
                        user = await authService.LoginAsync(txtUsername.Text, txtPassword.Text);
                    }
                    catch (Exception ex)
                    {
                        // Если не удалось через онлайн, пробуем офлайн
                        System.Diagnostics.Debug.WriteLine($"Онлайн аутентификация не удалась: {ex.Message}");
                    }
                }
                
                // Если не удалось через онлайн, пробуем офлайн БД
                if (user == null)
                {
                    try
                    {
                        var offlineUser = await Program.OfflineDbContext.Users
                            .FirstOrDefaultAsync(u => u.Username == txtUsername.Text && u.IsActive);
                        
                        if (offlineUser != null && offlineUser.VerifyPassword(txtPassword.Text))
                        {
                            user = offlineUser;
                            // Обновляем время последнего входа
                            offlineUser.LastLoginAt = DateTime.UtcNow;
                            await Program.OfflineDbContext.SaveChangesAsync();
                            UserActivityLogger.LogLogin(txtUsername.Text, true);
                        }
                        else
                        {
                            UserActivityLogger.LogLogin(txtUsername.Text, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Офлайн аутентификация не удалась: {ex.Message}");
                    }
                }

                if (user == null)
                {
                    ShowError("Неверный логин или пароль");
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Войти";
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                if (!user.IsActive)
                {
                    ShowError("Учетная запись заблокирована. Обратитесь к администратору.");
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Войти";
                    return;
                }

                Program.CurrentUser = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения: {ex.Message}");
                btnLogin.Enabled = true;
                btnLogin.Text = "Войти";
            }
        }

        private void ShowError(string message)
        {
            if (lblError != null)
            {
                // Обрезаем длинное сообщение об ошибке для пользователя
                var displayMessage = message;
                if (message.Length > 150)
                {
                    // Оставляем только первую часть сообщения
                    var shortMessage = message.Substring(0, 150);
                    var lastDot = shortMessage.LastIndexOf('.');
                    if (lastDot > 0)
                    {
                        displayMessage = shortMessage.Substring(0, lastDot + 1);
                    }
                    else
                    {
                        displayMessage = shortMessage + "...";
                    }
                }
                
                lblError.Text = displayMessage;
                lblError.Visible = true;
                
                // Показываем родительскую панель
                if (lblError.Parent != null)
                {
                    lblError.Parent.Visible = true;
                    // Устанавливаем высоту панели в зависимости от размера сообщения
                    var textHeight = TextRenderer.MeasureText(displayMessage, lblError.Font, 
                        new Size(lblError.Parent.Width - 20, int.MaxValue), 
                        TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
                    lblError.Parent.Height = Math.Min(textHeight + 20, 80); // Максимум 80px
                }
            }
        }
    }
}
