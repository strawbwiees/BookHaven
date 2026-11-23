using System.Drawing.Drawing2D;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Forms
{
    public class LoginForm : Form
    {
        private RoundedTextBox _txtUsername = null!;
        private RoundedTextBox _txtPassword = null!;
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "BookHaven - Welcome Back";
            Size = new Size(880, 680);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = UITheme.AppBackground;

            // 1. Top Banner
            var banner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = UITheme.BannerBackground
            };
            banner.Paint += PaintBanner;
            Controls.Add(banner);

            // 2. Center Card Panel
            var card = new CardPanel
            {
                Size = new Size(380, 420),
                BorderRadius = 24,
                BackgroundFill = Color.White
            };
            card.Location = new Point((ClientSize.Width - card.Width) / 2, 136);
            Controls.Add(card);

            // Avatar (Female Librarian matching PDF Page 7)
            var avatarBmp = new Bitmap(68, 68);
            using (var g = Graphics.FromImage(avatarBmp))
            {
                UITheme.DrawLibrarianAvatar(g, new Rectangle(0, 0, 68, 68));
            }

            var picAvatar = new PictureBox
            {
                Size = new Size(68, 68),
                Location = new Point((card.Width - 68) / 2, 28),
                Image = avatarBmp
            };
            card.Controls.Add(picAvatar);

            // Welcome Back
            var lblWelcome = new Label
            {
                Text = "WELCOME BACK!",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 106),
                Size = new Size(card.Width, 30)
            };
            card.Controls.Add(lblWelcome);

            var lblSub = new Label
            {
                Text = "Log in to manage the library",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 136),
                Size = new Size(card.Width, 20)
            };
            card.Controls.Add(lblSub);

            // Inputs
            int iy = 168;
            var lblUser = new Label { Text = "Username", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(35, iy), AutoSize = true };
            _txtUsername = new RoundedTextBox { Text = "admin", Location = new Point(35, iy + 20), Size = new Size(310, 36), BorderRadius = 10 };
            card.Controls.AddRange(new Control[] { lblUser, _txtUsername });

            iy += 64;
            var lblPass = new Label { Text = "Password", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(35, iy), AutoSize = true };
            _txtPassword = new RoundedTextBox { Text = "admin", UseSystemPasswordChar = true, Location = new Point(35, iy + 20), Size = new Size(310, 36), BorderRadius = 10 };
            card.Controls.AddRange(new Control[] { lblPass, _txtPassword });

            // Log in button
            var btnLogin = new PillButton
            {
                Text = "Log in",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(35, iy + 68),
                Size = new Size(310, 42)
            };
            btnLogin.Click += HandleLogin;
            card.Controls.Add(btnLogin);

            // 4 Dots decoration at bottom
            var dotsPanel = new Panel
            {
                Location = new Point((card.Width - 80) / 2, 382),
                Size = new Size(80, 16)
            };
            dotsPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color[] colors = { UITheme.SkyBlue, UITheme.HotPink, Color.FromArgb(168, 85, 247), UITheme.StatusGreen };
                for (int i = 0; i < 4; i++)
                {
                    using var b = new SolidBrush(colors[i]);
                    e.Graphics.FillEllipse(b, i * 20, 2, 10, 10);
                }
            };
            card.Controls.Add(dotsPanel);

            // Hint at bottom of form
            var lblHint = new Label
            {
                Text = "Demo Account: admin / admin",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 580),
                Size = new Size(ClientSize.Width, 24)
            };
            Controls.Add(lblHint);
        }

        private void PaintBanner(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background gradient
            var rect = new Rectangle(0, 0, Width, 88);
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(17, 30, 56), Color.FromArgb(32, 54, 96), 45f))
            {
                g.FillRectangle(brush, rect);
            }

            // Subtle concentric decorative rings
            using (var ringPen = new Pen(Color.FromArgb(20, 255, 255, 255), 1.2f))
            {
                int cx = Width / 2;
                int cy = 44;
                g.DrawEllipse(ringPen, cx - 180, cy - 180, 360, 360);
                g.DrawEllipse(ringPen, cx - 260, cy - 260, 520, 520);
                g.DrawEllipse(ringPen, cx - 340, cy - 340, 680, 680);
            }

            int centerX = Width / 2;

            // Book Circle Logo on left of title
            int circleX = centerX - 190;
            int circleY = (88 - 52) / 2;
            int circleSize = 52;
            using (var yellowBrush = new SolidBrush(Color.FromArgb(254, 218, 56)))
            {
                g.FillEllipse(yellowBrush, circleX, circleY, circleSize, circleSize);
            }
            using (var darkPen = new Pen(Color.FromArgb(24, 38, 68), 2.5f))
            {
                g.DrawEllipse(darkPen, circleX, circleY, circleSize, circleSize);
            }
            using (var iconFont = new Font("Segoe UI Symbol", 20f))
            using (var blueBrush = new SolidBrush(Color.FromArgb(24, 38, 68)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("📖", iconFont, blueBrush, new RectangleF(circleX, circleY - 1, circleSize, circleSize), sf);
            }

            // Subtitle
            using (var subFont = new Font("Segoe UI", 7.8f, FontStyle.Bold))
            using (var subBrush = new SolidBrush(Color.FromArgb(160, 195, 240)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("LIBRARY MANAGEMENT SYSTEM", subFont, subBrush, centerX, 12, sf);
            }

            // Main Title "BOOKHAVEN"
            using (var titleFont = new Font("Segoe UI", 20f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("BOOKHAVEN", titleFont, titleBrush, centerX, 24, sf);
            }

            // Pill under title "C# · WINDOWS FORMS · OOP"
            var pillRect = new Rectangle(centerX - 110, 58, 220, 20);
            using (var path = UITheme.GetRoundedRectanglePath(pillRect, 10))
            {
                using var strokePen = new Pen(Color.FromArgb(88, 182, 252), 1.4f);
                g.DrawPath(strokePen, path);

                using var pillFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(224, 242, 254));
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("C#  ·  WINDOWS FORMS  ·  OOP", pillFont, textBrush, pillRect, sf);
            }

            // Shield logo on right of title
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo_shield.png");
            if (!File.Exists(logoPath))
            {
                logoPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Resources", "logo_shield.png");
            }
            if (File.Exists(logoPath))
            {
                try
                {
                    using var img = Image.FromFile(logoPath);
                    g.DrawImage(img, centerX + 144, (88 - 52) / 2, 48, 52);
                }
                catch { }
            }

            // Cute smiling dots
            void DrawSmiley(Color color, int x, int y, int size)
            {
                using var b = new SolidBrush(color);
                g.FillEllipse(b, x, y, size, size);
                using var p = new Pen(Color.FromArgb(30, 40, 60), 1.5f);
                int eyeY = y + (int)(size * 0.35f);
                g.FillEllipse(b, x + (int)(size * 0.25f), eyeY, 2, 2);
                g.FillEllipse(b, x + (int)(size * 0.65f), eyeY, 2, 2);
                g.DrawArc(p, x + (int)(size * 0.25f), y + (int)(size * 0.35f), (int)(size * 0.5f), (int)(size * 0.35f), 0, 180);
            }

            DrawSmiley(UITheme.SkyBlue, Math.Max(20, centerX - 300), 40, 24);
            DrawSmiley(Color.FromArgb(217, 70, 239), Math.Max(20, centerX - 230), 16, 20);
            DrawSmiley(UITheme.HotPink, Math.Min(Width - 30, centerX + 220), 16, 20);
            DrawSmiley(Color.FromArgb(132, 204, 22), Math.Min(Width - 36, centerX + 290), 40, 24);
        }

        private void HandleLogin(object? sender, EventArgs e)
        {
            string user = _txtUsername.Text?.Trim() ?? string.Empty;
            string pass = _txtPassword.Text ?? string.Empty;

            if (user == _service.CurrentAdmin.Username && pass == _service.CurrentAdmin.Password)
            {
                Hide();
                var mainForm = new MainForm();
                mainForm.FormClosed += (s, args) => Close();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password. Default is: admin / admin", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
